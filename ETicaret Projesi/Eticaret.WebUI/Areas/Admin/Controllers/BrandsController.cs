using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class BrandsController : Controller
    {
        private readonly IService<Brand> _brandService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BrandsController(
            IService<Brand> brandService,
            IWebHostEnvironment webHostEnvironment)
        {
            _brandService = brandService;
            _webHostEnvironment = webHostEnvironment;
        }


        // =====================================================
        // GET: Admin/Brands
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var brands = await _brandService
                .GetAllAsync();

            brands = brands
                .OrderBy(x => x.OrderNo)
                .ToList();

            return View(brands);
        }


        // =====================================================
        // GET: Admin/Brands/Details/5
        // =====================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var brand =
                await _brandService.FindAsync(
                    id.Value);

            if (brand is null)
            {
                return NotFound();
            }

            return View(brand);
        }


        // =====================================================
        // GET: Admin/Brands/Create
        // =====================================================

        public IActionResult Create()
        {
            return View();
        }


        // =====================================================
        // POST: Admin/Brands/Create
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Brand brand,
            IFormFile? logoFile)
        {
            ValidateLogoFile(logoFile);

            if (!ModelState.IsValid)
            {
                return View(brand);
            }

            if (logoFile is not null &&
                logoFile.Length > 0)
            {
                brand.Logo =
                    await SaveLogoFileAsync(
                        logoFile);
            }

            brand.CreateDate =
                DateTime.UtcNow;

            await _brandService
                .AddAsync(brand);

            await _brandService
                .SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Marka başarıyla oluşturuldu.";

            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // GET: Admin/Brands/Edit/5
        // =====================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var brand =
                await _brandService.FindAsync(
                    id.Value);

            if (brand is null)
            {
                return NotFound();
            }

            return View(brand);
        }


        // =====================================================
        // POST: Admin/Brands/Edit/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Brand brand,
            IFormFile? logoFile)
        {
            if (id != brand.Id)
            {
                return NotFound();
            }

            var existingBrand =
                await _brandService.FindAsync(
                    id);

            if (existingBrand is null)
            {
                return NotFound();
            }

            ValidateLogoFile(logoFile);

            if (!ModelState.IsValid)
            {
                brand.Logo =
                    existingBrand.Logo;

                brand.CreateDate =
                    existingBrand.CreateDate;

                return View(brand);
            }

            // Sadece düzenlenebilir alanları değiştiriyoruz.
            existingBrand.Name =
                brand.Name;

            existingBrand.IsActive =
                brand.IsActive;

            existingBrand.OrderNo =
                brand.OrderNo;


            // Yeni logo yüklendiyse eski logoyu sil,
            // yeni dosyayı kaydet.
            if (logoFile is not null &&
                logoFile.Length > 0)
            {
                DeleteLogoFile(
                    existingBrand.Logo);

                existingBrand.Logo =
                    await SaveLogoFileAsync(
                        logoFile);
            }


            await _brandService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Marka başarıyla güncellendi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // GET: Admin/Brands/Delete/5
        // =====================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var brand =
                await _brandService.FindAsync(
                    id.Value);

            if (brand is null)
            {
                return NotFound();
            }

            return View(brand);
        }


        // =====================================================
        // POST: Admin/Brands/Delete/5
        // =====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var brand =
                await _brandService.FindAsync(
                    id);

            if (brand is null)
            {
                return NotFound();
            }


            DeleteLogoFile(
                brand.Logo);


            _brandService.Delete(
                brand);


            await _brandService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Marka başarıyla silindi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // LOGO VALIDATION
        // =====================================================

        private void ValidateLogoFile(
            IFormFile? logoFile)
        {
            if (logoFile is null ||
                logoFile.Length == 0)
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
                        logoFile.FileName)
                    .ToLowerInvariant();


            if (!allowedExtensions.Contains(
                    extension))
            {
                ModelState.AddModelError(
                    nameof(Brand.Logo),
                    "Sadece JPG, JPEG, PNG veya WEBP dosyası yükleyebilirsiniz.");
            }


            const long maxFileSize =
                2 * 1024 * 1024;


            if (logoFile.Length >
                maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(Brand.Logo),
                    "Logo dosyası en fazla 2 MB olabilir.");
            }
        }


        // =====================================================
        // LOGO KAYDET
        // =====================================================

        private async Task<string> SaveLogoFileAsync(
            IFormFile logoFile)
        {
            var extension =
                Path.GetExtension(
                        logoFile.FileName)
                    .ToLowerInvariant();


            var uploadDirectory =
                Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "uploads",
                    "brands");


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


            await logoFile.CopyToAsync(
                stream);


            return
                $"/uploads/brands/{fileName}";
        }


        // =====================================================
        // LOGO SİL
        // =====================================================

        private void DeleteLogoFile(
            string? logoPath)
        {
            if (string.IsNullOrWhiteSpace(
                    logoPath))
            {
                return;
            }


            var relativePath =
                logoPath.TrimStart('/');


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