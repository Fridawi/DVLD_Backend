using DVLD.CORE.Constants;
using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.UserID);

            builder.Property(u => u.UserName).HasColumnType("nvarchar").HasMaxLength(20).IsRequired();
            builder.HasIndex(u => u.UserName).IsUnique();

            builder.Property(u => u.Password).HasColumnType("nvarchar").HasMaxLength(100).IsRequired();

            builder.Property(u => u.Role).HasColumnType("nvarchar").HasMaxLength(20).IsRequired();

            builder.Property(u => u.IsActive).IsRequired();

            builder.HasOne(u => u.Person)
                .WithOne()
                .HasForeignKey<User>(u => u.PersonID);

            builder.ToTable("Users");
        }
    }
}
