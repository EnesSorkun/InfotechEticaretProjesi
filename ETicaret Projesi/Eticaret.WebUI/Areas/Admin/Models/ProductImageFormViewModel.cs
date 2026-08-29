using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Eticaret.WebUI.Areas.Admin.Models
{
    public class ProductImageFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün seçiniz.")]
        [DisplayName("Ürün")]
        public int ProductId { get; set; }

        [DisplayName("Resim")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImagePath { get; set; }

        [DisplayName("Ana Resim")]
        public bool IsMain { get; set; }

        [DisplayName("Sıra No")]
        public int OrderNo { get; set; }
    }
}