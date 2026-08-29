using System.ComponentModel.DataAnnotations;

namespace Eticaret.WebUI.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = null!;


        [Required(ErrorMessage = "Email adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; } = null!;


        [Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Şifreniz en az 6 karakter olmalıdır.")]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; } = null!;


        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "Şifreler birbiriyle eşleşmiyor.")]
        [Display(Name = "Yeni Şifre Tekrar")]
        public string ConfirmPassword { get; set; } = null!;
    }
}