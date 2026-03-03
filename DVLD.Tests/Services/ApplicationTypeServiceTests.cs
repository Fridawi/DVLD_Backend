using AutoMapper;
using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.DTOs.Users;
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
    public class ApplicationTypeServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<ApplicationType>> _applicationTypeRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<ApplicationTypeService>> _loggerMock;
        private readonly ApplicationTypeService _service;

        public ApplicationTypeServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _applicationTypeRepoMock = new Mock<IGenericRepository<ApplicationType>>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<ApplicationTypeService>>();

            _unitOfWorkMock.Setup(u => u.ApplicationTypes).Returns(_applicationTypeRepoMock.Object);
            _service = new ApplicationTypeService(_unitOfWorkMock.Object, _loggerMock.Object, _mapperMock.Object);
        }

        #region Data 
        private List<ApplicationType> GetfakeAppTypes()
        {
            return new List<ApplicationType>()
            {
                new ApplicationType { ApplicationTypeID = 1, Title = "New Local Driving License Service", Fees = 15 },
                new ApplicationType { ApplicationTypeID = 2, Title = "Renew Driving License Service", Fees = 7 },
                new ApplicationType { ApplicationTypeID = 3, Title = "Replacement for a Lost Driving License", Fees = 10 }
            };
        }

        private List<ApplicationTypeDto> GetfakeAppTypeDtos()
        {
            return new List<ApplicationTypeDto>()
            {
                new ApplicationTypeDto { ApplicationTypeID = 1, Title = "New Local Driving License Service", Fees = 15 },
                new ApplicationTypeDto { ApplicationTypeID = 2, Title = "Renew Driving License Service", Fees = 7 },
                new ApplicationTypeDto { ApplicationTypeID = 3, Title = "Replacement for a Lost Driving License", Fees = 10 }
            };
        }
        #endregion

        #region GetAllApplicationTypesAsync
        [Fact]
        public async Task GetAllApplicationTypesAsync_ReturnsAllApplicationTypes_WhenDataExists()
        {
            // Arrange
            var fakeAppTypes = GetfakeAppTypes();
            var fakeAppTypeDtos = GetfakeAppTypeDtos();
            int pageNumber = 1;
            int pageSize = 10;
            int expectedTotalCount = 50;

            _applicationTypeRepoMock
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<ApplicationType, bool>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<ApplicationType, object>>>(),
                    It.IsAny<EnOrderByDirection>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()
                )
                ).ReturnsAsync(fakeAppTypes);

            _applicationTypeRepoMock
                .Setup(r => r.CountAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>()))
                .ReturnsAsync(expectedTotalCount);

            _mapperMock.Setup(m => m.Map<IEnumerable<ApplicationTypeDto>>(fakeAppTypes)).Returns(fakeAppTypeDtos);

            // Act
            var result = await _service.GetAllApplicationTypesAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(fakeAppTypeDtos.Count(), result.Data.Count());
            Assert.Equal(expectedTotalCount, result.TotalCount);
            Assert.Equal(pageNumber, result.PageNumber);
            Assert.Equal(pageSize, result.PageSize);

            Assert.Equal("New Local Driving License Service", result.Data.First().Title);

            _applicationTypeRepoMock.Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<ApplicationType, bool>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<ApplicationType, object>>>(),
                    It.IsAny<EnOrderByDirection>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()
                ), Times.Once);

            _applicationTypeRepoMock.Verify(r => r.CountAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>()), Times.Once);
        }
        #endregion

        #region GetApplicationTypeByIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetApplicationTypeByIdAsync_ThrowsValidationException_WhenApplicationTypeIDIsLessOrEqualZero(int id)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.GetApplicationTypeByIdAsync(id));
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetApplicationTypeByIdAsync_ThrowsResourceNotFoundException_WhenApplicationTypeNotFound()
        {
            // Arrange
            _applicationTypeRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>()))
                                    .ReturnsAsync((ApplicationType)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.GetApplicationTypeByIdAsync(100));
            Assert.Contains("Application Type with ApplicationTypeID", exception.Message);
        }

        [Fact]
        public async Task GetApplicationTypeByIdAsync_ReturnsApplicationTypeDto_WhenApplicationTypeFound()
        {
            // Arrange
            var expectedApplicationType = GetfakeAppTypes()[0];
            var expectedDto = GetfakeAppTypeDtos()[0];

            _applicationTypeRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>()))
                                    .ReturnsAsync(expectedApplicationType);

            _mapperMock.Setup(m => m.Map<ApplicationTypeDto>(expectedApplicationType)).Returns(expectedDto);

            // Act
            var result = await _service.GetApplicationTypeByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.ApplicationTypeID, result.ApplicationTypeID);
        }
        #endregion

        #region AddApplicationTypeAsync
        [Fact]
        public async Task AddApplicationTypeAsync_ThrowsConflictException_WhenApplicationTypeTitleExists()
        {
            // Arrange
            var fakeApplicationTypeDto = GetfakeAppTypeDtos()[0];
            _applicationTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>()))
                                    .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _service.AddApplicationTypeAsync(fakeApplicationTypeDto));
            Assert.Contains("The Application Type Title", exception.Message);
            _applicationTypeRepoMock.Verify(r => r.AddAsync(It.IsAny<ApplicationType>()), Times.Never);
        }

        [Fact]
        public async Task AddApplicationTypeAsync_ReturnsDto_WhenApplicationTypeAddedSuccessfully()
        {
            // Arrange
            var fakeApplicationTypeDto = GetfakeAppTypeDtos()[0];
            var fakeApplicationType = GetfakeAppTypes()[0];

            _applicationTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>())).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<ApplicationType>(fakeApplicationTypeDto)).Returns(fakeApplicationType);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.AddApplicationTypeAsync(fakeApplicationTypeDto);

            // Assert
            Assert.NotNull(result); // تم التغيير من True إلى NotNull
            Assert.Equal(fakeApplicationType.ApplicationTypeID, result.ApplicationTypeID); // التأكد من الـ ID الجديد
            _applicationTypeRepoMock.Verify(r => r.AddAsync(It.IsAny<ApplicationType>()), Times.Once);
        }

        [Fact]
        public async Task AddApplicationTypeAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange
            var fakeApplicationTypeDto = GetfakeAppTypeDtos()[0];
            var fakeApplicationType = GetfakeAppTypes()[0];

            _applicationTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>())).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<ApplicationType>(fakeApplicationTypeDto)).Returns(fakeApplicationType);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.AddApplicationTypeAsync(fakeApplicationTypeDto);

            // Assert
            Assert.Null(result); 
        }

        [Fact]
        public async Task AddApplicationTypeAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var fakeApplicationTypeDto = GetfakeAppTypeDtos()[0];
            _applicationTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>())).ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<ApplicationType>(It.IsAny<ApplicationTypeDto>())).Throws(new Exception("DB Error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.AddApplicationTypeAsync(fakeApplicationTypeDto));
            Assert.Contains("An error occurred while saving the Application Type record", exception.Message);
        }
        #endregion

        #region UpdateApplicationTypeAsync
        [Fact]
        public async Task UpdateApplicationTypeAsync_ThrowsResourceNotFoundException_WhenApplicationTypeNotFound()
        {
            // Arrange
            var fakeApplicationTypeDto = new ApplicationTypeDto { ApplicationTypeID = 100 };
            _applicationTypeRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>(), null, true))
                                    .ReturnsAsync((ApplicationType)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(()
                => _service.UpdateApplicationTypeAsync(fakeApplicationTypeDto));
            Assert.Contains("Cannot update: Application Type with ID", exception.Message);
            _applicationTypeRepoMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task UpdateApplicationTypeAsync_ThrowsConflictException_WhenApplicationTypeTitleExistsForAnotherRecord()
        {
            // Arrange
            var fakeApplicationTypeDto = new ApplicationTypeDto { ApplicationTypeID = 1, Title = "New Title" };
            var existingInDb = new ApplicationType { ApplicationTypeID = 1, Title = "Old Title" };

            _applicationTypeRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>(), null, true))
                                    .ReturnsAsync(existingInDb);
            _applicationTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>()))
                                    .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateApplicationTypeAsync(fakeApplicationTypeDto));
            Assert.Contains("The Application Type Title", exception.Message);
            _applicationTypeRepoMock.Verify(r => r.IsExistAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>()), Times.Once);
            _applicationTypeRepoMock.Verify(r => r.Update(It.IsAny<ApplicationType>()), Times.Never);
        }

        [Fact]
        public async Task UpdateApplicationTypeAsync_ReturnsDto_WhenSuccessful()
        {
            // Arrange
            var dto = new ApplicationTypeDto { ApplicationTypeID = 1, Title = "Updated Title", Fees = 20 };
            var existingInDb = new ApplicationType { ApplicationTypeID = 1, Title = "Old Title", Fees = 10 };

            _applicationTypeRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>(), null, true))
                                    .ReturnsAsync(existingInDb);
            _applicationTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>())).ReturnsAsync(false);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.UpdateApplicationTypeAsync(dto);

            // Assert
            Assert.NotNull(result); 
            Assert.Equal(dto.Title, result.Title); 
            _applicationTypeRepoMock.Verify(r => r.Update(It.IsAny<ApplicationType>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateApplicationTypeAsync_ReturnsNull_WhenFailedToUpdateInDatabase()
        {
            // Arrange 
            var dto = new ApplicationTypeDto { ApplicationTypeID = 1, Title = "Updated Title", Fees = 20 };
            var existingInDb = new ApplicationType { ApplicationTypeID = 1, Title = "Old Title", Fees = 10 };

            _applicationTypeRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>(), null, true))
                                     .ReturnsAsync(existingInDb);

            _applicationTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>()))
                                     .ReturnsAsync(false);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.UpdateApplicationTypeAsync(dto);

            // Assert 
            Assert.Null(result);

            _applicationTypeRepoMock.Verify(r => r.Update(It.IsAny<ApplicationType>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateApplicationTypeAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var dto = new ApplicationTypeDto { ApplicationTypeID = 1 };
            _applicationTypeRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ApplicationType, bool>>>(), null, true))
                                    .ThrowsAsync(new Exception("Connection Failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.UpdateApplicationTypeAsync(dto));
            Assert.Contains("An error occurred while updating the Application Type record", exception.Message);
        }
        #endregion
    }
}