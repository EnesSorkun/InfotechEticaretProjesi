using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Controllers;

public class HomeController : Controller
{
    private readonly IService<Slider> _sliderService;
    private readonly IService<News> _newsService;
    private readonly IService<Product> _productService;
    private readonly IService<Contact> _contactService;

    public HomeController(
        IService<Slider> sliderService,
        IService<News> newsService,
        IService<Product> productService,
        IService<Contact> contactService)
    {
        _sliderService = sliderService;
        _newsService = newsService;
        _productService = productService;
        _contactService = contactService;
    }


    // =====================================================
    // HOME
    // =====================================================

    public async Task<IActionResult> Index()
    {
        var model = new HomePageViewModel
        {
            // Slider kayıtları
            Sliders = await _sliderService
                .GetAllAsync(),


            // Sadece aktif kampanyalar
            News = await _newsService
                .GetQueryable()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreateDate)
                .AsNoTracking()
                .ToListAsync(),


            // Ana sayfada gösterilecek aktif ürünler
            Products = await _productService
                .GetQueryable()
                .Where(x =>
                    x.IsActive &&
                    x.IsHome)
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .OrderBy(x => x.OrderNo)
                .AsNoTracking()
                .ToListAsync()
        };

        return View(model);
    }


    // =====================================================
    // PRIVACY
    // =====================================================

    public IActionResult Privacy()
    {
        return View();
    }


    // =====================================================
    // CONTACT US GET
    // =====================================================

    [HttpGet]
    public IActionResult ContactUs()
    {
        return View();
    }


    // =====================================================
    // CONTACT US POST
    // =====================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ContactUs(
        Contact contact)
    {
        if (!ModelState.IsValid)
        {
            return View(contact);
        }


        contact.CreateDate =
            DateTime.UtcNow;


        await _contactService
            .AddAsync(contact);


        await _contactService
            .SaveChangesAsync();


        TempData["ContactSuccess"] =
            "Mesajınız başarıyla gönderilmiştir.";


        return RedirectToAction(
            nameof(ContactUs));
    }
}