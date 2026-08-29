using Eticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eticaret.Data.Configurations
{
    public class OrderDetailConfiguration
        : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(
            EntityTypeBuilder<OrderDetail> builder)
        {
            builder.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);


            // Order - OrderDetail
            builder.HasOne(x => x.Order)
                .WithMany(x => x.OrderDetails)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);


            // Product - OrderDetail
            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasIndex(x => x.OrderId);

            builder.HasIndex(x => x.ProductId);
        }
    }
}