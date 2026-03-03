using AutoMapper;
using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Drivers;
using DVLD.CORE.DTOs.Drivers;
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
    public class DriverServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<Driver>> _driverRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<DriverService>> _loggerMock;
        private readonly DriverService _service;

        public DriverServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _driverRepoMock = new Mock<IGenericRepository<Driver>>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<DriverService>>();
            _unitOfWorkMock.Setup(u => u.Drivers).Returns(_driverRepoMock.Object);
            _service = new DriverService(_unitOfWorkMock.Object, _loggerMock.Object, _mapperMock.Object);
        }

        #region Data 
        private List<Driver> GetFakeDrivers()
        {
            return new List<Driver>()
            {
                new Driver
                {
                    DriverID = 1,
                    PersonID = 1,
                    CreatedByUserID = 2,
                    CreatedDate = DateTime.Parse("2026-01-01")
                },

                new Driver
                {
                    DriverID = 2,
                    PersonID = 2,
                    CreatedByUserID = 3,
                    CreatedDate = DateTime.Parse("2026-02-01")
                },
            };
        }
        private List<DriverDto> GetFakeDriverDtos()
        {
            return new List<DriverDto>()
            {
                new DriverDto
                {
                    DriverID = 1,
                    PersonID = 1,
                    NationalNo = "N2746744",
                    FullName = "jone max oscar",
                    CreatedByUserID = 2,
                    CreatedDate = DateTime.Parse("2026-01-01")
                },

                new DriverDto
                {
                    DriverID = 2,
                    PersonID = 2,
                    NationalNo = "N253234",
                    FullName = "Max Alax Ben",
                    CreatedByUserID = 3,
                    CreatedDate = DateTime.Parse("2026-02-01")
                },
            };
        }
        #endregion


        #region GetAllDriversAsync
        [Fact]
        public async Task GetAllDriversAsync_ReturnsPagedResult_WhenDataExists()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            var fakeDrivers = GetFakeDrivers(); 
            var fakeDriverDtos = GetFakeDriverDtos();
            int totalCount = fakeDrivers.Count();

            _unitOfWorkMock.Setup(u => u.Drivers.FindAllAsync(
                It.IsAny<Expression<Func<Driver, bool>>>(),  
                It.IsAny<string[]>(),                        
                It.IsAny<bool>(),                            
                It.IsAny<Expression<Func<Driver, object>>>(),
                It.IsAny<EnOrderByDirection>(),              
                It.IsAny<int?>(),                            
                It.IsAny<int?>()                             
            )).ReturnsAsync(fakeDrivers);

            _unitOfWorkMock.Setup(u => u.Drivers.CountAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                .ReturnsAsync(totalCount);

            _mapperMock.Setup(m => m.Map<IEnumerable<DriverDto>>(fakeDrivers))
                .Returns(fakeDriverDtos);

            // Act
            var result = await _service.GetAllDriversAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResultDto<DriverDto>>(result);
            Assert.Equal(totalCount, result.TotalCount);
            Assert.Equal(2, result.Data.Count());

            Assert.Equal("jone max oscar", result.Data.First().FullName);
            Assert.Equal("Max Alax Ben", result.Data.Last().FullName);

            _unitOfWorkMock.Verify(u => u.Drivers.FindAllAsync(
                It.IsAny<Expression<Func<Driver, bool>>>(),
                It.IsAny<string[]>(),
                false,
                It.IsAny<Expression<Func<Driver, object>>>(),
                EnOrderByDirection.Descending,
                0,
                pageSize
            ), Times.Once);

            _unitOfWorkMock.Verify(u => u.Drivers.CountAsync(It.IsAny<Expression<Func<Driver, bool>>>()), Times.Once);
        }
        #endregion

        #region GetDriverByIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetDriverByIdAsync_ThrowsValidationException_WhenDriverIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.GetDriverByIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetDriverByIdAsync_ThrowsResourceNotFoundException_WhenDriverIsNotFound()
        {
            // Arrange
            _driverRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Driver,bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((Driver)null!);

            // Act
            var result = _service.GetDriverByIdAsync(100);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Driver with ID ", exception.Message);
        }

        [Fact]
        public async Task GetDriverByIdAsync_ReturnsDriverDto_WhenDriverIsFound()
        {
            // Arrange
            var expectedDriver = GetFakeDrivers()[0];
            var expectedDriverDto = GetFakeDriverDtos()[0];

            _driverRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Driver, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(expectedDriver);

            _mapperMock.Setup(m => m.Map<DriverDto>(expectedDriver)).Returns(expectedDriverDto);

            // Act
            var result = await _service.GetDriverByIdAsync(100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDriverDto.DriverID, result.DriverID);
        }
        #endregion

        #region GetDriverByPersonIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetDriverByPersonIdAsync_ThrowsValidationException_WhenPersonIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.GetDriverByPersonIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetDriverByPersonIdAsync_ThrowsResourceNotFoundException_WhenDriverIsNotFound()
        {
            // Arrange
            _driverRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Driver,bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((Driver)null!);

            // Act
            var result = _service.GetDriverByPersonIdAsync(100);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Driver with PersonID ", exception.Message);
        }

        [Fact]
        public async Task GetDriverByPersonIdAsync_ReturnsDriverDto_WhenDriverIsFound()
        {
            // Arrange
            var expectedDriver = GetFakeDrivers()[0];
            var expectedDriverDto = GetFakeDriverDtos()[0];

            _driverRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Driver, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(expectedDriver);

            _mapperMock.Setup(m => m.Map<DriverDto>(expectedDriver)).Returns(expectedDriverDto);

            // Act
            var result = await _service.GetDriverByPersonIdAsync(100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDriverDto.DriverID, result.DriverID);
        }
        #endregion

        #region AddDriverAsync
        [Fact]
        public async Task AddDriverAsync_ThrowsUnauthorizedAccessException_WhenCurrentRoleOrUserIDAllowed()
        {
            // Arrange
            int currentUserId = 3;
            string currentUserRole = "super user"; // Not Admin
            var personId = GetFakeDriverDtos()[0].PersonID;
            _driverRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Driver, bool>>>())).ReturnsAsync(false);
            // Act
            var result = _service.AddDriverAsync(personId, currentUserId,currentUserRole);
            // Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => result);
            Assert.Contains("You do not have permission to add drivers to the system.", exception.Message);
            _driverRepoMock.Verify(r => r.AddAsync(It.IsAny<Driver>()), Times.Never);
        }
        [Fact]
        public async Task AddDriverAsync_ThrowsConflictException_WhenDriverPersonIDIsExist()
        {
            // Arrange
            int currentUserId = 3;
            string currentUserRole = UserRoles.User; // Not Admin
            var personId = GetFakeDriverDtos()[0].PersonID;
            _driverRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Driver, bool>>>())).ReturnsAsync(true);
            // Act
            var result = _service.AddDriverAsync(personId, currentUserId,currentUserRole);
            // Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => result);
            Assert.Contains("The Driver PersonID ", exception.Message);
            _driverRepoMock.Verify(r => r.AddAsync(It.IsAny<Driver>()), Times.Never);
        }

        [Fact]
        public async Task AddDriverAsync_ShouldReturnDto_WhenDriverIsAddedSuccessfully()
        {
            // Arrange
            int currentUserId = 3;
            string currentUserRole = UserRoles.User;
            var personId = 10;
            var generatedDriverId = 5;

            var driverEntity = new Driver
            {
                DriverID = generatedDriverId,
                PersonID = personId,
                CreatedByUserID = currentUserId
            };

            var expectedDto = new DriverDto { DriverID = generatedDriverId, PersonID = personId };

            _driverRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                .ReturnsAsync(false);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _driverRepoMock.Setup(r => r.AddAsync(It.IsAny<Driver>()))
                .Callback<Driver>(d => d.DriverID = generatedDriverId)
                .Returns(Task.CompletedTask);

            _driverRepoMock.Setup(r => r.FindAsync(
                    It.IsAny<Expression<Func<Driver, bool>>>(),
                    It.IsAny<string[]>(),  
                    It.IsAny<bool>()))     
                .ReturnsAsync(driverEntity);

            _mapperMock.Setup(m => m.Map<DriverDto>(It.IsAny<Driver>())).Returns(expectedDto);

            // Act
            var result = await _service.AddDriverAsync(personId, currentUserId, currentUserRole);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(generatedDriverId, result.DriverID);
        }

        [Fact]
        public async Task AddDriverAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange
            int currentUserId = 3;
            string currentUserRole = UserRoles.User;
            var personId = 10;

            _driverRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Driver, bool>>>())).ReturnsAsync(false);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.AddDriverAsync(personId, currentUserId, currentUserRole);

            // Assert
            Assert.Null(result); 
        }

        [Fact]
        public async Task AddDriverAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            int currentUserId = 3;
            string currentUserRole = UserRoles.User; // Not Admin
            var personId = GetFakeDriverDtos()[0].PersonID;
            var expectedDriver = GetFakeDrivers()[0];

            _driverRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Driver, bool>>>())).ReturnsAsync(false);
            _driverRepoMock.Setup(r => r.AddAsync(It.IsAny<Driver>())).ThrowsAsync(new Exception("Database Error"));
            // Act
            var result = _service.AddDriverAsync(personId, currentUserId, currentUserRole);
            // Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => result);
            Assert.Contains("An error occurred while saving the Driver record.", exception.Message);
            _driverRepoMock.Verify(r => r.AddAsync(It.IsAny<Driver>()), Times.Once);
        }
        #endregion

    }
}
