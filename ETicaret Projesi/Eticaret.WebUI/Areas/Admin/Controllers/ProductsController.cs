using Eticaret.Core.Entities;
using Eticaret.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(
            DatabaseContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .OrderBy(x => x.OrderNo)
                .ToListAsync();

            return View(products);
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            await FillSelectListsAsync();

            return View();
        }

        // POST: Admin/Products/Create
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
                    await SaveImageFileAsync(imageFile);
            }

            product.CreateDate = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var product =
                await _context.Products.FindAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            await FillSelectListsAsync(
                product.BrandId,
                product.CategoryId);

            return View(product);
        }

        // POST: Admin/Products/Edit/5
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
                await _context.Products.FindAsync(id);

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

            // Overposting'i önlemek için sadece düzenlenmesine
            // izin verilen alanları güncelliyoruz.
            // CreateDate ve mevcut görsel gibi sistem alanları korunuyor.
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

            // Yeni görsel seçildiyse eski görseli
            // sunucudan silip yeni görseli kaydediyoruz.
            if (imageFile is not null &&
                imageFile.Length > 0)
            {
                DeleteImageFile(
                    existingProduct.Image);

                existingProduct.Image =
                    await SaveImageFileAsync(
                        imageFile);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var product =
                await _context.Products.FindAsync(id);

            if (product is not null)
            {
                // Ürüne ait görsel varsa sunucudan siliyoruz.
                DeleteImageFile(product.Image);

                // Ardından ürünü veritabanından siliyoruz.
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Marka ve kategori dropdown listelerini dolduruyoruz.
        private async Task FillSelectListsAsync(
            int? selectedBrandId = null,
            int? selectedCategoryId = null)
        {
            var brands = await _context.Brands
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            var categories =
                await _context.Categories
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.OrderNo)
                    .ThenBy(x => x.Name)
                    .ToListAsync();

            ViewData["BrandId"] = new SelectList(
                brands,
                "Id",
                "Name",
                selectedBrandId);

            ViewData["CategoryId"] = new SelectList(
                categories,
                "Id",
                "Name",
                selectedCategoryId);
        }

        // Yüklenen görselin uzantısını ve boyutunu kontrol ediyoruz.
        private void ValidateImageFile(
            IFormFile? imageFile)
        {
            if (imageFile is null ||
                imageFile.Length == 0)
            {
                return;
            }

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
                ModelState.AddModelError(
                    nameof(Product.Image),
                    "Sadece JPG, JPEG, PNG veya WEBP dosyası yükleyebilirsiniz.");
            }

            const long maxFileSize =
                2 * 1024 * 1024;

            if (imageFile.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(Product.Image),
                    "Görsel dosyası en fazla 2 MB olabilir.");
            }
        }

        // Ürün görselini wwwroot/uploads/products
        // klasörüne kaydediyoruz.
        private async Task<string> SaveImageFileAsync(
            IFormFile imageFile)
        {
            var extension = Path
                .GetExtension(imageFile.FileName)
                .ToLowerInvariant();

            var uploadDirectory = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "products");

            Directory.CreateDirectory(
                uploadDirectory);

            var fileName =
                $"{Guid.NewGuid()}{extension}";

            var physicalPath = Path.Combine(
                uploadDirectory,
                fileName);

            await using var stream =
                new FileStream(
                    physicalPath,
                    FileMode.Create);

            await imageFile.CopyToAsync(stream);

            return $"/uploads/products/{fileName}";
        }

        // Ürün görselini fiziksel olarak sunucudan siliyoruz.
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

            var physicalPath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                relativePath);

            if (System.IO.File.Exists(
                physicalPath))
            {
                System.IO.File.Delete(
                    physicalPath);
            }
        }

        private bool ProductExists(int id)
        {
            return _context.Products
                .Any(x => x.Id == id);
        }
    }
}