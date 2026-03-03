using DVLD.CORE.Entities;
using Microsoft.EntityFrameworkCore;

namespace DVLD.INFRASTRUCTURE.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<TestType> TestTypes { get; set; }
        public DbSet<ApplicationType> ApplicationTypes { get; set; }
        public DbSet<LicenseClass> LicenseClasses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<LocalDrivingLicenseApplication> LocalDrivingLicenseApplications { get; set; }
        public DbSet<TestAppointment> TestAppointments { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<License> Licenses { get; set; }
        public DbSet<DetainedLicense> DetainedLicenses { get; set; }
        public DbSet<InternationalLicense> InternationalLicenses { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
