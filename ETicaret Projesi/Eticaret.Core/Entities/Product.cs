using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Eticaret.Core.Entities
{
    public class Product : IEntity
    {
        public int Id { get; set; }

        [DisplayName("Ad")]
        public string Name { get; set; }

        [DisplayName("Açıklama")]
        public string? Description { get; set; }

        [DisplayName("Resim")]
        public string? Image { get; set; }

        [DisplayName("Fiyat")]
        public decimal Price { get; set; }

        [DisplayName("Ürün Kodu")]
        public string? ProductCode { get; set; }

        [DisplayName("Stok")]
        public int Stock { get; set; }

        [DisplayName("Aktif/Pasif")]
        public bool IsActive { get; set; }

        [DisplayName("Anasayfada Göster")]
        public bool IsHome { get; set; }

        [DisplayName("Kategori")]
        public int CategoryId { get; set; }

        [DisplayName("Kategori")]
        public Category? Category { get; set; }

        [DisplayName("Marka")]
        public int BrandId { get; set; }

        [DisplayName("Marka")]
        public Brand? Brand { get; set; }

        [DisplayName("Sıra No")]
        public int OrderNo { get; set; }

        [DisplayName("Kayıt Tarihi")]
        public DateTime CreateDate { get; set; }
    }
}