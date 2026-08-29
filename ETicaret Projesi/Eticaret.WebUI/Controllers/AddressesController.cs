using System.Security.Claims;
using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.Controllers
{
    [Authorize]
    public class AddressesController : Controller
    {
        private readonly IService<Address> _addressService;

        public AddressesController(
            IService<Address> addressService)
        {
            _addressService = addressService;
        }


        // =====================================================
        // ADRESLERİM
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }


            // SADECE giriş yapan kullanıcının adresleri.
            var addresses =
                await _addressService.GetAllAsync(
                    x => x.AppUserId == userId.Value);


            addresses = addresses
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreateDate)
                .ToList();


            return View(addresses);
        }


        // =====================================================
        // ADRES EKLE GET
        // =====================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // =====================================================
        // ADRES EKLE POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Address address)
        {
            var userId =
                GetCurrentUserId();


            if (userId is null)
            {
                return Unauthorized();
            }


            /*
             * AppUserId formdan ALINMIYOR.
             *
             * Kullanıcı değiştiremesin diye
             * login olmuş kullanıcının Claim bilgisinden
             * kendimiz veriyoruz.
             */
            ModelState.Remove(
                nameof(Address.AppUser));

            ModelState.Remove(
                nameof(Address.AppUserId));


            if (!ModelState.IsValid)
            {
                return View(address);
            }


            address.AppUserId =
                userId.Value;


            address.CreateDate =
                DateTime.UtcNow;


            // Kullanıcının hiç adresi yoksa
            // ilk adres varsayılan olsun.
            var existingAddresses =
                await _addressService.GetAllAsync(
                    x => x.AppUserId == userId.Value);


            if (!existingAddresses.Any())
            {
                address.IsDefault =
                    true;
            }


            // Kullanıcı bunu varsayılan seçtiyse
            // diğer adreslerin varsayılanlığını kaldır.
            if (address.IsDefault)
            {
                foreach (var existingAddress
                         in existingAddresses)
                {
                    existingAddress.IsDefault =
                        false;
                }
            }


            await _addressService
                .AddAsync(address);


            await _addressService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Adresiniz başarıyla eklendi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // ADRES DÜZENLE GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var userId =
                GetCurrentUserId();


            if (userId is null)
            {
                return Unauthorized();
            }


            /*
             * BURASI ÇOK ÖNEMLİ.
             *
             * Sadece Id'ye göre aramıyoruz.
             *
             * Adres hem bu Id'ye sahip olmalı
             * hem de giriş yapan kullanıcıya ait olmalı.
             */
            var address =
                await _addressService.GetAsync(
                    x =>
                        x.Id == id &&
                        x.AppUserId == userId.Value);


            if (address is null)
            {
                return NotFound();
            }


            return View(address);
        }


        // =====================================================
        // ADRES DÜZENLE POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Address address)
        {
            var userId =
                GetCurrentUserId();


            if (userId is null)
            {
                return Unauthorized();
            }


            if (id != address.Id)
            {
                return NotFound();
            }


            ModelState.Remove(
                nameof(Address.AppUser));

            ModelState.Remove(
                nameof(Address.AppUserId));


            if (!ModelState.IsValid)
            {
                return View(address);
            }


            var existingAddress =
                await _addressService.GetAsync(
                    x =>
                        x.Id == id &&
                        x.AppUserId == userId.Value);


            if (existingAddress is null)
            {
                return NotFound();
            }


            existingAddress.Title =
                address.Title;

            existingAddress.Name =
                address.Name;

            existingAddress.Surname =
                address.Surname;

            existingAddress.Phone =
                address.Phone;

            existingAddress.City =
                address.City;

            existingAddress.District =
                address.District;

            existingAddress.FullAddress =
                address.FullAddress;

            existingAddress.PostalCode =
                address.PostalCode;


            // Varsayılan adres yapılıyorsa
            if (address.IsDefault &&
                !existingAddress.IsDefault)
            {
                var addresses =
                    await _addressService.GetAllAsync(
                        x =>
                            x.AppUserId ==
                            userId.Value &&
                            x.Id != id);


                foreach (var item in addresses)
                {
                    item.IsDefault =
                        false;
                }
            }


            existingAddress.IsDefault =
                address.IsDefault;


            await _addressService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Adresiniz başarıyla güncellendi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // ADRES SİL
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            var userId =
                GetCurrentUserId();


            if (userId is null)
            {
                return Unauthorized();
            }


            var address =
                await _addressService.GetAsync(
                    x =>
                        x.Id == id &&
                        x.AppUserId == userId.Value);


            if (address is null)
            {
                return NotFound();
            }


            var wasDefault =
                address.IsDefault;


            _addressService.Delete(
                address);


            await _addressService
                .SaveChangesAsync();


            /*
             * Silinen adres varsayılan adres ise
             * kalan ilk adresi varsayılan yap.
             */
            if (wasDefault)
            {
                var remainingAddresses =
                    await _addressService.GetAllAsync(
                        x =>
                            x.AppUserId ==
                            userId.Value);


                var newDefault =
                    remainingAddresses
                        .OrderByDescending(
                            x => x.CreateDate)
                        .FirstOrDefault();


                if (newDefault is not null)
                {
                    newDefault.IsDefault =
                        true;


                    await _addressService
                        .SaveChangesAsync();
                }
            }


            TempData["SuccessMessage"] =
                "Adresiniz başarıyla silindi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // AKTİF KULLANICI ID
        // =====================================================

        private int? GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return null;
            }


            return userId;
        }
    }
}