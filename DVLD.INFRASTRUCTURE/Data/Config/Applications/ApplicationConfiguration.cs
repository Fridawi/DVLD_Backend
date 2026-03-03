using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config.Applications
{
    public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
    {
        public void Configure(EntityTypeBuilder<Application> builder)
        {
            builder.HasKey(a => a.ApplicationID);

            builder.Property(a => a.ApplicantPersonID).IsRequired();

            builder.Ignore(a => a.ApplicantFullName);

            builder.Property(a=>a.ApplicationDate).HasColumnType("datetime").IsRequired();

            builder.Property(a => a.ApplicationTypeID).IsRequired();

            builder.Property(a => a.ApplicationStatus).HasColumnType("tinyint").IsRequired();

            builder.Ignore(a => a.StatusText);

            builder.Property(a => a.LastStatusDate).HasColumnType("datetime").IsRequired();

            builder.Property(a=>a.PaidFees).HasColumnType("smallmoney").IsRequired();

            builder.Property(a => a.CreatedByUserID).IsRequired();

            builder.HasOne(a=>a.PersonInfo)
                .WithMany()
                .HasForeignKey(a=>a.ApplicantPersonID);

            builder.HasOne(a => a.ApplicationTypeInfo)
                .WithMany()
                .HasForeignKey(a => a.ApplicationTypeID);

            builder.HasOne(a => a.CreatedByUserInfo)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.ToTable("Applications");

        }        
    }
}
