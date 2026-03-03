using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config
{
    public class LocalDrivingLicenseApplicationConfiguration : IEntityTypeConfiguration<LocalDrivingLicenseApplication>
    {
        public void Configure(EntityTypeBuilder<LocalDrivingLicenseApplication> builder)
        {
            builder.HasKey(ldla => ldla.LocalDrivingLicenseApplicationID);

            builder.HasOne(ldla => ldla.ApplicationInfo)
                   .WithOne()
                   .HasForeignKey<LocalDrivingLicenseApplication>(d => d.ApplicationID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ldla => ldla.LicenseClassInfo)
                   .WithMany()
                   .HasForeignKey(ldla => ldla.LicenseClassID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("LocalDrivingLicenseApplications");
        }
    }
}
