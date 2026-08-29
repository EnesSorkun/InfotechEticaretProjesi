using Eticaret.Core.Entities;

namespace Eticaret.WebUI.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public int UserCount { get; set; }

        public List<Order> Orders { get; set; } = new();

        public List<Product> Products { get; set; } = new();
    }
}