using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config.Tests
{
    public class TestTypeConfiguration : IEntityTypeConfiguration<TestType>
    {
        public void Configure(EntityTypeBuilder<TestType> builder)
        {
            builder.HasKey(tt => tt.TestTypeID);
            builder.Property(tt => tt.Title).HasColumnType("nvarchar").HasMaxLength(100).IsRequired();
            builder.HasIndex(tt => tt.Title).IsUnique();
            builder.Property(tt => tt.Description).HasColumnType("nvarchar").HasMaxLength(500).IsRequired();
            builder.Property(tt => tt.Fees).HasColumnType("smallmoney").IsRequired();
            builder.ToTable("TestTypes");

            builder.HasData(
            new TestType { TestTypeID = 1, Title = "Vision Test", Description = "Eye vision examination", Fees = 10 },
            new TestType { TestTypeID = 2, Title = "Written Test", Description = "Theoretical driving rules test", Fees = 20 },
            new TestType { TestTypeID = 3, Title = "Street Test", Description = "Practical driving test on the road", Fees = 30 }
            );
        }
    }
}
