using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class ProductImagesController : Controller
    {
        private readonly IService<ProductImage> _productImageService;
        private readonly IService<Product> _productService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductImagesController(
            IService<ProductImage> productImageService,
            IService<Product> productService,
            IWebHostEnvironment webHostEnvironment)
        {
            _productImageService = productImageService;
            _productService = productService;
            _webHostEnvironment = webHostEnvironment;
        }

        // --------------------------------------------------
        // LISTELEME + ÜRÜNE GÖRE FİLTRELEME
        // --------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Index(int? productId)
        {
            var query = _productImageService
                .GetQueryable()
                .Include(x => x.Product)
                .AsNoTracking();

            if (productId.HasValue)
            {
                query = query.Where(x => x.ProductId == productId.Value);
            }

            var images = await query
                .OrderBy(x => x.ProductId)
                .ThenBy(x => x.OrderNo)
                .ToListAsync();

            await LoadProductsAsync(productId);

            ViewBag.SelectedProductId = productId;

            return View(images);
        }

        // --------------------------------------------------
        // DETAY
        // --------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var image = await _productImageService
                .GetQueryable()
                .Include(x => x.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (image is null)
            {
                return NotFound();
            }

            return View(image);
        }

        // --------------------------------------------------
        // EKLEME GET
        // --------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Create(int? productId)
        {
            await LoadProductsAsync(productId);

            var model = new ProductImageFormViewModel();

            if (productId.HasValue)
            {
                model.ProductId = productId.Value;
            }

            return View(model);
        }

        // --------------------------------------------------
        // EKLEME POST
        // --------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ProductImageFormViewModel model)
        {
            if (model.ImageFile is null ||
                model.ImageFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.ImageFile),
                    "Lütfen bir resim seçiniz.");
            }

            var productExists = await _productService
                .GetQueryable()
                .AnyAsync(x => x.Id == model.ProductId);

            if (!productExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProductId),
                    "Seçilen ürün bulunamadı.");
            }

            if (!ModelState.IsValid)
            {
                await LoadProductsAsync(model.ProductId);

                return View(model);
            }

            var imagePath = await SaveImageAsync(
                model.ImageFile!);

            // Yeni resim ana resim olacaksa,
            // aynı üründeki diğer ana resimleri kapat.
            if (model.IsMain)
            {
                await ClearMainImagesAsync(model.ProductId);
            }

            var productImage = new ProductImage
            {
                ProductId = model.ProductId,
                ImagePath = imagePath,
                IsMain = model.IsMain,
                OrderNo = model.OrderNo
            };

            await _productImageService.AddAsync(productImage);
            await _productImageService.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Ürün resmi başarıyla eklendi.";

            return RedirectToAction(
                nameof(Index),
                new { productId = model.ProductId });
        }

        // --------------------------------------------------
        // GÜNCELLEME GET
        // --------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var image = await _productImageService
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (image is null)
            {
                return NotFound();
            }

            var model = new ProductImageFormViewModel
            {
                Id = image.Id,
                ProductId = image.ProductId,
                ExistingImagePath = image.ImagePath,
                IsMain = image.IsMain,
                OrderNo = image.OrderNo
            };

            await LoadProductsAsync(image.ProductId);

            return View(model);
        }

        // --------------------------------------------------
        // GÜNCELLEME POST
        // --------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ProductImageFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var image = await _productImageService.FindAsync(id);

            if (image is null)
            {
                return NotFound();
            }

            var productExists = await _productService
                .GetQueryable()
                .AnyAsync(x => x.Id == model.ProductId);

            if (!productExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProductId),
                    "Seçilen ürün bulunamadı.");
            }

            if (!ModelState.IsValid)
            {
                model.ExistingImagePath = image.ImagePath;

                await LoadProductsAsync(model.ProductId);

                return View(model);
            }

            var oldImagePath = image.ImagePath;

            // Kullanıcı yeni resim seçtiyse
            if (model.ImageFile is not null &&
                model.ImageFile.Length > 0)
            {
                var newImagePath =
                    await SaveImageAsync(model.ImageFile);

                image.ImagePath = newImagePath;

                DeleteImageFile(oldImagePath);
            }

            // Ana resim olarak işaretlendiyse
            if (model.IsMain)
            {
                await ClearMainImagesAsync(
                    model.ProductId,
                    image.Id);
            }

            image.ProductId = model.ProductId;
            image.IsMain = model.IsMain;
            image.OrderNo = model.OrderNo;

            _productImageService.Update(image);

            await _productImageService.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Ürün resmi başarıyla güncellendi.";

            return RedirectToAction(
                nameof(Index),
                new { productId = image.ProductId });
        }

        // --------------------------------------------------
        // SİLME GET
        // --------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var image = await _productImageService
                .GetQueryable()
                .Include(x => x.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (image is null)
            {
                return NotFound();
            }

            return View(image);
        }

        // --------------------------------------------------
        // SİLME POST
        // --------------------------------------------------

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var image = await _productImageService.FindAsync(id);

            if (image is null)
            {
                return NotFound();
            }

            var productId = image.ProductId;
            var wasMainImage = image.IsMain;

            DeleteImageFile(image.ImagePath);

            _productImageService.Delete(image);

            await _productImageService.SaveChangesAsync();

            // Silinen resim ana resimse,
            // kalan ilk resmi ana resim yap.
            if (wasMainImage)
            {
                var newMainImage = await _productImageService
                    .GetQueryable()
                    .Where(x => x.ProductId == productId)
                    .OrderBy(x => x.OrderNo)
                    .ThenBy(x => x.Id)
                    .FirstOrDefaultAsync();

                if (newMainImage is not null)
                {
                    newMainImage.IsMain = true;

                    _productImageService.Update(newMainImage);

                    await _productImageService
                        .SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] =
                "Ürün resmi başarıyla silindi.";

            return RedirectToAction(
                nameof(Index),
                new { productId });
        }

        // --------------------------------------------------
        // ÜRÜNLERİ DROPDOWN'A YÜKLE
        // --------------------------------------------------

        private async Task LoadProductsAsync(
            int? selectedProductId = null)
        {
            var products = await _productService
                .GetQueryable()
                .OrderBy(x => x.Name)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Products = new SelectList(
                products,
                "Id",
                "Name",
                selectedProductId);
        }

        // --------------------------------------------------
        // RESİM KAYDET
        // --------------------------------------------------

        private async Task<string> SaveImageAsync(
            IFormFile imageFile)
        {
            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension = Path
                .GetExtension(imageFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Sadece JPG, JPEG, PNG ve WEBP yüklenebilir.");
            }

            // Maksimum 5 MB
            if (imageFile.Length > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException(
                    "Resim boyutu en fazla 5 MB olabilir.");
            }

            var fileName =
                $"{Guid.NewGuid()}{extension}";

            var folderPath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "images",
                "products");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(
                folderPath,
                fileName);

            await using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create);

            await imageFile.CopyToAsync(stream);

            return $"/images/products/{fileName}";
        }

        // --------------------------------------------------
        // FİZİKSEL DOSYAYI SİL
        // --------------------------------------------------

        private void DeleteImageFile(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            var relativePath =
                imagePath.TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar);

            var fullPath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        // --------------------------------------------------
        // DİĞER ANA RESİMLERİ KAPAT
        // --------------------------------------------------

        private async Task ClearMainImagesAsync(
            int productId,
            int? exceptImageId = null)
        {
            var query = _productImageService
                .GetQueryable()
                .Where(x =>
                    x.ProductId == productId &&
                    x.IsMain);

            if (exceptImageId.HasValue)
            {
                query = query.Where(
                    x => x.Id != exceptImageId.Value);
            }

            var mainImages =
                await query.ToListAsync();

            foreach (var image in mainImages)
            {
                image.IsMain = false;

                _productImageService.Update(image);
            }
        }
    }
}