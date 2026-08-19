using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.Controllers
{
    public class NewsController : Controller
    {
        private readonly IService<News> _newsService;

        public NewsController(
            IService<News> newsService)
        {
            _newsService = newsService;
        }


        // =====================================================
        // HABERLERİ LİSTELE
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var news =
                await _newsService.GetAllAsync();

            return View(news);
        }


        // =====================================================
        // HABER DETAYI
        // =====================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }


            var news =
                await _newsService.FindAsync(
                    id.Value);


            if (news is null)
            {
                return NotFound();
            }


            return View(news);
        }
    }
}