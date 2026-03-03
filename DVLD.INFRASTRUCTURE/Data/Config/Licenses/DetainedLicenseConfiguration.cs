using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config.Licenses
{
    public class DetainedLicenseConfiguration : IEntityTypeConfiguration<DetainedLicense>
    {
        public void Configure(EntityTypeBuilder<DetainedLicense> builder)
        {
            builder.HasKey(dl => dl.DetainID);
            builder.Property(dl => dl.LicenseID).IsRequired();
            builder.Property(dl => dl.DetainDate).HasColumnType("smalldatetime").IsRequired();
            builder.Property(dl => dl.FineFees).HasColumnType("smallmoney").IsRequired();
            builder.Property(dl => dl.CreatedByUserID).IsRequired();
            builder.Property(dl => dl.IsReleased).IsRequired();
            builder.Property(dl => dl.ReleaseDate).HasColumnType("smalldatetime").IsRequired(false);
            builder.Property(dl => dl.ReleasedByUserID).IsRequired(false);
            builder.Property(dl => dl.ReleaseApplicationID).IsRequired(false);

            builder.HasOne(dl => dl.LicenseInfo)
                .WithMany(l => l.DetainedRecords) 
                .HasForeignKey(dl => dl.LicenseID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(dl => dl.CreatedByUserInfo)
                .WithMany()
                .HasForeignKey(dl => dl.CreatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(dl => dl.ReleasedByUserInfo)
                .WithMany()
                .HasForeignKey(dl => dl.ReleasedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Application>()
                .WithMany()
                .HasForeignKey(dl => dl.ReleaseApplicationID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("DetainedLicenses");
        }
    }
}
