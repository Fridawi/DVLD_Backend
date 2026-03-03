using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config.Applications
{
    public class ApplicationTypeConfiguration : IEntityTypeConfiguration<ApplicationType>
    {
        public void Configure(EntityTypeBuilder<ApplicationType> builder)
        {
            builder.HasKey(at => at.ApplicationTypeID);
            builder.Property(at => at.Title).HasColumnType("nvarchar").HasMaxLength(150).IsRequired();
            builder.HasIndex(at => at.Title).IsUnique();
            builder.Property(at => at.Fees).HasColumnType("smallmoney").IsRequired();
            builder.ToTable("ApplicationTypes");

            builder.HasData(
                new ApplicationType { ApplicationTypeID = 1, Title = "New Local Driving License Service", Fees = 15 },
                new ApplicationType { ApplicationTypeID = 2, Title = "Renew Driving License Service", Fees = 7 },
                new ApplicationType { ApplicationTypeID = 3, Title = "Replacement for a Lost Driving License", Fees = 10 },
                new ApplicationType { ApplicationTypeID = 4, Title = "Replacement for a Damaged Driving License", Fees = 5 },
                new ApplicationType { ApplicationTypeID = 5, Title = "Release Detained Driving Licsense", Fees = 15 },
                new ApplicationType { ApplicationTypeID = 6, Title = "New International License", Fees = 51 },
                new ApplicationType { ApplicationTypeID = 7, Title = "Retake Test", Fees = 5 }
            );
        }
    }
}
