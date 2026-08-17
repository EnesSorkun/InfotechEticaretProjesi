using Eticaret.Core.Entities;
using Eticaret.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class AppUsersController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public AppUsersController(DatabaseContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<AppUser>();
        }


        // GET: Admin/AppUsers
        public async Task<IActionResult> Index()
        {
            var users = await _context.AppUsers
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();

            return View(users);
        }


        // GET: Admin/AppUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var appUser = await _context.AppUsers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (appUser is null)
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


        // POST: Admin/AppUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUser appUser)
        {
            if (!ModelState.IsValid)
            {
                return View(appUser);
            }


            // Aynı email ile daha önce kullanıcı oluşturulmuş mu?
            var emailExists = await _context.AppUsers
                .AnyAsync(x => x.Email == appUser.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(AppUser.Email),
                    "Bu email adresi zaten kullanılmaktadır.");

                return View(appUser);
            }


            // Şifreyi düz metin olarak değil HASH olarak kaydediyoruz.
            appUser.Password = _passwordHasher.HashPassword(
                appUser,
                appUser.Password);


            // Sistem tarafından oluşturulan alanlar
            appUser.CreateDate = DateTime.UtcNow;
            appUser.UserGuid = Guid.NewGuid();


            await _context.AppUsers.AddAsync(appUser);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Kullanıcı başarıyla oluşturuldu.";

            return RedirectToAction(nameof(Index));
        }


        // GET: Admin/AppUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var appUser = await _context.AppUsers
                .FindAsync(id);

            if (appUser is null)
            {
                return NotFound();
            }


            // Veritabanındaki hash değerini forma kesinlikle basmıyoruz.
            // Şifre alanı boş açılacak.
            appUser.Password = string.Empty;


            return View(appUser);
        }


        // POST: Admin/AppUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            AppUser appUser)
        {
            if (id != appUser.Id)
            {
                return NotFound();
            }


            /*
             * Edit işleminde şifre değiştirmek zorunlu değil.
             * AppUser entity'sinde Password Required olduğu için
             * burada Password validation'ını kaldırıyoruz.
             */
            ModelState.Remove(nameof(AppUser.Password));


            if (!ModelState.IsValid)
            {
                return View(appUser);
            }


            var existingUser = await _context.AppUsers
                .FindAsync(id);

            if (existingUser is null)
            {
                return NotFound();
            }


            // Başka bir kullanıcı aynı email'i kullanıyor mu?
            var emailExists = await _context.AppUsers
                .AnyAsync(x =>
                    x.Email == appUser.Email &&
                    x.Id != id);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(AppUser.Email),
                    "Bu email adresi başka bir kullanıcı tarafından kullanılmaktadır.");

                return View(appUser);
            }


            // Düzenlenmesine izin verilen alanlar
            existingUser.Name = appUser.Name;
            existingUser.Surname = appUser.Surname;
            existingUser.Email = appUser.Email;
            existingUser.Phone = appUser.Phone;
            existingUser.UserName = appUser.UserName;
            existingUser.IsActive = appUser.IsActive;
            existingUser.IsAdmin = appUser.IsAdmin;


            /*
             * Admin yeni bir şifre girdiyse hashleyip değiştir.
             *
             * Şifre alanı boş bırakılmışsa eski hash aynen korunur.
             */
            if (!string.IsNullOrWhiteSpace(appUser.Password))
            {
                existingUser.Password =
                    _passwordHasher.HashPassword(
                        existingUser,
                        appUser.Password);
            }


            // CreateDate ve UserGuid değiştirilmez.
            // existingUser.CreateDate korunuyor.
            // existingUser.UserGuid korunuyor.


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


            TempData["SuccessMessage"] =
                "Kullanıcı başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }


        // GET: Admin/AppUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var appUser = await _context.AppUsers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (appUser is null)
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
            var appUser = await _context.AppUsers
                .FindAsync(id);

            if (appUser is null)
            {
                return NotFound();
            }


            _context.AppUsers.Remove(appUser);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Kullanıcı başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }


        private bool AppUserExists(int id)
        {
            return _context.AppUsers
                .Any(x => x.Id == id);
        }
    }
}