using AutoMapper;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.DTOs.Licenses.InternationalLicenses;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DVLD.Tests.Services
{
    public class InternationalLicenseServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<InternationalLicense>> _internationalLicenseRepoMock;
        private readonly Mock<IGenericRepository<ApplicationType>> _applicationTypeRepoMock;
        private readonly Mock<IGenericRepository<License>> _licenseRepoMock;
        private readonly Mock<IApplicationRepository> _applicationRepoMock;
        private readonly Mock<IUnitOfWorkTransaction> _dbContextTransactionMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<InternationalLicenseService>> _loggerMock;
        private readonly InternationalLicenseService _service;


        public InternationalLicenseServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _internationalLicenseRepoMock = new Mock<IGenericRepository<InternationalLicense>>();
            _applicationTypeRepoMock = new Mock<IGenericRepository<ApplicationType>>();
            _licenseRepoMock = new Mock<IGenericRepository<License>>();
            _applicationRepoMock = new Mock<IApplicationRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<InternationalLicenseService>>();
            _dbContextTransactionMock = new Mock<IUnitOfWorkTransaction>();
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_dbContextTransactionMock.Object);
            _unitOfWorkMock.Setup(u => u.InternationalLicenses).Returns(_internationalLicenseRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Applications).Returns(_applicationRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationTypes).Returns(_applicationTypeRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Licenses).Returns(_licenseRepoMock.Object);
            _service = new InternationalLicenseService(
                _unitOfWorkMock.Object,
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
        private List<InternationalLicense> GetFakeInternationalLicenses()
        {
            return new List<InternationalLicense>
            {
                new InternationalLicense { InternationalLicenseID = 1, DriverID = 10, IsActive = true, ExpirationDate = DateTime.UtcNow.AddYears(1) },
                new InternationalLicense { InternationalLicenseID = 2, DriverID = 11, IsActive = false, ExpirationDate = DateTime.UtcNow.AddYears(-1) }
            };
        }

        private List<InternationalLicenseDto> GetFakeInternationalLicenseDtos()
        {
            return new List<InternationalLicenseDto>
            {
                new InternationalLicenseDto { InternationalLicenseID = 1, DriverID = 10, IsActive = true },
                new InternationalLicenseDto { InternationalLicenseID = 2, DriverID = 11, IsActive = false }
            };
        }

        private List<DriverInternationalLicenseDto> GetFakeDriverInternationalLicenseDtos()
        {
            return new List<DriverInternationalLicenseDto>
            {
                new DriverInternationalLicenseDto
                {
                    InternationalLicenseID = 1,
                    LicenseID = 100,
                    ApplicationID = 500,
                    DriverID = 10,
                    DriverFullName = "Ahmed Mohammed Ali",
                    NationalNo = "N123456",
                    Gender = 0,
                    GenderText = "Male",
                    IssueDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddYears(1),
                    IsActive = true,
                    DriverBirthDate = new DateOnly(1990, 1, 1),
                    LicenseClassName = "Ordinary driving license",
                    IssueReasonText = "First Time",
                    DriverImageUrl = "https://localhost:7001/uploads/people/Ahmed.jpg",
                    CreatedByUserID = 1
                },
                new DriverInternationalLicenseDto
                {
                    InternationalLicenseID = 2,
                    LicenseID = 101,
                    ApplicationID = 501,
                    DriverID = 11,
                    DriverFullName = "Sara Mahmod Hassen",
                    NationalNo = "N987654",
                    Gender = 1,
                    GenderText = "Female",
                    IssueDate = DateTime.UtcNow.AddYears(-1),
                    ExpirationDate = DateTime.UtcNow.AddDays(-1), 
                    IsActive = false,
                    DriverBirthDate = new DateOnly(1995, 5, 10),
                    LicenseClassName = "Ordinary driving license",
                    IssueReasonText = "First Time",
                    DriverImageUrl = null, 
                    CreatedByUserID = 1
                }
            };
        }
        #endregion


        #region GetAllInternationalLicensesAsync
        [Fact]
        public async Task GetAllInternationalLicensesAsync_ReturnsPagedResult_WhenDataExists()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            var fakeInternationalLicenses = GetFakeInternationalLicenses();
            var fakeInternationalLicenseDtos = GetFakeInternationalLicenseDtos();
            int totalCount = fakeInternationalLicenses.Count();

            _unitOfWorkMock.Setup(u => u.InternationalLicenses.FindAllAsync(
                It.IsAny<Expression<Func<InternationalLicense, bool>>>(),  
                It.IsAny<string[]>(),                                      
                It.IsAny<bool>(),                                          
                It.IsAny<Expression<Func<InternationalLicense, object>>>(),
                It.IsAny<EnOrderByDirection>(),                            
                It.IsAny<int?>(),                                          
                It.IsAny<int?>()                                          
            )).ReturnsAsync(fakeInternationalLicenses);

            _unitOfWorkMock.Setup(u => u.InternationalLicenses.CountAsync(
                It.IsAny<Expression<Func<InternationalLicense, bool>>>()
            )).ReturnsAsync(totalCount);

            _mapperMock.Setup(m => m.Map<IEnumerable<InternationalLicenseDto>>(It.IsAny<IEnumerable<InternationalLicense>>()))
                .Returns(fakeInternationalLicenseDtos);

            // Act
            var result = await _service.GetAllInternationalLicensesAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResultDto<InternationalLicenseDto>>(result);
            Assert.Equal(totalCount, result.TotalCount);
            Assert.Equal(fakeInternationalLicenseDtos.Count, result.Data.Count());

            Assert.Equal(fakeInternationalLicenseDtos.First().InternationalLicenseID, result.Data.First().InternationalLicenseID);
            Assert.Equal(fakeInternationalLicenseDtos.First().DriverID, result.Data.First().DriverID);

            // Verify
            _unitOfWorkMock.Verify(u => u.InternationalLicenses.FindAllAsync(
                It.IsAny<Expression<Func<InternationalLicense, bool>>>(),
                null,
                false,
                It.IsAny<Expression<Func<InternationalLicense, object>>>(),
                EnOrderByDirection.Descending,
                0, 
                pageSize
            ), Times.Once);
        }
        #endregion

        #region GetInternationalLicenseByIdAsync

        [Fact]
        public async Task GetInternationalLicenseByIdAsync_ReturnsInternationalLicense_WhenInternationalLicenseIdIsValid()
        {
            // Arrange
            int driverId = 100;
            var fakeInternationalLicense = GetFakeInternationalLicenses()[0];
            var fakeInternationalLicenseDto = GetFakeInternationalLicenseDtos()[0];

            _internationalLicenseRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(fakeInternationalLicense);

            _mapperMock.Setup(m => m.Map<InternationalLicenseDto>( fakeInternationalLicense))
                .Returns(fakeInternationalLicenseDto);

            // Act
            var result = await _service.GetInternationalLicenseByIdAsync(driverId);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(fakeInternationalLicenseDto.InternationalLicenseID, result.InternationalLicenseID);
            Assert.Equal(fakeInternationalLicenseDto.DriverID, result.DriverID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetInternationalLicenseByIdAsync_ThrowsValidationException_WhenInternationalLicenseIdIsInvalid(int invalidId)
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.GetInternationalLicenseByIdAsync(invalidId));
        }

        [Fact]
        public async Task GetInternationalLicenseByIdAsync_ThrowsResourceNotFoundException_WhenNoInternationalLicenseFound()
        {
            // Arrange
            int driverId = 999;
            _internationalLicenseRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((InternationalLicense)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.GetInternationalLicenseByIdAsync(driverId));
        }
        #endregion

        #region GetDriverInternationalLicenseByIdAsync

        [Fact]
        public async Task GetDriverInternationalLicenseByIdAsync_ReturnsDriverInternationalLicense_WhenInternationalLicenseIdIsValid()
        {
            // Arrange
            int driverId = 100;
            var fakeInternationalLicense = GetFakeInternationalLicenses()[0];
            var fakeDriverInternationalLicenseDto = GetFakeDriverInternationalLicenseDtos()[0];

            _internationalLicenseRepoMock.Setup(repo => repo.FindAsync(
                It.IsAny<Expression<Func<InternationalLicense,bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()
                )).ReturnsAsync(fakeInternationalLicense);

            _mapperMock.Setup(m => m.Map<DriverInternationalLicenseDto>(fakeInternationalLicense,
                It.IsAny<Action<IMappingOperationOptions<object, DriverInternationalLicenseDto>>>()))
                .Returns(fakeDriverInternationalLicenseDto);

            // Act
            var result = await _service.GetDriverInternationalLicenseByIdAsync(driverId);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(fakeDriverInternationalLicenseDto.InternationalLicenseID, result.InternationalLicenseID);
            Assert.Equal(fakeDriverInternationalLicenseDto.DriverID, result.DriverID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetDriverInternationalLicenseByIdAsync_ThrowsValidationException_WhenInternationalLicenseIdIsInvalid(int invalidId)
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.GetDriverInternationalLicenseByIdAsync(invalidId));
        }

        [Fact]
        public async Task GetDriverInternationalLicenseByIdAsync_ThrowsResourceNotFoundException_WhenNoInternationalLicenseFound()
        {
            // Arrange
            int driverId = 999;
            _internationalLicenseRepoMock.Setup(repo => repo.FindAsync(
                It.IsAny<Expression<Func<InternationalLicense, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()
                )).ReturnsAsync((InternationalLicense)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.GetDriverInternationalLicenseByIdAsync(driverId));
        }
        #endregion

        #region GetInternationalLicensesByDriverIdAsync

        [Fact]
        public async Task GetInternationalLicensesByDriverIdAsync_ReturnsPagedInternationalLicenses_WhenDriverIdIsValid()
        {
            // Arrange
            int driverId = 10;
            int pageNumber = 1;
            int pageSize = 10;
            var fakeInternationalLicenses = GetFakeInternationalLicenses();
            var fakeInternationalLicenseDtos = GetFakeInternationalLicenseDtos();
            int totalCount = fakeInternationalLicenses.Count();

   
            _unitOfWorkMock.Setup(u => u.InternationalLicenses.FindAllAsync(
                It.IsAny<Expression<Func<InternationalLicense, bool>>>(),
                null,
                false,
                It.IsAny<Expression<Func<InternationalLicense, object>>>(),
                EnOrderByDirection.Descending,
                0,
                pageSize
            )).ReturnsAsync(fakeInternationalLicenses);

            _unitOfWorkMock.Setup(u => u.InternationalLicenses.CountAsync(
                It.IsAny<Expression<Func<InternationalLicense, bool>>>()
            )).ReturnsAsync(totalCount);

            _mapperMock.Setup(m => m.Map<IEnumerable<InternationalLicenseDto>>(fakeInternationalLicenses))
                .Returns(fakeInternationalLicenseDtos);

            // Act
            var result = await _service.GetInternationalLicensesByDriverIdAsync(driverId, pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResultDto<InternationalLicenseDto>>(result);
            Assert.Equal(totalCount, result.TotalCount);
            Assert.Equal(fakeInternationalLicenseDtos.First().InternationalLicenseID, result.Data.First().InternationalLicenseID);
            Assert.Equal(driverId, result.Data.First().DriverID);

            _unitOfWorkMock.Verify(u => u.InternationalLicenses.FindAllAsync(
                It.IsAny<Expression<Func<InternationalLicense, bool>>>(),
                null, false, It.IsAny<Expression<Func<InternationalLicense, object>>>(),
                EnOrderByDirection.Descending, 0, pageSize
            ), Times.Once);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetInternationalLicensesByDriverIdAsync_ThrowsValidationException_WhenDriverIdIsInvalid(int invalidId)
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _service.GetInternationalLicensesByDriverIdAsync(invalidId, pageNumber, pageSize));

            Assert.Equal("Invalid Driver ID", exception.Message);
        }
        #endregion

        #region IssueInternationalLicenseAsync
        [Fact]
        public async Task IssueInternationalLicenseAsync_ReturnsLicenseDto_WhenSavedSuccessfully()
        {
            // Arrange
            var userId = 1;
            var createDto = new InternationalLicenseCreateDto { LocalLicenseID = 100 };

            var driver = new Driver { DriverID = 10, PersonID = 5 };
            var localLicense = new License
            {
                LicenseID = 100,
                DriverID = 10,
                IsActive = true,
                ExpirationDate = DateTime.UtcNow.AddYears(1),
                LicenseClassID = 3,
                DriverInfo = driver
            };
            var fakeDto = new InternationalLicenseDto { InternationalLicenseID = 1, DriverID = 10 };

            _licenseRepoMock.Setup(r => r.GetByIdAsync(createDto.LocalLicenseID))
                .ReturnsAsync(localLicense);

            _licenseRepoMock.Setup(r => r.FindAsync(
                    It.IsAny<Expression<Func<License, bool>>>(),
                    It.IsAny<string[]>(),
                    false))
                .ReturnsAsync(localLicense);

            _internationalLicenseRepoMock.Setup(r => r.FindAsync(
                    It.IsAny<Expression<Func<InternationalLicense, bool>>>(),
                    null,
                    false))
                .ReturnsAsync((InternationalLicense)null!);

            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 50 });

            _internationalLicenseRepoMock.Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<InternationalLicense, bool>>>(),
                    null, true, null, It.IsAny<EnOrderByDirection>()))
                .ReturnsAsync(new List<InternationalLicense>());

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<InternationalLicenseDto>(It.IsAny<InternationalLicense>()))
                .Returns(fakeDto);

            // Act
            var result = await _service.IssueInternationalLicenseAsync(createDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fakeDto.InternationalLicenseID, result.InternationalLicenseID);
            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task IssueInternationalLicenseAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange
            var userId = 1;
            var createDto = new InternationalLicenseCreateDto { LocalLicenseID = 100 };
            var localLicense = new License
            {
                LicenseID = 100,
                DriverID = 10,
                IsActive = true,
                ExpirationDate = DateTime.UtcNow.AddYears(1),
                LicenseClassID = 3,
                DriverInfo = new Driver { PersonID = 5, DriverID = 10 }
            };

            _licenseRepoMock.Setup(r => r.GetByIdAsync(createDto.LocalLicenseID)).ReturnsAsync(localLicense);
            _licenseRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<License, bool>>>(), It.IsAny<string[]>(), false))
                .ReturnsAsync(localLicense);

            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 50 });

            _internationalLicenseRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<InternationalLicense, bool>>>(), null, false))
                .ReturnsAsync((InternationalLicense)null!);

            _internationalLicenseRepoMock.Setup(r => r.FindAllAsync(It.IsAny<Expression<Func<InternationalLicense, bool>>>(),
                null, true, null, It.IsAny<EnOrderByDirection>()))
                .ReturnsAsync(new List<InternationalLicense>());

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await  _service.IssueInternationalLicenseAsync(createDto, userId);

            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task IssueInternationalLicenseAsync_ThrowsValidationException_WhenInputsAreInvalid(int invalidLocalLicenseId)
        {
            // Arrange
            var createDto = new InternationalLicenseCreateDto { LocalLicenseID = invalidLocalLicenseId };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.IssueInternationalLicenseAsync(createDto, 1));
        }

        [Fact]
        public async Task IssueInternationalLicenseAsync_ThrowsResourceNotFoundException_WhenLocalLicenseDoesNotExist()
        {
            // Arrange
            var createDto = new InternationalLicenseCreateDto { LocalLicenseID = 100 };
            _unitOfWorkMock.Setup(u => u.Licenses.GetByIdAsync(createDto.LocalLicenseID)).ReturnsAsync((License)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.IssueInternationalLicenseAsync(createDto, 1));
        }

        [Fact]
        public async Task IssueInternationalLicenseAsync_ThrowsValidationException_WhenLocalLicenseIsNotActive()
        {
            // Arrange
            var createDto = new InternationalLicenseCreateDto { LocalLicenseID = 100 };
            var inactiveLicense = new License { LicenseID = 100, IsActive = false }; // غير نشطة
            _unitOfWorkMock.Setup(u => u.Licenses.GetByIdAsync(createDto.LocalLicenseID)).ReturnsAsync(inactiveLicense);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.IssueInternationalLicenseAsync(createDto, 1));
        }

        [Fact]
        public async Task IssueInternationalLicenseAsync_ThrowsValidationException_WhenLicenseClassIsInvalid()
        {
            // Arrange
            var createDto = new InternationalLicenseCreateDto { LocalLicenseID = 100 };
            var wrongClassLicense = new License { LicenseID = 100, IsActive = true, ExpirationDate = DateTime.UtcNow.AddYears(1), LicenseClassID = 1 }; // Class 1 instead of 3

            _unitOfWorkMock.Setup(u => u.Licenses.GetByIdAsync(createDto.LocalLicenseID)).ReturnsAsync(wrongClassLicense);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.IssueInternationalLicenseAsync(createDto, 1));
        }

        [Fact]
        public async Task IssueInternationalLicenseAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var userId = 1;
            var createDto = new InternationalLicenseCreateDto { LocalLicenseID = 100 };
            var localLicense = new License
            {
                LicenseID = 100,
                IsActive = true,
                ExpirationDate = DateTime.UtcNow.AddYears(1),
                LicenseClassID = 3,
                DriverID = 10,
                DriverInfo = new Driver { PersonID = 5 }
            };


            _licenseRepoMock.Setup(r => r.GetByIdAsync(createDto.LocalLicenseID)).ReturnsAsync(localLicense);
            _licenseRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<License, bool>>>(), It.IsAny<string[]>(), false))
                .ReturnsAsync(localLicense);
            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 50 });

            _internationalLicenseRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<InternationalLicense, bool>>>(), null, false))
                .ReturnsAsync((InternationalLicense)null!);

            _internationalLicenseRepoMock.Setup(r => r.FindAllAsync(It.IsAny<Expression<Func<InternationalLicense, bool>>>(), null, true, null,
                It.IsAny<EnOrderByDirection>()))
                .ReturnsAsync(new List<InternationalLicense>());

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.IssueInternationalLicenseAsync(createDto, userId));

            Assert.Contains("An error occurred while issuing the international license", exception.Message);

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
        }
        #endregion

        #region DeactivateInternationalLicenseAsync
        [Fact]
        public async Task DeactivateInternationalLicenseAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            var licenseId = 1;
            var license = new InternationalLicense { InternationalLicenseID = licenseId, IsActive = true };

            _internationalLicenseRepoMock.Setup(r => r.GetByIdAsync(licenseId)).ReturnsAsync(license);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.DeactivateInternationalLicenseAsync(licenseId);

            // Assert
            Assert.True(result);
            Assert.False(license.IsActive);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeactivateInternationalLicenseAsync_ReturnsFalse_WhenLicenseDoesNotExist()
        {
            // Arrange
            var licenseId = 1;
            _internationalLicenseRepoMock.Setup(r => r.GetByIdAsync(licenseId)).ReturnsAsync((InternationalLicense)null!);

            // Act
            var result = await _service.DeactivateInternationalLicenseAsync(licenseId);

            // Assert
            Assert.False(result);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeactivateInternationalLicenseAsync_ThrowsValidationException_WhenIdIsInvalid(int invalidId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.DeactivateInternationalLicenseAsync(invalidId));
        }

        [Fact]
        public async Task DeactivateInternationalLicenseAsync_ReturnsFalse_WhenDatabaseSaveFails()
        {
            // Arrange
            var licenseId = 1;
            var license = new InternationalLicense { InternationalLicenseID = licenseId, IsActive = true };

            _internationalLicenseRepoMock.Setup(r => r.GetByIdAsync(licenseId)).ReturnsAsync(license);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0); // فشل الحفظ

            // Act
            var result = await _service.DeactivateInternationalLicenseAsync(licenseId);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsDriverEligibleForInternationalLicenseAsync

        [Fact]
        public async Task IsDriverEligibleForInternationalLicenseAsync_ReturnsTrue_WhenDriverIsEligible()
        {
            // Arrange
            var localId = 100;
            var localLicense = new License
            {
                LicenseID = localId,
                DriverID = 10,
                IsActive = true,
                ExpirationDate = DateTime.UtcNow.AddYears(1),
                LicenseClassID = 3
            };

            _licenseRepoMock.Setup(r => r.GetByIdAsync(localId)).ReturnsAsync(localLicense);
            _internationalLicenseRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<InternationalLicense, bool>>>(), null, false))
                .ReturnsAsync((InternationalLicense)null!);

            // Act
            var result = await _service.IsDriverEligibleForInternationalLicenseAsync(localId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsDriverEligibleForInternationalLicenseAsync_ThrowsConflictException_WhenDriverAlreadyHasActiveLicense()
        {
            // Arrange
            var localId = 100;
            var driverId = 10;
            var localLicense = new License { LicenseID = localId, DriverID = driverId, IsActive = true, ExpirationDate = DateTime.UtcNow.AddYears(1), LicenseClassID = 3 };
            var activeIntLicense = new InternationalLicense { InternationalLicenseID = 500, DriverID = driverId, IsActive = true, ExpirationDate = DateTime.UtcNow.AddYears(1) };

            _licenseRepoMock.Setup(r => r.GetByIdAsync(localId)).ReturnsAsync(localLicense);
            _internationalLicenseRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<InternationalLicense, bool>>>(), null, false))
                .ReturnsAsync(activeIntLicense);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _service.IsDriverEligibleForInternationalLicenseAsync(localId));
        }

        [Fact]
        public async Task IsDriverEligibleForInternationalLicenseAsync_ThrowsValidationException_WhenLicenseIsExpired()
        {
            // Arrange
            var localId = 100;
            var expiredLicense = new License { LicenseID = localId, IsActive = true, ExpirationDate = DateTime.UtcNow.AddDays(-1), LicenseClassID = 3 };

            _licenseRepoMock.Setup(r => r.GetByIdAsync(localId)).ReturnsAsync(expiredLicense);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.IsDriverEligibleForInternationalLicenseAsync(localId));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task IsDriverEligibleForInternationalLicenseAsync_ThrowsValidationException_WhenIdIsInvalid(int invalidId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.IsDriverEligibleForInternationalLicenseAsync(invalidId));
        }

        [Fact]
        public async Task IsDriverEligibleForInternationalLicenseAsync_ThrowsResourceNotFoundException_WhenLicenseNotFound()
        {
            // Arrange
            _licenseRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync((License)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.IsDriverEligibleForInternationalLicenseAsync(100));
        }

        [Fact]
        public async Task IsDriverEligibleForInternationalLicenseAsync_ThrowsValidationException_WhenClassIsNot3()
        {
            // Arrange
            var localId = 100;
            var wrongClassLicense = new License { LicenseID = localId, IsActive = true, ExpirationDate = DateTime.UtcNow.AddYears(1), LicenseClassID = 1 };

            _licenseRepoMock.Setup(r => r.GetByIdAsync(localId)).ReturnsAsync(wrongClassLicense);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.IsDriverEligibleForInternationalLicenseAsync(localId));
        }

        #endregion

        #region GetActiveInternationalLicenseIdByDriverIdAsync

        [Fact]
        public async Task GetActiveInternationalLicenseIdByDriverIdAsync_ReturnsLicenseId_WhenActiveLicenseExists()
        {
            // Arrange
            var driverId = 10;
            var activeLicense = new InternationalLicense
            {
                InternationalLicenseID = 50,
                DriverID = driverId,
                IsActive = true,
                ExpirationDate = DateTime.UtcNow.AddMonths(6)
            };

            _internationalLicenseRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<InternationalLicense, bool>>>(), null, false))
                .ReturnsAsync(activeLicense);

            // Act
            var result = await _service.GetActiveInternationalLicenseIdByDriverIdAsync(driverId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(activeLicense.InternationalLicenseID, result);
        }

        [Fact]
        public async Task GetActiveInternationalLicenseIdByDriverIdAsync_ReturnsNull_WhenNoActiveLicenseExists()
        {
            // Arrange
            var driverId = 10;
            _internationalLicenseRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<InternationalLicense, bool>>>(), null, false))
                .ReturnsAsync((InternationalLicense)null!);

            // Act
            var result = await _service.GetActiveInternationalLicenseIdByDriverIdAsync(driverId);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetActiveInternationalLicenseIdByDriverIdAsync_ThrowsValidationException_WhenDriverIdIsInvalid(int invalidId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.GetActiveInternationalLicenseIdByDriverIdAsync(invalidId));
        }

        #endregion
    }

}

