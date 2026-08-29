using System.ComponentModel.DataAnnotations;
using Eticaret.Core.Entities;

namespace Eticaret.WebUI.Models
{
    public class CheckoutViewModel
    {
        // =====================================================
        // TESLİMAT ADRESİ
        // =====================================================

        [Required(ErrorMessage = "Lütfen bir teslimat adresi seçiniz.")]
        [Display(Name = "Teslimat Adresi")]
        public int? SelectedAddressId { get; set; }


        public List<Address> Addresses { get; set; } = new();


        // =====================================================
        // ÖDEME BİLGİLERİ
        // =====================================================

        [Required(
            ErrorMessage = "Kart üzerindeki isim alanını boş bırakmayınız.")]
        [Display(Name = "Kart Üzerindeki İsim")]
        public string CardHolderName { get; set; } = null!;


        [Required(
            ErrorMessage = "Kart numarasını boş bırakmayınız.")]
        [Display(Name = "Kart Numarası")]
        public string CardNumber { get; set; } = null!;


        [Required(
            ErrorMessage = "Son kullanma tarihini giriniz.")]
        [Display(Name = "Son Kullanma Tarihi")]
        public string ExpirationDate { get; set; } = null!;


        [Required(
            ErrorMessage = "CVV alanını boş bırakmayınız.")]
        [Display(Name = "CVV")]
        public string Cvv { get; set; } = null!;


        // =====================================================
        // SEPET
        // =====================================================

        public List<CartLine> CartLines { get; set; } = new();

        public decimal TotalPrice { get; set; }
    }
}