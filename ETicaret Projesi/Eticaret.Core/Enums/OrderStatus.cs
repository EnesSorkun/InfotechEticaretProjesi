using System.ComponentModel.DataAnnotations;

namespace Eticaret.Core.Enums
{
    public enum OrderStatus
    {
        [Display(Name = "Onay Bekliyor")]
        PendingApproval = 1,

        [Display(Name = "Onaylandı")]
        Approved = 2,

        [Display(Name = "Hazırlanıyor")]
        Preparing = 3,

        [Display(Name = "Kargoya Verildi")]
        Shipped = 4,

        [Display(Name = "Teslim Edildi")]
        Delivered = 5,

        [Display(Name = "İptal Edildi")]
        Cancelled = 6
    }
}