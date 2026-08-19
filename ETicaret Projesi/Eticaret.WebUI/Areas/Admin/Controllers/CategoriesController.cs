using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class CategoriesController : Controller
    {
        private readonly IService<Category> _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoriesController(
            IService<Category> categoryService,
            IWebHostEnvironment webHostEnvironment)
        {
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }


        // =====================================================
        // GET: Admin/Categories
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService
                .GetQueryable()
                .OrderBy(x => x.OrderNo)
                .AsNoTracking()
                .ToListAsync();

            return View(categories);
        }


        // =====================================================
        // GET: Admin/Categories/Details/5
        // =====================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var category =
                await _categoryService.FindAsync(
                    id.Value);

            if (category is null)
            {
                return NotFound();
            }

            return View(category);
        }


        // =====================================================
        // GET: Admin/Categories/Create
        // =====================================================

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories =
                await GetActiveCategoriesAsync();

            return View();
        }


        // =====================================================
        // POST: Admin/Categories/Create
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Category category,
            IFormFile? imageFile)
        {
            ValidateImageFile(imageFile);


            if (!ModelState.IsValid)
            {
                ViewBag.Categories =
                    await GetActiveCategoriesAsync();

                return View(category);
            }


            // Görsel seçildiyse sunucuya kaydediyoruz.
            if (imageFile is not null &&
                imageFile.Length > 0)
            {
                category.Image =
                    await SaveImageFileAsync(
                        imageFile);
            }


            category.CreateDate =
                DateTime.UtcNow;


            await _categoryService
                .AddAsync(category);


            await _categoryService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Kategori başarıyla oluşturuldu.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // GET: Admin/Categories/Edit/5
        // =====================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }


            var category =
                await _categoryService.FindAsync(
                    id.Value);


            if (category is null)
            {
                return NotFound();
            }


            // Düzenlenen kategori kendisini
            // üst kategori olarak seçemez.
            ViewBag.Categories =
                await GetActiveCategoriesAsync(
                    category.Id);


            return View(category);
        }


        // =====================================================
        // POST: Admin/Categories/Edit/5
        // =====================================================

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
                await _categoryService.FindAsync(
                    id);


            if (existingCategory is null)
            {
                return NotFound();
            }


            ValidateImageFile(imageFile);


            if (!ModelState.IsValid)
            {
                category.Image =
                    existingCategory.Image;

                category.CreateDate =
                    existingCategory.CreateDate;


                ViewBag.Categories =
                    await GetActiveCategoriesAsync(
                        category.Id);


                return View(category);
            }


            // Sadece düzenlenmesine izin verilen
            // alanları güncelliyoruz.
            existingCategory.Name =
                category.Name;

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


            // Yeni görsel seçildiyse eski görseli
            // siliyoruz ve yenisini kaydediyoruz.
            if (imageFile is not null &&
                imageFile.Length > 0)
            {
                DeleteImageFile(
                    existingCategory.Image);


                existingCategory.Image =
                    await SaveImageFileAsync(
                        imageFile);
            }


            await _categoryService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Kategori başarıyla güncellendi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // GET: Admin/Categories/Delete/5
        // =====================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }


            var category =
                await _categoryService.FindAsync(
                    id.Value);


            if (category is null)
            {
                return NotFound();
            }


            return View(category);
        }


        // =====================================================
        // POST: Admin/Categories/Delete/5
        // =====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var category =
                await _categoryService.FindAsync(
                    id);


            if (category is null)
            {
                return NotFound();
            }


            // Kategoriye ait görsel varsa
            // fiziksel dosyayı siliyoruz.
            DeleteImageFile(
                category.Image);


            // Veritabanındaki kategori kaydını siliyoruz.
            _categoryService.Delete(
                category);


            await _categoryService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Kategori başarıyla silindi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // AKTİF KATEGORİLERİ GETİR
        // =====================================================

        private async Task<List<Category>>
            GetActiveCategoriesAsync(
                int? excludedCategoryId = null)
        {
            var query = _categoryService
                .GetQueryable()
                .Where(x => x.IsActive);


            // Edit ekranında düzenlenen kategoriyi
            // üst kategori listesinden çıkarıyoruz.
            if (excludedCategoryId.HasValue)
            {
                query = query.Where(x =>
                    x.Id != excludedCategoryId.Value);
            }


            return await query
                .OrderBy(x => x.OrderNo)
                .AsNoTracking()
                .ToListAsync();
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
                    nameof(Category.Image),
                    "Sadece JPG, JPEG, PNG veya WEBP dosyası yükleyebilirsiniz.");
            }


            const long maxFileSize =
                2 * 1024 * 1024;


            if (imageFile.Length >
                maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(Category.Image),
                    "Görsel dosyası en fazla 2 MB olabilir.");
            }
        }


        // =====================================================
        // GÖRSELİ KAYDET
        // =====================================================

        private async Task<string>
            SaveImageFileAsync(
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
                    "categories");


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
                $"/uploads/categories/{fileName}";
        }


        // =====================================================
        // GÖRSELİ SİL
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