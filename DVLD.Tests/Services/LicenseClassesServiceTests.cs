using AutoMapper;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.DTOs.TestTypes;
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
    public class LicenseClassesServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<LicenseClass>> _licenseClassRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<LicenseClassService>> _loggerMock;
        private readonly LicenseClassService _service;

        public LicenseClassesServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _licenseClassRepoMock = new Mock<IGenericRepository<LicenseClass>>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<LicenseClassService>>();
            _unitOfWorkMock.Setup(u => u.LicenseClasses).Returns(_licenseClassRepoMock.Object);
            _service = new LicenseClassService(_unitOfWorkMock.Object, _loggerMock.Object, _mapperMock.Object);
        }

        #region Data 
        private List<LicenseClass> GetFakeLicenseClasses()
        {
            return new List<LicenseClass>()
            {
                new LicenseClass {
                    LicenseClassID = 1, ClassName = "Class 1 - Small Motorcycle",
                    ClassDescription = "Small motorcycles with engine capacity less than 125cc.",
                    MinimumAllowedAge = 16, DefaultValidityLength = 5, ClassFees = 15.0f
                },
                new LicenseClass {
                    LicenseClassID = 2, ClassName = "Class 2 - Heavy Motorcycle",
                    ClassDescription = "Motorcycles with engine capacity more than 125cc.",
                    MinimumAllowedAge = 18, DefaultValidityLength = 5, ClassFees = 30.0f
                },
                new LicenseClass {
                    LicenseClassID = 3, ClassName = "Class 3 - Ordinary driving license",
                    ClassDescription = "Standard cars and small pickups.",
                    MinimumAllowedAge = 18, DefaultValidityLength = 10, ClassFees = 20.0f
                },
                new LicenseClass {
                    LicenseClassID = 4, ClassName = "Class 4 - Commercial",
                    ClassDescription = "Vehicles used for commercial purposes like Taxis.",
                    MinimumAllowedAge = 21, DefaultValidityLength = 10, ClassFees = 200.0f
                },
                new LicenseClass {
                    LicenseClassID = 5, ClassName = "Class 5 - Agricultural",
                    ClassDescription = "Agricultural tractors and machinery.",
                    MinimumAllowedAge = 18, DefaultValidityLength = 10, ClassFees = 50.0f
                },
                new LicenseClass {
                    LicenseClassID = 6, ClassName = "Class 6 - Small and Medium Truck",
                    ClassDescription = "Trucks with total weight between 3.5 and 7.5 tons.",
                    MinimumAllowedAge = 21, DefaultValidityLength = 10, ClassFees = 250.0f
                },
                new LicenseClass {
                    LicenseClassID = 7, ClassName = "Class 7 - Heavy Truck",
                    ClassDescription = "Large trucks and trailers with weight exceeding 7.5 tons.",
                    MinimumAllowedAge = 21, DefaultValidityLength = 10, ClassFees = 300.0f
                }
            };
        }
        private List<LicenseClassDto> GetFakeLicenseClassesDtos()
        {
            return new List<LicenseClassDto>()
            {
                new LicenseClassDto {
                    LicenseClassID = 1, ClassName = "Class 1 - Small Motorcycle",
                    ClassDescription = "Small motorcycles with engine capacity less than 125cc.",
                    MinimumAllowedAge = 16, DefaultValidityLength = 5, ClassFees = 15.0f
                },
                new LicenseClassDto {
                    LicenseClassID = 2, ClassName = "Class 2 - Heavy Motorcycle",
                    ClassDescription = "Motorcycles with engine capacity more than 125cc.",
                    MinimumAllowedAge = 18, DefaultValidityLength = 5, ClassFees = 30.0f
                },
                new LicenseClassDto {
                    LicenseClassID = 3, ClassName = "Class 3 - Ordinary driving license",
                    ClassDescription = "Standard cars and small pickups.",
                    MinimumAllowedAge = 18, DefaultValidityLength = 10, ClassFees = 20.0f
                },
                new LicenseClassDto {
                    LicenseClassID = 4, ClassName = "Class 4 - Commercial",
                    ClassDescription = "Vehicles used for commercial purposes like Taxis.",
                    MinimumAllowedAge = 21, DefaultValidityLength = 10, ClassFees = 200.0f
                },
                new LicenseClassDto {
                    LicenseClassID = 5, ClassName = "Class 5 - Agricultural",
                    ClassDescription = "Agricultural tractors and machinery.",
                    MinimumAllowedAge = 18, DefaultValidityLength = 10, ClassFees = 50.0f
                },
                new LicenseClassDto {
                    LicenseClassID = 6, ClassName = "Class 6 - Small and Medium Truck",
                    ClassDescription = "Trucks with total weight between 3.5 and 7.5 tons.",
                    MinimumAllowedAge = 21, DefaultValidityLength = 10, ClassFees = 250.0f
                },
                new LicenseClassDto {
                    LicenseClassID = 7, ClassName = "Class 7 - Heavy Truck",
                    ClassDescription = "Large trucks and trailers with weight exceeding 7.5 tons.",
                    MinimumAllowedAge = 21, DefaultValidityLength = 10, ClassFees = 300.0f
                }
            };
        }
        #endregion


        #region GetAllLicenseClassesAsync
        [Fact]
        public async Task GetAllLicenseClassesAsync_ReturnsAllLicenseClasses_WhenDataExists()
        {
            // Arrange
            var fakeLicenseClasses = GetFakeLicenseClasses();
            var fakeLicenseClassesDtos = GetFakeLicenseClassesDtos();
            int pageNumber = 1;
            int pageSize = 10;
            int expectedTotalCount = 50;

            _licenseClassRepoMock
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<LicenseClass, bool>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<LicenseClass, object>>>(),
                    It.IsAny<EnOrderByDirection>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()
                )
                ).ReturnsAsync(fakeLicenseClasses);

            _licenseClassRepoMock
                .Setup(r => r.CountAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync(expectedTotalCount);

            _mapperMock.Setup(m => m.Map<IEnumerable<LicenseClassDto>>(fakeLicenseClasses)).Returns(fakeLicenseClassesDtos);

            // Act
            var result = await _service.GetAllLicenseClassesAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(fakeLicenseClassesDtos.Count(), result.Data.Count());
            Assert.Equal(expectedTotalCount, result.TotalCount);
            Assert.Equal(pageNumber, result.PageNumber);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Equal("Class 1 - Small Motorcycle", result.Data.First().ClassName);
            Assert.Equal("Class 7 - Heavy Truck", result.Data.Last().ClassName);

            _licenseClassRepoMock.Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<LicenseClass, bool>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<LicenseClass, object>>>(),
                    It.IsAny<EnOrderByDirection>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()
                ), Times.Once);

            _licenseClassRepoMock.Verify(r => r.CountAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()), Times.Once);
        }
        #endregion

        #region GetLicenseClassByIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetLicenseClassByIdAsync_ThrowsValidationException_WhenLicenseClassIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act 
            var result = _service.GetLicenseClassByIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetLicenseClassByIdAsync_ThrowsResourceNotFoundException_WhenLicenseClassIsNotFound()
        {
            // Arrange
            _licenseClassRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync((LicenseClass)null!);

            // Act
            var result = _service.GetLicenseClassByIdAsync(100);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("License Class with LicenseClassID ", exception.Message);
        }

        [Fact]
        public async Task GetLicenseClassByIdAsync_ReturnsLicenseClassDto_WhenLicenseClassIsFound()
        {
            // Arrange
            var expectedLicenseClass = GetFakeLicenseClasses()[0];
            var expectedLicenseClassDto = GetFakeLicenseClassesDtos()[0];

            _licenseClassRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync(expectedLicenseClass);

            _mapperMock.Setup(m => m.Map<LicenseClassDto>(expectedLicenseClass)).Returns(expectedLicenseClassDto);

            // Act
            var result = await _service.GetLicenseClassByIdAsync(100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLicenseClassDto.LicenseClassID, result.LicenseClassID);
        }
        #endregion

        #region GetLicenseClassByClassNameAsync
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetLicenseClassByClassNameAsync_ThrowsValidationException_WhenClassNameoIsNullOrWhiteSpace(string className)
        {
            // Act & Assert
            var result = _service.GetLicenseClassByClassNameAsync(className);

            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);

            Assert.Contains("Class Name cannot be null or empty.", exception.Message);
        }

        [Fact]
        public async Task GetLicenseClassByClassNameAsync_ThrowsResourceNotFoundException_WhenLicenseClassIsNotFound()
        {
            // Arrange
            var className = "No.1";

            _licenseClassRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync((LicenseClass)null!);

            // Act
            var result = _service.GetLicenseClassByClassNameAsync(className);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);

            Assert.Contains("License Class with ClassName ", exception.Message);
        }

        [Fact]
        public async Task GetLicenseClassByClassNameAsync_ReturnsLicenseClassDto_WhenClassNameIsNotNullOrWhiteSpace()
        {
            // Arrange 
            var className = "No.1";
            var fakeLicenseClass = new LicenseClass { LicenseClassID = 1, ClassName = className };
            var expectedDto = new LicenseClassDto { LicenseClassID = 1, ClassName = className };

            _licenseClassRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync(fakeLicenseClass);

            _mapperMock.Setup(m => m.Map<LicenseClassDto>(fakeLicenseClass)).Returns(expectedDto);

            // Act
            var result = await _service.GetLicenseClassByClassNameAsync(className);

            // Assert 
            Assert.NotNull(result);
            Assert.Equal(className, result.ClassName);
        }

        [Fact]
        public async Task GetLicenseClassByClassNameAsync_ShouldTrimClassName_BeforeSearching()
        {
            // Arrange
            var rawClassName = "  Class 1  ";
            var trimmedClassName = "Class 1";

            _licenseClassRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync(new LicenseClass { ClassName = trimmedClassName, LicenseClassID = 1 });

            // Act
            await _service.GetLicenseClassByClassNameAsync(rawClassName);

            // Assert
            _licenseClassRepoMock.Verify(r => r.FindAsync(It.Is<Expression<Func<LicenseClass, bool>>>(exp =>
                CheckExpressionResult(exp, trimmedClassName))), Times.Once);
        }
        private bool CheckExpressionResult(Expression<Func<LicenseClass, bool>> exp, string expectedValue)
        {
            var compiled = exp.Compile();
            return compiled.Invoke(new LicenseClass { ClassName = expectedValue });
        }
        #endregion

        #region AddLicenseClassAsync
        [Fact]
        public async Task AddLicenseClassAsync_ThrowsConflictException_WhenLicenseClassCLassNameIsExist()
        {
            // Arrange
            var fakeLicenseClassDto = GetFakeLicenseClassesDtos()[0];
            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>())).ReturnsAsync(true);
            // Act
            var result = _service.AddLicenseClassAsync(fakeLicenseClassDto);
            // Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => result);
            Assert.Contains("The License Class ClassName ", exception.Message);
            _licenseClassRepoMock.Verify(r => r.AddAsync(It.IsAny<LicenseClass>()), Times.Never);
        }

        [Fact]
        public async Task AddLicenseClassAsync_ShouldReturnDto_WhenAddedSuccessfully()
        {
            // Arrange
            var fakeLicenseClassDto = GetFakeLicenseClassesDtos()[0];
            var expectedLicenseClass = GetFakeLicenseClasses()[0];
            expectedLicenseClass.LicenseClassID = 5;

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<LicenseClass>(fakeLicenseClassDto))
                .Returns(expectedLicenseClass);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.AddLicenseClassAsync(fakeLicenseClassDto);

            // Assert
            Assert.NotNull(result); 
            Assert.Equal(5, result.LicenseClassID); 
            _licenseClassRepoMock.Verify(r => r.AddAsync(It.IsAny<LicenseClass>()), Times.Once);
        }

        [Fact]
        public async Task AddLicenseClassAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange
            var fakeLicenseClassDto = GetFakeLicenseClassesDtos()[0];
            var expectedLicenseClass = GetFakeLicenseClasses()[0];

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<LicenseClass>(fakeLicenseClassDto))
                .Returns(expectedLicenseClass);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.AddLicenseClassAsync(fakeLicenseClassDto);

            // Assert
            Assert.Null(result); 
        }

        [Fact]
        public async Task AddLicenseClassAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var fakeLicenseClassDto = GetFakeLicenseClassesDtos()[0];
            var expectedLicenseClass = GetFakeLicenseClasses()[0];

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>())).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<LicenseClass>(fakeLicenseClassDto)).Returns(expectedLicenseClass);
            _licenseClassRepoMock.Setup(r => r.AddAsync(It.IsAny<LicenseClass>())).ThrowsAsync(new Exception("Database Error"));
            // Act
            var result = _service.AddLicenseClassAsync(fakeLicenseClassDto);
            // Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => result);
            Assert.Contains("An error occurred while saving the License Class record.", exception.Message);
            _licenseClassRepoMock.Verify(r => r.AddAsync(It.IsAny<LicenseClass>()), Times.Once);
        }
        #endregion

        #region UpdateLicenseClassAsync
        [Fact]
        public async Task UpdateLicenseClassAsync_ThrowsResourceNotFoundException_WhenLicenseClassIsNotFound()
        {
            // Arrange
            var fakeLicenseClassDto = new LicenseClassDto() { LicenseClassID = 100 };
            _licenseClassRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>())).ReturnsAsync((LicenseClass)null!);

            // Act
            var result = _service.UpdateLicenseClassAsync(fakeLicenseClassDto);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Cannot update: License Class with LicenseClassID", exception.Message);
            _licenseClassRepoMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task UpdateLicenseClassAsync_ThrowsConflictException_WhenClassNameExistsForAnotherLicenseClass()
        {
            // Arrange
            var dto = new LicenseClassDto { LicenseClassID = 1, ClassName = "New Class 1" };
            var existingInDb = new LicenseClass { LicenseClassID = 1, ClassName = "Old Class 1" };

            _licenseClassRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LicenseClass,
                bool>>>())).ReturnsAsync(existingInDb);

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>())).ReturnsAsync(true);
            // Act
            var result = _service.UpdateLicenseClassAsync(dto);
            // Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => result);
            Assert.Contains("The License Class ClassName", exception.Message);

            _licenseClassRepoMock.Verify(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()), Times.Once);
            _licenseClassRepoMock.Verify(r => r.Update(It.IsAny<LicenseClass>()), Times.Never);
        }

        [Fact]
        public async Task UpdateLicenseClassAsync_ShouldReturnDto_WhenUpdateIsSuccessful()
        {
            // Arrange 
            var dto = new LicenseClassDto { LicenseClassID = 1, ClassName = "Class 1", ClassFees = 55.5f };
            var existingInDb = new LicenseClass { LicenseClassID = 1, ClassName = "Class 1", ClassFees = 10.0f };

            _licenseClassRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                                 .ReturnsAsync(existingInDb);

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                                 .ReturnsAsync(false);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.UpdateLicenseClassAsync(dto);

            // Assert 
            Assert.NotNull(result); 
            Assert.Equal(dto.ClassFees, result.ClassFees);

            _licenseClassRepoMock.Verify(r => r.Update(It.IsAny<LicenseClass>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateLicenseClassAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange 
            var dto = new LicenseClassDto { LicenseClassID = 1, ClassName = "Class 1", ClassFees = 55.5f };
            var existingInDb = new LicenseClass { LicenseClassID = 1, ClassName = "Class 1", ClassFees = 10.0f };

            _licenseClassRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                                 .ReturnsAsync(existingInDb);

            _licenseClassRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                                 .ReturnsAsync(false);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.UpdateLicenseClassAsync(dto);

            // Assert 
            Assert.Null(result); 

            _licenseClassRepoMock.Verify(r => r.Update(It.IsAny<LicenseClass>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateLicenseClassAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var dto = new LicenseClassDto { LicenseClassID = 1, ClassName = "Class 1", ClassFees = 55.5f };
            _licenseClassRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LicenseClass, bool>>>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var result = _service.UpdateLicenseClassAsync(dto);
            var exception = await Assert.ThrowsAsync<Exception>(() => result);
            Assert.Contains($"An error occurred while updating the License Class record", exception.Message);
        }
        #endregion 
    }
}
