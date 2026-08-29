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
            // Bütün aktif kategorileri getiriyoruz.
            // IsTopMenu kontrolünü View tarafında
            // sadece ana kategoriler için yapacağız.
            var categories =
                await _categoryService.GetAllAsync(
                    c => c.IsActive);

            return View(categories);
        }
    }
}