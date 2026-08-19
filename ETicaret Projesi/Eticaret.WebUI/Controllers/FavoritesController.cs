using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.ExtensionMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly IService<Product> _productService;

        public FavoritesController(
            IService<Product> productService)
        {
            _productService = productService;
        }


        // =====================================================
        // FAVORİLERİ LİSTELE
        // =====================================================

        public IActionResult Index()
        {
            var products = GetProducts();

            return View(products);
        }


        // =====================================================
        // FAVORİYE EKLE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            int id,
            string? returnUrl = null)
        {
            var product = await _productService
                .GetQueryable()
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);


            if (product is null)
            {
                return NotFound();
            }


            var products = GetProducts();


            // Ürün daha önce favorilere eklenmiş mi?
            var exists = products
                .Any(x => x.Id == product.Id);


            if (!exists)
            {
                products.Add(product);


                HttpContext.Session.SetJson(
                    "Favorites",
                    products);


                TempData["SuccessMessage"] =
                    "Ürün favorilerinize eklendi.";
            }
            else
            {
                TempData["InfoMessage"] =
                    "Bu ürün zaten favorilerinizde.";
            }


            // Kullanıcı hangi sayfadaysa
            // favoriye ekledikten sonra aynı sayfaya dönsün.
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // FAVORİLERDEN KALDIR
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int id)
        {
            var products = GetProducts();


            var product = products
                .FirstOrDefault(x => x.Id == id);


            if (product is not null)
            {
                products.Remove(product);


                HttpContext.Session.SetJson(
                    "Favorites",
                    products);


                TempData["SuccessMessage"] =
                    "Ürün favorilerinizden kaldırıldı.";
            }


            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // SESSION'DAKİ FAVORİLERİ GETİR
        // =====================================================

        private List<Product> GetProducts()
        {
            var products =
                HttpContext.Session
                    .GetJson<List<Product>>(
                        "Favorites");


            return products
                   ?? new List<Product>();
        }
    }
}