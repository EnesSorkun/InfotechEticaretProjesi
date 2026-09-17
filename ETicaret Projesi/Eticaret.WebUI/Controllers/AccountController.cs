using System.Security.Claims;
using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace Eticaret.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IService<AppUser> _userService;
        private readonly PasswordHasher<AppUser> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AccountController(
            IService<AppUser> userService, IConfiguration configuration)
        {
            _userService = userService;
            _passwordHasher = new PasswordHasher<AppUser>();
            _configuration = configuration;
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

            HttpContext.Session.Remove("Cart");


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
            // Kullanıcının authentication cookie'sini siliyoruz.
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);



            return RedirectToAction(
                "Index",
                "Home");
        }

        // =====================================================
        // FORGOT PASSWORD GET
        // =====================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // =====================================================
        // FORGOT PASSWORD POST
        // =====================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var email =
                model.Email.Trim();


            var user = await _userService
                .GetAsync(x => x.Email == email);


            // Güvenlik nedeniyle kullanıcı bulunmasa bile
            // "bu email kayıtlı değil" demiyoruz.
            //
            // Böylece dışarıdan biri hangi email adreslerinin
            // sistemde kayıtlı olduğunu öğrenemez.
            if (user is not null && user.IsActive)
            {
                // Güvenli rastgele token oluşturuyoruz.
                var tokenBytes =
                    RandomNumberGenerator.GetBytes(64);


                var token =
                    Convert.ToBase64String(tokenBytes);


                // URL içerisinde güvenli kullanılabilmesi için
                // Base64Url formatına dönüştürüyoruz.
                token = token
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .TrimEnd('=');


                // Veritabanına token'ın kendisini değil
                // hashlenmiş halini kaydediyoruz.
                user.PasswordResetToken =
                    HashResetToken(token);


                // Token 30 dakika geçerli.
                user.PasswordResetTokenExpireDate =
                    DateTime.UtcNow.AddMinutes(30);


                _userService.Update(user);

                await _userService
                    .SaveChangesAsync();


                // Şifre yenileme adresini oluşturuyoruz.
                var resetUrl =
                    Url.Action(
                        nameof(ResetPassword),
                        "Account",
                        new
                        {
                            token,
                            email = user.Email
                        },
                        Request.Scheme);


                if (!string.IsNullOrWhiteSpace(resetUrl))
                {
                    await SendPasswordResetEmailAsync(
                        user.Email,
                        resetUrl);
                }
            }


            // Kullanıcı var/yok bilgisini burada açıklamıyoruz.
            TempData["SuccessMessage"] =
                "Eğer bu email adresi sistemimizde kayıtlıysa şifre yenileme bağlantısı gönderildi.";


            return RedirectToAction(
                nameof(ForgotPassword));
        }

        // =====================================================
        // RESET PASSWORD GET
        // =====================================================

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(
            string token,
            string email)
        {
            if (string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(email))
            {
                return BadRequest();
            }


            var user = await _userService
                .GetAsync(x => x.Email == email);


            if (user is null ||
                string.IsNullOrWhiteSpace(
                    user.PasswordResetToken) ||
                user.PasswordResetTokenExpireDate is null)
            {
                TempData["ErrorMessage"] =
                    "Şifre yenileme bağlantısı geçersizdir.";

                return RedirectToAction(
                    nameof(ForgotPassword));
            }


            // Token'ın süresi geçmiş mi?
            if (user.PasswordResetTokenExpireDate.Value <
                DateTime.UtcNow)
            {
                TempData["ErrorMessage"] =
                    "Şifre yenileme bağlantısının süresi dolmuştur. Lütfen tekrar şifre yenileme talebi oluşturunuz.";

                return RedirectToAction(
                    nameof(ForgotPassword));
            }


            // Gelen token'ı hashleyip DB'deki hash ile
            // karşılaştırıyoruz.
            var tokenHash =
                HashResetToken(token);


            if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(tokenHash),
                Convert.FromHexString(user.PasswordResetToken)))
            {
                TempData["ErrorMessage"] =
                    "Şifre yenileme bağlantısı geçersizdir.";

                return RedirectToAction(
                    nameof(ForgotPassword));
            }


            var model =
                new ResetPasswordViewModel
                {
                    Email = email,
                    Token = token
                };


            return View(model);
        }

        // =====================================================
        // RESET PASSWORD POST
        // =====================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var user = await _userService
                .GetAsync(x =>
                    x.Email == model.Email);


            if (user is null ||
                string.IsNullOrWhiteSpace(
                    user.PasswordResetToken) ||
                user.PasswordResetTokenExpireDate is null)
            {
                ModelState.AddModelError(
                    "",
                    "Şifre yenileme bağlantısı geçersizdir.");

                return View(model);
            }


            // Token süresi dolmuş mu?
            if (user.PasswordResetTokenExpireDate.Value <
                DateTime.UtcNow)
            {
                ModelState.AddModelError(
                    "",
                    "Şifre yenileme bağlantısının süresi dolmuştur.");

                return View(model);
            }


            var tokenHash =
                HashResetToken(model.Token);


            if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(tokenHash),
                Convert.FromHexString(user.PasswordResetToken)))
            {
                ModelState.AddModelError(
                    "",
                    "Şifre yenileme bağlantısı geçersizdir.");

                return View(model);
            }


            // =================================================
            // YENİ ŞİFREYİ HASHLE
            // =================================================

            user.Password =
                _passwordHasher.HashPassword(
                    user,
                    model.NewPassword);


            // =================================================
            // RESET TOKEN ARTIK KULLANILAMAZ
            // =================================================

            user.PasswordResetToken =
                null;

            user.PasswordResetTokenExpireDate =
                null;


            _userService.Update(user);


            await _userService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Şifreniz başarıyla yenilendi. Yeni şifrenizle giriş yapabilirsiniz.";


            return RedirectToAction(
                nameof(SignIn));
        }

        // =====================================================
        // RESET TOKEN HASH
        // =====================================================

        private static string HashResetToken(
            string token)
        {
            var tokenBytes =
                Encoding.UTF8.GetBytes(token);


            var hashBytes =
                SHA256.HashData(tokenBytes);


            return Convert.ToHexString(hashBytes);
        }

        // =====================================================
        // PASSWORD RESET EMAIL
        // =====================================================

        private async Task SendPasswordResetEmailAsync(
            string email,
            string resetUrl)
        {
            var host =
     _configuration["MailSettings:Host"];

            var portValue =
                _configuration["MailSettings:Port"];

            var userName =
                _configuration["MailSettings:UserName"];

            var password =
                _configuration["MailSettings:Password"];

            var fromEmail =
                _configuration["MailSettings:FromEmail"];

            var fromName =
                _configuration["MailSettings:FromName"];


            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portValue) ||
                string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException(
                    "Email ayarları eksik.");
            }


            if (!int.TryParse(
                    portValue,
                    out var port))
            {
                throw new InvalidOperationException(
                    "Email port bilgisi geçersiz.");
            }


            using var mail =
                new MailMessage();


            mail.From =
                new MailAddress(
                    fromEmail,
                    fromName ?? "Eticaret");


            mail.To.Add(email);


            mail.Subject =
                "Şifre Yenileme";


            mail.IsBodyHtml =
                true;


            mail.Body =
                $"""
        <div style="font-family:Arial,sans-serif;
                    max-width:600px;
                    margin:auto;">

            <h2>Şifre Yenileme</h2>

            <p>
                Hesabınız için bir şifre yenileme talebi aldık.
            </p>

            <p>
                Aşağıdaki butona tıklayarak yeni şifrenizi oluşturabilirsiniz.
            </p>

            <p style="margin:30px 0;">

                <a href="{resetUrl}"
                   style="
                        background:#212529;
                        color:white;
                        padding:12px 24px;
                        text-decoration:none;
                        border-radius:6px;">

                    Şifremi Yenile

                </a>

            </p>

            <p>
                Bu bağlantı 30 dakika geçerlidir.
            </p>

            <p>
                Bu işlemi siz yapmadıysanız bu maili dikkate almayabilirsiniz.
            </p>

        </div>
        """;


            using var smtp =
                new SmtpClient(
                    host,
                    port);


            smtp.Credentials =
                new NetworkCredential(
                    userName,
                    password);


            smtp.EnableSsl =
                true;


            await smtp.SendMailAsync(mail);
        }
    }
}