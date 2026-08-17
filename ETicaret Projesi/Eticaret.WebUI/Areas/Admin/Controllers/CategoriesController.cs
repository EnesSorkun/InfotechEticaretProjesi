using Eticaret.Core.Entities;
using Eticaret.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class CategoriesController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoriesController(
            DatabaseContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .OrderBy(x => x.OrderNo)
                .ToListAsync();

            return View(categories);
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category is null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Categories/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .Where(x => x.IsActive)
                .OrderBy(x => x.OrderNo)
                .ToListAsync();

            return View();
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Category category,
            IFormFile? imageFile)
        {
            ValidateImageFile(imageFile);

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.OrderNo)
                    .ToListAsync();

                return View(category);
            }

            if (imageFile is not null && imageFile.Length > 0)
            {
                category.Image =
                    await SaveImageFileAsync(imageFile);
            }

            category.CreateDate = DateTime.UtcNow;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var category =
                await _context.Categories.FindAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            // Üst kategori listesini dolduruyoruz.
            // Düzenlenen kategori kendi üst kategorisi olamaz.
            ViewBag.Categories = await _context.Categories
                .Where(x =>
                    x.IsActive &&
                    x.Id != category.Id)
                .OrderBy(x => x.OrderNo)
                .ToListAsync();

            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Category category,
            IFormFile? imageFile)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            var existingCategory =
                await _context.Categories.FindAsync(id);

            if (existingCategory is null)
            {
                return NotFound();
            }

            ValidateImageFile(imageFile);

            if (!ModelState.IsValid)
            {
                category.Image = existingCategory.Image;
                category.CreateDate =
                    existingCategory.CreateDate;

                ViewBag.Categories =
                    await _context.Categories
                        .Where(x =>
                            x.IsActive &&
                            x.Id != category.Id)
                        .OrderBy(x => x.OrderNo)
                        .ToListAsync();

                return View(category);
            }

            // Sadece düzenlenmesine izin verilen alanları güncelliyoruz.
            // CreateDate ve mevcut görsel gibi sistem alanları korunuyor.
            existingCategory.Name = category.Name;
            existingCategory.Description =
                category.Description;
            existingCategory.IsActive =
                category.IsActive;
            existingCategory.IsTopMenu =
                category.IsTopMenu;
            existingCategory.ParentId =
                category.ParentId;
            existingCategory.OrderNo =
                category.OrderNo;

            // Yeni görsel seçildiyse eski görseli sunucudan siliyoruz
            // ve yeni görseli kaydediyoruz.
            if (imageFile is not null &&
                imageFile.Length > 0)
            {
                DeleteImageFile(
                    existingCategory.Image);

                existingCategory.Image =
                    await SaveImageFileAsync(
                        imageFile);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category is null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var category =
                await _context.Categories.FindAsync(id);

            if (category is not null)
            {
                // Kategoriye ait görsel varsa önce sunucudan siliyoruz.
                DeleteImageFile(category.Image);

                // Ardından veritabanındaki kategori kaydını siliyoruz.
                _context.Categories.Remove(category);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
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
                    nameof(Category.Image),
                    "Sadece JPG, JPEG, PNG veya WEBP dosyası yükleyebilirsiniz.");
            }

            const long maxFileSize =
                2 * 1024 * 1024;

            if (imageFile.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(Category.Image),
                    "Görsel dosyası en fazla 2 MB olabilir.");
            }
        }

        // Görseli wwwroot/uploads/categories klasörüne kaydediyoruz.
        private async Task<string> SaveImageFileAsync(
            IFormFile imageFile)
        {
            var extension = Path
                .GetExtension(imageFile.FileName)
                .ToLowerInvariant();

            var uploadDirectory = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "categories");

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

            return $"/uploads/categories/{fileName}";
        }

        // Görselin fiziksel dosyasını sunucudan siliyoruz.
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

        private bool CategoryExists(int id)
        {
            return _context.Categories
                .Any(x => x.Id == id);
        }
    }
}