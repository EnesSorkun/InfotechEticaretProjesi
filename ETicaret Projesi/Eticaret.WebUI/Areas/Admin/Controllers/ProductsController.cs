using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class ProductsController : Controller
    {
        private readonly IService<Product> _productService;
        private readonly IService<Brand> _brandService;
        private readonly IService<Category> _categoryService;
        private readonly IService<ProductImage> _productImageService;
        private readonly IService<OrderDetail> _orderDetailService;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public ProductsController(
            IService<Product> productService,
            IService<Brand> brandService,
            IService<Category> categoryService,
            IService<ProductImage> productImageService,
            IService<OrderDetail> orderDetailService,
            IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _brandService = brandService;
            _categoryService = categoryService;
            _productImageService = productImageService;
            _orderDetailService = orderDetailService;
            _webHostEnvironment = webHostEnvironment;
        }


        // =====================================================
        // GET: Admin/Products
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var products = await _productService
                .GetQueryable()
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .OrderBy(x => x.OrderNo)
                .AsNoTracking()
                .ToListAsync();

            return View(products);
        }


        // =====================================================
        // GET: Admin/Products/Details/5
        // =====================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }


            var product = await _productService
                .GetQueryable()
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id.Value);


            if (product is null)
            {
                return NotFound();
            }


            return View(product);
        }


        // =====================================================
        // GET: Admin/Products/Create
        // =====================================================

        public async Task<IActionResult> Create()
        {
            await FillSelectListsAsync();

            return View();
        }


        // =====================================================
        // POST: Admin/Products/Create
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Product product,
            IFormFile? imageFile)
        {
            ValidateImageFile(imageFile);


            if (!ModelState.IsValid)
            {
                await FillSelectListsAsync(
                    product.BrandId,
                    product.CategoryId);

                return View(product);
            }


            if (imageFile is not null &&
                imageFile.Length > 0)
            {
                product.Image =
                    await SaveImageFileAsync(
                        imageFile);
            }


            product.CreateDate =
                DateTime.UtcNow;


            await _productService
                .AddAsync(product);


            await _productService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Ürün başarıyla oluşturuldu.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // GET: Admin/Products/Edit/5
        // =====================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }


            var product =
                await _productService.FindAsync(
                    id.Value);


            if (product is null)
            {
                return NotFound();
            }


            await FillSelectListsAsync(
                product.BrandId,
                product.CategoryId);


            return View(product);
        }


        // =====================================================
        // POST: Admin/Products/Edit/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Product product,
            IFormFile? imageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }


            var existingProduct =
                await _productService.FindAsync(
                    id);


            if (existingProduct is null)
            {
                return NotFound();
            }


            ValidateImageFile(imageFile);


            if (!ModelState.IsValid)
            {
                product.Image =
                    existingProduct.Image;

                product.CreateDate =
                    existingProduct.CreateDate;


                await FillSelectListsAsync(
                    product.BrandId,
                    product.CategoryId);


                return View(product);
            }


            // =====================================================
            // DÜZENLENEBİLİR ALANLARI GÜNCELLE
            // =====================================================

            existingProduct.Name =
                product.Name;

            existingProduct.Description =
                product.Description;

            existingProduct.Price =
                product.Price;

            existingProduct.ProductCode =
                product.ProductCode;

            existingProduct.Stock =
                product.Stock;

            existingProduct.IsActive =
                product.IsActive;

            existingProduct.IsHome =
                product.IsHome;

            existingProduct.CategoryId =
                product.CategoryId;

            existingProduct.BrandId =
                product.BrandId;

            existingProduct.OrderNo =
                product.OrderNo;


            // =====================================================
            // YENİ GÖRSEL YÜKLENDİYSE DEĞİŞTİR
            // =====================================================

            if (imageFile is not null &&
                imageFile.Length > 0)
            {
                DeleteImageFile(
                    existingProduct.Image);


                existingProduct.Image =
                    await SaveImageFileAsync(
                        imageFile);
            }


            await _productService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Ürün başarıyla güncellendi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // GET: Admin/Products/Delete/5
        // =====================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }


            var product = await _productService
                .GetQueryable()
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id.Value);


            if (product is null)
            {
                return NotFound();
            }


            return View(product);
        }


        // =====================================================
        // POST: Admin/Products/Delete/5
        // =====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var product =
                await _productService.FindAsync(
                    id);


            if (product is null)
            {
                return NotFound();
            }


            // =====================================================
            // ÜRÜN DAHA ÖNCE SİPARİŞ EDİLMİŞ Mİ?
            // =====================================================

            var hasOrderHistory =
                await _orderDetailService
                    .GetQueryable()
                    .AnyAsync(x =>
                        x.ProductId == id);


            // =====================================================
            // SİPARİŞ GEÇMİŞİ VARSA ÜRÜNÜ KALICI SİLME
            // =====================================================

            if (hasOrderHistory)
            {
                TempData["ErrorMessage"] =
                    "Bu ürün daha önce bir siparişte kullanıldığı için kalıcı olarak silinemez. Ürünü pasif duruma getirebilirsiniz.";


                return RedirectToAction(
                    nameof(Index));
            }


            // =====================================================
            // ÜRÜNE AİT EK RESİMLERİ BUL
            // =====================================================

            var productImages =
                await _productImageService
                    .GetQueryable()
                    .Where(x =>
                        x.ProductId == id)
                    .ToListAsync();


            // =====================================================
            // FİZİKSEL DOSYA YOLLARINI SAKLA
            // =====================================================

            var additionalImagePaths =
                productImages
                    .Select(x => x.ImagePath)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .ToList();


            // =====================================================
            // PRODUCTIMAGE KAYITLARINI SİL
            // =====================================================

            foreach (var productImage in productImages)
            {
                _productImageService.Delete(
                    productImage);
            }


            await _productImageService
                .SaveChangesAsync();


            // =====================================================
            // ÜRÜNÜ VERİTABANINDAN SİL
            // =====================================================

            _productService.Delete(
                product);


            await _productService
                .SaveChangesAsync();


            // =====================================================
            // DB İŞLEMLERİ BAŞARILI OLDUKTAN SONRA
            // FİZİKSEL DOSYALARI SİL
            // =====================================================

            DeleteImageFile(
                product.Image);


            foreach (var imagePath in additionalImagePaths)
            {
                DeleteImageFile(
                    imagePath);
            }


            TempData["SuccessMessage"] =
                "Ürün başarıyla silindi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // MARKA VE KATEGORİ DROPDOWN
        // =====================================================

        private async Task FillSelectListsAsync(
            int? selectedBrandId = null,
            int? selectedCategoryId = null)
        {
            var brands = await _brandService
                .GetQueryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .AsNoTracking()
                .ToListAsync();


            var categories = await _categoryService
                .GetQueryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.OrderNo)
                .ThenBy(x => x.Name)
                .AsNoTracking()
                .ToListAsync();


            ViewData["BrandId"] =
                new SelectList(
                    brands,
                    "Id",
                    "Name",
                    selectedBrandId);


            ViewData["CategoryId"] =
                new SelectList(
                    categories,
                    "Id",
                    "Name",
                    selectedCategoryId);
        }


        // =====================================================
        // GÖRSEL DOĞRULAMA
        // =====================================================

        private void ValidateImageFile(
            IFormFile? imageFile)
        {
            if (imageFile is null ||
                imageFile.Length == 0)
            {
                return;
            }


            var allowedExtensions =
                new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };


            var extension =
                Path.GetExtension(
                        imageFile.FileName)
                    .ToLowerInvariant();


            if (!allowedExtensions.Contains(
                    extension))
            {
                ModelState.AddModelError(
                    nameof(Product.Image),
                    "Sadece JPG, JPEG, PNG veya WEBP dosyası yükleyebilirsiniz.");
            }


            const long maxFileSize =
                2 * 1024 * 1024;


            if (imageFile.Length >
                maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(Product.Image),
                    "Görsel dosyası en fazla 2 MB olabilir.");
            }
        }


        // =====================================================
        // GÖRSEL KAYDET
        // =====================================================

        private async Task<string> SaveImageFileAsync(
            IFormFile imageFile)
        {
            var extension =
                Path.GetExtension(
                        imageFile.FileName)
                    .ToLowerInvariant();


            var uploadDirectory =
                Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "uploads",
                    "products");


            Directory.CreateDirectory(
                uploadDirectory);


            var fileName =
                $"{Guid.NewGuid()}{extension}";


            var physicalPath =
                Path.Combine(
                    uploadDirectory,
                    fileName);


            await using var stream =
                new FileStream(
                    physicalPath,
                    FileMode.Create);


            await imageFile.CopyToAsync(
                stream);


            return
                $"/uploads/products/{fileName}";
        }


        // =====================================================
        // GÖRSEL SİL
        // =====================================================

        private void DeleteImageFile(
            string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(
                    imagePath))
            {
                return;
            }


            var relativePath =
                imagePath.TrimStart('/');


            var physicalPath =
                Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    relativePath);


            if (System.IO.File.Exists(
                    physicalPath))
            {
                System.IO.File.Delete(
                    physicalPath);
            }
        }
    }
}