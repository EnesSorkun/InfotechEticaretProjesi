using Eticaret.Core.Entities;
using Eticaret.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrandsController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BrandsController(
            DatabaseContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Brands
        public async Task<IActionResult> Index()
        {
            var brands = await _context.Brands
                .OrderBy(x => x.OrderNo)
                .ToListAsync();

            return View(brands);
        }

        // GET: Admin/Brands/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .FirstOrDefaultAsync(x => x.Id == id);

            if (brand is null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // GET: Admin/Brands/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Brands/Create
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

            if (logoFile is not null && logoFile.Length > 0)
            {
                brand.Logo = await SaveLogoFileAsync(logoFile);
            }

            brand.CreateDate = DateTime.UtcNow;

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Brands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FindAsync(id);

            if (brand is null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // POST: Admin/Brands/Edit/5
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

            var existingBrand = await _context.Brands.FindAsync(id);

            if (existingBrand is null)
            {
                return NotFound();
            }

            ValidateLogoFile(logoFile);

            if (!ModelState.IsValid)
            {
                brand.Logo = existingBrand.Logo;
                brand.CreateDate = existingBrand.CreateDate;

                return View(brand);
            }

            // Overposting'i önlemek için sadece düzenlenebilir alanları güncelliyoruz.
            // CreateDate gibi sistem tarafından oluşturulan alanlar korunuyor.
            existingBrand.Name = brand.Name;
            existingBrand.IsActive = brand.IsActive;
            existingBrand.OrderNo = brand.OrderNo;

            if (logoFile is not null && logoFile.Length > 0)
            {
                DeleteLogoFile(existingBrand.Logo);

                existingBrand.Logo =
                    await SaveLogoFileAsync(logoFile);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BrandExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Brands/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .FirstOrDefaultAsync(x => x.Id == id);

            if (brand is null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // POST: Admin/Brands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _context.Brands.FindAsync(id);

            if (brand is not null)
            {
                DeleteLogoFile(brand.Logo);

                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void ValidateLogoFile(IFormFile? logoFile)
        {
            if (logoFile is null || logoFile.Length == 0)
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
                .GetExtension(logoFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(Brand.Logo),
                    "Sadece JPG, JPEG, PNG veya WEBP dosyası yükleyebilirsiniz.");
            }

            const long maxFileSize = 2 * 1024 * 1024;

            if (logoFile.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(Brand.Logo),
                    "Logo dosyası en fazla 2 MB olabilir.");
            }
        }

        private async Task<string> SaveLogoFileAsync(
            IFormFile logoFile)
        {
            var extension = Path
                .GetExtension(logoFile.FileName)
                .ToLowerInvariant();

            var uploadDirectory = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "brands");

            Directory.CreateDirectory(uploadDirectory);

            var fileName =
                $"{Guid.NewGuid()}{extension}";

            var physicalPath = Path.Combine(
                uploadDirectory,
                fileName);

            await using var stream = new FileStream(
                physicalPath,
                FileMode.Create);

            await logoFile.CopyToAsync(stream);

            return $"/uploads/brands/{fileName}";
        }

        private void DeleteLogoFile(string? logoPath)
        {
            if (string.IsNullOrWhiteSpace(logoPath))
            {
                return;
            }

            var relativePath = logoPath.TrimStart('/');

            var physicalPath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                relativePath);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }

        private bool BrandExists(int id)
        {
            return _context.Brands.Any(x => x.Id == id);
        }
    }
}