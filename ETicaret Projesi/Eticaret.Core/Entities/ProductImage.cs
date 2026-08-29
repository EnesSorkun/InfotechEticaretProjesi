using System.ComponentModel.DataAnnotations;

namespace Eticaret.Core.Entities
{
    public class ProductImage : IEntity
    {
        [Key]
        public int Id { get; set; }


        [Required]
        [Display(Name = "Ürün")]
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;


        [Required]
        [StringLength(500)]
        [Display(Name = "Ürün Resmi")]
        public string ImagePath { get; set; } = null!;


        [Display(Name = "Ana Resim")]
        public bool IsMain { get; set; }


        [Display(Name = "Sıra No")]
        public int OrderNo { get; set; }
    }
}