using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class MainController : Controller
    {
        private readonly IService<AppUser> _userService;
        private readonly IService<Order> _orderService;
        private readonly IService<Product> _productService;

        public MainController(
            IService<AppUser> userService,
            IService<Order> orderService,
            IService<Product> productService)
        {
            _userService = userService;
            _orderService = orderService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            // Kayıtlı müşteri sayısı
            var userCount = await _userService
                .GetQueryable()
                .CountAsync(x => !x.IsAdmin);


            // Siparişler
            var orders = await _orderService
                .GetQueryable()
                .Include(x => x.AppUser)
                .OrderByDescending(x => x.OrderDate)
                .AsNoTracking()
                .ToListAsync();


            // Ürünler
            var products = await _productService
                .GetQueryable()
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .OrderBy(x => x.OrderNo)
                .AsNoTracking()
                .ToListAsync();


            var model = new AdminDashboardViewModel
            {
                UserCount = userCount,
                Orders = orders,
                Products = products
            };


            return View(model);
        }
    }
}