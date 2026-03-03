using DVLD.CORE.Entities;
using DVLD.CORE.Interfaces;
using DVLD.INFRASTRUCTURE.Data;

namespace DVLD.INFRASTRUCTURE.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IGenericRepository<Person> People { get; private set; }
        public IGenericRepository<Country> Countries { get; private set; }
        public IGenericRepository<TestType> TestTypes { get; private set; }
        public IGenericRepository<LicenseClass> LicenseClasses { get; private set; }
        public IGenericRepository<ApplicationType> ApplicationTypes { get; private set; }
        public IGenericRepository<User> Users { get; private set; }
        public IApplicationRepository Applications { get; private set; }
        public IGenericRepository<Driver> Drivers { get; private set; }
        public ILocalDrivingLicenseApplicationRepository LocalDrivingLicenseApplications { get; private set; }
        public IGenericRepository<TestAppointment> TestAppointments { get; private set; }
        public IGenericRepository<Test> Tests { get; private set; }
        public IGenericRepository<License> Licenses { get; private set; }
        public IGenericRepository<DetainedLicense> DetainedLicenses { get; private set; }
        public IGenericRepository<InternationalLicense> InternationalLicenses { get; private set; }
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            People = new GenericRepository<Person>(_context);
            Countries = new GenericRepository<Country>(_context);
            TestTypes = new GenericRepository<TestType>(_context);
            LicenseClasses = new GenericRepository<LicenseClass>(_context);
            ApplicationTypes = new GenericRepository<ApplicationType>(_context);
            Users = new GenericRepository<User>(_context);
            Applications = new ApplicationRepository(_context);
            Drivers = new GenericRepository<Driver>(_context);
            LocalDrivingLicenseApplications = new LocalDrivingLicenseApplicationRepository(_context);
            TestAppointments = new GenericRepository<TestAppointment>(_context);
            Tests = new GenericRepository<Test>(_context);
            Licenses = new GenericRepository<License>(_context);
            DetainedLicenses = new GenericRepository<DetainedLicense>(_context);
            InternationalLicenses = new GenericRepository<InternationalLicense>(_context);
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public async Task<IUnitOfWorkTransaction> BeginTransactionAsync()
        {
            var efTransaction = await _context.Database.BeginTransactionAsync();
            return new UnitOfWorkTransaction(efTransaction);
        }

        public void Dispose() => _context.Dispose();
    }
}
