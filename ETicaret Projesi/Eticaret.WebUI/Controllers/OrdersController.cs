using System.Security.Claims;
using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IService<Order> _orderService;

        public OrdersController(
            IService<Order> orderService)
        {
            _orderService = orderService;
        }


        // =====================================================
        // SİPARİŞLERİM
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId =
                GetCurrentUserId();


            if (userId is null)
            {
                return Unauthorized();
            }


            var orders =
                await _orderService
                    .GetQueryable()
                    .Where(x =>
                        x.AppUserId ==
                        userId.Value)
                    .OrderByDescending(x =>
                        x.OrderDate)
                    .ToListAsync();


            return View(orders);
        }


        // =====================================================
        // SİPARİŞ DETAYI
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var userId =
                GetCurrentUserId();


            if (userId is null)
            {
                return Unauthorized();
            }


            var order =
                await _orderService
                    .GetQueryable()
                    .Include(x =>
                        x.OrderDetails)
                    .ThenInclude(x =>
                        x.Product)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.AppUserId ==
                        userId.Value);


            if (order is null)
            {
                return NotFound();
            }


            return View(order);
        }


        // =====================================================
        // AKTİF KULLANICI ID
        // =====================================================

        private int? GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return null;
            }


            return userId;
        }
    }
}