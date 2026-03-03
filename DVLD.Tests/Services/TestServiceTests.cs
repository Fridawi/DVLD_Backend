using AutoMapper;
using DVLD.CORE.DTOs.Tests;
using DVLD.CORE.DTOs.TestTypes;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.INFRASTRUCTURE.Repositories;
using DVLD.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace DVLD.Tests.Services
{
    public class TestServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUnitOfWorkTransaction> _dbContextTransactionMock;

        private readonly Mock<IGenericRepository<Test>> _testRepoMock;
        private readonly Mock<IGenericRepository<TestAppointment>> _testAppointmentRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<TestService>> _loggerMock;
        private readonly TestService _service;

        public TestServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _testRepoMock = new Mock<IGenericRepository<Test>>();
            _testAppointmentRepoMock = new Mock<IGenericRepository<TestAppointment>>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<TestService>>();
            _dbContextTransactionMock = new Mock<IUnitOfWorkTransaction>();

            _unitOfWorkMock.Setup(u => u.Tests).Returns(_testRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.TestAppointments).Returns(_testAppointmentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_dbContextTransactionMock.Object);

            _service = new TestService(_unitOfWorkMock.Object, _mapperMock.Object, _loggerMock.Object);
        }


        #region Data 
        private List<Test> GetFakeTests()
        {
            return new List<Test>()
            {
                new Test { TestID = 1, TestAppointmentID = 101, TestResult = true, Notes = "Passed with minor mistakes", CreatedByUserID = 1 },
                new Test { TestID = 2, TestAppointmentID = 102, TestResult = false, Notes = "Failed - Reckless driving", CreatedByUserID = 1 },
                new Test { TestID = 3, TestAppointmentID = 103, TestResult = true, Notes = null, CreatedByUserID = 2 }
            };
        }

        private List<TestDto> GetFakeTestDtos()
        {
            return new List<TestDto>()
            {
                new TestDto { TestID = 1, TestAppointmentID = 101, TestResult = true, Notes = "Passed with minor mistakes", CreatedByUserID = 1 },
                new TestDto { TestID = 2, TestAppointmentID = 102, TestResult = false, Notes = "Failed - Reckless driving", CreatedByUserID = 1 },
                new TestDto { TestID = 3, TestAppointmentID = 103, TestResult = true, Notes = null!, CreatedByUserID = 2 }

            };
        }

        private List<TestCreateDto> GetFakeTestCreateDtos()
        {
            return new List<TestCreateDto>()
            {
                new TestCreateDto { TestAppointmentID = 104, TestResult = true, Notes = "Initial Vision Test" },
                new TestCreateDto { TestAppointmentID = 105, TestResult = false, Notes = "Poor focus" }
            };
        }

        private List<TestUpdateDto> GetFakeTestUpdateDtos()
        {
            return new List<TestUpdateDto>()
            {
                new TestUpdateDto { TestID = 1, TestResult = true, Notes = "Updated notes: exceptional performance" },
                new TestUpdateDto { TestID = 2, TestResult = true, Notes = "Changed from fail to pass after review" }
            };
        }
        #endregion

        #region GetAllTestsAsync
        public async Task GetAllTestTypeAsync_ReturnsAllTestTypes_WhenDataExists()
        {
            // Arrange
            var fakeTests = GetFakeTests();
            var fakeTestDtos = GetFakeTestDtos();
            int pageNumber = 1;
            int pageSize = 10;
            int expectedTotalCount = 50;

            _testRepoMock
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<Test, bool>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Test, object>>>(),
                    It.IsAny<EnOrderByDirection>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()
                )
                ).ReturnsAsync(fakeTests);

            _testRepoMock
                .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Test, bool>>>()))
                .ReturnsAsync(expectedTotalCount);

            _mapperMock.Setup(m => m.Map<IEnumerable<TestDto>>(fakeTests)).Returns(fakeTestDtos);

            // Act
            var result = await _service.GetAllTestsAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(fakeTestDtos.Count(), result.Data.Count());
            Assert.Equal(expectedTotalCount, result.TotalCount);
            Assert.Equal(pageNumber, result.PageNumber);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Equal("Passed with minor mistakes", result.Data.First().Notes);

            _testRepoMock.Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<Test, bool>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Test, object>>>(),
                    It.IsAny<EnOrderByDirection>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()
                ), Times.Once);

            _testRepoMock.Verify(r => r.CountAsync(It.IsAny<Expression<Func<Test, bool>>>()), Times.Once);
        }
        #endregion

        #region GetTestByIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetTestByIdAsync_ThrowsValidationException_WhenTestIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.GetTestByIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetTestByIdAsync_ThrowsResourceNotFoundException_WhenTestIsNotFound()
        {
            // Arrange
            _testRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Test, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync((Test)null!);

            // Act
            var result = _service.GetTestByIdAsync(100);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Test with ID ", exception.Message);
        }

        [Fact]
        public async Task GetTestByIdAsync_ReturnsTestTypeDto_WhenTestIsFound()
        {
            // Arrange
            var expectedTest = GetFakeTests()[0];
            var expectedTestDto = GetFakeTestDtos()[0];

            _testRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Test, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(expectedTest);

            _mapperMock.Setup(m => m.Map<TestDto>(expectedTest)).Returns(expectedTestDto);

            // Act
            var result = await _service.GetTestByIdAsync(100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTestDto.TestID, result.TestID);
        }
        #endregion

        #region PassedAllTestsAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task PassedAllTestsAsync_ThrowsValidationException_WhenlocalAppIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.PassedAllTestsAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task PassedAllTestsAsync_ReturnsTrue_WhenPersonPassedAllThreeTests()
        {
            // Arrange
            int localAppID = 1;
            var fakeTests = new List<Test>
            {
                new Test { TestResult = true },
                new Test { TestResult = true },
                new Test { TestResult = true }
            };

            _testRepoMock.Setup(r => r.FindAllAsync(
                It.IsAny<Expression<Func<Test, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()
            )).ReturnsAsync(fakeTests);

            // Act
            var result = await _service.PassedAllTestsAsync(localAppID);

            // Assert
            Assert.True(result);
            _testRepoMock.Verify(r => r.FindAllAsync(It.IsAny<Expression<Func<Test, bool>>>(), It.IsAny<string[]>(), false), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task PassedAllTestsAsync_ReturnsFalse_WhenPassedTestCountIsIncorrect(int count)
        {
            // Arrange
            int localAppID = 1;
            var fakeTests = Enumerable.Repeat(new Test { TestResult = true }, count).ToList();

            _testRepoMock.Setup(r => r.FindAllAsync(
                It.IsAny<Expression<Func<Test, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()
            )).ReturnsAsync(fakeTests);

            // Act
            var result = await _service.PassedAllTestsAsync(localAppID);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetLastTestPerPersonAndLicenseClassAsync
        [Theory]
        [InlineData(-1, 0, 0)]
        [InlineData(0, -1, 0)]
        [InlineData(0, 0, -1)]
        [InlineData(1, 0, 0)]
        [InlineData(0, 1, 0)]
        [InlineData(0, 0, 1)]
        public async Task GetLastTestPerPersonAndLicenseClassAsync_ThrowsValidationException_WhenPersonIDOrLocalAppIDOrTestTypeIDIsLessOrEqualZero(int personID, int localAppID, int testTypeID)
        {
            // Arrange & Act
            var result = _service.GetLastTestPerPersonAndLicenseClassAsync(personID, localAppID, testTypeID);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetLastTestPerPersonAndLicenseClassAsync_ReturnNull_WhenTestIsNotFound()
        {
            // Arrange
            int personID = 1, localAppID = 100, testTypeID = 1;

            _testRepoMock.Setup(r => r.FindAllAsync(
                It.IsAny<Expression<Func<Test, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<Test, object>>>(),
                It.IsAny<EnOrderByDirection>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
            .ReturnsAsync(new List<Test>());

            // Act
            var result = await _service.GetLastTestPerPersonAndLicenseClassAsync(personID, localAppID, testTypeID);

            // Assert
            Assert.Null(result);
            _mapperMock.Verify(m => m.Map<TestDto>(It.IsAny<Test>()), Times.Never);
        }

        [Fact]
        public async Task GetLastTestPerPersonAndLicenseClassAsync_ReturnsDto_WhenRecordExists()
        {
            // Arrange
            var fakeList = GetFakeTests();
            var expectedDto = GetFakeTestDtos()[0];

            _testRepoMock.Setup(r => r.FindAllAsync(
                It.IsAny<Expression<Func<Test, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<Test, object>>>(),
                It.IsAny<EnOrderByDirection>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()
            ))
            .ReturnsAsync(fakeList);

            _mapperMock.Setup(m => m.Map<TestDto>(It.IsAny<Test>()))
                       .Returns(expectedDto);

            // Act
            var result = await _service.GetLastTestPerPersonAndLicenseClassAsync(1, 10, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.TestID, result.TestID);
        }
        #endregion

        #region AddTestAsync
        [Fact]
        public async Task AddTestAsync_ShouldRollback_WhenDatabaseErrorOccurs()
        {
            // Arrange 
            int currentUserId = 1;
            var fakeDto = GetFakeTestCreateDtos()[0];
            var fakeTest = GetFakeTests()[0];

            _testRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Test, bool>>>())).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<Test>(fakeDto)).Returns(fakeTest);

            _testRepoMock.Setup(r => r.AddAsync(It.IsAny<Test>()))
                .ThrowsAsync(new Exception("Database connection failed during test insertion"));

            // Act & Assert 
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _service.AddTestAsync(fakeDto, currentUserId));

            Assert.Contains("Database connection failed", exception.Message);

            _testRepoMock.Verify(r => r.AddAsync(It.IsAny<Test>()), Times.Once);
            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task AddTestAsync_ShouldReturnNull_WhenDatabaseSaveFails()
        {
            // Arrange
            int currentUserId = 1;
            var fakeDto = GetFakeTestCreateDtos()[0]; 
            var fakeTestEntity = GetFakeTests()[0];
            var fakeAppointment = new TestAppointment { TestAppointmentID = fakeDto.TestAppointmentID, IsLocked = false };

            _testRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Test, bool>>>(), null))
                .ReturnsAsync(false);

            _testAppointmentRepoMock.Setup(r => r.GetByIdAsync(fakeDto.TestAppointmentID))
                .ReturnsAsync(fakeAppointment);

            _mapperMock.Setup(m => m.Map<Test>(fakeDto)).Returns(fakeTestEntity);

 
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.AddTestAsync(fakeDto, currentUserId);

            // Assert
            Assert.Null(result);
 
            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Never);

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Never);

            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddTestAsync_ReturnsDto_WhenTestIsAddedSuccessfully()
        {
            // Arrange
            int currentUserId = 1;
            var fakeDto = GetFakeTestCreateDtos()[0];  
            var fakeTestEntity = GetFakeTests()[0];    
            var fakeAppointment = new TestAppointment { TestAppointmentID = fakeDto.TestAppointmentID, IsLocked = false };
            var expectedResultDto = GetFakeTestDtos()[0];

            _testRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Test, bool>>>(), null))
                .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<Test>(fakeDto)).Returns(fakeTestEntity);
            _testAppointmentRepoMock.Setup(r => r.GetByIdAsync(fakeDto.TestAppointmentID)).ReturnsAsync(fakeAppointment);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _testRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Test, bool>>>(),
                It.IsAny<string[]>(),
                false))
                .ReturnsAsync(fakeTestEntity);

            _mapperMock.Setup(m => m.Map<TestDto>(fakeTestEntity)).Returns(expectedResultDto);

            // Act
            var result = await _service.AddTestAsync(fakeDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResultDto.TestID, result.TestID);

            Assert.True(fakeAppointment.IsLocked);

            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Once);
            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Never);

            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddTestAsync_ThrowsConflictException_WhenTestAlreadyExists()
        {
            // Arrange
            var fakeDto = GetFakeTestCreateDtos()[0];
            _testRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Test, bool>>>())).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() =>
                _service.AddTestAsync(fakeDto, 1));

            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task AddTestAsync_ShouldReturnNull_WhenDatabaseSaveFailsDetailed()
        {
            // Arrange
            int currentUserId = 1;
            var fakeDto = GetFakeTestCreateDtos()[0];
            var fakeTestEntity = GetFakeTests()[0];
            var fakeAppointment = new TestAppointment { TestAppointmentID = fakeDto.TestAppointmentID, IsLocked = false };

            _testRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Test, bool>>>(), null))
                .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<Test>(fakeDto)).Returns(fakeTestEntity);
            _testAppointmentRepoMock.Setup(r => r.GetByIdAsync(fakeDto.TestAppointmentID)).ReturnsAsync(fakeAppointment);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.AddTestAsync(fakeDto, currentUserId);

            // Assert
            Assert.Null(result);

            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Never);

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Never);

            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddTestAsync_ThrowsResourceNotFoundException_WhenAppointmentNotFound()
        {
            // Arrange
            int currentUserId = 1;
            var fakeDto = GetFakeTestCreateDtos()[0];
            var fakeTest = GetFakeTests()[0];

            _testRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Test, bool>>>())).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<Test>(fakeDto)).Returns(fakeTest);
            _testAppointmentRepoMock.Setup(r => r.GetByIdAsync(fakeDto.TestAppointmentID)).ReturnsAsync((TestAppointment)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
                _service.AddTestAsync(fakeDto, currentUserId));
            Assert.Contains("Test Appointment not found", exception.Message);

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task AddTestAsync_ShouldReturnNull_WhenDatabaseSaveFailsForTestRecord()
        {
            // Arrange
            int currentUserId = 1;
            var fakeDto = GetFakeTestCreateDtos()[0];
            var fakeTestEntity = GetFakeTests()[0];
            var fakeAppointment = new TestAppointment { TestAppointmentID = fakeDto.TestAppointmentID, IsLocked = false };

            _testRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Test, bool>>>(), null))
                .ReturnsAsync(false);
            _testAppointmentRepoMock.Setup(r => r.GetByIdAsync(fakeDto.TestAppointmentID))
                .ReturnsAsync(fakeAppointment);

            _mapperMock.Setup(m => m.Map<Test>(fakeDto)).Returns(fakeTestEntity);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.AddTestAsync(fakeDto, currentUserId);

            // Assert
            Assert.Null(result);

            _dbContextTransactionMock.Verify(t => t.CommitAsync(), Times.Never);

            _dbContextTransactionMock.Verify(t => t.RollbackAsync(), Times.Never);

            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }
        #endregion

        #region UpdateTestAsync
        [Fact]
        public async Task UpdateTestAsync_ThrowsResourceNotFoundException_WhenTestIsNotFound()
        {
            // Arrange
            var fakeTestUpdateDto = GetFakeTestUpdateDtos()[0];
            

            _testRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Test)null!);

            // Act
            var result = _service.UpdateTestAsync(fakeTestUpdateDto);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Test with ID", exception.Message);
            _testRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTestAsync_ReturnsUpdatedDto_WhenSuccessful()
        {
            // Arrange
            var fakeTestUpdateDto = GetFakeTestUpdateDtos()[0]; 
            var existingInDb = GetFakeTests()[0];
            var expectedResultDto = GetFakeTestDtos()[0];

            _testRepoMock.Setup(r => r.GetByIdAsync(fakeTestUpdateDto.TestID))
                .ReturnsAsync(existingInDb);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _testRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Test, bool>>>(),
                It.IsAny<string[]>(),
                false))
                .ReturnsAsync(existingInDb);

            _mapperMock.Setup(m => m.Map<TestDto>(existingInDb)).Returns(expectedResultDto);

            // Act
            var result = await _service.UpdateTestAsync(fakeTestUpdateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fakeTestUpdateDto.TestID, result.TestID);

            _testRepoMock.Verify(r => r.Update(It.IsAny<Test>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTestAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange             
            var fakeTestUpdateDto = GetFakeTestUpdateDtos()[0]; 
            var existingInDb = GetFakeTests()[0];

            _testRepoMock.Setup(r => r.GetByIdAsync(fakeTestUpdateDto.TestID))
                .ReturnsAsync(existingInDb);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.UpdateTestAsync(fakeTestUpdateDto);

            // Assert 
            Assert.Null(result);

            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);

            _testRepoMock.Verify(r => r.Update(It.IsAny<Test>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTestAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var fakeTestUpdateDto = GetFakeTestUpdateDtos()[0];
            _testRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var result = _service.UpdateTestAsync(fakeTestUpdateDto);

            var exception = await Assert.ThrowsAsync<Exception>(() => result);
            Assert.Contains($"An error occurred while updating the Test record.", exception.Message);
        }
        #endregion
    }
}
