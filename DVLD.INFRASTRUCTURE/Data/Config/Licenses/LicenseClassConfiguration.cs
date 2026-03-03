using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config.Licenses
{
    public class LicenseClassConfiguration : IEntityTypeConfiguration<LicenseClass>
    {
        public void Configure(EntityTypeBuilder<LicenseClass> builder)
        {
            builder.HasKey(lc=>lc.LicenseClassID);
            builder.Property(lc => lc.ClassName).HasColumnType("nvarchar").HasMaxLength(50).IsRequired();
            builder.HasIndex(lc => lc.ClassName).IsUnique();
            builder.Property(lc => lc.ClassDescription).HasColumnType("nvarchar").HasMaxLength(500).IsRequired();
            builder.Property(lc => lc.MinimumAllowedAge).HasColumnType("tinyint").IsRequired();
            builder.Property(lc => lc.DefaultValidityLength).HasColumnType("tinyint").IsRequired();
            builder.Property(lc => lc.ClassFees).HasColumnType("smallmoney").IsRequired();
            builder.ToTable("LicenseClasses");

            builder.HasData(LoadLicenseClassesData());
        }

        private List<LicenseClass> LoadLicenseClassesData()
        {
            return new List<LicenseClass>
            {
                new LicenseClass
                {
                    LicenseClassID = 1,
                    ClassName = "Class 1 - Small Motorcycle",
                    ClassDescription = "Small motorcycles with engine capacity less than 125cc.",
                    MinimumAllowedAge = 16,
                    DefaultValidityLength = 5,
                    ClassFees = 15
                },
                new LicenseClass
                {
                    LicenseClassID = 2,
                    ClassName = "Class 2 - Heavy Motorcycle",
                    ClassDescription = "Motorcycles with engine capacity more than 125cc.",
                    MinimumAllowedAge = 18,
                    DefaultValidityLength = 5,
                    ClassFees = 30
                },
                new LicenseClass
                {
                    LicenseClassID = 3,
                    ClassName = "Class 3 - Ordinary driving license",
                    ClassDescription = "Standard cars and small pickups. (Most Common)",
                    MinimumAllowedAge = 18,
                    DefaultValidityLength = 10,
                    ClassFees = 20
                },
                new LicenseClass
                {
                    LicenseClassID = 4,
                    ClassName = "Class 4 - Commercial",
                    ClassDescription = "Vehicles used for commercial purposes like Taxis and small buses.",
                    MinimumAllowedAge = 21,
                    DefaultValidityLength = 10,
                    ClassFees = 200
                },
                new LicenseClass
                {
                    LicenseClassID = 5,
                    ClassName = "Class 5 - Agricultural",
                    ClassDescription = "Agricultural tractors and specialized machinery.",
                    MinimumAllowedAge = 18,
                    DefaultValidityLength = 10,
                    ClassFees = 50
                },
                new LicenseClass
                {
                    LicenseClassID = 6,
                    ClassName = "Class 6 - Small and Medium Truck",
                    ClassDescription = "Trucks with total weight between 3.5 and 7.5 tons.",
                    MinimumAllowedAge = 21,
                    DefaultValidityLength = 10,
                    ClassFees = 250
                },
                new LicenseClass
                {
                    LicenseClassID = 7,
                    ClassName = "Class 7 - Heavy Truck",
                    ClassDescription = "Large trucks and trailers with weight exceeding 7.5 tons.",
                    MinimumAllowedAge = 21,
                    DefaultValidityLength = 10,
                    ClassFees = 300
                }
            };
        }
    }
}
