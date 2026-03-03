using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config.Licenses
{
    public class LicenseConfiguration : IEntityTypeConfiguration<License>
    {
        public void Configure(EntityTypeBuilder<License> builder)
        {
            builder.HasKey(l => l.LicenseID);
            builder.Property(l => l.ApplicationID).IsRequired();
            builder.HasIndex(l => l.ApplicationID).IsUnique();
            builder.Property(l => l.DriverID).IsRequired();
            builder.Property(l => l.LicenseClassID).IsRequired();
            builder.Property(l => l.IssueDate).HasColumnType("datetime").IsRequired();
            builder.Property(l => l.ExpirationDate).HasColumnType("datetime").IsRequired();
            builder.Property(l => l.Notes).HasMaxLength(500).IsRequired(false);
            builder.Property(l => l.PaidFees).HasColumnType("smallmoney").IsRequired();
            builder.Property(l => l.IsActive).IsRequired();
            builder.Property(l => l.IssueReason).HasColumnType("tinyint").IsRequired();
            builder.Property(l => l.CreatedByUserID).IsRequired();

            builder.HasOne(l => l.ApplicationInfo)
                   .WithOne()
                   .HasForeignKey<License>(l => l.ApplicationID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.DriverInfo)
                     .WithMany()
                     .HasForeignKey(l => l.DriverID)
                     .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.LicenseClassInfo)
                        .WithMany()
                        .HasForeignKey(l => l.LicenseClassID)
                        .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(l => l.DetainedRecords)
                   .WithOne(d => d.LicenseInfo)
                   .HasForeignKey(d => d.LicenseID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Licenses");    
        }
    }
}
