using Eticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eticaret.Data.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasColumnType("varchar(50)").HasMaxLength(50);
            builder.Property(x => x.Surname).IsRequired().HasColumnType("varchar(50)").HasMaxLength(50);
            builder.Property(x => x.Email).IsRequired().HasColumnType("varchar(50)").HasMaxLength(50);
            builder.Property(x => x.Phone).HasColumnType("varchar(15)").HasMaxLength(15);
            builder.Property(x => x.Password).IsRequired().HasColumnType("varchar(200)").HasMaxLength(200);
            builder.Property(x => x.UserName).HasColumnType("varchar(70)").HasMaxLength(70);
            builder.HasData(
                new AppUser
                {
                    Id = 1,
                    UserName = "Admin",
                    Email = "admin@eticaret.io",
                    IsActive = true,
                    IsAdmin = true,
                    Name = "Test",
                    Password = "123456",
                    Surname = "User",
                    CreateDate = DateTime.UtcNow,
                    UserGuid = Guid.NewGuid()
                }
            );
        }
    }
}