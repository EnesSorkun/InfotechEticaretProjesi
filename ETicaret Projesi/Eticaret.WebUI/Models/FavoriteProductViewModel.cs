namespace Eticaret.WebUI.Models
{
    public class FavoriteProductViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? ProductCode { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string? Image { get; set; }

        public string? BrandName { get; set; }

        public string? CategoryName { get; set; }
    }
}