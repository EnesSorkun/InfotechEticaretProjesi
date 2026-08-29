using System.ComponentModel.DataAnnotations;

namespace Eticaret.Core.Entities
{
    public class Address : IEntity
    {
        public int Id { get; set; }


        // =====================================================
        // KULLANICI
        // =====================================================

        public int AppUserId { get; set; }

        public AppUser AppUser { get; set; } = null!;


        // =====================================================
        // ADRES BİLGİLERİ
        // =====================================================

        [Required(
            ErrorMessage = "Adres başlığını boş bırakmayınız.")]
        [Display(Name = "Adres Başlığı")]
        public string Title { get; set; } = null!;


        [Required(
            ErrorMessage = "Ad alanını boş bırakmayınız.")]
        [Display(Name = "Ad")]
        public string Name { get; set; } = null!;


        [Required(
            ErrorMessage = "Soyad alanını boş bırakmayınız.")]
        [Display(Name = "Soyad")]
        public string Surname { get; set; } = null!;


        [Required(
            ErrorMessage = "Telefon alanını boş bırakmayınız.")]
        [Display(Name = "Telefon")]
        public string Phone { get; set; } = null!;


        [Required(
            ErrorMessage = "Şehir alanını boş bırakmayınız.")]
        [Display(Name = "Şehir")]
        public string City { get; set; } = null!;


        [Required(
            ErrorMessage = "İlçe alanını boş bırakmayınız.")]
        [Display(Name = "İlçe")]
        public string District { get; set; } = null!;


        [Required(
            ErrorMessage = "Adres alanını boş bırakmayınız.")]
        [Display(Name = "Adres")]
        public string FullAddress { get; set; } = null!;


        [Display(Name = "Posta Kodu")]
        [MaxLength(10, ErrorMessage = "Posta kodu en fazla 10 karakter olabilir.")]
        public string? PostalCode { get; set; }


        // =====================================================
        // SİSTEM
        // =====================================================

        public bool IsDefault { get; set; }

        public DateTime CreateDate { get; set; }
    }
}