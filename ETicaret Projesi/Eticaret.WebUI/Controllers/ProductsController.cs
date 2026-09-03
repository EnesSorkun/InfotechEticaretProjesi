using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IService<Product> _productService;

        public ProductsController(
            IService<Product> productService)
        {
            _productService = productService;
        }


        // =====================================================
        // ÜRÜNLERİ LİSTELE
        // KATEGORİ FİLTRESİ + ARAMA
        // =====================================================

        public async Task<IActionResult> Index(
            int? categoryId,
            string? search)
        {
            var query = _productService
                .GetQueryable()
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Where(x => x.IsActive)
                .AsQueryable();


            // =====================================================
            // KATEGORİ FİLTRESİ
            // =====================================================

            if (categoryId.HasValue)
            {
                query = query.Where(x =>
                    x.CategoryId == categoryId.Value);
            }


            // =====================================================
            // ARAMA
            // =====================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                var searchPattern = $"%{search}%";

                query = query.Where(x =>

                    EF.Functions.ILike(
                        x.Name,
                        searchPattern)

                    ||

                    (x.ProductCode != null &&
                     EF.Functions.ILike(
                         x.ProductCode,
                         searchPattern))

                    ||

                    (x.Brand != null &&
                     EF.Functions.ILike(
                         x.Brand.Name,
                         searchPattern))

                    ||

                    (x.Category != null &&
                     EF.Functions.ILike(
                         x.Category.Name,
                         searchPattern))
                );
            }


            // =====================================================
            // ÜRÜNLERİ GETİR
            // =====================================================

            var products = await query
                .OrderBy(x => x.OrderNo)
                .AsNoTracking()
                .ToListAsync();


            ViewBag.CategoryId = categoryId;
            ViewBag.Search = search;


            return View(products);
        }


        // =====================================================
        // ÜRÜN DETAYI
        // =====================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }


            var product = await _productService
                .GetQueryable()
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id.Value &&
                    x.IsActive);


            if (product is null)
            {
                return NotFound();
            }


            // =====================================================
            // BENZER ÜRÜNLER
            // =====================================================

            var relatedProducts = await _productService
                .GetQueryable()
                .Where(x =>
                    x.IsActive &&
                    x.CategoryId == product.CategoryId &&
                    x.Id != product.Id)
                .OrderBy(x => x.OrderNo)
                .AsNoTracking()
                .ToListAsync();


            var model = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts
            };


            return View(model);
        }
    }
}