using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eticaret.Core.Entities;

namespace Eticaret.Service.Abstract
{
    public interface ICartService
    {
        void AddProduct(Product product, int quantity);
        void UpdateProduct(Product product, int quantity);
        void RemoveProduct(Product product);
        decimal TotalPrice();
        void ClearAll();
    }
}