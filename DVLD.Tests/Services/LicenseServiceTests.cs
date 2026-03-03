using AutoMapper;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.CORE.Interfaces.Tests;
using DVLD.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace DVLD.Tests.Services
{
    public class LicenseServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<License>> _licenseRepoMock;
        private readonly Mock<IGenericRepository<ApplicationType>> _appTypeRepoMock;
        private readonly Mock<IGenericRepository<Driver>> _driverRepoMock;
        private readonly Mock<IApplicationRepository> _applicationRepoMock;
        private readonly Mock<ITestService> _testServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IUnitOfWorkTransaction> _dbContextTransactionMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<LicenseService>> _loggerMock;
        private readonly LicenseService _service;


        public LicenseServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _licenseRepoMock = new Mock<IGenericRepository<License>>();
            _appTypeRepoMock = new Mock<IGenericRepository<ApplicationType>>();
            _driverRepoMock = new Mock<IGenericRepository<Driver>>();
            _testServiceMock = new Mock<ITestService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _dbContextTransactionMock = new Mock<IUnitOfWorkTransaction>();
            _applicationRepoMock = new Mock<IApplicationRepository>();
            _unitOfWorkMock.Setup(u => u.ApplicationTypes).Returns(_appTypeRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Drivers).Returns(_driverRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Applications).Returns(_applicationRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_dbContextTransactionMock.Object);
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<LicenseService>>();
            _unitOfWorkMock.Setup(u => u.Licenses).Returns(_licenseRepoMock.Object);
            _service = new LicenseService(
                _unitOfWorkMock.Object,
                _testServiceMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _httpContextAccessorMock.Object
            );
            SetupMockHttpContext();
        }
        private void SetupMockHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("localhost", 7001);
            _httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(context);
        }


        #region Data 
        private List<License> GetFakeLicenses()
        {
            var person = new Person
            {
                PersonID = 50,
                FirstName = "Mohamed",
                SecondName = "Ali",
                ThirdName = "Ahmed",
                LastName = "Mansour",
                NationalNo = "N123456",
                DateOfBirth = new DateTime(1995, 5, 15),
                Gendor = 0,
                Address = "Main St, Cairo",
                Phone = "0123456789",
                ImagePath = "profile_pic.jpg"
            };

            var licenseClass = new LicenseClass
            {
                LicenseClassID = 3,
                ClassName = "Class 3 - Ordinary Driving License",
                ClassDescription = "Driving small cars",
                MinimumAllowedAge = 18,
                DefaultValidityLength = 10,
                ClassFees = 20.0f
            };

            return new List<License>
            {
                new License
                {
                    LicenseID = 1,
                    ApplicationID = 10,
                    DriverID = 100,
                    LicenseClassID = 3,
                    IssueDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddYears(licenseClass.DefaultValidityLength),
                    IsActive = true,
                    PaidFees = licenseClass.ClassFees,
                    IssueReason = EnIssueReason.FirstTime,
                    CreatedByUserID = 1,
                    LicenseClassInfo = licenseClass,
                    ApplicationInfo = new Application
                    {
                        ApplicationID = 10,
                        ApplicantPersonID = 50,
                        PersonInfo = person,
                        ApplicationStatus = EnApplicationStatus.Completed,
                        ApplicationDate = DateTime.UtcNow.AddDays(-1)
                    }
                }
            };
        }

        private List<LicenseDto> GetFakeLicenseDtos()
        {
            return new List<LicenseDto>
            {
                new LicenseDto
                {
                    LicenseID = 1,
                    ApplicationID = 10,
                    DriverID = 100,
                    LicenseClassName = "Class 3 - Ordinary Driving License",
                    IssueDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddYears(10),
                    IsActive = true,
                    PaidFees = 20.0f,
                    IssueReasonText = "First Time",
                    CreatedByUserID = 1
                }
            };
        }

        private List<DriverLicenseDto> GetFakeDriverLicenseDtos()
        {
            return new List<DriverLicenseDto>
            {
                new DriverLicenseDto
                {
                    LicenseID = 1,
                    ApplicationID = 10,
                    DriverID = 100,
                    LicenseClassName = "Class 3 - Ordinary Driving License",
                    DriverFullName = "Mohamed Ali Ahmed Mansour", // تم دمج الأسماء بناءً على الـ Logic الخاص بك
                    NationalNo = "N123456",
                    Gender = 0,
                    GenderText = "Male",
                    IssueDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddYears(10),
                    IsActive = true,
                    IssueReasonText = "First Time",
                    DriverBirthDate = new DateOnly(1995, 5, 15),
                    DriverImageUrl = "http://localhost/uploads/people/profile_pic.jpg",
                    CreatedByUserID = 1
                }
            };
        }

        private List<LicenseCreateDto> GetFakeLicenseCreateDtos()
        {
            return new List<LicenseCreateDto>
            {
                new LicenseCreateDto
                {
                    LocalDrivingLicenseApplicationID = 10,
                    IssueReason = EnIssueReason.FirstTime,
                    Notes = "Issuing first time license after passing tests."
                }
            };
        }

        #endregion


        #region GetAllLicensesAsync

        [Fact]
        public async Task GetAllLicensesAsync_ReturnsPagedResult_WhenDataExists()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            var fakeLicenses = GetFakeLicenses(); 
            var fakeLicenseDtos = GetFakeLicenseDtos();
            int totalCount = fakeLicenses.Count();

            _unitOfWorkMock.Setup(u => u.Licenses.FindAllAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<License, object>>>(),
                It.IsAny<EnOrderByDirection>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(fakeLicenses);

            _unitOfWorkMock.Setup(u => u.Licenses.CountAsync(It.IsAny<Expression<Func<License, bool>>>()))
                .ReturnsAsync(totalCount);

            _mapperMock.Setup(m => m.Map<IEnumerable<LicenseDto>>(It.IsAny<IEnumerable<License>>()))
                .Returns(fakeLicenseDtos);

            // Act
            var result = await _service.GetAllLicensesAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResultDto<LicenseDto>>(result);
            Assert.Equal(totalCount, result.TotalCount);
            Assert.Equal(pageNumber, result.PageNumber);
            Assert.Equal(fakeLicenseDtos.Count(), result.Data.Count());

            Assert.Equal(fakeLicenseDtos.First().LicenseID, result.Data.First().LicenseID);
        }

        #endregion

        #region GetLicensesByDriverIdAsync

        [Fact]
        public async Task GetLicensesByDriverIdAsync_ReturnsPagedResult_WhenDriverIdIsValid()
        {
            // Arrange
            int driverId = 100;
            int pageNumber = 1;
            int pageSize = 10;
            var fakeLicenses = GetFakeLicenses();
            var fakeDriverLicenseDtos = GetFakeDriverLicenseDtos();
            int totalCount = fakeLicenses.Count();

            _unitOfWorkMock.Setup(u => u.Licenses.FindAllAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<License, object>>>(),
                It.IsAny<EnOrderByDirection>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()
            )).ReturnsAsync(fakeLicenses);

            _unitOfWorkMock.Setup(u => u.Licenses.CountAsync(It.IsAny<Expression<Func<License, bool>>>()))
                .ReturnsAsync(totalCount);

            _mapperMock.Setup(m => m.Map<IEnumerable<DriverLicenseDto>>(
                It.IsAny<IEnumerable<License>>(),
                It.IsAny<Action<IMappingOperationOptions<object, IEnumerable<DriverLicenseDto>>>>()))
                .Returns(fakeDriverLicenseDtos);

            // Act
            var result = await _service.GetLicensesByDriverIdAsync(driverId, pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResultDto<DriverLicenseDto>>(result); 
            Assert.Equal(totalCount, result.TotalCount);
            Assert.Equal(fakeDriverLicenseDtos.Count(), result.Data.Count());
            Assert.Equal(fakeDriverLicenseDtos.First().LicenseID, result.Data.First().LicenseID);
            Assert.Equal(driverId, result.Data.First().DriverID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetLicensesByDriverIdAsync_ThrowsValidationException_WhenDriverIdIsInvalid(int invalidDriverId)
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.GetLicensesByDriverIdAsync(invalidDriverId, 1, 10));
        }

        [Fact]
        public async Task GetLicensesByDriverIdAsync_ReturnsEmptyPagedResult_WhenNoLicensesFound()
        {
            // Arrange
            int driverId = 999;
            int pageNumber = 1;
            int pageSize = 10;

            _unitOfWorkMock.Setup(u => u.Licenses.FindAllAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<License, object>>>(),
                It.IsAny<EnOrderByDirection>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()
            )).ReturnsAsync(new List<License>());

            _unitOfWorkMock.Setup(u => u.Licenses.CountAsync(It.IsAny<Expression<Func<License, bool>>>()))
                .ReturnsAsync(0);

            _mapperMock.Setup(m => m.Map<IEnumerable<DriverLicenseDto>>(
                It.IsAny<IEnumerable<License>>(),
                It.IsAny<Action<IMappingOperationOptions<object, IEnumerable<DriverLicenseDto>>>>()))
                .Returns(new List<DriverLicenseDto>());

            // Act
            var result = await _service.GetLicensesByDriverIdAsync(driverId, pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data); 
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(pageNumber, result.PageNumber);
        }

        #endregion

        #region GetDriverLicensesByIdAsync
        [Fact]
        public async Task GetDriverLicensesByIdAsync_ReturnsDriverLicenseDto_WhenLicenseExists()
        {
            // Arrange
            int licenseId = 1;
            var fakeLicenses = GetFakeLicenses()[0];
            var fakeDriverLicenseDtos = GetFakeDriverLicenseDtos()[0];

            _licenseRepoMock.Setup(repo => repo.FindAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()
            )).ReturnsAsync(fakeLicenses);

            _mapperMock.Setup(m => m.Map<DriverLicenseDto>(
                fakeLicenses,
                It.IsAny<Action<IMappingOperationOptions<object, DriverLicenseDto>>>()))
                .Returns(fakeDriverLicenseDtos);

            // Act
            var result = await _service.GetDriverLicensesByIdAsync(licenseId);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(fakeDriverLicenseDtos.LicenseID, result.LicenseID);
            Assert.Equal(fakeDriverLicenseDtos.DriverID, result.DriverID);
        }

        [Fact]
        public async Task GetDriverLicensesByIdAsync_ReturnsNull_WhenLicenseNotFound()
        {
            // Arrange
            int licenseId = 999;
            _licenseRepoMock.Setup(repo => repo.FindAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()
            )).ReturnsAsync((License)null!);

            // Act & Assert
            var result = await _service.GetDriverLicensesByIdAsync(licenseId);
            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task GetDriverLicensesByIdAsync_ThrowsValidationException_WhenLicenseIdIsInvalid(int invalidLicenseId)
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.GetDriverLicensesByIdAsync(invalidLicenseId));
        }
        #endregion

        #region GetActiveLicenseByPersonIDAndLicenseClassID
        [Fact]
        public async Task GetActiveLicenseByPersonIDAndLicenseClassID_ReturnsLicenseDto_WhenLicenseExists()
        {
            // Arrange
            int personID = 50;
            int licenseClassID = 3;
            var fakeLicenses = GetFakeLicenses()[0];
            var fakeLicenseDtos = GetFakeLicenseDtos()[0];

            _licenseRepoMock.Setup(repo => repo.FindAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>()
            )).ReturnsAsync(fakeLicenses);

            _mapperMock.Setup(m => m.Map<LicenseDto>(fakeLicenses)).Returns(fakeLicenseDtos);

            // Act
            var result = await _service.GetActiveLicenseByPersonIDAndLicenseClassID(personID, licenseClassID);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fakeLicenseDtos.LicenseID, result.LicenseID);
            Assert.Equal(fakeLicenseDtos.DriverID, result.DriverID);
        }

        [Theory]
        [InlineData(0, 3)]
        [InlineData(50, -1)]
        public async Task GetActiveLicenseByPersonIDAndLicenseClassID_ThrowsValidationException_WhenInputsAreInvalid(int personID, int licenseClassID)
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.GetActiveLicenseByPersonIDAndLicenseClassID(personID, licenseClassID));
        }

        [Fact]
        public async Task GetActiveLicenseByPersonIDAndLicenseClassID_ReturnsNull_WhenLicenseNotFound()
        {
            // Arrange
            int personID = 50;
            int licenseClassID = 3;

            _licenseRepoMock.Setup(repo => repo.FindAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>()
            )).ReturnsAsync((License)null!);

            // Act
            var result = await _service.GetActiveLicenseByPersonIDAndLicenseClassID(personID, licenseClassID);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetLicenseByIdAsync
        [Fact]
        public async Task GetLicenseByIdAsync_ReturnsLicenseDto_WhenLicenseExists()
        {
            // Arrange
            int licenseId = 1;
            var fakeLicenses = GetFakeLicenses()[0];
            var fakeLicenseDtos = GetFakeLicenseDtos()[0];

            _licenseRepoMock.Setup(repo => repo.FindAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>()
            )).ReturnsAsync(fakeLicenses);

            _mapperMock.Setup(m => m.Map<LicenseDto>(fakeLicenses)).Returns(fakeLicenseDtos);
            // Act
            var result = await _service.GetLicenseByIdAsync(licenseId);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(fakeLicenseDtos.LicenseID, result.LicenseID);
            Assert.Equal(fakeLicenseDtos.DriverID, result.DriverID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task GetLicenseByIdAsync_ThrowsValidationException_WhenLicenseIdIsInvalid(int invalidLicenseId)
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.GetLicenseByIdAsync(invalidLicenseId));
        }

        [Fact]
        public async Task GetLicenseByIdAsync_ReturnsNull_WhenLicenseNotFound()
        {
            // Arrange
            int licenseId = 999;
            _licenseRepoMock.Setup(repo => repo.FindAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>()
            )).ReturnsAsync((License)null!);
            // Act
            var result = await _service.GetLicenseByIdAsync(licenseId);
            // Assert
            Assert.Null(result);
        }

        #endregion

        #region DeactivateLicense 
        [Fact]
        public async Task DeactivateLicense_SetsIsActiveToFalse_WhenLicenseExists()
        {
            // Arrange
            int licenseId = 1;
            var fakeLicense = GetFakeLicenses()[0];

            _licenseRepoMock.Setup(repo => repo.GetByIdAsync(licenseId)).ReturnsAsync(fakeLicense);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _service.DeactivateLicense(licenseId);

            // Assert
            Assert.False(fakeLicense.IsActive);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeactivateLicense_ReturnsFalse_WhenCompleteAsyncFails()
        {
            // Arrange
            int licenseId = 1;
            var fakeLicense = GetFakeLicenses()[0];

            _licenseRepoMock.Setup(repo => repo.GetByIdAsync(licenseId)).ReturnsAsync(fakeLicense);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);
            // Act & Assert
            var result = await _service.DeactivateLicense(licenseId);
            Assert.False(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task DeactivateLicense_ThrowsValidationException_WhenLicenseIdIsInvalid(int invalidLicenseId)
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.DeactivateLicense(invalidLicenseId));
        }

        [Fact]
        public async Task DeactivateLicense_ThrowsResourceNotFoundException_WhenLicenseNotFound()
        {
            // Arrange
            int licenseId = 999;
            _licenseRepoMock.Setup(repo => repo.GetByIdAsync(licenseId)).ReturnsAsync((License)null!);
            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.DeactivateLicense(licenseId));
        }

        [Fact]
        public async Task DeactivateLicense_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            int licenseId = 1;
            var fakeLicense = GetFakeLicenses()[0];

            _licenseRepoMock.Setup(repo => repo.GetByIdAsync(licenseId)).ReturnsAsync(fakeLicense);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.DeactivateLicense(licenseId));

        }
        #endregion

        #region IssueFirstTimeLicenseAsync
        [Fact]
        public async Task IssueFirstTimeLicenseAsync_ReturnsLicenseDto_WhenSavedSuccessfully()
        {
            // Arrange
            var userId = 1;
            var createDto = GetFakeLicenseCreateDtos().First();
            var fakeLicense = GetFakeLicenses().First();
            var fakeLicenseDto = GetFakeLicenseDtos().First();
            var localApp = new LocalDrivingLicenseApplication
            {
                ApplicationID = 10,
                LicenseClassID = 3,
                LicenseClassInfo = fakeLicense.LicenseClassInfo,
                ApplicationInfo = fakeLicense.ApplicationInfo
            };

            _unitOfWorkMock.Setup(u => u.LocalDrivingLicenseApplications.FindAsync(It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(localApp);

            _testServiceMock.Setup(t => t.PassedAllTestsAsync(createDto.LocalDrivingLicenseApplicationID))
                .ReturnsAsync(true);

            _unitOfWorkMock.Setup(u => u.Drivers.FindAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                .ReturnsAsync(new Driver { DriverID = 100, PersonID = 50 });

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<LicenseDto>(It.IsAny<License>())).Returns(fakeLicenseDto);

            // Act
            var result = await _service.IssueFirstTimeLicenseAsync(createDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fakeLicenseDto.LicenseID, result.LicenseID);
            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task IssueFirstTimeLicenseAsync_ThrowsResourceNotFoundException_WhenLocalAppDoesNotExist()
        {
            // Arrange
            var createDto = GetFakeLicenseCreateDtos().First();
            _unitOfWorkMock.Setup(u => u.LocalDrivingLicenseApplications.FindAsync(It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync((LocalDrivingLicenseApplication)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.IssueFirstTimeLicenseAsync(createDto, 1));
        }

        [Fact]
        public async Task IssueFirstTimeLicenseAsync_ThrowsValidationException_WhenPersonHasNotPassedAllTests()
        {
            // Arrange
            var createDto = GetFakeLicenseCreateDtos().First();
            _unitOfWorkMock.Setup(u => u.LocalDrivingLicenseApplications.FindAsync(It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(new LocalDrivingLicenseApplication());

            _testServiceMock.Setup(t => t.PassedAllTestsAsync(createDto.LocalDrivingLicenseApplicationID)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.IssueFirstTimeLicenseAsync(createDto, 1));
        }

        [Fact]
        public async Task IssueFirstTimeLicenseAsync_ThrowsException_AndRollbacks_WhenDatabaseErrorOccursDuringSave()
        {
            // Arrange
            var createDto = GetFakeLicenseCreateDtos().First();
            var fakeLicense = GetFakeLicenses().First();
            var localApp = new LocalDrivingLicenseApplication
            {
                ApplicationID = 10,
                LicenseClassID = 3,
                LicenseClassInfo = fakeLicense.LicenseClassInfo,
                ApplicationInfo = fakeLicense.ApplicationInfo
            };

            _unitOfWorkMock.Setup(u => u.LocalDrivingLicenseApplications.FindAsync(It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(localApp);

            _testServiceMock.Setup(t => t.PassedAllTestsAsync(It.IsAny<int>())).ReturnsAsync(true);

            _unitOfWorkMock.Setup(u => u.Drivers.FindAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                .ReturnsAsync(new Driver { DriverID = 100 });

            _unitOfWorkMock.Setup(u => u.CompleteAsync())
                .ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.IssueFirstTimeLicenseAsync(createDto, 1));

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task IssueFirstTimeLicenseAsync_ShouldCreateNewDriver_WhenPersonIsNotAlreadyADriver()
        {
            // Arrange
            var createDto = new LicenseCreateDto { LocalDrivingLicenseApplicationID = 1, Notes = "First Time" };

            var localApp = new LocalDrivingLicenseApplication
            {
                LocalDrivingLicenseApplicationID = 1,
                ApplicationID = 100,
                LicenseClassID = 3,
                ApplicationInfo = new Application { ApplicantPersonID = 50, ApplicationID = 100 },
                LicenseClassInfo = new LicenseClass { DefaultValidityLength = 10, ClassFees = 20 }
            };

            _unitOfWorkMock.Setup(u => u.LocalDrivingLicenseApplications.FindAsync(
                It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(localApp);

            _testServiceMock.Setup(t => t.PassedAllTestsAsync(It.IsAny<int>())).ReturnsAsync(true);

            _unitOfWorkMock.Setup(u => u.Drivers.FindAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                .ReturnsAsync((Driver)null!);

            _mapperMock.Setup(m => m.Map<LicenseDto>(It.IsAny<License>()))
                       .Returns(new LicenseDto { LicenseID = 1 });

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.IssueFirstTimeLicenseAsync(createDto, 1);

            // Assert
            _unitOfWorkMock.Verify(u => u.Drivers.AddAsync(It.IsAny<Driver>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Licenses.AddAsync(It.IsAny<License>()), Times.Once);

            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Once);
            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Never);

            Assert.NotNull(result);
        }
        [Fact]
        public async Task IssueFirstTimeLicenseAsync_ReturnsNull_WhenFinalSaveFails()
        {
            // Arrange
            var createDto = GetFakeLicenseCreateDtos().First();
            var localApp = new LocalDrivingLicenseApplication
            {
                LicenseClassInfo = new LicenseClass { DefaultValidityLength = 10 },
                ApplicationInfo = new Application { ApplicantPersonID = 50 }
            };

            _unitOfWorkMock.Setup(u => u.LocalDrivingLicenseApplications.FindAsync(It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(localApp);
            _testServiceMock.Setup(t => t.PassedAllTestsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.Drivers.FindAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                .ReturnsAsync(new Driver { DriverID = 100 });

            // محاكاة فشل الحفظ (CompleteAsync يعيد 0)
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.IssueFirstTimeLicenseAsync(createDto, 1);

            // Assert
            Assert.Null(result);
            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
        }
        #endregion

        #region RenewLicenseAsync
        [Fact]
        public async Task RenewLicenseAsync_ReturnsLicenseDto_WhenRenewedSuccessfully()
        {
            // Arrange
            var userId = 1;
            var fakeLicenseDto = GetFakeLicenseDtos().First();
            var appTypeInfo = new ApplicationType { ApplicationTypeID = 2, Fees = 50.0f };

            var oldLicense = new License
            {
                LicenseID = 1,
                IsActive = true,
                LicenseClassID = 3,
                LicenseClassInfo = new LicenseClass { DefaultValidityLength = 10, ClassFees = 20.0f },
                DriverInfo = new Driver { DriverID = 100, PersonID = 50 }
            };

            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(oldLicense);

            _unitOfWorkMock.Setup(u => u.ApplicationTypes.GetByIdAsync((int)EnApplicationType.RenewDrivingLicense))
                .ReturnsAsync(appTypeInfo);

            _unitOfWorkMock.Setup(u => u.Drivers.FindAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                .ReturnsAsync(oldLicense.DriverInfo);

            _applicationRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Application>()))
                .Returns(Task.CompletedTask);

            _licenseRepoMock.Setup(repo => repo.AddAsync(It.IsAny<License>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map<LicenseDto>(It.IsAny<License>()))
                .Returns(fakeLicenseDto);

            // Act
            var result = await _service.RenewLicenseAsync(oldLicense.LicenseID, "Renewal Notes", userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(oldLicense.IsActive);
            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RenewLicenseAsync_ThrowsValidationException_WhenLicenseNotExpired()
        {
            // Arrange
            int oldLicenseId = 100;
            var notExpiredLicense = new License
            {
                LicenseID = oldLicenseId,
                IsActive = true,
                ExpirationDate = DateTime.UtcNow.AddDays(10) 
            };

            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(notExpiredLicense);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _service.RenewLicenseAsync(oldLicenseId, "Notes", 1));

            Assert.Equal("License is not yet expired.", exception.Message);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task RenewLicenseAsync_ThrowsValidationException_WhenOldLicenseIsInactive()
        {
            // Arrange
            var oldLicense = new License { LicenseID = 1, IsActive = false };
            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(It.IsAny<Expression<Func<License, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(oldLicense);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.RenewLicenseAsync(1, "Notes", 1));
        }

        [Fact]
        public async Task RenewLicenseAsync_ThrowsValidationException_WhenOldLicenseDoesNotExist()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((License)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.RenewLicenseAsync(1, "Notes", 1));
        }

        [Fact]
        public async Task RenewLicenseAsync_ThrowsResourceNotFound_WhenApplicationTypeNotFound()
        {
            // Arrange
            var oldLicense = new License { LicenseID = 1, IsActive = true };
            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(It.IsAny<Expression<Func<License, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(oldLicense);

            _unitOfWorkMock.Setup(u => u.ApplicationTypes.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((ApplicationType)null!); 

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.RenewLicenseAsync(1, "Notes", 1));
        }

        [Fact]
        public async Task RenewLicenseAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange
            var userId = 1;
            var oldLicense = new License
            {
                LicenseID = 1,
                IsActive = true,
                LicenseClassID = 3,
                LicenseClassInfo = new LicenseClass { DefaultValidityLength = 10, ClassFees = 20.0f },
                DriverInfo = new Driver { DriverID = 100, PersonID = 50 }
            };

            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(It.IsAny<Expression<Func<License, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(oldLicense);

            _unitOfWorkMock.Setup(u => u.ApplicationTypes.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 50 });

            _unitOfWorkMock.Setup(u => u.Drivers.FindAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                .ReturnsAsync(oldLicense.DriverInfo);


            _unitOfWorkMock.Setup(u => u.CompleteAsync())
                .ThrowsAsync(new Exception("Database Save Failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.RenewLicenseAsync(oldLicense.LicenseID, "Notes", userId));

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
        }
        #endregion

        #region ReplaceLicenseAsync
        [Fact]
        public async Task ReplaceLicenseAsync_ReturnsLicenseDto_WhenReplacedSuccessfully()
        {
            // Arrange
            var userId = 1;
            var fakeLicenseDto = GetFakeLicenseDtos().First();

            var oldLicense = new License
            {
                LicenseID = 1,
                IsActive = true,
                DriverID = 100,
                LicenseClassID = 3,
                ExpirationDate = DateTime.UtcNow.AddYears(5),
                Notes = "Old Notes",
                DriverInfo = new Driver { DriverID = 100, PersonID = 50 },
                LicenseClassInfo = new LicenseClass { LicenseClassID = 3, DefaultValidityLength = 10 }
            };

            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(
                It.IsAny<Expression<Func<License, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(oldLicense);

            _unitOfWorkMock.Setup(u => u.ApplicationTypes.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 10.0f });

            _unitOfWorkMock.Setup(u => u.Drivers.FindAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                .ReturnsAsync(oldLicense.DriverInfo);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map<LicenseDto>(It.IsAny<License>()))
                .Returns(fakeLicenseDto);

            // Act
            var result = await _service.ReplaceLicenseAsync(oldLicense.LicenseID, EnIssueReason.DamagedReplacement, userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(oldLicense.IsActive); 
            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(EnIssueReason.DamagedReplacement, EnApplicationType.ReplaceDamagedDrivingLicense)]
        [InlineData(EnIssueReason.LostReplacement, EnApplicationType.ReplaceLostDrivingLicense)]
        public async Task ReplaceLicenseAsync_UsesCorrectApplicationType_BasedOnReason(EnIssueReason reason, EnApplicationType expectedAppType)
        {
            // Arrange
            var oldLicense = new License
            {
                LicenseID = 1,
                IsActive = true,
                DriverID = 100,
                DriverInfo = new Driver { DriverID = 100, PersonID = 50 }, 
                LicenseClassInfo = new LicenseClass { LicenseClassID = 3 }
            };

            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(It.IsAny<Expression<Func<License, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(oldLicense);

            _unitOfWorkMock.Setup(u => u.ApplicationTypes.GetByIdAsync((int)expectedAppType))
                .ReturnsAsync(new ApplicationType { Fees = 10.0f });

            _unitOfWorkMock.Setup(u => u.Drivers.FindAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                .ReturnsAsync(oldLicense.DriverInfo);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _service.ReplaceLicenseAsync(oldLicense.LicenseID, reason, 1);

            // Assert
            _applicationRepoMock.Verify(repo => repo.AddAsync(It.Is<Application>(a => a.ApplicationTypeID == (int)expectedAppType)), Times.Once);
        }

        [Fact]
        public async Task ReplaceLicenseAsync_ThrowsResourceNotFound_WhenOldLicenseDoesNotExist()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(It.IsAny<Expression<Func<License, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync((License)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
                _service.ReplaceLicenseAsync(1, EnIssueReason.DamagedReplacement, 1));
        }

        [Fact]
        public async Task ReplaceLicenseAsync_ThrowsValidationException_WhenOldLicenseIsInactive()
        {
            // Arrange
            var inactiveLicense = new License { LicenseID = 1, IsActive = false };
            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(It.IsAny<Expression<Func<License, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(inactiveLicense);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.ReplaceLicenseAsync(1, EnIssueReason.DamagedReplacement, 1));
        }


        [Fact]
        public async Task ReplaceLicenseAsync_ThrowsValidationException_WhenOldLicenseIsAlreadyInactive()
        {
            // Arrange
            var oldLicense = new License { LicenseID = 1, IsActive = false };
            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(It.IsAny<Expression<Func<License, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(oldLicense);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.ReplaceLicenseAsync(1, EnIssueReason.DamagedReplacement, 1));
        }

        [Fact]
        public async Task ReplaceLicenseAsync_ShouldCreateDriverAndSucceed_WhenDriverWasMissingForSomeReason()
        {
            // Arrange
            var oldLicense = new License
            {
                LicenseID = 1,
                IsActive = true,
                DriverID = 100,
                LicenseClassID = 3,
                DriverInfo = new Driver { PersonID = 50 }, 
                LicenseClassInfo = new LicenseClass { LicenseClassID = 3, ClassFees = 20 }
            };

            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(It.IsAny<Expression<Func<License, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(oldLicense);

            _unitOfWorkMock.Setup(u => u.ApplicationTypes.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 10 });

            _unitOfWorkMock.Setup(u => u.Drivers.FindAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                .ReturnsAsync((Driver)null!);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<LicenseDto>(It.IsAny<License>()))
                       .Returns(new LicenseDto { LicenseID = 99 }); 

            // Act
            var result = await _service.ReplaceLicenseAsync(1, EnIssueReason.DamagedReplacement, 1);

            // Assert
            _unitOfWorkMock.Verify(u => u.Drivers.AddAsync(It.IsAny<Driver>()), Times.Once);

            Assert.False(oldLicense.IsActive);

            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Once);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ReplaceLicenseAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange
            var oldLicense = new License
            {
                LicenseID = 1,
                IsActive = true,
                DriverID = 100,
                DriverInfo = new Driver { PersonID = 50 },
                LicenseClassInfo = new LicenseClass { LicenseClassID = 3 }
            };

            _unitOfWorkMock.Setup(u => u.Licenses.FindAsync(It.IsAny<Expression<Func<License, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(oldLicense);

            _unitOfWorkMock.Setup(u => u.ApplicationTypes.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 10 });

            _unitOfWorkMock.Setup(u => u.Drivers.FindAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                .ReturnsAsync(oldLicense.DriverInfo);

            _unitOfWorkMock.Setup(u => u.CompleteAsync())
                .ThrowsAsync(new Exception("Database Save Failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.ReplaceLicenseAsync(oldLicense.LicenseID, EnIssueReason.DamagedReplacement, 1));

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
        }
        #endregion
    }

}

