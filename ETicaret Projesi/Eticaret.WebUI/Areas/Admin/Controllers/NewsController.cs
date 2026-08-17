using Eticaret.Core.Entities;
using Eticaret.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class NewsController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NewsController(
            DatabaseContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/News
        public async Task<IActionResult> Index()
        {
            var news = await _context.News
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();

            return View(news);
        }

        // GET: Admin/News/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var news = await _context.News
                .FirstOrDefaultAsync(x => x.Id == id);

            if (news is null)
            {
                return NotFound();
            }

            return View(news);
        }

        // GET: Admin/News/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            News news,
            IFormFile? imageFile)
        {
            ValidateImageFile(imageFile);

            if (!ModelState.IsValid)
            {
                return View(news);
            }

            if (imageFile is not null && imageFile.Length > 0)
            {
                news.Image =
                    await SaveImageFileAsync(imageFile);
            }

            news.CreateDate = DateTime.UtcNow;

            _context.News.Add(news);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var news = await _context.News.FindAsync(id);

            if (news is null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: Admin/News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            News news,
            IFormFile? imageFile)
        {
            if (id != news.Id)
            {
                return NotFound();
            }

            var existingNews =
                await _context.News.FindAsync(id);

            if (existingNews is null)
            {
                return NotFound();
            }

            ValidateImageFile(imageFile);

            if (!ModelState.IsValid)
            {
                news.Image = existingNews.Image;
                news.CreateDate = existingNews.CreateDate;

                return View(news);
            }

            existingNews.Name = news.Name;
            existingNews.Description = news.Description;
            existingNews.IsActive = news.IsActive;

            if (imageFile is not null &&
                imageFile.Length > 0)
            {
                DeleteImageFile(existingNews.Image);

                existingNews.Image =
                    await SaveImageFileAsync(imageFile);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/News/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var news = await _context.News
                .FirstOrDefaultAsync(x => x.Id == id);

            if (news is null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: Admin/News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.FindAsync(id);

            if (news is not null)
            {
                DeleteImageFile(news.Image);

                _context.News.Remove(news);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

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
                    nameof(News.Image),
                    "Sadece JPG, JPEG, PNG veya WEBP dosyası yükleyebilirsiniz.");
            }

            const long maxFileSize =
                2 * 1024 * 1024;

            if (imageFile.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(News.Image),
                    "Görsel dosyası en fazla 2 MB olabilir.");
            }
        }

        private async Task<string> SaveImageFileAsync(
            IFormFile imageFile)
        {
            var extension = Path
                .GetExtension(imageFile.FileName)
                .ToLowerInvariant();

            var uploadDirectory = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "news");

            Directory.CreateDirectory(uploadDirectory);

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

            return $"/uploads/news/{fileName}";
        }

        private void DeleteImageFile(
            string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            var relativePath =
                imagePath.TrimStart('/');

            var physicalPath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                relativePath);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(x => x.Id == id);
        }
    }
}