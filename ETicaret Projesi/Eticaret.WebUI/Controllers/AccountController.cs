using System.Security.Claims;
using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IService<AppUser> _userService;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public AccountController(
            IService<AppUser> userService)
        {
            _userService = userService;
            _passwordHasher = new PasswordHasher<AppUser>();
        }


        public IActionResult Index()
        {
            return View();
        }


        // =====================================================
        // SIGN IN GET
        // =====================================================

        [HttpGet]
        public IActionResult SignIn(string? returnUrl = null)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }


        // =====================================================
        // SIGN IN POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(
            LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }


            // Kullanıcıyı email adresine göre buluyoruz.
            var user = await _userService
                .GetAsync(x =>
                    x.Email == loginViewModel.Email);


            if (user is null)
            {
                ModelState.AddModelError(
                    "",
                    "Email veya şifre hatalı.");

                return View(loginViewModel);
            }


            // Kullanıcı aktif değilse giriş yapamaz.
            if (!user.IsActive)
            {
                ModelState.AddModelError(
                    "",
                    "Hesabınız aktif değildir.");

                return View(loginViewModel);
            }


            // Hashlenmiş şifreyi kontrol ediyoruz.
            PasswordVerificationResult passwordResult;

            try
            {
                passwordResult =
                    _passwordHasher.VerifyHashedPassword(
                        user,
                        user.Password,
                        loginViewModel.Password);
            }
            catch (FormatException)
            {
                ModelState.AddModelError(
                    "",
                    "Bu kullanıcı hesabının şifre yapısı eski veya geçersiz. Lütfen şifrenizi yenileyiniz.");

                return View(loginViewModel);
            }


            if (passwordResult ==
                PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "Email veya şifre hatalı.");

                return View(loginViewModel);
            }


            // Kullanıcının Cookie içerisinde tutulacak bilgileri.
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.UserName ?? user.Email),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    "FullName",
                    $"{user.Name} {user.Surname}"),

                new Claim(
                    ClaimTypes.Role,
                    user.IsAdmin
                        ? "Admin"
                        : "Customer"),

                new Claim(
                    "RememberMe",
                    loginViewModel.RememberMe.ToString())
            };


            var identity =
                new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);


            var principal =
                new ClaimsPrincipal(identity);


            var authProperties =
                new AuthenticationProperties
                {
                    IsPersistent =
                        loginViewModel.RememberMe
                };


            // Cookie oluşturulur.
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);


            // ReturnUrl varsa kullanıcıyı geldiği sayfaya döndür.
            if (!string.IsNullOrWhiteSpace(
                    loginViewModel.ReturnUrl) &&
                Url.IsLocalUrl(
                    loginViewModel.ReturnUrl))
            {
                return Redirect(
                    loginViewModel.ReturnUrl);
            }


            return RedirectToAction(
                "Index",
                "Home");
        }


        // =====================================================
        // SIGN UP GET
        // =====================================================

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }


        // =====================================================
        // SIGN UP POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(
            AppUser appUser)
        {
            // Site üzerinden kayıt olan kullanıcı admin olamaz.
            appUser.IsAdmin = false;

            // Yeni kullanıcı aktif oluşturulur.
            appUser.IsActive = true;


            if (!ModelState.IsValid)
            {
                return View(appUser);
            }


            // Email daha önce kullanılmış mı?
            var existingUser =
                await _userService.GetAsync(x =>
                    x.Email == appUser.Email);


            if (existingUser is not null)
            {
                ModelState.AddModelError(
                    nameof(AppUser.Email),
                    "Bu email adresi zaten kayıtlıdır.");

                return View(appUser);
            }


            appUser.CreateDate =
                DateTime.UtcNow;


            appUser.UserGuid =
                Guid.NewGuid();


            // Şifreyi hashleyerek kaydediyoruz.
            appUser.Password =
                _passwordHasher.HashPassword(
                    appUser,
                    appUser.Password);


            await _userService
                .AddAsync(appUser);


            await _userService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Kaydınız başarıyla oluşturuldu. Giriş yapabilirsiniz.";


            return RedirectToAction(
                nameof(SignIn));
        }


        // =====================================================
        // ACCESS DENIED
        // =====================================================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // =====================================================
        // PROFILE
        // =====================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return RedirectToAction(
                    nameof(SignIn));
            }


            var user =
                await _userService.FindAsync(
                    userId);


            if (user is null)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    nameof(SignIn));
            }


            return View(user);
        }


        // =====================================================
        // EDIT PROFILE GET
        // =====================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return RedirectToAction(
                    nameof(SignIn));
            }


            var user =
                await _userService.FindAsync(
                    userId);


            if (user is null)
            {
                return NotFound();
            }


            // Hashlenmiş şifreyi forma göndermiyoruz.
            user.Password =
                string.Empty;


            return View(user);
        }


        // =====================================================
        // EDIT PROFILE POST
        // =====================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(
            AppUser appUser,
            string? newPassword)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return RedirectToAction(
                    nameof(SignIn));
            }


            // Password AppUser içerisinde Required olduğu için
            // profil düzenleme sırasında validation'dan çıkarıyoruz.
            ModelState.Remove(
                nameof(AppUser.Password));


            if (!ModelState.IsValid)
            {
                return View(appUser);
            }


            var existingUser =
                await _userService.FindAsync(
                    userId);


            if (existingUser is null)
            {
                return NotFound();
            }


            // Email başka kullanıcı tarafından kullanılıyor mu?
            var emailOwner =
                await _userService.GetAsync(x =>
                    x.Email == appUser.Email &&
                    x.Id != existingUser.Id);


            if (emailOwner is not null)
            {
                ModelState.AddModelError(
                    nameof(AppUser.Email),
                    "Bu email adresi başka bir kullanıcı tarafından kullanılmaktadır.");

                return View(appUser);
            }


            // Profil bilgilerini güncelliyoruz.
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


            // Kullanıcı yeni şifre girdiyse değiştir.
            // Boş bıraktıysa eski hash korunur.
            if (!string.IsNullOrWhiteSpace(
                    newPassword))
            {
                existingUser.Password =
                    _passwordHasher.HashPassword(
                        existingUser,
                        newPassword);
            }


            await _userService
                .SaveChangesAsync();


            // =================================================
            // BENİ HATIRLA TERCİHİNİ AL
            // =================================================

            var rememberMeClaim =
                User.FindFirst(
                    "RememberMe")?.Value;


            var rememberMe =
                bool.TryParse(
                    rememberMeClaim,
                    out var rememberMeValue)
                && rememberMeValue;


            // =================================================
            // COOKIE CLAIM'LERİNİ YENİLE
            // =================================================

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    existingUser.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    existingUser.UserName
                    ?? existingUser.Email),

                new Claim(
                    ClaimTypes.Email,
                    existingUser.Email),

                new Claim(
                    "FullName",
                    $"{existingUser.Name} {existingUser.Surname}"),

                new Claim(
                    ClaimTypes.Role,
                    existingUser.IsAdmin
                        ? "Admin"
                        : "Customer"),

                new Claim(
                    "RememberMe",
                    rememberMe.ToString())
            };


            var identity =
                new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);


            var principal =
                new ClaimsPrincipal(
                    identity);


            var authProperties =
                new AuthenticationProperties
                {
                    IsPersistent =
                        rememberMe
                };


            // Cookie'yi yeni bilgilerle tekrar oluşturuyoruz.
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);


            TempData["SuccessMessage"] =
                "Bilgileriniz başarıyla güncellendi.";


            return RedirectToAction(
                nameof(Profile));
        }


        // =====================================================
        // SIGN OUT
        // =====================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);


            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}