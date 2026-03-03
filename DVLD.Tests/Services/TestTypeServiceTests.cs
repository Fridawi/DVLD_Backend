using AutoMapper;
using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.DTOs.TestTypes;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.INFRASTRUCTURE.Repositories;
using DVLD.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq.Expressions;

namespace DVLD.Tests.Services
{
    public class TestTypeServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<TestType>> _testTypeRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<TestTypeService>> _loggerMock;
        private readonly TestTypeService _service;

        public TestTypeServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _testTypeRepoMock = new Mock<IGenericRepository<TestType>>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<TestTypeService>>();
            _unitOfWorkMock.Setup(u => u.TestTypes).Returns(_testTypeRepoMock.Object);
            _service = new TestTypeService(_unitOfWorkMock.Object, _loggerMock.Object, _mapperMock.Object);
        }

        #region Data 
        private List<TestType> GetFakeTestTypes()
        {
            return new List<TestType>()
            {
                new TestType {
                    TestTypeID = 1,
                    Title = "Vision Test",
                    Description = "An eye examination to ensure the applicant's sight meets driving standards.",
                    Fees = 10.00f
                },
                new TestType {
                    TestTypeID = 2,
                    Title = "Written (Theory) Test",
                    Description = "A computer-based test covering traffic laws, signs, and general driving rules.",
                    Fees = 20.00f
                },
                new TestType {
                    TestTypeID = 3,
                    Title = "Practical (Street) Test",
                    Description = "A real-world driving test to evaluate the applicant's ability to operate a vehicle safely.",
                    Fees = 30.00f
                }
            };
        }
        private List<TestTypeDto> GetFakeTestTypeDtos()
        {
            return new List<TestTypeDto>()
            {
                new TestTypeDto {
                    TestTypeID = 1,
                    Title = "Vision Test",
                    Description = "An eye examination to ensure the applicant's sight meets driving standards.",
                    Fees = 10.00f
                },
                new TestTypeDto {
                    TestTypeID = 2,
                    Title = "Written (Theory) Test",
                    Description = "A computer-based test covering traffic laws, signs, and general driving rules.",
                    Fees = 20.00f
                },
                new TestTypeDto {
                    TestTypeID = 3,
                    Title = "Practical (Street) Test",
                    Description = "A real-world driving test to evaluate the applicant's ability to operate a vehicle safely.",
                    Fees = 30.00f
                }
            };
        }
        #endregion

        #region GetAllTestTypeAsync
        [Fact]
        public async Task GetAllTestTypeAsync_ReturnsAllTestTypes_WhenDataExists()
        {
            // Arrange
            var fakeTestTypes = GetFakeTestTypes();
            var fackTestTypesDtos = GetFakeTestTypeDtos();
            int pageNumber = 1;
            int pageSize = 10;
            int expectedTotalCount = 50;

            _testTypeRepoMock
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<TestType, bool>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<TestType, object>>>(),
                    It.IsAny<EnOrderByDirection>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()
                )
                ).ReturnsAsync(fakeTestTypes);

            _testTypeRepoMock
                .Setup(r => r.CountAsync(It.IsAny<Expression<Func<TestType, bool>>>()))
                .ReturnsAsync(expectedTotalCount);

            _mapperMock.Setup(m => m.Map<IEnumerable<TestTypeDto>>(fakeTestTypes)).Returns(fackTestTypesDtos);

            // Act
            var result = await _service.GetAllTestTypesAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(fackTestTypesDtos.Count(), result.Data.Count());
            Assert.Equal(expectedTotalCount, result.TotalCount);
            Assert.Equal(pageNumber, result.PageNumber);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Equal("Vision Test", result.Data.First().Title);
            Assert.Equal("Practical (Street) Test", result.Data.Last().Title);

            _testTypeRepoMock.Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<TestType, bool>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<TestType, object>>>(),
                    It.IsAny<EnOrderByDirection>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()
                ), Times.Once);

            _testTypeRepoMock.Verify(r => r.CountAsync(It.IsAny<Expression<Func<TestType, bool>>>()), Times.Once);
        }
        #endregion

        #region GetTestTypeByIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetTestTypeByIdAsync_ThrowsValidationException_WhenTestTypeIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result =  _service.GetTestTypeByIdAsync(id);

            // Assert
            var exception =await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetTestTypeByIdAsync_ThrowsResourceNotFoundException_WhenTestTypeIsNotFound()
        {
            // Arrange
            _testTypeRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<TestType,
                bool>>>()))
                .ReturnsAsync((TestType)null!);

            // Act
            var result = _service.GetTestTypeByIdAsync(100);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("TestType with ID ", exception.Message);
        }

        [Fact]
        public async Task GetTestTypeByIdAsync_ReturnsTestTypeDto_WhenTestTypeIsFound()
        {
            // Arrange
            var expectedTestType = GetFakeTestTypes()[0];
            var expectedTestTypeDto = GetFakeTestTypeDtos()[0];

            _testTypeRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<TestType,
                bool>>>()))
                .ReturnsAsync(expectedTestType);

            _mapperMock.Setup(m=>m.Map<TestTypeDto>(expectedTestType)).Returns(expectedTestTypeDto);

            // Act
            var result =await _service.GetTestTypeByIdAsync(100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTestTypeDto.TestTypeID, result.TestTypeID);
        }
        #endregion

        #region AddTestTypeAsync
        [Fact]
        public async Task AddTestTypeAsync_ThrowsConflictException_WhenTestTypeTitleIsExist()
        {
            // Arrange
            var fakeTestTypeDto = GetFakeTestTypeDtos()[0];
            _testTypeRepoMock.Setup(r=>r.IsExistAsync(It.IsAny<Expression<Func<TestType, bool>>>())).ReturnsAsync(true);
            // Act
            var result = _service.AddTestTypeAsync(fakeTestTypeDto);
            // Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => result);
            Assert.Contains("The Test Type Title", exception.Message);
            _testTypeRepoMock.Verify(r => r.AddAsync(It.IsAny<TestType>()), Times.Never);
        }

        [Fact]
        public async Task AddTestTypeAsync_ReturnsDto_WhenSuccessful()
        {
            // Arrange
            var fakeTestTypeDto = GetFakeTestTypeDtos()[0]; 
            var expectedTestTypeEntity = GetFakeTestTypes()[0];

            _testTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestType, bool>>>(), null))
                .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<TestType>(fakeTestTypeDto)).Returns(expectedTestTypeEntity);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.AddTestTypeAsync(fakeTestTypeDto);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(expectedTestTypeEntity.TestTypeID, result.TestTypeID);

            _testTypeRepoMock.Verify(r => r.AddAsync(It.IsAny<TestType>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddTestTypeAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange
            var fakeTestTypeDto = GetFakeTestTypeDtos()[0];
            var expectedTestTypeEntity = GetFakeTestTypes()[0];

            _testTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestType, bool>>>(), null))
                .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<TestType>(fakeTestTypeDto)).Returns(expectedTestTypeEntity);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.AddTestTypeAsync(fakeTestTypeDto);

            // Assert
            Assert.Null(result);

            _testTypeRepoMock.Verify(r => r.AddAsync(It.IsAny<TestType>()), Times.Once);

            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddTestTypeAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var fakeTestTypeDto = GetFakeTestTypeDtos()[0];
            var expectedTestType = GetFakeTestTypes()[0];

            _testTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestType, bool>>>())).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<TestType>(fakeTestTypeDto)).Returns(expectedTestType);
            _testTypeRepoMock.Setup(r => r.AddAsync(It.IsAny<TestType>())).ThrowsAsync(new Exception("Database Error"));
            // Act
            var result = _service.AddTestTypeAsync(fakeTestTypeDto);
            // Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => result);
            Assert.Contains("An error occurred while saving the Test Type record.", exception.Message);
            _testTypeRepoMock.Verify(r => r.AddAsync(It.IsAny<TestType>()), Times.Once);
        }
        #endregion

        #region UpdateTestTypeAsync
        [Fact]
        public async Task UpdateTestTypeAsync_ThrowsResourceNotFoundException_WhenTestTypeIsNotFound()
        {
            // Arrange
            var fakeTestTypeDto = new TestTypeDto() {TestTypeID = 100 };
            _testTypeRepoMock.Setup(r=>r.FindAsync(It.IsAny<Expression<Func<TestType,bool>>>())).ReturnsAsync((TestType)null!);

            // Act
            var result = _service.UpdateTestTypeAsync(fakeTestTypeDto);

            // Assert
            var exception =await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Cannot update: TestType with ID", exception.Message);
            _testTypeRepoMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<TestType, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTestTypeAsync_ThrowsConflictException_WhenTitleExistsForAnotherTestType()
        {
            // Arrange
            var dto = new TestTypeDto { TestTypeID = 1, Title = "New Title" };
            var existingInDb = new TestType { TestTypeID = 1, Title = "Old Title" };

            _testTypeRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TestType, 
                bool>>>())).ReturnsAsync(existingInDb); 

            _testTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestType, bool>>>())).ReturnsAsync(true);
            // Act
            var result = _service.UpdateTestTypeAsync(dto);
            // Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => result);
            Assert.Contains("The Test Type Title", exception.Message);

            _testTypeRepoMock.Verify(r => r.IsExistAsync(It.IsAny<Expression<Func<TestType, bool>>>()), Times.Once);
            _testTypeRepoMock.Verify(r => r.Update(It.IsAny<TestType>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTestTypeAsync_ReturnsDto_WhenTitleIsSameAsCurrentRecord()
        {
            // Arrange 
            var dto = new TestTypeDto { TestTypeID = 1, Title = "Vision Test", Fees = 55.5f };
            var existingInDb = new TestType { TestTypeID = 1, Title = "Vision Test", Fees = 10.0f };

            _testTypeRepoMock.Setup(r => r.FindAsync(tt=>tt.TestTypeID == dto.TestTypeID))
                             .ReturnsAsync(existingInDb);

            _testTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestType, bool>>>(), null))
                             .ReturnsAsync(false);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.UpdateTestTypeAsync(dto);

            // Assert 
            Assert.NotNull(result);
            Assert.Equal(dto.Fees, result.Fees);
            Assert.Equal("Vision Test", result.Title);

            _testTypeRepoMock.Verify(r => r.Update(It.IsAny<TestType>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTestTypeAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange 
            var dto = new TestTypeDto { TestTypeID = 1, Title = "Vision Test", Fees = 55.5f };
            var existingInDb = new TestType { TestTypeID = 1, Title = "Vision Test", Fees = 10.0f };

            _testTypeRepoMock.Setup(r => r.FindAsync(tt => tt.TestTypeID == dto.TestTypeID))
                             .ReturnsAsync(existingInDb);

            _testTypeRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<TestType, bool>>>(), null))
                             .ReturnsAsync(false);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.UpdateTestTypeAsync(dto);

            // Assert 
            Assert.Null(result);

            _testTypeRepoMock.Verify(r => r.Update(It.IsAny<TestType>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTestTypeAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var dto = new TestTypeDto { TestTypeID = 1, Title = "Vision Test", Fees = 55.5f };
            _testTypeRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TestType, bool>>>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var result = _service.UpdateTestTypeAsync(dto);
            var exception = await Assert.ThrowsAsync<Exception>(() => result );
            Assert.Contains($"An error occurred while updating the Test Type record", exception.Message);
        }
        #endregion 
    }
}
