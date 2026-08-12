using System.Diagnostics;
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
        var sliders = await _context.Sliders
            .ToListAsync();

        return View(sliders);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult ContactUs()
    {
        return View();
    }
}