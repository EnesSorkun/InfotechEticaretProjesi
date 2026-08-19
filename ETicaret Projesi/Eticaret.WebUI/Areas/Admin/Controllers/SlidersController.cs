using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class SlidersController : Controller
    {
        private readonly IService<Slider> _sliderService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SlidersController(
            IService<Slider> sliderService,
            IWebHostEnvironment webHostEnvironment)
        {
            _sliderService = sliderService;
            _webHostEnvironment = webHostEnvironment;
        }


        // =====================================================
        // GET: Admin/Sliders
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var sliders = await _sliderService
                .GetQueryable()
                .OrderByDescending(x => x.Id)
                .AsNoTracking()
                .ToListAsync();

            return View(sliders);
        }


        // =====================================================
        // GET: Admin/Sliders/Details/5
        // =====================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var slider =
                await _sliderService.FindAsync(
                    id.Value);

            if (slider is null)
            {
                return NotFound();
            }

            return View(slider);
        }


        // =====================================================
        // GET: Admin/Sliders/Create
        // =====================================================

        public IActionResult Create()
        {
            return View();
        }


        // =====================================================
        // POST: Admin/Sliders/Create
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Slider slider,
            IFormFile? imageFile)
        {
            ValidateImageFile(imageFile);

            if (!ModelState.IsValid)
            {
                return View(slider);
            }

            if (imageFile is not null &&
                imageFile.Length > 0)
            {
                slider.Image =
                    await SaveImageFileAsync(
                        imageFile);
            }

            await _sliderService
                .AddAsync(slider);

            await _sliderService
                .SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Slider başarıyla oluşturuldu.";

            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // GET: Admin/Sliders/Edit/5
        // =====================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var slider =
                await _sliderService.FindAsync(
                    id.Value);

            if (slider is null)
            {
                return NotFound();
            }

            return View(slider);
        }


        // =====================================================
        // POST: Admin/Sliders/Edit/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Slider slider,
            IFormFile? imageFile)
        {
            if (id != slider.Id)
            {
                return NotFound();
            }

            var existingSlider =
                await _sliderService.FindAsync(
                    id);

            if (existingSlider is null)
            {
                return NotFound();
            }

            ValidateImageFile(imageFile);

            if (!ModelState.IsValid)
            {
                slider.Image =
                    existingSlider.Image;

                return View(slider);
            }

            // Sadece düzenlenebilir alanları güncelliyoruz.
            existingSlider.Title =
                slider.Title;

            existingSlider.Description =
                slider.Description;

            existingSlider.Link =
                slider.Link;


            // Yeni görsel seçildiyse eski görseli siliyoruz
            // ve yeni görseli kaydediyoruz.
            if (imageFile is not null &&
                imageFile.Length > 0)
            {
                DeleteImageFile(
                    existingSlider.Image);

                existingSlider.Image =
                    await SaveImageFileAsync(
                        imageFile);
            }

            await _sliderService
                .SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Slider başarıyla güncellendi.";

            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // GET: Admin/Sliders/Delete/5
        // =====================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var slider =
                await _sliderService.FindAsync(
                    id.Value);

            if (slider is null)
            {
                return NotFound();
            }

            return View(slider);
        }


        // =====================================================
        // POST: Admin/Sliders/Delete/5
        // =====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var slider =
                await _sliderService.FindAsync(
                    id);

            if (slider is null)
            {
                return NotFound();
            }

            // Slider görselini sunucudan siliyoruz.
            DeleteImageFile(
                slider.Image);

            // Slider kaydını veritabanından siliyoruz.
            _sliderService.Delete(
                slider);

            await _sliderService
                .SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Slider başarıyla silindi.";

            return RedirectToAction(
                nameof(Index));
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
                    nameof(Slider.Image),
                    "Sadece JPG, JPEG, PNG veya WEBP dosyası yükleyebilirsiniz.");
            }

            const long maxFileSize =
                2 * 1024 * 1024;

            if (imageFile.Length >
                maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(Slider.Image),
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
                    "sliders");

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
                $"/uploads/sliders/{fileName}";
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