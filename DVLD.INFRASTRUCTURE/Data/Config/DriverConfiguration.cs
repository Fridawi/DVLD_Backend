using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.HasKey(d => d.DriverID);

            builder.Property(d => d.PersonID).IsRequired();

            builder.HasIndex(d => d.PersonID).IsUnique();

            builder.Property(d => d.CreatedByUserID).IsRequired();

            builder.Property(d => d.CreatedDate).HasColumnType("smalldatetime").IsRequired();

            builder.HasOne(d => d.PersonInfo)
                   .WithOne()
                   .HasForeignKey<Driver>(d => d.PersonID)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.ToTable("Drivers");

            builder.HasData(LoadDriversData());
        }

        private List<Driver> LoadDriversData()
        {
            return new List<Driver>
            {
                new Driver
                {
                    DriverID = 1,
                    PersonID = 1,
                    CreatedByUserID = 2,
                    CreatedDate = new DateTime(2026, 2, 1, 10, 30, 0)
                },

                new Driver
                {
                    DriverID = 2,
                    PersonID = 2,
                    CreatedByUserID = 3,
                    CreatedDate = new DateTime(2026, 2, 1, 10, 30, 0)
                },
            };
        }
    }
}
