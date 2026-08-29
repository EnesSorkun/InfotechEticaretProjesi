using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;

namespace Eticaret.Service.Concrete
{
    public class CartService : ICartService
    {
        public List<CartLine> CartLines = new();


        // =====================================================
        // SEPETE ÜRÜN EKLE
        // =====================================================

        public void AddProduct(
            Product product,
            int quantity)
        {
            // Ürün sepette daha önce var mı?
            var cartLine = CartLines
                .FirstOrDefault(x =>
                    x.Product.Id == product.Id);


            // Ürün sepette yoksa yeni CartLine oluşturuyoruz.
            if (cartLine is null)
            {
                CartLines.Add(
                    new CartLine
                    {
                        Product = product,
                        Quantity = quantity
                    });

                return;
            }


            // Ürün zaten sepette varsa
            // mevcut miktarın üzerine ekliyoruz.
            cartLine.Quantity += quantity;
        }


        // =====================================================
        // SEPETTEN ÜRÜN KALDIR
        // =====================================================

        public void RemoveProduct(
            Product product)
        {
            var cartLine = CartLines
                .FirstOrDefault(x =>
                    x.Product.Id == product.Id);


            if (cartLine is not null)
            {
                CartLines.Remove(cartLine);
            }
        }


        // =====================================================
        // ÜRÜN ADEDİNİ GÜNCELLE
        // =====================================================

        public void UpdateProduct(
    Product product,
    int quantity)
        {
            var cartLine =
                CartLines.FirstOrDefault(x =>
                    x.Product.Id == product.Id);


            if (cartLine is null)
            {
                return;
            }


            if (quantity <= 0)
            {
                CartLines.Remove(cartLine);

                return;
            }


            cartLine.Quantity =
                quantity;
        }


        // =====================================================
        // SEPETİ TEMİZLE
        // =====================================================

        public void ClearAll()
        {
            CartLines.Clear();
        }


        // =====================================================
        // SEPET TOPLAM TUTARI
        // =====================================================

        public decimal TotalPrice()
        {
            return CartLines.Sum(x =>
                x.Product.Price * x.Quantity);
        }
    }
}