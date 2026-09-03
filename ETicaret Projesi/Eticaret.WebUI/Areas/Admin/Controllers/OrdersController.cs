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
        private readonly IService<Product> _productService;

        public OrdersController(
            IService<Order> orderService,
            IService<Product> productService)
        {
            _orderService = orderService;
            _productService = productService;
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
            // Siparişi OrderDetails ve Product bilgileriyle
            // birlikte getiriyoruz.
            var order = await _orderService
                .GetQueryable()
                .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order is null)
            {
                return NotFound();
            }


            // Siparişin mevcut durumunu saklıyoruz.
            var oldStatus = order.Status;


            // =====================================================
            // SİPARİŞ İPTAL EDİLİYORSA STOĞU GERİ EKLE
            // =====================================================
            //
            // Sipariş daha önce iptal edilmemişken şimdi
            // Cancelled durumuna geçiriliyorsa ürünleri stoğa
            // geri ekliyoruz.
            //
            // oldStatus kontrolü sayesinde aynı sipariş tekrar
            // Cancelled yapılırsa stok ikinci kez artmaz.
            // =====================================================

            if (oldStatus != OrderStatus.Cancelled &&
                status == OrderStatus.Cancelled)
            {
                foreach (var orderDetail in order.OrderDetails)
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
            }


            // =====================================================
            // İPTAL EDİLEN SİPARİŞ TEKRAR AKTİF EDİLİRSE
            // =====================================================
            //
            // Örneğin:
            //
            // Cancelled -> Approved
            //
            // yapılırsa ürünleri tekrar stoktan düşmemiz gerekir.
            // Ancak önce yeterli stok olup olmadığını kontrol ediyoruz.
            // =====================================================

            if (oldStatus == OrderStatus.Cancelled &&
                status != OrderStatus.Cancelled)
            {
                foreach (var orderDetail in order.OrderDetails)
                {
                    if (orderDetail.Product is null)
                    {
                        continue;
                    }

                    if (orderDetail.Product.Stock <
                        orderDetail.Quantity)
                    {
                        TempData["ErrorMessage"] =
                            $"{orderDetail.Product.Name} ürünü için " +
                            $"yeterli stok bulunmamaktadır. " +
                            $"Mevcut stok: {orderDetail.Product.Stock}";

                        return RedirectToAction(
                            nameof(Details),
                            new { id });
                    }
                }


                // Bütün ürünlerin stoğu yeterliyse
                // stokları tekrar azaltıyoruz.
                foreach (var orderDetail in order.OrderDetails)
                {
                    if (orderDetail.Product is null)
                    {
                        continue;
                    }

                    orderDetail.Product.Stock -=
                        orderDetail.Quantity;

                    _productService.Update(
                        orderDetail.Product);
                }
            }


            // =====================================================
            // YENİ SİPARİŞ DURUMUNU KAYDET
            // =====================================================

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