using Eticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eticaret.Data.Configurations
{
    public class AddressConfiguration
        : IEntityTypeConfiguration<Address>
    {
        public void Configure(
            EntityTypeBuilder<Address> builder)
        {
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Surname)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.District)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.FullAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.PostalCode)
                .HasMaxLength(10);


            builder.HasOne(x => x.AppUser)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasIndex(x =>
                x.AppUserId);
        }
    }
}