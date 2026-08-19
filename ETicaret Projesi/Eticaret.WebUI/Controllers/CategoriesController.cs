using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IService<Category> _categoryService;

        public CategoriesController(
            IService<Category> categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var category = await _categoryService
                .GetQueryable()
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category is null)
            {
                return NotFound();
            }

            return View(category);
        }
    }
}