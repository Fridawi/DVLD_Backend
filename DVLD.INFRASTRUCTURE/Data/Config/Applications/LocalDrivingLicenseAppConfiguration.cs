using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config.Applications
{
    public class LocalDrivingLicenseAppConfiguration : IEntityTypeConfiguration<LocalDrivingLicenseApplication>
    {
        public void Configure(EntityTypeBuilder<LocalDrivingLicenseApplication> builder)
        {
            builder.HasKey(la => la.LocalDrivingLicenseApplicationID);
            builder.Property(la => la.ApplicationID).IsRequired();
            builder.Property(la => la.LicenseClassID).IsRequired();
            builder.HasOne(la => la.ApplicationInfo)
                   .WithOne() 
                   .HasForeignKey<LocalDrivingLicenseApplication>(la => la.ApplicationID)
                   .OnDelete(DeleteBehavior.Cascade); 

            builder.HasOne(la => la.LicenseClassInfo)
                   .WithMany()
                   .HasForeignKey(la => la.LicenseClassID)
                   .OnDelete(DeleteBehavior.Restrict);  
            
            builder.ToTable("LocalDrivingLicenseApplications");
        }
    }
}
