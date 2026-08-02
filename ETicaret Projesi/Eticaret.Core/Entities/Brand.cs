using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Eticaret.Core.Entities
{
    public class Brand : IEntity
    {
        public int Id { get; set; }

        [DisplayName("Ad")]
        public string Name { get; set; }

        [DisplayName("Açıklama")]
        public string? Description { get; set; }
        public string? Logo { get; set; }

        [DisplayName("Aktif/Pasif")]
        public bool IsActive { get; set; }

        [DisplayName("Sıra No")]
        public int OrderNo { get; set; }

        [DisplayName("Kayıt Tarihi")]
        public DateTime CreateDate { get; set; }
        public ICollection<Product>? Products { get; set; }
    }
}