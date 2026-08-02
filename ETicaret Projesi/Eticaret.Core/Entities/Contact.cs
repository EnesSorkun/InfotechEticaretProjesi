using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Eticaret.Core.Entities
{
    public class Contact : IEntity
    {
        public int Id { get; set; }

        [DisplayName("Ad")]
        public string Name { get; set; }

        [DisplayName("Soyad")]
        public string Surname { get; set; }

        [DisplayName("Email")]
        public string? Email { get; set; }

        [DisplayName("Telefon")]
        public string? Phone { get; set; }

        [DisplayName("Mesaj")]
        public string Message { get; set; }

        [DisplayName("Kayıt Tarihi")]
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}