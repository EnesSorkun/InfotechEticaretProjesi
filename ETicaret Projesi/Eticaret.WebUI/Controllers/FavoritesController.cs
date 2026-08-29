using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.ExtensionMethods;
using Eticaret.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Controllers
{
    [Authorize]
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
                .AsNoTracking()
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
                var favoriteProduct =
                    new FavoriteProductViewModel
                    {
                        Id = product.Id,

                        Name = product.Name,

                        ProductCode = product.ProductCode,

                        Price = product.Price,

                        Stock = product.Stock,

                        Image = product.Image,

                        BrandName = product.Brand?.Name,

                        CategoryName = product.Category?.Name
                    };


                products.Add(
                    favoriteProduct);


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
        public IActionResult Remove(
            int id,
            string? returnUrl = null)
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


            // Kullanıcı hangi sayfadaysa
            // favoriden kaldırdıktan sonra aynı sayfaya dönsün.
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }


            return RedirectToAction(
                nameof(Index));
        }

        // =====================================================
        // SESSION'DAKİ FAVORİLERİ GETİR
        // =====================================================

        private List<FavoriteProductViewModel> GetProducts()
        {
            var products =
                HttpContext.Session
                    .GetJson<List<FavoriteProductViewModel>>(
                        "Favorites");


            return products
                   ?? new List<FavoriteProductViewModel>();
        }
    }
}