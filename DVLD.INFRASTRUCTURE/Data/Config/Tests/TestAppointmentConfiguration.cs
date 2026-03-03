using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace DVLD.INFRASTRUCTURE.Data.Config.Tests
{
    public class TestAppointmentConfiguration : IEntityTypeConfiguration<TestAppointment>
    {
        public void Configure(EntityTypeBuilder<TestAppointment> builder)
        {
            builder.HasKey(ta => ta.TestAppointmentID);
            builder.Property(ta => ta.TestTypeID).HasConversion<int>().IsRequired();
            builder.Property(ta => ta.LocalDrivingLicenseApplicationID).IsRequired();
            builder.Property(ta => ta.AppointmentDate).HasColumnType("smalldatetime").IsRequired();
            builder.Property(ta => ta.PaidFees).HasColumnType("smallmoney").IsRequired();
            builder.Property(ta => ta.CreatedByUserID).IsRequired();
            builder.Property(ta => ta.IsLocked).HasDefaultValue(false).IsRequired();
            builder.Property(ta => ta.RetakeTestApplicationID).IsRequired(false);

            builder.HasOne(ta => ta.RetakeTestAppInfo)
                   .WithMany()
                   .HasForeignKey(ta => ta.RetakeTestApplicationID)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ta => ta.LocalAppInfo)
                   .WithMany() 
                   .HasForeignKey(ta => ta.LocalDrivingLicenseApplicationID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(ta => ta.CreatedByUserID)
                   .OnDelete(DeleteBehavior.Restrict);


            builder.ToTable("TestAppointments");
        }
    }
}
