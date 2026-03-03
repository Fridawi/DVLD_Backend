using DVLD.CORE.Entities;
namespace DVLD.CORE.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Person> People { get; }
        IGenericRepository<Country> Countries { get; }
        IGenericRepository<TestType> TestTypes { get; }
        IGenericRepository<LicenseClass> LicenseClasses { get; }
        IGenericRepository<ApplicationType> ApplicationTypes { get; }
        IGenericRepository<User> Users { get; }
        IApplicationRepository Applications { get; }
        IGenericRepository<Driver> Drivers { get; }
        ILocalDrivingLicenseApplicationRepository LocalDrivingLicenseApplications { get; }
        IGenericRepository<TestAppointment> TestAppointments { get; }
        IGenericRepository<Test> Tests { get; }
        IGenericRepository<License> Licenses { get; }
        IGenericRepository<DetainedLicense> DetainedLicenses { get; }
        IGenericRepository<InternationalLicense> InternationalLicenses { get; } 

        Task<int> CompleteAsync();
        Task<IUnitOfWorkTransaction> BeginTransactionAsync();
    }
}
