using System.ComponentModel.DataAnnotations;

namespace Eticaret.WebUI.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email alanını boş geçmeyiniz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Şifre alanını boş geçmeyiniz.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = null!;

        public string? ReturnUrl { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}