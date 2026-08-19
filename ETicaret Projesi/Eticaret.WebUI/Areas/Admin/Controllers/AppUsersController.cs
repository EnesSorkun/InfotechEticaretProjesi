using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class AppUsersController : Controller
    {
        private readonly IService<AppUser> _userService;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public AppUsersController(
            IService<AppUser> userService)
        {
            _userService = userService;
            _passwordHasher = new PasswordHasher<AppUser>();
        }


        // =====================================================
        // GET: Admin/AppUsers
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var users = await _userService
                .GetAllAsync();

            users = users
                .OrderByDescending(x => x.CreateDate)
                .ToList();

            return View(users);
        }


        // =====================================================
        // GET: Admin/AppUsers/Details/5
        // =====================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }


            var appUser =
                await _userService.FindAsync(
                    id.Value);


            if (appUser is null)
            {
                return NotFound();
            }


            return View(appUser);
        }


        // =====================================================
        // GET: Admin/AppUsers/Create
        // =====================================================

        public IActionResult Create()
        {
            return View();
        }


        // =====================================================
        // POST: Admin/AppUsers/Create
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AppUser appUser)
        {
            if (!ModelState.IsValid)
            {
                return View(appUser);
            }


            // Aynı email ile daha önce kullanıcı oluşturulmuş mu?
            var existingUser =
                await _userService.GetAsync(x =>
                    x.Email == appUser.Email);


            if (existingUser is not null)
            {
                ModelState.AddModelError(
                    nameof(AppUser.Email),
                    "Bu email adresi zaten kullanılmaktadır.");

                return View(appUser);
            }


            // Şifreyi düz metin olarak değil
            // hashlenmiş şekilde kaydediyoruz.
            appUser.Password =
                _passwordHasher.HashPassword(
                    appUser,
                    appUser.Password);


            // Sistem tarafından oluşturulan alanlar
            appUser.CreateDate =
                DateTime.UtcNow;


            appUser.UserGuid =
                Guid.NewGuid();


            await _userService
                .AddAsync(appUser);


            await _userService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Kullanıcı başarıyla oluşturuldu.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // GET: Admin/AppUsers/Edit/5
        // =====================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }


            var appUser =
                await _userService.FindAsync(
                    id.Value);


            if (appUser is null)
            {
                return NotFound();
            }


            // Veritabanındaki hash değerini forma basmıyoruz.
            appUser.Password =
                string.Empty;


            return View(appUser);
        }


        // =====================================================
        // POST: Admin/AppUsers/Edit/5
        // =====================================================

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


            // Edit işleminde şifre değiştirmek zorunlu değil.
            ModelState.Remove(
                nameof(AppUser.Password));


            if (!ModelState.IsValid)
            {
                return View(appUser);
            }


            var existingUser =
                await _userService.FindAsync(
                    id);


            if (existingUser is null)
            {
                return NotFound();
            }


            // Başka kullanıcı aynı email'i kullanıyor mu?
            var emailOwner =
                await _userService.GetAsync(x =>
                    x.Email == appUser.Email &&
                    x.Id != id);


            if (emailOwner is not null)
            {
                ModelState.AddModelError(
                    nameof(AppUser.Email),
                    "Bu email adresi başka bir kullanıcı tarafından kullanılmaktadır.");

                return View(appUser);
            }


            // Sadece düzenlenmesine izin verilen alanlar
            existingUser.Name =
                appUser.Name;

            existingUser.Surname =
                appUser.Surname;

            existingUser.Email =
                appUser.Email;

            existingUser.Phone =
                appUser.Phone;

            existingUser.UserName =
                appUser.UserName;

            existingUser.IsActive =
                appUser.IsActive;

            existingUser.IsAdmin =
                appUser.IsAdmin;


            // Admin yeni şifre girdiyse
            // hashleyerek değiştiriyoruz.
            if (!string.IsNullOrWhiteSpace(
                    appUser.Password))
            {
                existingUser.Password =
                    _passwordHasher.HashPassword(
                        existingUser,
                        appUser.Password);
            }


            // CreateDate ve UserGuid değiştirilmez.
            // existingUser.CreateDate korunur.
            // existingUser.UserGuid korunur.


            await _userService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Kullanıcı başarıyla güncellendi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // GET: Admin/AppUsers/Delete/5
        // =====================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }


            var appUser =
                await _userService.FindAsync(
                    id.Value);


            if (appUser is null)
            {
                return NotFound();
            }


            return View(appUser);
        }


        // =====================================================
        // POST: Admin/AppUsers/Delete/5
        // =====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var appUser =
                await _userService.FindAsync(
                    id);


            if (appUser is null)
            {
                return NotFound();
            }


            _userService.Delete(
                appUser);


            await _userService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Kullanıcı başarıyla silindi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // USER EXISTS
        // =====================================================

        private async Task<bool> AppUserExistsAsync(
            int id)
        {
            var user =
                await _userService.FindAsync(
                    id);


            return user is not null;
        }
    }
}