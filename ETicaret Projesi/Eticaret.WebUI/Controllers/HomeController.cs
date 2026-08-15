using System.Diagnostics;
using Eticaret.Core.Entities;
using Eticaret.Data;
using Eticaret.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Controllers;

public class HomeController : Controller
{
    private readonly DatabaseContext _context;

    public HomeController(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Front-end'de pasif kayıtları göstermedik.
        var model = new HomePageViewModel
        {
            Sliders = await _context.Sliders
                .ToListAsync(),

            News = await _context.News
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync(),

            Products = await _context.Products
                .Where(x => x.IsActive && x.IsHome)
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .OrderBy(x => x.OrderNo)
                .ToListAsync()
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult ContactUs()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ContactUs(Contact contact)
    {
        if (!ModelState.IsValid)
        {
            return View(contact);
        }

        contact.CreateDate = DateTime.UtcNow;

        _context.Contacts.Add(contact);

        await _context.SaveChangesAsync();

        TempData["ContactSuccess"] =
            "Mesajınız başarıyla gönderilmiştir.";

        return RedirectToAction(nameof(ContactUs));
    }
}