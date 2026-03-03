using AutoMapper;
using DVLD.CORE.DTOs.Applications.LocalDrivingLicenseApplication;
using DVLD.CORE.Entities;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using ApplicationType = DVLD.CORE.Entities.ApplicationType;

namespace DVLD.Tests.Services
{
    public class LocalDrivingLicenseApplicationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILocalDrivingLicenseApplicationRepository> _localAppRepositoryMock;
        private readonly Mock<IApplicationRepository> _appRepositoryMock;
        private readonly Mock<IGenericRepository<ApplicationType>> _applicationTypeRepoMock;
        private readonly Mock<IGenericRepository<LicenseClass>> _licenseClassRepoMock;
        private readonly Mock<IGenericRepository<TestAppointment>> _testAppointmentRepoMock;
        private readonly Mock<IUnitOfWorkTransaction> _dbContextTransactionMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<LocalDrivingLicenseApplicationService>> _loggerMock;
        private readonly LocalDrivingLicenseApplicationService _service;

        public LocalDrivingLicenseApplicationServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _localAppRepositoryMock = new Mock<ILocalDrivingLicenseApplicationRepository>();
            _appRepositoryMock = new Mock<IApplicationRepository>();
            _applicationTypeRepoMock = new Mock<IGenericRepository<ApplicationType>>();
            _licenseClassRepoMock = new Mock<IGenericRepository<LicenseClass>>();
            _testAppointmentRepoMock = new Mock<IGenericRepository<TestAppointment>>();

            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<LocalDrivingLicenseApplicationService>>();

            _dbContextTransactionMock = new Mock<IUnitOfWorkTransaction>();
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_dbContextTransactionMock.Object);

            _unitOfWorkMock.Setup(r => r.LocalDrivingLicenseApplications).Returns(_localAppRepositoryMock.Object);
            _unitOfWorkMock.Setup(r => r.Applications).Returns(_appRepositoryMock.Object);
            _unitOfWorkMock.Setup(r => r.ApplicationTypes).Returns(_applicationTypeRepoMock.Object);
            _unitOfWorkMock.Setup(r => r.LicenseClasses).Returns(_licenseClassRepoMock.Object);
            _unitOfWorkMock.Setup(r => r.TestAppointments).Returns(_testAppointmentRepoMock.Object);

            _service = new LocalDrivingLicenseApplicationService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        #region Data 
        private List<LocalDrivingLicenseApplication> GetFakeLocalDrivingLicenseApplications()
        {
            return new List<LocalDrivingLicenseApplication>()
            {
                new LocalDrivingLicenseApplication
                {
                    LocalDrivingLicenseApplicationID = 1,
                    ApplicationID = 100,
                    LicenseClassID = 3 // Class 3 - Ordinary driving license
                },
                new LocalDrivingLicenseApplication
                {
                    LocalDrivingLicenseApplicationID = 2,
                    ApplicationID = 101,
                    LicenseClassID = 1 // Class 1 - Small Motorcycle
                }
            };
        }
        private List<LocalDrivingLicenseApplicationDto> GetFakeLocalDrivingLicenseApplicationDtos()
        {
            return new List<LocalDrivingLicenseApplicationDto>()
            {
                new LocalDrivingLicenseApplicationDto
                {
                    LocalDrivingLicenseApplicationID = 1,
                    ApplicationID = 100,
                    ClassName = "Class 3 - Ordinary driving license",
                    NationalNo = "N123",
                    FullName = "Ahmed Mohamed Ali",
                    ApplicationDate = DateTime.Now.AddDays(-5),
                    PassedTestCount = 1,
                    Status = "New"
                },
                new LocalDrivingLicenseApplicationDto
                {
                    LocalDrivingLicenseApplicationID = 2,
                    ApplicationID = 101,
                    ClassName = "Class 1 - Small Motorcycle",
                    NationalNo = "N456",
                    FullName = "Sami Omar",
                    ApplicationDate = DateTime.Now.AddDays(-2),
                    PassedTestCount = 0,
                    Status = "New"
                }
            };
        }
        private List<LocalDrivingLicenseApplicationCreateDto> GetFakeLocalDrivingLicenseApplicationCreateDtos()
        {
            return new List<LocalDrivingLicenseApplicationCreateDto>()
            {
                new LocalDrivingLicenseApplicationCreateDto
                {
                    PersonID = 10,
                    LicenseClassID = 3,
                    ApplicationTypeID = 1 // New Local Driving License Service
                },
                new LocalDrivingLicenseApplicationCreateDto
                {
                    PersonID = 11,
                    LicenseClassID = 2,
                    ApplicationTypeID = 1
                }
            };
        }
        private List<LocalDrivingLicenseApplicationUpdateDto> GetFakeLocalDrivingLicenseApplicationUpdateDtos()
        {
            return new List<LocalDrivingLicenseApplicationUpdateDto>()
            {
                new LocalDrivingLicenseApplicationUpdateDto
                {
                    LocalDrivingLicenseApplicationID = 1,
                    LicenseClassID = 4 // تغيير الصنف من 3 إلى 4 (Heavy Truck)
                }
            };
        }
        #endregion

        #region GetAllLocalDrivingLicenseApplicationsAsync
        [Fact]
        public async Task GetAllLocalDrivingLicenseApplicationsAsync_ReturnsPagedResult_WhenDataExists()
        {
            // Arrange
            var fakeLocalAppDtos = GetFakeLocalDrivingLicenseApplicationDtos();
            int pageNumber = 1;
            int pageSize = 10;
            int expectedTotalCount = 50;

            _localAppRepositoryMock
                .Setup(r => r.GetPagedApplicationsAsync(
                    It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(),
                    pageNumber,
                    pageSize
                ))
                .ReturnsAsync((fakeLocalAppDtos, expectedTotalCount));

            // Act
            var result = await _service.GetAllLocalDrivingLicenseApplicationsAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTotalCount, result.TotalCount);
            Assert.Equal(fakeLocalAppDtos.Count(), result.Data.Count());
            Assert.Equal(pageNumber, result.PageNumber);
            Assert.Equal(pageSize, result.PageSize);

            Assert.Equal("Class 3 - Ordinary driving license", result.Data.First().ClassName);

            _localAppRepositoryMock.Verify(r => r.GetPagedApplicationsAsync(
                It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(),
                pageNumber,
                pageSize
            ), Times.Once);
        }
        #endregion

        #region GetByIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetByIdAsync_ThrowsValidationException_WhenLocalDrivingLicenseApplicationIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.GetByIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsResourceNotFoundException_WhenLocalDrivingLicenseApplicationIsNotFound()
        {
            // Arrange
            _localAppRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync((LocalDrivingLicenseApplication)null!);

            // Act
            var result = _service.GetByIdAsync(100);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Local Driving License Application with ID ", exception.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsLocalDrivingLicenseApplicationDto_WithPassedTestCount_WhenFound()
        {
            // Arrange
            var fakeLocalApp = GetFakeLocalDrivingLicenseApplications()[0];
            var expectedDto = GetFakeLocalDrivingLicenseApplicationDtos()[0];
            int localAppID = 1;
            byte expectedPassedTests = 3;

            _localAppRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(fakeLocalApp);

            // 2. Setup للـ Mapper
            _mapperMock.Setup(m => m.Map<LocalDrivingLicenseApplicationDto>(fakeLocalApp))
                       .Returns(expectedDto);

            _unitOfWorkMock.Setup(u => u.Tests.FindAllAsync(
                It.IsAny<Expression<Func<Test, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                null, 0, null, null))
                .ReturnsAsync(new List<Test>(new Test[expectedPassedTests]));

            // Act
            var result = await _service.GetByIdAsync(localAppID);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(localAppID, result.LocalDrivingLicenseApplicationID);

            Assert.Equal(expectedPassedTests, result.PassedTestCount);

            _localAppRepositoryMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(), It.IsAny<string[]>(), It.IsAny<bool>()), Times.Once);
        }

        #endregion

        #region GetByApplicationIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetByApplicationIdAsync_ThrowsValidationException_WhenApplicationIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.GetByApplicationIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetByApplicationIdAsync_ThrowsResourceNotFoundException_WhenLocalDrivingLicenseApplicationIsNotFound()
        {
            // Arrange
            _localAppRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync((LocalDrivingLicenseApplication)null!);

            // Act
            var result = _service.GetByApplicationIdAsync(100);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Local Driving License Application with ApplicationID ", exception.Message);
        }

        [Fact]
        public async Task GetByApplicationIdAsync_ReturnsDtoWithPassedTestCount_WhenFound()
        {
            // Arrange
            var fakeLocalApp = GetFakeLocalDrivingLicenseApplications()[0];
            var expectedDto = GetFakeLocalDrivingLicenseApplicationDtos()[0];
            int applicationIdToTest = 1;
            byte expectedPassedTests = 2; 

            _localAppRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(fakeLocalApp);

            _mapperMock.Setup(m => m.Map<LocalDrivingLicenseApplicationDto>(fakeLocalApp))
                       .Returns(expectedDto);

            _unitOfWorkMock.Setup(u => u.Tests.FindAllAsync(
                It.IsAny<Expression<Func<Test, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<Test>(new Test[expectedPassedTests]));

            // Act
            var result = await _service.GetByApplicationIdAsync(applicationIdToTest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.LocalDrivingLicenseApplicationID, result.LocalDrivingLicenseApplicationID);
            Assert.Equal(expectedPassedTests, result.PassedTestCount); 
            Assert.Equal(expectedDto.FullName, result.FullName);

            // Verify
            _localAppRepositoryMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(), It.IsAny<string[]>(), It.IsAny<bool>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Tests.FindAllAsync(It.IsAny<Expression<Func<Test, bool>>>(), It.IsAny<string[]>(), It.IsAny<bool>()), Times.Once);
        }

        #endregion

        #region GetActiveApplicationIdForLicenseClassAsync
        [Theory]
        [InlineData(-1, 1, 1)]
        [InlineData(1, -1, 1)]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, -1)]
        public async Task GetActiveApplicationIdForLicenseClassAsync_ThrowsValidationException_WhenPersonIDOrApplicationTypeIDOrLicenseClassIDIsLessOrEqualZero
            (int personID, int applicationTypeID, int licenseClassID)
        {
            // Arrange & Act
            var result = _service.GetActiveApplicationIdForLicenseClassAsync(personID, applicationTypeID, licenseClassID);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetActiveApplicationIdForLicenseClassAsync_ReturnsLessThenZero_WhenActiveApplicationIdForLicenseClassIsNotFound()
        {
            int applicationTypeID = 1, personID = 1, licenseClassID = 1;
            // Arrange
            _localAppRepositoryMock.Setup(r => r.GetActiveApplicationIdForLicenseClassAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(-1);

            // Act
            var result = await _service.GetActiveApplicationIdForLicenseClassAsync(personID, applicationTypeID, licenseClassID);

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public async Task GetActiveApplicationIdForLicenseClassAsync_ReturnsGreatThenZero_WhenActiveApplicationIdForLicenseClassIsFound()
        {
            // Arrange
            int applicationTypeID = 1, personID = 1, licenseClassID = 1;
            // Arrange
            _localAppRepositoryMock.Setup(r => r.GetActiveApplicationIdForLicenseClassAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.GetActiveApplicationIdForLicenseClassAsync(personID, applicationTypeID, licenseClassID);

            // Assert
            Assert.True(result > 0);
        }
        #endregion

        #region AddLocalDrivingLicenseApplicationAsync
        [Fact]
        public async Task AddLocalDrivingLicenseApplicationAsync_ShouldRollback_WhenDatabaseErrorOccurs()
        {
            // Arrange 
            int currentUserId = 1;
            var fakeApplicationCreateDto = GetFakeLocalDrivingLicenseApplicationCreateDtos()[0];


            _localAppRepositoryMock.Setup(r => r.GetActiveApplicationIdForLicenseClassAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(-1);


            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync(true);

            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 15 });


            _appRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Application>()))
                .ThrowsAsync(new Exception("Database Error"));

            // Act & Assert 
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _service.AddLocalDrivingLicenseApplicationAsync(fakeApplicationCreateDto, currentUserId));

            Assert.Contains("Database Error", exception.Message);

            _appRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Application>()), Times.Once);

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task AddLocalDrivingLicenseApplicationAsync_ShouldRollbackAndReturnNull_WhenDatabaseSaveFailsForApplicationBase()
        {
            // Arrange
            int currentUserId = 1;
            var fakeDto = GetFakeLocalDrivingLicenseApplicationCreateDtos()[0];

            _localAppRepositoryMock.Setup(r => r.GetActiveApplicationIdForLicenseClassAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(-1);

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync(true);

            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 15 });

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.AddLocalDrivingLicenseApplicationAsync(fakeDto, currentUserId);

            // Assert
            Assert.Null(result);

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);

            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Never);

            _localAppRepositoryMock.Verify(r => r.AddAsync(It.IsAny<LocalDrivingLicenseApplication>()), Times.Never);
        }

        [Fact]
        public async Task AddLocalDrivingLicenseApplicationAsync_ShouldRollbackAndReturnNull_WhenLocalAppSaveFails()
        {
            // Arrange
            int currentUserId = 1;
            var fakeDto = GetFakeLocalDrivingLicenseApplicationCreateDtos()[0];

            _localAppRepositoryMock.Setup(r => r.GetActiveApplicationIdForLicenseClassAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(-1);

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>())).ReturnsAsync(true);
            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new ApplicationType { Fees = 15 });

            // محاكاة: نجاح حفظ الطلب الأساسي (1) وفشل حفظ الطلب المحلي (0)
            _unitOfWorkMock.SetupSequence(u => u.CompleteAsync())
                .ReturnsAsync(1)
                .ReturnsAsync(0);

            // Act
            var result = await _service.AddLocalDrivingLicenseApplicationAsync(fakeDto, currentUserId);

            // Assert
            Assert.Null(result); // السيرفس تعيد null عند فشل الحفظ (كما في الكود الخاص بك)

            // التأكد من استدعاء Rollback مرة واحدة بسبب فشل الحفظ الثاني
            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task AddLocalDrivingLicenseApplicationAsync_ReturnsDto_WhenAddedSuccessfully()
        {
            // Arrange
            int currentUserId = 1;
            var fakeDto = GetFakeLocalDrivingLicenseApplicationCreateDtos()[0];
            var expectedResultDto = new LocalDrivingLicenseApplicationDto
            {
                LocalDrivingLicenseApplicationID = 1,
                ClassName = "Class 3",
                PassedTestCount = 0 
            };

            _localAppRepositoryMock.Setup(r => r.GetActiveApplicationIdForLicenseClassAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(-1);

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync(true);
            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 15 });

            _appRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Application>()))
                .Callback<Application>(a => a.ApplicationID = 1);
            _localAppRepositoryMock.Setup(r => r.AddAsync(It.IsAny<LocalDrivingLicenseApplication>()))
                .Callback<LocalDrivingLicenseApplication>(la => la.LocalDrivingLicenseApplicationID = 1);

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(_dbContextTransactionMock.Object);

            _unitOfWorkMock.SetupSequence(u => u.CompleteAsync())
                .ReturnsAsync(1)
                .ReturnsAsync(1); 

            _localAppRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new LocalDrivingLicenseApplication { LocalDrivingLicenseApplicationID = 1 });

            _mapperMock.Setup(m => m.Map<LocalDrivingLicenseApplicationDto>(It.IsAny<LocalDrivingLicenseApplication>()))
                .Returns(expectedResultDto);

            _unitOfWorkMock.Setup(u => u.Tests.FindAllAsync(
                It.IsAny<Expression<Func<Test, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<Test>()); 

            // Act
            var result = await _service.AddLocalDrivingLicenseApplicationAsync(fakeDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResultDto.LocalDrivingLicenseApplicationID, result.LocalDrivingLicenseApplicationID);
            Assert.Equal(0, result.PassedTestCount); 

            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Once);
            _appRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Application>()), Times.Once);
        }

        [Fact]
        public async Task AddLocalDrivingLicenseApplicationAsync_ThrowsConflictException_WhenThereIsActiveApplicationIdForLicenseClassId()
        {
            // Arrange
            int currentUserId = 1;
            var fakeDto = GetFakeLocalDrivingLicenseApplicationCreateDtos()[0];

            _localAppRepositoryMock.Setup(r => r.GetActiveApplicationIdForLicenseClassAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(1);

            // Act
            var result = _service.AddLocalDrivingLicenseApplicationAsync(fakeDto, currentUserId);
            // Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => result);
            Assert.Contains("This person already has an active application for this type of license.", exception.Message);
        }

        [Fact]
        public async Task AddLocalDrivingLicenseApplicationAsync_ThrowsResourceNotFoundException_WhenLicenseClassNotFound()
        {
            // Arrange
            int currentUserId = 1;
            var fakeDto = GetFakeLocalDrivingLicenseApplicationCreateDtos()[0];

            _localAppRepositoryMock.Setup(r => r.GetActiveApplicationIdForLicenseClassAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(-1);

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>())).ReturnsAsync(false);

            // Act
            var result = _service.AddLocalDrivingLicenseApplicationAsync(fakeDto, currentUserId);
            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("The specified License Class", exception.Message);
        }

        [Fact]
        public async Task AddLocalDrivingLicenseApplicationAsync_ThrowsResourceNotFoundException_WhenApplicationTypeNotFound()
        {
            // Arrange
            int currentUserId = 1;
            var fakeDto = GetFakeLocalDrivingLicenseApplicationCreateDtos()[0];

            _localAppRepositoryMock.Setup(r => r.GetActiveApplicationIdForLicenseClassAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(-1);

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>())).ReturnsAsync(true);

            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ApplicationType)null!);

            // Act
            var result = _service.AddLocalDrivingLicenseApplicationAsync(fakeDto, currentUserId);
            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Application Type with ID", exception.Message);
        }
        #endregion

        #region UpdateLocalDrivingLicenseApplicationAsync
        [Fact]
        public async Task UpdateLocalDrivingLicenseApplicationAsync_ThrowsResourceNotFoundException_WhenLicenseClassNotFound()
        {
            // Arrange
            var fakeApplicationUpdateDto = GetFakeLocalDrivingLicenseApplicationUpdateDtos()[0];

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>())).ReturnsAsync(false);

            // Act
            var result = _service.UpdateLocalDrivingLicenseApplicationAsync(fakeApplicationUpdateDto);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("The specified License Class", exception.Message);
            _licenseClassRepoMock.Verify(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task UpdateLocalDrivingLicenseApplicationAsync_ThrowsResourceNotFoundException_WhenLocalAppNotFound()
        {
            // Arrange
            var fakeApplicationUpdateDto = GetFakeLocalDrivingLicenseApplicationUpdateDtos()[0];

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>())).ReturnsAsync(true);

            _localAppRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((LocalDrivingLicenseApplication)null!);

            // Act
            var result = _service.UpdateLocalDrivingLicenseApplicationAsync(fakeApplicationUpdateDto);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Cannot update: Local Driving License Application with ID ", exception.Message);
            _localAppRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task UpdateLocalDrivingLicenseApplicationAsync_ReturnsDto_WhenLocalAppIsUpdated()
        {
            // Arrange             
            var fakeApplicationUpdateDto = GetFakeLocalDrivingLicenseApplicationUpdateDtos()[0];
            var existingInDb = GetFakeLocalDrivingLicenseApplications()[0];
            var expectedDto = new LocalDrivingLicenseApplicationDto
            {
                LocalDrivingLicenseApplicationID = fakeApplicationUpdateDto.LocalDrivingLicenseApplicationID
            };
            byte expectedPassedTests = 1; 

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync(true);

            _localAppRepositoryMock.Setup(r => r.GetByIdAsync(fakeApplicationUpdateDto.LocalDrivingLicenseApplicationID))
                .ReturnsAsync(existingInDb);

            _testAppointmentRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestAppointment, bool>>>()))
                .ReturnsAsync(false);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);


            _localAppRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(existingInDb);

            _mapperMock.Setup(m => m.Map<LocalDrivingLicenseApplicationDto>(existingInDb))
                .Returns(expectedDto);

            _unitOfWorkMock.Setup(u => u.Tests.FindAllAsync(
                It.IsAny<Expression<Func<Test, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<Test>(new Test[expectedPassedTests]));

            // Act
            var result = await _service.UpdateLocalDrivingLicenseApplicationAsync(fakeApplicationUpdateDto);

            // Assert 
            Assert.NotNull(result);
            Assert.Equal(fakeApplicationUpdateDto.LocalDrivingLicenseApplicationID, result.LocalDrivingLicenseApplicationID);
            Assert.Equal(expectedPassedTests, result.PassedTestCount); 

            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _localAppRepositoryMock.Verify(r => r.Update(existingInDb), Times.Once);
        }
        [Fact]
        public async Task UpdateLocalDrivingLicenseApplicationAsync_ReturnsNull_WhenFailedToUpdateInDatabase()
        {
            // Arrange             
            var fakeApplicationUpdateDto = GetFakeLocalDrivingLicenseApplicationUpdateDtos()[0];
            var existingInDb = GetFakeLocalDrivingLicenseApplications()[0];

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync(true);

            _localAppRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(existingInDb);

            _testAppointmentRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestAppointment, bool>>>()))
                .ReturnsAsync(false);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.UpdateLocalDrivingLicenseApplicationAsync(fakeApplicationUpdateDto);

            // Assert 
            Assert.Null(result);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateLocalDrivingLicenseApplicationAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var fakeApplicationUpdateDto = GetFakeLocalDrivingLicenseApplicationUpdateDtos()[0];

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>())).ReturnsAsync(true);

            _localAppRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var result = _service.UpdateLocalDrivingLicenseApplicationAsync(fakeApplicationUpdateDto);
            var exception = await Assert.ThrowsAsync<Exception>(() => result);
            Assert.Contains($"An error occurred while updating the Local Driving License Application record.", exception.Message);
        }
        #endregion

        #region DeleteLocalDrivingLicenseApplicationAsync
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteLocalDrivingLicenseApplicationAsync_ThrowsValidationException_WhenIdIsLessOrEqualZero(int invalidId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.DeleteLocalDrivingLicenseApplicationAsync(invalidId));

            Assert.Contains("Invalid ID provided.", exception.Message);
        }

        [Fact]
        public async Task DeleteLocalDrivingLicenseApplicationAsync_ThrowsResourceNotFoundException_WhenApplicationDoesNotExist()
        {
            // Arrange
            var InvalidID = 100;
            _localAppRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((LocalDrivingLicenseApplication)null!);

            // Act
            var result = _service.DeleteLocalDrivingLicenseApplicationAsync(InvalidID);
            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);

            Assert.Contains("Local Application ", exception.Message);

            _localAppRepositoryMock.Verify(r => r.Delete(It.IsAny<LocalDrivingLicenseApplication>()), Times.Never);
        }

        [Fact]
        public async Task DeleteLocalDrivingLicenseApplicationAsync_ReturnsTrue_WhenLocalDrivingLicenseApplicationIsDeleted()
        {
            // Arrange
            int localAppID = 1;
            var localApp = GetFakeLocalDrivingLicenseApplications()[0];

            _localAppRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(localApp);

            _appRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Application());

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteLocalDrivingLicenseApplicationAsync(localAppID);

            // Assert
            Assert.True(result);
            _localAppRepositoryMock.Verify(r => r.Delete(It.IsAny<LocalDrivingLicenseApplication>()), Times.Once);
            _appRepositoryMock.Verify(r => r.Delete(It.IsAny<Application>()), Times.Once);
        }

        [Fact]
        public async Task DeleteLocalDrivingLicenseApplicationAsync_ReturnsFalse_WhenNoRowsAffected()
        {
            // Arrange
            int localAppID = 1;

            _localAppRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new LocalDrivingLicenseApplication());

            _appRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Application());

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.DeleteLocalDrivingLicenseApplicationAsync(localAppID);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteLocalDrivingLicenseApplicationAsync_ThrowsConflictException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            int localAppID = 1;
            var localApp = GetFakeLocalDrivingLicenseApplications()[0];
            _localAppRepositoryMock.Setup(r => r.GetByIdAsync(localAppID)).ReturnsAsync(localApp);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ThrowsAsync(new Exception("Foreign key violation"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteLocalDrivingLicenseApplicationAsync(localAppID));
            Assert.Contains("The request cannot be deleted because it is linked to other records such as tests or appointments.", exception.Message);

            _localAppRepositoryMock.Verify(r => r.Delete(It.IsAny<LocalDrivingLicenseApplication>()), Times.Once);
            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Never);
        }

        #endregion

    }
}
