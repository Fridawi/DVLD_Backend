using AutoMapper;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.TestAppointments;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.CORE.Interfaces.Tests;
using DVLD.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace DVLD.Tests.Services
{
    public class TestAppointmentServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<TestAppointment>> _testAppointmentRepoMock;
        private readonly Mock<IGenericRepository<ApplicationType>> _appTypeRepoMock;
        private readonly Mock<ILocalDrivingLicenseApplicationRepository> _localAppRepo;
        private readonly Mock<IUnitOfWorkTransaction> _dbContextTransactionMock;
        private readonly Mock<IApplicationRepository> _appRepo;
        private readonly Mock<ITestService> _testServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<TestAppointmentService>> _loggerMock;
        private readonly TestAppointmentService _service;

        public TestAppointmentServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _testAppointmentRepoMock = new Mock<IGenericRepository<TestAppointment>>();
            _appTypeRepoMock = new Mock<IGenericRepository<ApplicationType>>();
            _dbContextTransactionMock = new Mock<IUnitOfWorkTransaction>();
            _localAppRepo = new Mock<ILocalDrivingLicenseApplicationRepository>();
            _appRepo = new Mock<IApplicationRepository>();
            _testServiceMock = new Mock<ITestService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<TestAppointmentService>>();
            _unitOfWorkMock.Setup(u => u.TestAppointments).Returns(_testAppointmentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationTypes).Returns(_appTypeRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_dbContextTransactionMock.Object);
            _unitOfWorkMock.Setup(u => u.LocalDrivingLicenseApplications).Returns(_localAppRepo.Object);
            _unitOfWorkMock.Setup(u => u.Applications).Returns(_appRepo.Object);
            _service = new TestAppointmentService(_unitOfWorkMock.Object, _testServiceMock.Object, _loggerMock.Object, _mapperMock.Object);
        }

        #region Data 
        private List<TestAppointment> GetFakeTestAppointments()
        {
            return new List<TestAppointment>()
            {
                new TestAppointment
                {
                    TestAppointmentID = 1,
                    TestTypeID = EnTestType.VisionTest,
                    LocalDrivingLicenseApplicationID = 10,
                    AppointmentDate = DateTime.Now.AddDays(2),
                    PaidFees = 10,
                    CreatedByUserID = 1,
                    IsLocked = false,
                    LocalAppInfo = new LocalDrivingLicenseApplication {
                        LicenseClassInfo = new LicenseClass { ClassName = "Class 3 - Ordinary Driving License" },
                        ApplicationInfo = new Application {
                            PersonInfo = new Person { FirstName = "Ali", SecondName = "Ahmed", ThirdName = "Hassan", LastName = "Saleh" }
                        }
                    }
                },
                new TestAppointment
                {
                    TestAppointmentID = 2,
                    TestTypeID = EnTestType.WrittenTest,
                    LocalDrivingLicenseApplicationID = 12,
                    AppointmentDate = DateTime.Now.AddDays(-1),
                    PaidFees = 20,
                    CreatedByUserID = 1,
                    IsLocked = true,
                    LocalAppInfo = new LocalDrivingLicenseApplication {
                        LicenseClassInfo = new LicenseClass { ClassName = "Class 3 - Ordinary Driving License" },
                        ApplicationInfo = new Application {
                            PersonInfo = new Person { FirstName = "Saleh", SecondName = "Ali", ThirdName = "Ahmad", LastName = "Hassan" }
                        }
                    }
                }
            };
        }
        private List<TestAppointmentDto> GetFakeTestAppointmentDtos()
        {
            return new List<TestAppointmentDto>()
            {
                new TestAppointmentDto
                {
                    TestAppointmentID = 1,
                    TestTypeName = "Vision Test",
                    FullName = "Ali Ahmed Hassan Saleh",
                    ClassName = "Class 3 - Ordinary Driving License",
                    AppointmentDate = DateTime.Now.AddDays(2),
                    PaidFees = 10,
                    IsLocked = false
                },
                new TestAppointmentDto
                {
                    TestAppointmentID = 2,
                    TestTypeName = "Street Test",
                    FullName = "Saleh Ali Hassan Saleh",
                    ClassName = "Class 3 - Ordinary Ahmad Hassan",
                    AppointmentDate = DateTime.Now.AddDays(2),
                    PaidFees = 10,
                    IsLocked = false
                }
            };
        }
        private List<TestAppointmentCreateDto> GetFakeTestAppointmentCreateDtos()
        {
            return new List<TestAppointmentCreateDto>()
            {
                new TestAppointmentCreateDto
                {
                    TestTypeID = 1,
                    LocalDrivingLicenseApplicationID = 10,
                    AppointmentDate = DateTime.Now.AddDays(5),
                    PaidFees = 10
                },
                new TestAppointmentCreateDto
                {
                    TestTypeID = 1,
                    LocalDrivingLicenseApplicationID = 10,
                    AppointmentDate = DateTime.Now.AddDays(7),
                    PaidFees = 15
                }
            };
        }
        #endregion


        #region GetAllTestAppointmentsAsync
        [Fact]
        public async Task GetAllTestAppointmentsAsync_ReturnsPagedResult_WhenDataExists()
        {
            // Arrange
            int pageNumber = 1, pageSize = 10;
            var fakeTestAppointments = GetFakeTestAppointments();
            var fakeTestAppointmentDtos = GetFakeTestAppointmentDtos();
            int totalCount = fakeTestAppointments.Count();

            _testAppointmentRepoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<TestAppointment, bool>>>()))
                .ReturnsAsync(totalCount);

            _testAppointmentRepoMock.Setup(r => r.FindAllAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<TestAppointment, object>>>(),
                It.IsAny<EnOrderByDirection>(),
                It.IsAny<int?>(), 
                It.IsAny<int?>() 
                )).ReturnsAsync(fakeTestAppointments);

            _mapperMock.Setup(m => m.Map<IEnumerable<TestAppointmentDto>>(fakeTestAppointments))
                .Returns(fakeTestAppointmentDtos);

            // Act
            var result = await _service.GetAllTestAppointmentsAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResultDto<TestAppointmentDto>>(result);
            Assert.Equal(totalCount, result.TotalCount);
            Assert.Equal(fakeTestAppointmentDtos.Count(), result.Data.Count());
            Assert.Equal("Vision Test", result.Data.First().TestTypeName);

            _testAppointmentRepoMock.Verify(r => r.FindAllAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<TestAppointment, object>>>(),
                It.IsAny<EnOrderByDirection>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
        }
        #endregion

        #region GetApplicationTestAppointmentsPerTestTypeAsync
        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, 1)]
        public async Task GetApplicationTestAppointmentsPerTestTypeAsync_ThrowsValidationException_WhenIDsAreInvalid(int localAppID, int testTypeID)
        {
            // Arrange & Act
            var result = _service.GetApplicationTestAppointmentsPerTestTypeAsync(localAppID, testTypeID, 1, 10);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetApplicationTestAppointmentsPerTestTypeAsync_ReturnsPagedResult_WhenDataExists()
        {
            // Arrange
            int localAppID = 1, testTypeID = 1;
            int pageNumber = 1, pageSize = 10;
            var fakeTestAppointments = GetFakeTestAppointments();
            var fakeTestAppointmentDtos = GetFakeTestAppointmentDtos();

            _testAppointmentRepoMock.Setup(r => r.FindAllAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<TestAppointment, object>>>(),
                It.IsAny<EnOrderByDirection>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()  
                )).ReturnsAsync(fakeTestAppointments);

            _testAppointmentRepoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<TestAppointment, bool>>>()))
                .ReturnsAsync(fakeTestAppointments.Count());

            _mapperMock.Setup(m => m.Map<IEnumerable<TestAppointmentDto>>(fakeTestAppointments))
                .Returns(fakeTestAppointmentDtos);

            // Act
            var result = await _service.GetApplicationTestAppointmentsPerTestTypeAsync(localAppID, testTypeID, pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResultDto<TestAppointmentDto>>(result);
            Assert.Equal(fakeTestAppointmentDtos.Count(), result.Data.Count());

            _testAppointmentRepoMock.Verify(r => r.FindAllAsync(
                It.Is<Expression<Func<TestAppointment, bool>>>(expr => expr.Compile().Invoke(new TestAppointment { LocalDrivingLicenseApplicationID = localAppID, TestTypeID = (EnTestType)testTypeID })),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<TestAppointment, object>>>(),
                It.IsAny<EnOrderByDirection>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
        }
        #endregion

        #region GetTestAppointmentByIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetTestAppointmentByIdAsync_ThrowsValidationException_WhenTestAppointmentIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.GetTestAppointmentByIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetTestAppointmentByIdAsync_ThrowsResourceNotFoundException_WhenTestAppointmentIsNotFound()
        {
            // Arrange
            _testAppointmentRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync((TestAppointment)null!);

            // Act
            var result = _service.GetTestAppointmentByIdAsync(100);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Test Appointment with ID ", exception.Message);
        }

        [Fact]
        public async Task GetTestAppointmentByIdAsync_ReturnsTestAppointmentDto_WhenTestAppointmentIsFound()
        {
            // Arrange
            var expectedTestAppointment = GetFakeTestAppointments()[0];
            var expectedTestAppointmentDto = GetFakeTestAppointmentDtos()[0];

            _testAppointmentRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(expectedTestAppointment);

            _mapperMock.Setup(m => m.Map<TestAppointmentDto>(expectedTestAppointment)).Returns(expectedTestAppointmentDto);

            // Act
            var result = await _service.GetTestAppointmentByIdAsync(100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTestAppointmentDto.TestAppointmentID, result.TestAppointmentID);
        }
        #endregion

        #region GetLastTestAppointmentAsync
        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        public async Task GetLastTestAppointmentAsync_ThrowsValidationException_WhenLocalAppIDOrTestTypeIDIsLessOrEqualZero(int localAppID, int testTypeID)
        {
            // Arrange & Act
            var result = _service.GetLastTestAppointmentAsync(localAppID, testTypeID);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetLastTestAppointmentAsync_ReturnNull_WhenTestAppointmentIsNotFound()
        {
            // Arrange
            int localAppID = 100, testTypeID = 1;

            _testAppointmentRepoMock.Setup(r => r.FindAllAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<TestAppointment, object>>>(),
                It.IsAny<EnOrderByDirection>(),
                It.IsAny<int>()))
            .ReturnsAsync(new List<TestAppointment>());

            // Act
            var result = await _service.GetLastTestAppointmentAsync(localAppID, testTypeID);

            // Assert
            Assert.Null(result);
            _mapperMock.Verify(m => m.Map<TestAppointmentDto>(It.IsAny<TestAppointment>()), Times.Never);
        }

        [Fact]
        public async Task GetLastTestAppointmentAsync_ReturnsDto_WhenRecordExists()
        {
            // Arrange
            var fakeList = GetFakeTestAppointments();
            var expectedDto = GetFakeTestAppointmentDtos()[0];

            _testAppointmentRepoMock.Setup(r => r.FindAllAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<TestAppointment, object>>>(),
                It.IsAny<EnOrderByDirection>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()
            ))
            .ReturnsAsync(fakeList);

            _mapperMock.Setup(m => m.Map<TestAppointmentDto>(It.IsAny<TestAppointment>()))
                       .Returns(expectedDto);

            // Act
            var result = await _service.GetLastTestAppointmentAsync(10, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.TestAppointmentID, result.TestAppointmentID);
        }
        #endregion

        #region HasActiveAppointmentAsync
        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        public async Task HasActiveAppointmentAsync_ThrowsValidationException_WhenLocalAppIDOrTestTypeIDIsLessOrEqualZero(int localAppID, int testTypeID)
        {
            // Arrange & Act
            var result = _service.HasActiveAppointmentAsync(localAppID, testTypeID);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task HasActiveAppointmentAsync_ReturnsTrue_WhenTestAppointmentIsExist()
        {
            // Arrange
            int localAppID = 1, testTypeID = 1;

            _testAppointmentRepoMock.Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.HasActiveAppointmentAsync(localAppID, testTypeID);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasActiveAppointmentAsync_ReturnsFalse_WhenTestAppointmentIsNotExist()
        {
            // Arrange
            int localAppID = 1, testTypeID = 1;

            _testAppointmentRepoMock.Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.HasActiveAppointmentAsync(localAppID, testTypeID);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region AddTestAppointmentAsync
        [Fact]
        public async Task AddTestAppointmentAsync_ThrowsInvalidOperationException_WhenActiveTestAppointmentExist()
        {
            // Arrange
            int currentUserId = 1;
            var fakeTestAppointmentDto = new TestAppointmentCreateDto
            {
                LocalDrivingLicenseApplicationID = 1,
                TestTypeID = (int)EnTestType.VisionTest
            };

            _testAppointmentRepoMock.Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>()))
                .ReturnsAsync(true);

            // Act
            var action = () => _service.AddTestAppointmentAsync(fakeTestAppointmentDto, currentUserId);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);

            Assert.Equal("The applicant already has an active appointment for this test.", exception.Message);

            _testAppointmentRepoMock.Verify(r => r.AddAsync(It.IsAny<TestAppointment>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task AddTestAppointmentAsync_ThrowsInvalidOperation_WhenPrerequisiteTestNotPassed()
        {
            // Arrange
            var fakeCreateDto = new TestAppointmentCreateDto
            {
                LocalDrivingLicenseApplicationID = 1,
                TestTypeID = (int)EnTestType.StreetTest
            };

            _testServiceMock.Setup(s => s.HasPassedAsync(1, (int)EnTestType.WrittenTest))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddTestAppointmentAsync(fakeCreateDto, 1));

            Assert.Contains("before passing Written Test", exception.Message);
        }

        [Fact]
        public async Task AddTestAppointmentAsync_ThrowsInvalidOperation_WhenAlreadyHasActiveAppointment()
        {
            // Arrange
            var fakeCreateDto = new TestAppointmentCreateDto { LocalDrivingLicenseApplicationID = 1, TestTypeID = 1 };

            _unitOfWorkMock.Setup(u => u.TestAppointments.IsExistAsync(It.IsAny<Expression<Func<TestAppointment, bool>>>(), null))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddTestAppointmentAsync(fakeCreateDto, 1));
        }

        [Fact]
        public async Task AddTestAppointmentAsync_CreatesRetakeApplication_WhenRetakeIsRequired()
        {
            // Arrange
            var fakeCreateDto = new TestAppointmentCreateDto
            {
                LocalDrivingLicenseApplicationID = 1,
                TestTypeID = 1,
                AppointmentDate = DateTime.Now.AddDays(1)
            };
            var expectedAppointment = new TestAppointment { TestAppointmentID = 10 };
            var localApp = new LocalDrivingLicenseApplication
            {
                ApplicationInfo = new Application { ApplicantPersonID = 50 }
            };

            _testAppointmentRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestAppointment, bool>>>(), null))
                .ReturnsAsync(false);
            _testServiceMock.Setup(s => s.HasPassedAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _testServiceMock.Setup(s => s.RequiresRetakeAsync(1, 1)).ReturnsAsync(true);

            _localAppRepo.Setup(u => u.FindAsync(It.IsAny<Expression<Func<LocalDrivingLicenseApplication, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(localApp);

            _appTypeRepoMock.Setup(u => u.GetByIdAsync((int)EnApplicationType.RetakeTest))
                .ReturnsAsync(new ApplicationType { Fees = 5 });

            _mapperMock.Setup(m => m.Map<TestAppointment>(fakeCreateDto))
                .Returns(expectedAppointment);

            _testAppointmentRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>())) 
                .ReturnsAsync(expectedAppointment);

            _mapperMock.Setup(m => m.Map<TestAppointmentDto>(expectedAppointment))
                .Returns(new TestAppointmentDto { TestAppointmentID = 10 });

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.AddTestAppointmentAsync(fakeCreateDto, 1);
            // Assert
            _appRepo.Verify(u => u.AddAsync(It.Is<Application>(a => a.ApplicationTypeID == (int)EnApplicationType.RetakeTest)), Times.Once);
            Assert.NotNull(result);
            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task AddTestAppointmentAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange
            int currentUserId = 1;
            var fakeCreateDto = new TestAppointmentCreateDto
            {
                LocalDrivingLicenseApplicationID = 10,
                TestTypeID = (int)EnTestType.VisionTest,
                AppointmentDate = DateTime.Now.AddDays(5),
                PaidFees = 10
            };

            var expectedEntity = new TestAppointment { TestAppointmentID = 10 };

            _testAppointmentRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestAppointment, bool>>>(), null))
                .ReturnsAsync(false);
            _testServiceMock.Setup(s => s.HasPassedAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _testServiceMock.Setup(s => s.RequiresRetakeAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<TestAppointment>(fakeCreateDto)).Returns(expectedEntity);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.AddTestAppointmentAsync(fakeCreateDto, currentUserId);

            // Assert
            Assert.Null(result);

            _testAppointmentRepoMock.Verify(r => r.AddAsync(It.IsAny<TestAppointment>()), Times.Once);

            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);

            _testAppointmentRepoMock.Verify(r => r.FindAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task AddTestAppointmentAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            int currentUserId = 1;
            var fakeTestAppointmentDto = GetFakeTestAppointmentCreateDtos()[0];
            var expectedTestAppointment = GetFakeTestAppointments()[0];

            _testAppointmentRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestAppointment, bool>>>(), null))
                .ReturnsAsync(false);
            _testServiceMock.Setup(s => s.HasPassedAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            _testServiceMock.Setup(s => s.RequiresRetakeAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<TestAppointment>(fakeTestAppointmentDto)).Returns(expectedTestAppointment);

            _testAppointmentRepoMock.Setup(r => r.AddAsync(It.IsAny<TestAppointment>()))
                .ThrowsAsync(new Exception("Database Error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _service.AddTestAppointmentAsync(fakeTestAppointmentDto, currentUserId));

            // Assert
            _testAppointmentRepoMock.Verify(r => r.AddAsync(It.IsAny<TestAppointment>()), Times.Once);

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task AddTestAppointmentAsync_ThrowsInvalidOperation_WhenStreetTestPrerequisiteNotMet()
        {
            // Arrange
            var dto = new TestAppointmentCreateDto
            {
                LocalDrivingLicenseApplicationID = 1,
                TestTypeID = (int)EnTestType.StreetTest
            };

            _testAppointmentRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestAppointment, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(false);
            _testServiceMock.Setup(s => s.HasPassedAsync(dto.LocalDrivingLicenseApplicationID, dto.TestTypeID)).ReturnsAsync(false);

            _testServiceMock.Setup(s => s.HasPassedAsync(dto.LocalDrivingLicenseApplicationID, (int)EnTestType.WrittenTest))
                .ReturnsAsync(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddTestAppointmentAsync(dto, 1));
            Assert.Equal("Cannot schedule Street Test before passing Written Test.", ex.Message);
        }

        [Fact]
        public async Task AddTestAppointmentAsync_ThrowsInvalidOperation_WhenApplicantAlreadyPassedSameTest()
        {
            // Arrange
            var dto = new TestAppointmentCreateDto { LocalDrivingLicenseApplicationID = 1, TestTypeID = (int)EnTestType.VisionTest };

            _testAppointmentRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestAppointment, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(false);

            _testServiceMock.Setup(s => s.HasPassedAsync(dto.LocalDrivingLicenseApplicationID, dto.TestTypeID))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddTestAppointmentAsync(dto, 1));
            Assert.Equal("Applicant already passed this test.", ex.Message);
        }

        [Fact]
        public async Task AddTestAppointmentAsync_ThrowsInvalidOperation_WhenWrittenTestPrerequisiteNotMet()
        {
            // Arrange
            var dto = new TestAppointmentCreateDto
            {
                LocalDrivingLicenseApplicationID = 1,
                TestTypeID = (int)EnTestType.WrittenTest
            };

            _testAppointmentRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestAppointment, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(false);
            _testServiceMock.Setup(s => s.HasPassedAsync(dto.LocalDrivingLicenseApplicationID, dto.TestTypeID)).ReturnsAsync(false);

            _testServiceMock.Setup(s => s.HasPassedAsync(dto.LocalDrivingLicenseApplicationID, (int)EnTestType.VisionTest))
                .ReturnsAsync(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddTestAppointmentAsync(dto, 1));
            Assert.Equal("Cannot schedule Written Test before passing Vision Test.", ex.Message);
        }
        #endregion

        #region UpdateTestAppointmentAsync
        [Fact]
        public async Task UpdateTestAppointmentAsync_ThrowsResourceNotFoundException_WhenAppointmentNotFound()
        {
            // Arrange
            var updateDto = new TestAppointmentUpdateDto
            {
                TestAppointmentID = 999,
                AppointmentDate = DateTime.Now.AddDays(1)
            };

            _testAppointmentRepoMock.Setup(r => r.GetByIdAsync(updateDto.TestAppointmentID))
                .ReturnsAsync((TestAppointment)null!);

            // Act
            var action = () => _service.UpdateTestAppointmentAsync(updateDto);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(action);

            Assert.Equal($"Appointment with ID {updateDto.TestAppointmentID} was not found.", exception.Message);

            _testAppointmentRepoMock.Verify(r => r.Update(It.IsAny<TestAppointment>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateTestAppointmentAsync_ReturnsDto_WhenUpdateIsSuccessful()
        {
            // Arrange
            var updateDto = new TestAppointmentUpdateDto
            {
                TestAppointmentID = 1,
                AppointmentDate = DateTime.Now.AddDays(2)
            };

            var existingAppointment = new TestAppointment { TestAppointmentID = 1, IsLocked = false };
            var expectedResultDto = new TestAppointmentDto { TestAppointmentID = 1, AppointmentDate = updateDto.AppointmentDate };

            _testAppointmentRepoMock.Setup(r => r.GetByIdAsync(updateDto.TestAppointmentID))
                .ReturnsAsync(existingAppointment);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _testAppointmentRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<TestAppointment, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(existingAppointment);

            _mapperMock.Setup(m => m.Map<TestAppointmentDto>(existingAppointment)).Returns(expectedResultDto);

            // Act
            var result = await _service.UpdateTestAppointmentAsync(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.AppointmentDate, result.AppointmentDate);

            _testAppointmentRepoMock.Verify(r => r.Update(It.IsAny<TestAppointment>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTestAppointmentAsync_ReturnsFalse_WhenDatabaseUpdateFails()
        {
            // Arrange
            var updateDto = new TestAppointmentUpdateDto { TestAppointmentID = 1, AppointmentDate = DateTime.Now.AddDays(2) };
            var existingAppointment = new TestAppointment { TestAppointmentID = 1, IsLocked = false };

            _testAppointmentRepoMock.Setup(r => r.GetByIdAsync(updateDto.TestAppointmentID))
                .ReturnsAsync(existingAppointment);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.UpdateTestAppointmentAsync(updateDto);

            // Assert
            //Assert.False(result);
        }

        [Fact]
        public async Task UpdateTestAppointmentAsync_ThrowsInvalidOperation_WhenAppointmentIsLocked()
        {
            // Arrange
            var updateDto = new TestAppointmentUpdateDto { TestAppointmentID = 5, AppointmentDate = DateTime.Now.AddDays(1) };
            var lockedAppointment = new TestAppointment { TestAppointmentID = 5, IsLocked = true };

            _testAppointmentRepoMock.Setup(r => r.GetByIdAsync(updateDto.TestAppointmentID))
                .ReturnsAsync(lockedAppointment);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateTestAppointmentAsync(updateDto));
            Assert.Contains("locked and cannot be updated", ex.Message);

            _testAppointmentRepoMock.Verify(r => r.Update(It.IsAny<TestAppointment>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTestAppointmentAsync_ThrowsInvalidOperation_WhenDateIsInPast()
        {
            // Arrange
            var updateDto = new TestAppointmentUpdateDto
            {
                TestAppointmentID = 5,
                AppointmentDate = DateTime.Now.AddDays(-1)
            };
            var appointment = new TestAppointment { TestAppointmentID = 5, IsLocked = false };

            _testAppointmentRepoMock.Setup(r => r.GetByIdAsync(updateDto.TestAppointmentID))
                .ReturnsAsync(appointment);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateTestAppointmentAsync(updateDto));
            Assert.Equal("Cannot set an appointment date in the past.", ex.Message);
        }

        [Fact]
        public async Task UpdateTestAppointmentAsync_ThrowsException_WhenUnexpectedDatabaseErrorOccurs()
        {
            // Arrange
            var updateDto = new TestAppointmentUpdateDto
            {
                TestAppointmentID = 1,
                AppointmentDate = DateTime.Now.AddDays(2)
            };

            _testAppointmentRepoMock.Setup(r => r.GetByIdAsync(updateDto.TestAppointmentID))
                .ThrowsAsync(new Exception("Database connection lost"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateTestAppointmentAsync(updateDto));

            Assert.Contains("An error occurred while updating the Test Appointment record", exception.Message);
            _testAppointmentRepoMock.Verify(r => r.Update(It.IsAny<TestAppointment>()), Times.Never);

        }
        #endregion 
    }
}
