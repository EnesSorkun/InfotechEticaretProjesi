using Eticaret.Core.Entities;
using Eticaret.Core.Enums;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class OrdersController : Controller
    {
        private readonly IService<Order> _orderService;

        public OrdersController(IService<Order> orderService)
        {
            _orderService = orderService;
        }


        // =========================================================
        // SİPARİŞ LİSTESİ
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService
                .GetQueryable()
                .Include(x => x.AppUser)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            return View(orders);
        }


        // =========================================================
        // SİPARİŞ DETAYI
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService
                .GetQueryable()

                // Siparişi veren kullanıcı
                .Include(x => x.AppUser)

                // Siparişteki ürünler
                .Include(x => x.OrderDetails)

                // Her OrderDetail içindeki Product
                .ThenInclude(x => x.Product)

                .FirstOrDefaultAsync(x => x.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            return View(order);
        }


        // =========================================================
        // SİPARİŞ DURUMUNU GÜNCELLE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
    int id,
    OrderStatus status)
        {
            var order = await _orderService.FindAsync(id);

            if (order is null)
            {
                return NotFound();
            }

            order.Status = status;

            _orderService.Update(order);

            await _orderService.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Sipariş durumu başarıyla güncellendi.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }
}