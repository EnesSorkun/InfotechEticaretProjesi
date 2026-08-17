using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eticaret.Core.Entities
{
    public class AppUser : IEntity
    {
        public int Id { get; set; }


        [DisplayName("Ad")]
        [Required(ErrorMessage = "Ad alanını boş geçmeyiniz.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string Name { get; set; }


        [DisplayName("Soyad")]
        [Required(ErrorMessage = "Soyad alanını boş geçmeyiniz.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string Surname { get; set; }


        [DisplayName("Email")]
        [Required(ErrorMessage = "Email alanını boş geçmeyiniz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; }


        [DisplayName("Telefon")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(15, ErrorMessage = "Telefon en fazla 15 karakter olabilir.")]
        public string? Phone { get; set; }


        [DisplayName("Şifre")]
        [Required(ErrorMessage = "Şifre alanını boş geçmeyiniz.")]
        public string Password { get; set; }


        [DisplayName("Kullanıcı Adı")]
        [Required(ErrorMessage = "Kullanıcı adı alanını boş geçmeyiniz.")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir.")]
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