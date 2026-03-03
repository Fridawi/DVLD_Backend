using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config.Tests
{
    public class TestConfiguration : IEntityTypeConfiguration<Test>
    {
        public void Configure(EntityTypeBuilder<Test> builder)
        {
            builder.HasKey(t => t.TestID);
            builder.Property(t => t.TestResult).HasColumnType("bit").IsRequired();
            builder.Property(t => t.Notes).HasColumnType("nvarchar").HasMaxLength(500).IsRequired(false);
            builder.Property(t => t.CreatedByUserID).IsRequired();

            builder.HasOne(t => t.TestAppointmentInfo) 
                   .WithOne(ta => ta.TestRecord)     
                   .HasForeignKey<Test>(t => t.TestAppointmentID)
                   .OnDelete(DeleteBehavior.Restrict);


            builder.ToTable("Tests");
        }
    }
}
