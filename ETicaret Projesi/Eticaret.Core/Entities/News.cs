using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Eticaret.Core.Entities
{
    public class News : IEntity
    {
        public int Id { get; set; }

        [DisplayName("Ad")]
        public string Name { get; set; }

        [DisplayName("Açıklama")]
        public string? Description { get; set; }

        [DisplayName("Resim")]
        public string? Image { get; set; }

        [DisplayName("Pasif/Aktif")]
        public bool IsActive { get; set; }

        [DisplayName("Kayıt Tarihi")]
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}