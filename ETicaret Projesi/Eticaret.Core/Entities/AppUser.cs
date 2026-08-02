using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Eticaret.Core.Entities
{
    public class AppUser : IEntity
    {
        public int Id { get; set; }

        [DisplayName("Ad")]
        public string Name { get; set; }

        [DisplayName("Soyad")]
        public string Surname { get; set; }

        [DisplayName("Email")]
        public string Email { get; set; }

        [DisplayName("Telefon")]
        public string? Phone { get; set; }

        [DisplayName("Şifre")]
        public string Password { get; set; }

        [DisplayName("Kullanıcı Adı")]
        public string? UserName { get; set; }

        [DisplayName("Aktif/Pasif")]
        public bool IsActive { get; set; }

        [DisplayName("Admin")]
        public bool IsAdmin { get; set; }

        [DisplayName("Kayıt Tarihi")]
        public DateTime CreateDate { get; set; }
        public Guid? UserGuid { get; set; }

    }
}