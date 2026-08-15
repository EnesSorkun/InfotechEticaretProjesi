using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Eticaret.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Eticaret.WebUI.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DatabaseContext _context;
        public CategoriesController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexAsync(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var category = await _context
            .Categories
            .Include(p => p.Products)
            .FirstOrDefaultAsync(x => x.Id == id);

            if (category is null)
            {
                return NotFound();
            }

            return View(category);
        }
    }
}