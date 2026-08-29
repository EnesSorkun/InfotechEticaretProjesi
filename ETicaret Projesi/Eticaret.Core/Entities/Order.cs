using System.ComponentModel.DataAnnotations;
using Eticaret.Core.Enums;

namespace Eticaret.Core.Entities
{
    public class Order : IEntity
    {
        public int Id { get; set; }


        [Display(Name = "Sipariş No")]
        [StringLength(50)]
        public string OrderNumber { get; set; } = null!;


        [Display(Name = "Sipariş Toplamı")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Sipariş Durumu")]
        public OrderStatus Status { get; set; }


        // =====================================================
        // KULLANICI
        // =====================================================

        [Display(Name = "Müşteri No")]
        public int AppUserId { get; set; }

        public AppUser AppUser { get; set; } = null!;


        // =====================================================
        // ADRESLER
        // =====================================================

        [Display(Name = "Fatura Adresi")]
        [StringLength(500)]
        public string BillingAddress { get; set; } = null!;


        [Display(Name = "Teslimat Adresi")]
        [StringLength(500)]
        public string DeliveryAddress { get; set; } = null!;


        // =====================================================
        // SİPARİŞ TARİHİ
        // =====================================================

        [Display(Name = "Sipariş Tarihi")]
        public DateTime OrderDate { get; set; }


        // =====================================================
        // SİPARİŞ DETAYLARI
        // =====================================================

        public ICollection<OrderDetail> OrderDetails { get; set; }
            = new List<OrderDetail>();
    }
}