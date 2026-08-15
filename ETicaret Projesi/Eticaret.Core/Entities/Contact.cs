using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Eticaret.Core.Entities
{
    public class Contact : IEntity
    {
        public int Id { get; set; }

        [DisplayName("Ad")]
        [Required(ErrorMessage = "Ad kısmını boş geçmeyiniz.")]
        public string Name { get; set; }

        [DisplayName("Soyad")]
        [Required(ErrorMessage = "Soyad kısmını boş geçmeyiniz.")]
        public string Surname { get; set; }

        [DisplayName("Email")]
        [Required(ErrorMessage = "Email adresini boş geçmeyiniz.")]
        public string? Email { get; set; }

        [DisplayName("Telefon")]
        public string? Phone { get; set; }

        [DisplayName("Mesaj")]
        [Required(ErrorMessage = "Mesaj kısmını boş geçmeyiniz.")]
        public string Message { get; set; }

        [DisplayName("Kayıt Tarihi")]
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}