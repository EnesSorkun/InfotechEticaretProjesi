using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eticaret.WebUI.DTOs.Categories;

namespace Eticaret.WebUI.Controllers.Api
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesApiController : ControllerBase
    {
        private readonly IService<Category> _categoryService;

        public CategoriesApiController(
            IService<Category> categoryService)
        {
            _categoryService = categoryService;
        }


        // =====================================================
        // TÜM KATEGORİLER
        // GET: /api/categories
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories =
                await _categoryService
                    .GetQueryable()
                    .AsNoTracking()
                    .OrderBy(x => x.OrderNo)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.Description,
                        x.Image,
                        x.IsActive,
                        x.IsTopMenu,
                        x.ParentId,
                        x.OrderNo,
                        x.CreateDate
                    })
                    .ToListAsync();

            return Ok(categories);
        }


        // =====================================================
        // ID'YE GÖRE KATEGORİ
        // GET: /api/categories/11
        // =====================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(
            int id)
        {
            var category =
                await _categoryService
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(x => x.Id == id)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.Description,
                        x.Image,
                        x.IsActive,
                        x.IsTopMenu,
                        x.ParentId,
                        x.OrderNo,
                        x.CreateDate
                    })
                    .FirstOrDefaultAsync();

            if (category is null)
            {
                return NotFound(new
                {
                    message = "Kategori bulunamadı."
                });
            }

            return Ok(category);
        }


        // =====================================================
        // KATEGORİ EKLE
        // POST: /api/categories
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
    CreateCategoryDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new
                {
                    message = "Kategori adı zorunludur."
                });
            }

            var category = new Category
            {
                Name = request.Name.Trim(),
                Description = request.Description,
                Image = request.Image,
                IsActive = request.IsActive,
                IsTopMenu = request.IsTopMenu,
                ParentId = request.ParentId,
                OrderNo = request.OrderNo,
                CreateDate = DateTime.UtcNow
            };

            await _categoryService.AddAsync(category);
            await _categoryService.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.Id },
                new
                {
                    category.Id,
                    category.Name,
                    category.Description,
                    category.Image,
                    category.IsActive,
                    category.IsTopMenu,
                    category.ParentId,
                    category.OrderNo,
                    category.CreateDate
                });
        }


        // =====================================================
        // KATEGORİ GÜNCELLE
        // PUT: /api/categories/11
        // =====================================================

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
    int id,
    UpdateCategoryDto request)
        {
            var category =
                await _categoryService.FindAsync(id);

            if (category is null)
            {
                return NotFound(new
                {
                    message = "Kategori bulunamadı."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new
                {
                    message = "Kategori adı zorunludur."
                });
            }

            category.Name = request.Name.Trim();
            category.Description = request.Description;
            category.Image = request.Image;
            category.IsActive = request.IsActive;
            category.IsTopMenu = request.IsTopMenu;
            category.ParentId = request.ParentId;
            category.OrderNo = request.OrderNo;

            _categoryService.Update(category);

            await _categoryService.SaveChangesAsync();

            return Ok(new
            {
                message = "Kategori başarıyla güncellendi."
            });
        }


        // =====================================================
        // KATEGORİ SİL
        // DELETE: /api/categories/11
        // =====================================================

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            var category =
                await _categoryService
                    .GetQueryable()
                    .Include(x => x.Products)
                    .FirstOrDefaultAsync(
                        x => x.Id == id);

            if (category is null)
            {
                return NotFound(new
                {
                    message =
                        "Kategori bulunamadı."
                });
            }


            // Kategoriye bağlı ürün varsa silmiyoruz.
            if (category.Products is not null &&
                category.Products.Any())
            {
                return BadRequest(new
                {
                    message =
                        "Bu kategoriye bağlı ürünler bulunduğu için kategori silinemez."
                });
            }


            _categoryService.Delete(
                category);

            await _categoryService
                .SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Kategori başarıyla silindi."
            });
        }
    }
}