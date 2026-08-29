using System.Security.Claims;
using Eticaret.Core.Entities;
using Eticaret.Core.Enums;
using Eticaret.Service.Abstract;
using Eticaret.Service.Concrete;
using Eticaret.WebUI.ExtensionMethods;
using Eticaret.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.Controllers
{
    public class CartController : Controller
    {
        private readonly IService<Product> _productService;
        private readonly IService<Address> _addressService;
        private readonly IService<Order> _orderService;

        public CartController(
            IService<Product> productService,
            IService<Address> addressService,
            IService<Order> orderService)
        {
            _productService = productService;
            _addressService = addressService;
            _orderService = orderService;
        }


        // =====================================================
        // SEPETİ GÖSTER
        // =====================================================

        public IActionResult Index()
        {
            var cart =
                GetCart();


            var model =
                new CartViewModel
                {
                    CartLines =
                        cart.CartLines,

                    TotalPrice =
                        cart.TotalPrice()
                };


            return View(model);
        }


        // =====================================================
        // SEPETE ÜRÜN EKLE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(
            int productId,
            int quantity = 1)
        {
            var product =
                _productService.Find(
                    productId);


            if (product is null)
            {
                return NotFound();
            }


            if (!product.IsActive)
            {
                TempData["ErrorMessage"] =
                    "Bu ürün şu anda satışta değildir.";

                return RedirectToAction(
                    nameof(Index));
            }


            if (product.Stock <= 0)
            {
                TempData["ErrorMessage"] =
                    "Bu ürün stokta bulunmamaktadır.";

                return RedirectToAction(
                    nameof(Index));
            }


            if (quantity <= 0)
            {
                quantity = 1;
            }


            var cart =
                GetCart();


            var existingCartLine =
                cart.CartLines
                    .FirstOrDefault(x =>
                        x.Product.Id ==
                        product.Id);


            var currentQuantity =
                existingCartLine?.Quantity ?? 0;


            // Sepetteki mevcut adet + yeni eklenecek adet
            // stoktan fazla olamaz.
            if (currentQuantity + quantity >
                product.Stock)
            {
                TempData["ErrorMessage"] =
                    $"Bu üründen en fazla {product.Stock} adet sepete ekleyebilirsiniz.";

                return RedirectToAction(
                    nameof(Index));
            }


            cart.AddProduct(
                product,
                quantity);


            SaveCart(
                cart);


            TempData["SuccessMessage"] =
                "Ürün sepete eklendi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // SEPETTEN ÜRÜN KALDIR
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(
            int productId)
        {
            var product =
                _productService.Find(
                    productId);


            if (product is null)
            {
                return NotFound();
            }


            var cart =
                GetCart();


            cart.RemoveProduct(
                product);


            SaveCart(
                cart);


            TempData["SuccessMessage"] =
                "Ürün sepetten kaldırıldı.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // ÜRÜN ADEDİNİ GÜNCELLE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(
            int productId,
            int quantity)
        {
            var product =
                _productService.Find(
                    productId);


            if (product is null)
            {
                return NotFound();
            }


            if (quantity <= 0)
            {
                quantity = 1;
            }


            if (quantity > product.Stock)
            {
                quantity =
                    product.Stock;

                TempData["ErrorMessage"] =
                    $"Bu üründen stokta en fazla {product.Stock} adet bulunmaktadır.";
            }


            var cart =
                GetCart();


            cart.UpdateProduct(
                product,
                quantity);


            SaveCart(
                cart);


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // SEPETİ TEMİZLE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            var cart =
                GetCart();


            cart.ClearAll();


            SaveCart(
                cart);


            TempData["SuccessMessage"] =
                "Sepet temizlendi.";


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // CHECKOUT GET
        // =====================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart =
                GetCart();


            // Sepet boşsa ödeme ekranına geçilemez.
            if (!cart.CartLines.Any())
            {
                TempData["ErrorMessage"] =
                    "Ödeme işlemine devam edebilmek için sepetinizde ürün bulunmalıdır.";

                return RedirectToAction(
                    nameof(Index));
            }


            var userId =
                GetCurrentUserId();


            if (userId is null)
            {
                return Unauthorized();
            }


            // =================================================
            // KULLANICININ ADRESLERİNİ GETİR
            // =================================================

            var addresses =
                await _addressService
                    .GetAllAsync(x =>
                        x.AppUserId ==
                        userId.Value);


            addresses = addresses
                .OrderByDescending(x =>
                    x.IsDefault)
                .ThenByDescending(x =>
                    x.CreateDate)
                .ToList();


            // Kullanıcının hiç adresi yoksa
            // Checkout yerine adres ekleme ekranına gönder.
            if (!addresses.Any())
            {
                TempData["InfoMessage"] =
                    "Ödeme işlemine devam edebilmek için önce bir teslimat adresi eklemelisiniz.";

                return RedirectToAction(
                    "Create",
                    "Addresses");
            }


            // Varsayılan adres varsa onu,
            // yoksa listedeki ilk adresi seç.
            var defaultAddress =
                addresses
                    .FirstOrDefault(x =>
                        x.IsDefault)
                ?? addresses.First();


            var model =
                new CheckoutViewModel
                {
                    Addresses =
                        addresses,

                    SelectedAddressId =
                        defaultAddress.Id,

                    CartLines =
                        cart.CartLines,

                    TotalPrice =
                        cart.TotalPrice()
                };


            return View(model);
        }


        // =====================================================
        // CHECKOUT POST
        // =====================================================

        // =====================================================
        // CHECKOUT POST
        // =====================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(
            CheckoutViewModel model)
        {
            var cart =
                GetCart();


            // =================================================
            // SEPET KONTROLÜ
            // =================================================

            if (!cart.CartLines.Any())
            {
                TempData["ErrorMessage"] =
                    "Sepetiniz boş.";

                return RedirectToAction(
                    nameof(Index));
            }


            // =================================================
            // AKTİF KULLANICI
            // =================================================

            var userId =
                GetCurrentUserId();


            if (userId is null)
            {
                return Unauthorized();
            }


            // =================================================
            // KULLANICININ ADRESLERİNİ GETİR
            // =================================================
            //
            // Validation hatası oluşursa Checkout sayfasını
            // yeniden gösterebilmek için adresleri tekrar
            // modele dolduruyoruz.
            // =================================================

            var addresses =
                await _addressService
                    .GetAllAsync(x =>
                        x.AppUserId ==
                        userId.Value);


            addresses = addresses
                .OrderByDescending(x =>
                    x.IsDefault)
                .ThenByDescending(x =>
                    x.CreateDate)
                .ToList();


            model.Addresses =
                addresses;


            model.CartLines =
                cart.CartLines;


            model.TotalPrice =
                cart.TotalPrice();


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =================================================
            // SEÇİLEN ADRES KONTROLÜ
            // =================================================

            var selectedAddress =
                await _addressService
                    .GetAsync(x =>
                        x.Id ==
                        model.SelectedAddressId &&
                        x.AppUserId ==
                        userId.Value);


            if (selectedAddress is null)
            {
                ModelState.AddModelError(
                    nameof(
                        model.SelectedAddressId),
                    "Seçilen teslimat adresi bulunamadı.");


                return View(model);
            }


            // =================================================
            // ÜRÜNLERİ DB'DEN GETİR VE STOK KONTROLÜ YAP
            // =================================================
            //
            // Ürünleri Dictionary içerisinde tutuyoruz.
            // Böylece aşağıda tekrar tekrar DB'den
            // ürün çekmek zorunda kalmayacağız.
            // =================================================

            var products =
                new Dictionary<int, Product>();


            foreach (var cartLine
                     in cart.CartLines)
            {
                var product =
                    await _productService
                        .FindAsync(
                            cartLine.Product.Id);


                if (product is null)
                {
                    ModelState.AddModelError(
                        "",
                        $"{cartLine.Product.Name} ürünü artık bulunamıyor.");


                    return View(model);
                }


                if (!product.IsActive)
                {
                    ModelState.AddModelError(
                        "",
                        $"{product.Name} ürünü artık satışta değildir.");


                    return View(model);
                }


                if (product.Stock <
                    cartLine.Quantity)
                {
                    ModelState.AddModelError(
                        "",
                        $"{product.Name} ürünü için yeterli stok bulunmamaktadır. Mevcut stok: {product.Stock}");


                    return View(model);
                }


                products.Add(
                    product.Id,
                    product);
            }


            // =================================================
            // TESLİMAT ADRESİNİ METİN OLARAK HAZIRLA
            // =================================================
            //
            // Kullanıcı daha sonra Address kaydını değiştirse bile
            // geçmiş siparişin adresinin değişmemesi için
            // adres bilgisini Order içine kopyalıyoruz.
            // =================================================

            var deliveryAddress =
                $"{selectedAddress.Name} {selectedAddress.Surname} - " +
                $"{selectedAddress.FullAddress} " +
                $"{selectedAddress.District}/{selectedAddress.City} " +
                $"Tel: {selectedAddress.Phone}";


            if (!string.IsNullOrWhiteSpace(
                    selectedAddress.PostalCode))
            {
                deliveryAddress +=
                    $" Posta Kodu: {selectedAddress.PostalCode}";
            }


            // =================================================
            // ORDER OLUŞTUR
            // =================================================

            var order =
                new Order
                {
                    // Siparişi giriş yapan kullanıcıya bağlıyoruz.
                    AppUserId =
                        userId.Value,


                    // Benzersiz sipariş numarası oluşturuyoruz.
                    OrderNumber =
                        $"TG-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",

                    OrderDate =
                        DateTime.UtcNow,

                    Status = OrderStatus.PendingApproval,


                    // Aşağıda OrderDetail oluştururken
                    // toplam tutarı hesaplayacağız.
                    TotalPrice =
                        0,


                    // Şimdilik fatura ve teslimat adresini
                    // aynı adres kabul ediyoruz.
                    DeliveryAddress =
                        deliveryAddress,

                    BillingAddress =
                        deliveryAddress
                };


            // =================================================
            // ORDER DETAIL OLUŞTUR
            // =================================================

            foreach (var cartLine
                     in cart.CartLines)
            {
                var product =
                    products[
                        cartLine.Product.Id];


                var orderDetail =
                    new OrderDetail
                    {
                        ProductId =
                            product.Id,

                        UnitPrice =
                            product.Price,

                        Quantity =
                            cartLine.Quantity
                    };


                // Order'a OrderDetail ekliyoruz.
                order.OrderDetails.Add(
                    orderDetail);


                // Sipariş toplamını hesaplıyoruz.
                order.TotalPrice +=
                    product.Price *
                    cartLine.Quantity;


                // Satın alınan miktarı stoktan düşürüyoruz.
                product.Stock -=
                    cartLine.Quantity;
            }


            // =================================================
            // DEMO ÖDEME
            // =================================================
            //
            // Şu an burada gerçek bir ödeme servisi yok.
            //
            // İleride:
            //
            // iyzico
            // PayTR
            // Stripe
            //
            // gibi bir ödeme sağlayıcısı burada kullanılabilir.
            //
            // CardNumber ve CVV veritabanına
            // KAYDEDİLMEMELİ.
            // =================================================


            // =================================================
            // SİPARİŞİ VERİTABANINA EKLE
            // =================================================

            await _orderService
                .AddAsync(order);


            /*
             * Product ve Order servisleri aynı Scoped
             * DatabaseContext'i kullandığı için tek SaveChanges
             * ile hem:
             *
             * Orders
             * OrderDetails
             * Products.Stock
             *
             * değişiklikleri kaydedilir.
             */

            await _orderService
                .SaveChangesAsync();


            // =================================================
            // SEPETİ TEMİZLE
            // =================================================
            //
            // Sipariş başarıyla veritabanına kaydedildikten
            // sonra sepet temizleniyor.
            // =================================================

            cart.ClearAll();


            SaveCart(
                cart);


            TempData["SuccessMessage"] =
                $"Siparişiniz başarıyla oluşturuldu. Sipariş No: {order.OrderNumber}";


            return RedirectToAction(
                nameof(CheckoutSuccess),
                new
                {
                    orderId =
                        order.Id
                });
        }


        // =====================================================
        // CHECKOUT SUCCESS
        // =====================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CheckoutSuccess(int orderId)
        {
            var userId =
                GetCurrentUserId();


            if (userId is null)
            {
                return Unauthorized();
            }


            var order =
                await _orderService
                    .GetAsync(x =>
                        x.Id == orderId &&
                        x.AppUserId ==
                        userId.Value);


            if (order is null)
            {
                return NotFound();
            }


            return View(order);
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


        // =====================================================
        // SESSION'DAN SEPETİ GETİR
        // =====================================================

        private CartService GetCart()
        {
            return HttpContext.Session
                       .GetJson<CartService>(
                           "Cart")
                   ?? new CartService();
        }


        // =====================================================
        // SEPETİ SESSION'A KAYDET
        // =====================================================

        private void SaveCart(
            CartService cart)
        {
            HttpContext.Session.SetJson(
                "Cart",
                cart);
        }
    }
}