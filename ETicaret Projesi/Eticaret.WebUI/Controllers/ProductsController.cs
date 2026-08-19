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
        // ÜRÜNLERİ LİSTELE + ARAMA
        // =====================================================

        public async Task<IActionResult> Index(string? search)
        {
            var query = _productService
                .GetQueryable()
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Where(x => x.IsActive)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.Name.Contains(search) ||
                    x.ProductCode.Contains(search) ||
                    (x.Brand != null &&
                     x.Brand.Name.Contains(search)) ||
                    (x.Category != null &&
                     x.Category.Name.Contains(search)));
            }


            var products = await query
                .OrderBy(x => x.OrderNo)
                .AsNoTracking()
                .ToListAsync();


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
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id.Value);


            if (product is null)
            {
                return NotFound();
            }


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