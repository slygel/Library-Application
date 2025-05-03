using LibraryAPI.Entities;
using LibraryAPI.Enums;
using LibraryAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryAPI.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(100);
        builder.Property(u => u.PhoneNumber).HasMaxLength(100);
        builder.Property(u => u.Address).HasMaxLength(200);
        builder.Property(u => u.Username).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Password).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Role).IsRequired();
        
        builder.HasData(
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Tai Tue",
                Email = "nttue03@gmail.com",
                PhoneNumber = "0383291503",
                Address = "Ba Vi, Ha Noi",
                Username = "admin",
                Password = PasswordHashHandler.HashPassword("admin"),
                Role = Role.Admin
            }
        );
    }
}