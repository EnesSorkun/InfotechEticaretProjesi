using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.ViewComponents
{
    public class Categories : ViewComponent
    {
        private readonly IService<Category> _categoryService;

        public Categories(
            IService<Category> categoryService)
        {
            _categoryService = categoryService;
        }


        // =====================================================
        // KATEGORİLERİ GETİR
        // =====================================================

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories =
                await _categoryService.GetAllAsync(c => c.IsTopMenu && c.IsActive);

            return View(categories);
        }
    }
}