using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eticaret.Core.Entities;

namespace Eticaret.WebUI.Models
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public IEnumerable<Product>? RelatedProducts { get; set; }
    }
}