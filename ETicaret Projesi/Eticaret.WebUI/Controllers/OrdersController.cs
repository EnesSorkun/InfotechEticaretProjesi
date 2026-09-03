using System.Security.Claims;
using Eticaret.Core.Entities;
using Eticaret.Core.Enums;
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
        private readonly IService<Product> _productService;

        public OrdersController(
            IService<Order> orderService,
            IService<Product> productService)
        {
            _orderService = orderService;
            _productService = productService;
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
        // SİPARİŞ İPTAL ET
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(
            int id)
        {
            // -------------------------------------------------
            // AKTİF KULLANICI
            // -------------------------------------------------

            var userId =
                GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }


            // -------------------------------------------------
            // SİPARİŞİ GETİR
            // -------------------------------------------------
            //
            // AppUserId kontrolü çok önemli.
            //
            // Kullanıcı URL veya form üzerinden başka bir
            // sipariş ID'si gönderse bile yalnızca kendi
            // siparişini iptal edebilir.
            // -------------------------------------------------

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


            // -------------------------------------------------
            // SİPARİŞ DURUMU KONTROLÜ
            // -------------------------------------------------
            //
            // Müşteri sadece henüz admin tarafından
            // onaylanmamış siparişi iptal edebilir.
            // -------------------------------------------------

            if (order.Status !=
                OrderStatus.PendingApproval)
            {
                TempData["ErrorMessage"] =
                    "Bu sipariş artık iptal edilemez.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // -------------------------------------------------
            // ÜRÜNLERİ STOĞA GERİ EKLE
            // -------------------------------------------------
            //
            // Sipariş oluşturulurken stoktan düşmüştük.
            // Sipariş iptal edildiği için miktarları
            // tekrar stoklara ekliyoruz.
            // -------------------------------------------------

            foreach (var orderDetail
                     in order.OrderDetails)
            {
                if (orderDetail.Product is null)
                {
                    continue;
                }

                orderDetail.Product.Stock +=
                    orderDetail.Quantity;

                _productService.Update(
                    orderDetail.Product);
            }


            // -------------------------------------------------
            // SİPARİŞİ İPTAL EDİLDİ OLARAK İŞARETLE
            // -------------------------------------------------

            order.Status =
                OrderStatus.Cancelled;

            _orderService.Update(order);


            // -------------------------------------------------
            // DEĞİŞİKLİKLERİ KAYDET
            // -------------------------------------------------

            await _orderService
                .SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Siparişiniz başarıyla iptal edildi.";


            return RedirectToAction(
                nameof(Details),
                new { id });
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