using System.ComponentModel.DataAnnotations;

namespace Eticaret.WebUI.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [Display(Name = "Email Adresi")]
        public string Email { get; set; } = null!;
    }
}