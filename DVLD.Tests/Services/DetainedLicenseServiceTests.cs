using AutoMapper;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses.DetainedLicenses;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace DVLD.Tests.Services
{
    public class DetainedLicenseServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<DetainedLicense>> _detainedLicenseRepoMock;
        private readonly Mock<IApplicationRepository> _applicationRepoMock;
        private readonly Mock<IGenericRepository<ApplicationType>> _appTypeRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUnitOfWorkTransaction> _dbContextTransactionMock;
        private readonly Mock<ILogger<DetainedLicenseService>> _loggerMock;
        private readonly DetainedLicenseService _service;

        public DetainedLicenseServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _detainedLicenseRepoMock = new Mock<IGenericRepository<DetainedLicense>>();
            _appTypeRepoMock = new Mock<IGenericRepository<ApplicationType>>();
            _applicationRepoMock = new Mock<IApplicationRepository>();
            _mapperMock = new Mock<IMapper>();
            _dbContextTransactionMock = new Mock<IUnitOfWorkTransaction>();
            _loggerMock = new Mock<ILogger<DetainedLicenseService>>();
            _unitOfWorkMock.Setup(u => u.Applications).Returns(_applicationRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_dbContextTransactionMock.Object);
            _unitOfWorkMock.Setup(u => u.DetainedLicenses).Returns(_detainedLicenseRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationTypes).Returns(_appTypeRepoMock.Object);
            _service = new DetainedLicenseService(_unitOfWorkMock.Object, _loggerMock.Object, _mapperMock.Object);
        }


        #region Data 
        private List<DetainedLicense> GetFakeDetainedLicenses()
        {
            return new List<DetainedLicense>
            {
                new DetainedLicense
                {
                    DetainID = 1,
                    LicenseID = 10,
                    DetainDate = DateTime.UtcNow.AddDays(-2),
                    FineFees = 150.0f,
                    IsReleased = false,
                    CreatedByUserID = 1,
                    CreatedByUserInfo = new User { UserID = 1, UserName = "Admin" },
                    LicenseInfo = new License {
                        LicenseID = 10,
                        ApplicationInfo = new Application {
                            PersonInfo = new Person { NationalNo = "N123" }
                        }
                    }
                },
                new DetainedLicense
                {
                    DetainID = 2,
                    LicenseID = 20,
                    DetainDate = DateTime.UtcNow.AddDays(-5),
                    FineFees = 200.0f,
                    IsReleased = true,
                    ReleaseDate = DateTime.UtcNow,
                    CreatedByUserID = 1,
                    CreatedByUserInfo = new User { UserID = 1, UserName = "Admin" },
                    ReleasedByUserID = 2,
                    ReleasedByUserInfo = new User { UserID = 2, UserName = "Manager" },
                    LicenseInfo = new License {
                        LicenseID = 20,
                        ApplicationInfo = new Application {
                            PersonInfo = new Person { NationalNo = "N456" }
                        }
                    }
                }
            };
        }

        private List<DetainedLicenseDto> GetFakeDetainedLicenseDtos()
        {
            return new List<DetainedLicenseDto>
            {
                new DetainedLicenseDto
                {
                    DetainID = 1,
                    LicenseID = 10,
                    DetainDate = DateTime.UtcNow.AddDays(-2),
                    FineFees = 150.0f,
                    IsReleased = false,
                    NationalNo = "N123",
                    CreatedByUserName = "Admin",
                    CreatedByUserID = 1
                },
                new DetainedLicenseDto
                {
                    DetainID = 2,
                    LicenseID = 20,
                    DetainDate = DateTime.UtcNow.AddDays(-5),
                    FineFees = 200.0f,
                    IsReleased = true,
                    ReleaseDate = DateTime.UtcNow,
                    NationalNo = "N456",
                    CreatedByUserName = "Admin",
                    CreatedByUserID = 1,
                    ReleasedByUserName = "Manager",
                    ReleasedByUserID = 2
                }
            };
        }
        #endregion


        #region GetAllDetainedLicensesAsync
        [Fact]
        public async Task GetAllDetainedLicensesAsync_ReturnsPagedResult()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            var fakeDetainedLicenses = GetFakeDetainedLicenses(); 
            var fakeDetainedLicenseDtos = GetFakeDetainedLicenseDtos();
            int totalCount = fakeDetainedLicenses.Count();

            _unitOfWorkMock.Setup(u => u.DetainedLicenses.FindAllAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>(), 
                It.IsAny<string[]>(),                                
                It.IsAny<bool>(),                                    
                It.IsAny<Expression<Func<DetainedLicense, object>>>(), 
                It.IsAny<EnOrderByDirection>(),                     
                It.IsAny<int?>(),                                   
                It.IsAny<int?>()                                     
            )).ReturnsAsync(fakeDetainedLicenses);

            _unitOfWorkMock.Setup(u => u.DetainedLicenses.CountAsync(It.IsAny<Expression<Func<DetainedLicense, bool>>>()))
                .ReturnsAsync(totalCount);

            _mapperMock.Setup(m => m.Map<IEnumerable<DetainedLicenseDto>>(It.IsAny<IEnumerable<DetainedLicense>>()))
                .Returns(fakeDetainedLicenseDtos);

            // Act
            var result = await _service.GetAllDetainedLicensesAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResultDto<DetainedLicenseDto>>(result);
            Assert.Equal(totalCount, result.TotalCount);
            Assert.Equal(pageNumber, result.PageNumber);
            Assert.Equal(fakeDetainedLicenseDtos.Count(), result.Data.Count());

            _unitOfWorkMock.Verify(u => u.DetainedLicenses.FindAllAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>(),
                It.IsAny<string[]>(),
                false,
                It.IsAny<Expression<Func<DetainedLicense, object>>>(),
                EnOrderByDirection.Descending,
                0, 
                pageSize
            ), Times.Once);
        }
        #endregion

        #region GetDetainedLicenseByIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetDetainedLicenseByIdAsync_ThrowsValidationException_WhenDetainedLicenseIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.GetDetainedLicenseByIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetDetainedLicenseByIdAsync_ThrowsResourceNotFoundException_WhenDetainedLicenseIsNotFound()
        {
            // Arrange
            _detainedLicenseRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((DetainedLicense)null!);

            // Act
            var result = _service.GetDetainedLicenseByIdAsync(100);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Detained License with ID ", exception.Message);
        }

        [Fact]
        public async Task GetDetainedLicenseByIdAsync_ReturnsTestAppointmentDto_WhenDetainedLicenseIsFound()
        {
            // Arrange
            var fakeDetainedLicenses = GetFakeDetainedLicenses();
            var fakeDetainedLicenseDtos = GetFakeDetainedLicenseDtos();

            _detainedLicenseRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(fakeDetainedLicenses[0]);

            _mapperMock
                .Setup(m => m.Map<DetainedLicenseDto>(It.IsAny<DetainedLicense>()))
                .Returns(fakeDetainedLicenseDtos[0]);

            // Act
            var result = await _service.GetDetainedLicenseByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.DetainID);

            _detainedLicenseRepoMock.Verify(r => r.FindAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>(),
                It.IsAny<string[]>()), Times.Once);

            _mapperMock.Verify(m => m.Map<DetainedLicenseDto>(It.IsAny<DetainedLicense>()), Times.Once);

        }
        #endregion

        #region GetDetainedLicenseByLicenseIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetDetainedLicenseByLicenseIdAsync_ThrowsValidationException_WhenLicenseIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.GetDetainedLicenseByLicenseIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid License ID", exception.Message);
        }

        [Fact]
        public async Task GetDetainedLicenseByLicenseIdAsync_ReturnsNull_WhenDetainedLicenseIsNotFound()
        {
            // Arrange
            _detainedLicenseRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((DetainedLicense)null!);

            // Act
            var result =await _service.GetDetainedLicenseByLicenseIdAsync(100);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDetainedLicenseByLicenseIdAsync_ReturnsTestAppointmentDto_WhenDetainedLicenseIsFound()
        {
            // Arrange
            var fakeDetainedLicenses = GetFakeDetainedLicenses();
            var fakeDetainedLicenseDtos = GetFakeDetainedLicenseDtos();

            _detainedLicenseRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(fakeDetainedLicenses[0]);

            _mapperMock
                .Setup(m => m.Map<DetainedLicenseDto>(It.IsAny<DetainedLicense>()))
                .Returns(fakeDetainedLicenseDtos[0]);

            // Act
            var result = await _service.GetDetainedLicenseByLicenseIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.DetainID);

            _detainedLicenseRepoMock.Verify(r => r.FindAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>(),
                It.IsAny<string[]>()), Times.Once);

            _mapperMock.Verify(m => m.Map<DetainedLicenseDto>(It.IsAny<DetainedLicense>()), Times.Once);

        }
        #endregion

        #region IsLicenseDetainedAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task IsLicenseDetainedAsync_ReturnsFalse_WhenLicenseIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result =await _service.IsLicenseDetainedAsync(id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsLicenseDetainedAsync_ReturnsTrue_WhenLicenseIsDetained()
        {
            // Arrange
            _detainedLicenseRepoMock.Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>())).ReturnsAsync(true);

            // Act
            var result = await _service.IsLicenseDetainedAsync(10);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsLicenseDetainedAsync_ReturnsFalse_WhenLicenseIsNotDetained()
        {
            // Arrange
            _detainedLicenseRepoMock.Setup(r => r.IsExistAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>())).ReturnsAsync(false);
            // Act
            var result = await _service.IsLicenseDetainedAsync(10);
            // Assert
            Assert.False(result);
        }

        #endregion

        #region DetainLicenseAsync

        [Fact]
        public async Task DetainLicenseAsync_ThrowsValidationException_WhenDetainDtoIsNull()
        {
            // Arrange & Act
            var result = _service.DetainLicenseAsync(null!, 1);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Detain data is null.", exception.Message);
        }

        [Theory]
        [InlineData(0, 150)] 
        [InlineData(10, -5)] 
        public async Task DetainLicenseAsync_ThrowsValidationException_WhenDataIsInvalid(int licenseId, float fineFees)
        {
            // Arrange
            var createDto = new DetainLicenseCreateDto { LicenseID = licenseId, FineFees = fineFees };

            // Act
            var result = _service.DetainLicenseAsync(createDto, 1);

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => result);
        }

        [Fact]
        public async Task DetainLicenseAsync_ThrowsResourceNotFoundException_WhenLicenseDoesNotExist()
        {
            // Arrange
            var createDto = new DetainLicenseCreateDto { LicenseID = 99, FineFees = 100 };
            _unitOfWorkMock.Setup(u => u.Licenses.IsExistAsync(It.IsAny<Expression<Func<License, bool>>>()))
                .ReturnsAsync(false);

            // Act
            var result = _service.DetainLicenseAsync(createDto, 1);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task DetainLicenseAsync_ThrowsValidationException_WhenLicenseIsAlreadyDetained()
        {
            // Arrange
            var createDto = new DetainLicenseCreateDto { LicenseID = 10, FineFees = 100 };

            _unitOfWorkMock.Setup(u => u.Licenses.IsExistAsync(It.IsAny<Expression<Func<License, bool>>>()))
                .ReturnsAsync(true);

            _detainedLicenseRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<DetainedLicense, bool>>>()))
                .ReturnsAsync(true);

            // Act
            var result = _service.DetainLicenseAsync(createDto, 1);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("already detained", exception.Message);
        }

        [Fact]
        public async Task DetainLicenseAsync_ReturnsDetainedLicenseDto_WhenDetainIsSuccessful()
        {
            // Arrange
            var createDto = new DetainLicenseCreateDto { LicenseID = 10, FineFees = 100 };
            var fakeDetainedLicenses = GetFakeDetainedLicenses();
            var fakeDetainedLicenseDtos = GetFakeDetainedLicenseDtos();

            _unitOfWorkMock.Setup(u => u.Licenses.IsExistAsync(It.IsAny<Expression<Func<License, bool>>>())).ReturnsAsync(true);
            _detainedLicenseRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<DetainedLicense, bool>>>())).ReturnsAsync(false);

            _detainedLicenseRepoMock.Setup(r => r.AddAsync(It.IsAny<DetainedLicense>())).Callback<DetainedLicense>(dl => dl.DetainID = 1).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _detainedLicenseRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<DetainedLicense, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(fakeDetainedLicenses[0]);
            _mapperMock.Setup(m => m.Map<DetainedLicenseDto>(It.IsAny<DetainedLicense>()))
                .Returns(fakeDetainedLicenseDtos[0]);

            // Act
            var result = await _service.DetainLicenseAsync(createDto, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fakeDetainedLicenseDtos[0].DetainID, result.DetainID);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DetainLicenseAsync_ReturnsNull_WhenSaveFails()
        {
            // Arrange
            var createDto = new DetainLicenseCreateDto { LicenseID = 10, FineFees = 100 };
            _unitOfWorkMock.Setup(u => u.Licenses.IsExistAsync(It.IsAny<Expression<Func<License, bool>>>())).ReturnsAsync(true);
            _detainedLicenseRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<DetainedLicense, bool>>>())).ReturnsAsync(false);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.DetainLicenseAsync(createDto, 1);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ReleaseLicenseAsync
        [Fact]
        public async Task ReleaseLicenseAsync_ThrowsResourceNotFoundException_WhenNoActiveDetentionFound()
        {
            // Arrange
            _detainedLicenseRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((DetainedLicense)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
                _service.ReleaseLicenseAsync(10, 1));

            Assert.Contains("No active detention record found", exception.Message);
        }

        [Fact]
        public async Task ReleaseLicenseAsync_ThrowsResourceNotFound_WhenNoActiveDetentionFound()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.DetainedLicenses.FindAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((DetainedLicense)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
                _service.ReleaseLicenseAsync(10, 1));

            Assert.Contains("No active detention record found", exception.Message);
        }

        [Fact]
        public async Task ReleaseLicenseAsync_ReturnsDto_WhenReleaseIsSuccessful()
        {
            // Arrange
            int licenseId = 10, releaseAppId = 500, userId = 1;

            var targetDetainedLicense = new DetainedLicense
            {
                DetainID = 1,
                LicenseID = licenseId,
                IsReleased = false,
                LicenseInfo = new License
                {
                    DriverInfo = new Driver { PersonID = 50 }
                }
            };

            var expectedDto = new DetainedLicenseDto
            {
                LicenseID = licenseId,
                IsReleased = true,
                ReleaseApplicationID = releaseAppId
            };

            _unitOfWorkMock.Setup(u => u.DetainedLicenses.FindAsync(
                It.IsAny<Expression<Func<DetainedLicense, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(targetDetainedLicense);

            _unitOfWorkMock.Setup(u => u.ApplicationTypes.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 15.0f });

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map<DetainedLicenseDto>(It.IsAny<DetainedLicense>()))
                .Returns(expectedDto);

            // Act
            var result = await _service.ReleaseLicenseAsync(licenseId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(targetDetainedLicense.IsReleased);
            Assert.Equal(userId, targetDetainedLicense.ReleasedByUserID);
            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task ReleaseLicenseAsync_ThrowsException_WhenSaveFails()
        {
            // Arrange
            int licenseId = 10, userId = 1;
            var targetDetainedLicense = new DetainedLicense
            {
                LicenseID = licenseId,
                IsReleased = false,
                LicenseInfo = new License { DriverInfo = new Driver { PersonID = 50 } }
            };

            _unitOfWorkMock.Setup(u => u.DetainedLicenses.FindAsync(It.IsAny<Expression<Func<DetainedLicense, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(targetDetainedLicense);

            _unitOfWorkMock.Setup(u => u.ApplicationTypes.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 15.0f });

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _service.ReleaseLicenseAsync(licenseId, userId));

            Assert.Contains("Failed to update", exception.Message);

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task ReleaseLicenseAsync_RethrowsResourceNotFound_WhenThrownInsideTransaction()
        {
            // Arrange
            var targetDetainedLicense = new DetainedLicense
            {
                LicenseID = 10,
                LicenseInfo = new License { DriverInfo = new Driver { PersonID = 50 } }
            };

            _unitOfWorkMock.Setup(u => u.DetainedLicenses.FindAsync(It.IsAny<Expression<Func<DetainedLicense, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(targetDetainedLicense);

            _unitOfWorkMock.Setup(u => u.ApplicationTypes.GetByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new ResourceNotFoundException("App Type Missing"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
                _service.ReleaseLicenseAsync(10, 1));

            Assert.Equal("App Type Missing", exception.Message);
            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
        }
        #endregion
    }
}
