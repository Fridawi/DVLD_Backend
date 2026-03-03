using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.INFRASTRUCTURE.Data.Config
{
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.HasKey(p => p.PersonID);

            builder.Property(p => p.NationalNo).HasColumnType("nvarchar").HasMaxLength(20).IsRequired();
            builder.HasIndex(p => p.NationalNo).IsUnique();

            builder.Property(p => p.FirstName).HasColumnType("nvarchar").HasMaxLength(20).IsRequired();
            builder.Property(p => p.SecondName).HasColumnType("nvarchar").HasMaxLength(20).IsRequired();
            builder.Property(p => p.ThirdName).HasColumnType("nvarchar").HasMaxLength(20).IsRequired(false);
            builder.Property(p => p.LastName).HasColumnType("nvarchar").HasMaxLength(20).IsRequired();

            builder.Ignore(p => p.FullName);

            builder.Property(p => p.DateOfBirth).HasColumnType("datetime").IsRequired();
            builder.Property(p => p.Gendor).HasColumnType("tinyint").IsRequired();
            builder.Property(p => p.Address).HasColumnType("nvarchar").HasMaxLength(500).IsRequired();
            builder.Property(p => p.Phone).HasColumnType("nvarchar").HasMaxLength(20).IsRequired();
            builder.Property(p => p.Email).HasColumnType("nvarchar").HasMaxLength(50).IsRequired(false);
            builder.Property(p => p.ImagePath).HasColumnType("nvarchar").HasMaxLength(250).IsRequired(false);

            builder.Property(p => p.NationalityCountryID).HasColumnType("int").IsRequired();
            builder.HasOne(p => p.CountryInfo)
                .WithMany()
                .HasForeignKey(p => p.NationalityCountryID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("People");
            builder.HasData(LoadPeopleData());
        }
        private static List<Person> LoadPeopleData()
        {
            return new List<Person>
            {
                new Person
                {
                    PersonID = 1,
                    NationalNo = "N2746744",
                    FirstName = "jone",
                    SecondName = "max",
                    ThirdName = "alax",
                    LastName = "", 
                    Gendor = 0, 
                    DateOfBirth = new DateTime(2005, 08, 09),
                    Email = "jone@example.com",
                    Phone = "0791234567",
                    Address = "123 Amman St, Al-Abdali District",
                    NationalityCountryID = 80,
                    ImagePath = null
                },
                new Person
                {
                    PersonID = 2,
                    NationalNo = "N253234",
                    FirstName = "ben",
                    SecondName = "park",
                    ThirdName = "jake",
                    LastName = "",
                    Gendor = 0,
                    DateOfBirth = new DateTime(1998, 08, 09),
                    Email = "ben@example.com",
                    Phone = "0791234567",
                    Address = "123 Amman St, Al-Abdali District",
                    NationalityCountryID = 80,
                    ImagePath = "e0acd4bc-4e9a-4ef2-b345-fb90bd39ba84.avif"
                },
                new Person
                {
                    PersonID =  3,
                    NationalNo = "N2742234",
                    FirstName = "Better",
                    SecondName = "Make",
                    ThirdName = "Maksuel",
                    LastName = "",
                    Gendor = 0,
                    DateOfBirth = new DateTime(2005, 08, 09),
                    Email = "Better@example.com",
                    Phone = "0791234567",
                    Address = "123 Amman St, Al-Abdali District",
                    NationalityCountryID = 80,
                    ImagePath = "ddeb3e90-c473-41ad-bdaf-519fb194cb98.avif"
                },
                new Person
                {
                    PersonID = 4,
                    NationalNo = "N1224437",
                    FirstName = "Alxander",
                    SecondName = "Karter",
                    ThirdName = "max",
                    LastName = "met",
                    Gendor = 0,
                    DateOfBirth = new DateTime(1998, 08, 09),
                    Email = "Alxander@example.com",
                    Phone = "0791234567",
                    Address = "123 Amman St, Al-Abdali District",
                    NationalityCountryID = 80,
                    ImagePath = "425d43ef-7226-459a-8556-0e71bbf9633d.avif"
                },
                new Person
                {
                    PersonID = 5,
                    NationalNo = "N1224227",
                    FirstName = "jonson",
                    SecondName = "wllet",
                    ThirdName = "marker",
                    LastName = "ben",
                    Gendor = 0,
                    DateOfBirth = new DateTime(1998, 08, 09),
                    Email = "jonson@example.com",
                    Phone = "0791234567",
                    Address = "123 Amman St, Al-Abdali District",
                    NationalityCountryID = 80,
                    ImagePath = "c76dd4de-31c9-4b22-b7d5-521212046c51.avif"
                },
                new Person
                {
                    PersonID = 6,
                    NationalNo = "N2586744",
                    FirstName = "Adim",
                    SecondName = "Elson",
                    ThirdName = "max",
                    LastName = "",
                    Gendor = 0,
                    DateOfBirth = new DateTime(2005, 08, 09),
                    Email = "Adim@example.com",
                    Phone = "0791234567",
                    Address = "123 Amman St, Al-Abdali District",
                    NationalityCountryID = 80,
                    ImagePath = null
                },
                new Person
                {
                    PersonID = 7,
                    NationalNo = "N274264",
                    FirstName = "Rafile",
                    SecondName = "gurge",
                    ThirdName = "carter",
                    LastName = "",
                    Gendor = 0,
                    DateOfBirth = new DateTime(2005, 08, 09),
                    Email = "Rafile@example.com",
                    Phone = "0791234567",
                    Address = "123 Amman St, Al-Abdali District",
                    NationalityCountryID = 80,
                    ImagePath = null
                }
            };
        }
    }
}

