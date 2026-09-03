namespace Eticaret.WebUI.DTOs.Categories
{
    public class UpdateCategoryDto
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? Image { get; set; }

        public bool IsActive { get; set; }

        public bool IsTopMenu { get; set; }

        public int ParentId { get; set; }

        public int OrderNo { get; set; }
    }
}