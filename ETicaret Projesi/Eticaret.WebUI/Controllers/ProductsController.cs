using Eticaret.Data;
using Eticaret.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DatabaseContext _context;

        public ProductsController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Products
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
                    (x.Brand != null && x.Brand.Name.Contains(search)) ||
                    (x.Category != null && x.Category.Name.Contains(search)));
            }

            var products = await query
                .OrderBy(x => x.OrderNo)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            var model = new ProductDetailViewModel()
            {
                Product = product,
                RelatedProducts = _context.Products.Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.Id != product.Id)
            };

            return View(model);
        }
    }
}