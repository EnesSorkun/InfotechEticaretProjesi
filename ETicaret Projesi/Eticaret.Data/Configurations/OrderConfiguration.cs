using Eticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eticaret.Data.Configurations
{
    public class OrderConfiguration
        : IEntityTypeConfiguration<Order>
    {
        public void Configure(
            EntityTypeBuilder<Order> builder)
        {
            // =================================================
            // SİPARİŞ NUMARASI
            // =================================================

            builder.Property(x => x.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);


            builder.HasIndex(x => x.OrderNumber)
                .IsUnique();


            // =================================================
            // TOPLAM TUTAR
            // =================================================

            builder.Property(x => x.TotalPrice)
                .HasPrecision(18, 2);


            // =================================================
            // FATURA ADRESİ
            // =================================================

            builder.Property(x => x.BillingAddress)
                .IsRequired()
                .HasMaxLength(500);


            // =================================================
            // TESLİMAT ADRESİ
            // =================================================

            builder.Property(x => x.DeliveryAddress)
                .IsRequired()
                .HasMaxLength(500);


            // =================================================
            // ENUM - SİPARİŞ DURUMU
            // =================================================
            builder.Property(x => x.Status)
                           .HasConversion<string>()
                           .HasMaxLength(30)
                           .IsRequired();


            // =================================================
            // USER - ORDER İLİŞKİSİ
            // =================================================

            builder.HasOne(x => x.AppUser)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // INDEXLER
            // =================================================

            builder.HasIndex(x => x.AppUserId);

            builder.HasIndex(x => x.OrderDate);
        }
    }
}