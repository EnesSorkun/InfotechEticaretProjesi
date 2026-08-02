using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eticaret.Core.Entities;
using Eticaret.Data;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AppUsersController : Controller
    {
        private readonly DatabaseContext _context;

        public AppUsersController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/AppUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.AppUsers.ToListAsync());
        }

        // GET: Admin/AppUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await _context.AppUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        // GET: Admin/AppUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUser appUser)
        {
            if (ModelState.IsValid)
            {
                appUser.CreateDate = DateTime.UtcNow;
                appUser.UserGuid = Guid.NewGuid();

                _context.Add(appUser);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(appUser);
        }

        // GET: Admin/AppUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await _context.AppUsers.FindAsync(id);
            if (appUser == null)
            {
                return NotFound();
            }
            return View(appUser);
        }

        // POST: Admin/AppUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppUser appUser)
        {
            if (id != appUser.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(appUser);
            }

            // Overposting'i(Kullanıcının değiştirmemesi gereken yeri değiştirmesi durumu) önlemek için mevcut kullanıcıyı veritabanından alıyoruz.
            // Böylece sadece izin verilen alanlar güncellenecek.
            var existingUser = await _context.AppUsers.FindAsync(id);

            if (existingUser is null)
            {
                return NotFound();
            }

            // Sadece düzenlenebilir alanları güncelliyoruz.
            // CreateDate ve UserGuid gibi sistem alanları korunuyor.
            existingUser.Name = appUser.Name;
            existingUser.Surname = appUser.Surname;
            existingUser.Email = appUser.Email;
            existingUser.Phone = appUser.Phone;
            existingUser.UserName = appUser.UserName;
            existingUser.Password = appUser.Password;
            existingUser.IsActive = appUser.IsActive;
            existingUser.IsAdmin = appUser.IsAdmin;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppUserExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/AppUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await _context.AppUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        // POST: Admin/AppUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appUser = await _context.AppUsers.FindAsync(id);
            if (appUser != null)
            {
                _context.AppUsers.Remove(appUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppUserExists(int id)
        {
            return _context.AppUsers.Any(e => e.Id == id);
        }
    }
}
