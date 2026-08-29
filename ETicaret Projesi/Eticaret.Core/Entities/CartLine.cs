using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eticaret.Core.Entities
{
    public class CartLine
    {
        public int Id { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
    }
}