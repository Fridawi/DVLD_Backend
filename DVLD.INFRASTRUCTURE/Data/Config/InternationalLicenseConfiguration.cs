using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config
{
    public class InternationalLicenseConfiguration : IEntityTypeConfiguration<InternationalLicense>
    {
        public void Configure(EntityTypeBuilder<InternationalLicense> builder)
        {
            builder.HasKey(il => il.InternationalLicenseID);

            builder.Property(il => il.IssueDate).HasColumnType("smalldatetime").IsRequired();

            builder.Property(il => il.ExpirationDate).HasColumnType("smalldatetime").IsRequired();

            builder.Property(il => il.IsActive).HasDefaultValue(true).IsRequired();


            builder.HasOne(il => il.ApplicationInfo)
                .WithMany() 
                .HasForeignKey(il => il.ApplicationID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(il => il.DriverInfo)
                .WithMany()
                .HasForeignKey(il => il.DriverID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(il => il.LocalLicenseInfo)
                .WithMany()
                .HasForeignKey(il => il.IssuedUsingLocalLicenseID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(il => il.CreatedByUserInfo)
                .WithMany()
                .HasForeignKey(il => il.CreatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);


            builder.ToTable("InternationalLicenses");
        }
    }
}
