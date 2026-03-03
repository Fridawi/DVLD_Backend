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
using System;
using System.Linq.Expressions;
using ApplicationType = DVLD.CORE.Entities.ApplicationType;

namespace DVLD.Tests.Services
{
    public class ApplicationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IApplicationRepository> _appRepositoryMock;
        private readonly Mock<IGenericRepository<ApplicationType>> _applicationTypeRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<ApplicationService>> _loggerMock;
        private readonly ApplicationService _service;

        public ApplicationServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _appRepositoryMock = new Mock<IApplicationRepository>();
            _applicationTypeRepoMock = new Mock<IGenericRepository<ApplicationType>>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<ApplicationService>>();
            _unitOfWorkMock.Setup(r => r.Applications).Returns(_appRepositoryMock.Object);
            _unitOfWorkMock.Setup(r => r.ApplicationTypes).Returns(_applicationTypeRepoMock.Object);
            _service = new ApplicationService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        #region Data 
        private List<Application> GetfakeLocalApps()
        {
            return new List<Application>()
            {
                new Application
                {
                    ApplicationID = 1,
                    ApplicantPersonID = 10,
                    PersonInfo = new Person
                    {
                        PersonID = 10,
                        FirstName = "Ahmed",
                        SecondName = "Mohammed",
                        ThirdName = "Ali",
                        LastName = "Mansour",
                        NationalNo = "N100",
                        DateOfBirth = new DateTime(1990, 5, 10),
                        Gendor = 0,
                        Address = "Baghdad, Iraq",
                        Phone = "07701234567",
                        NationalityCountryID = 1
                    },
                    ApplicationDate = new DateTime(2025, 12, 1),
                    ApplicationTypeID = 1,
                    ApplicationTypeInfo = new ApplicationType { ApplicationTypeID = 1, Title = "New Local Driving License" },
                    ApplicationStatus = EnApplicationStatus.New,
                    LastStatusDate = new DateTime(2025, 12, 1),
                    PaidFees = 15.0f,
                    CreatedByUserID = 1,
                    CreatedByUserInfo = new User { UserID = 1, UserName = "Admin01" }
                },
                new Application
                {
                    ApplicationID = 2,
                    ApplicantPersonID = 25,
                    PersonInfo = new Person
                    {
                        PersonID = 25,
                        FirstName = "Sara",
                        SecondName = "Ibrahim",
                        ThirdName = null,
                        LastName = "Khalid",
                        NationalNo = "N200",
                        DateOfBirth = new DateTime(1995, 8, 20),
                        Gendor = 1,
                        Address = "Amman, Jordan",
                        Phone = "07901234567",
                        NationalityCountryID = 2
                    },
                    ApplicationDate = new DateTime(2025, 11, 20),
                    ApplicationTypeID = 3,
                    ApplicationTypeInfo = new ApplicationType { ApplicationTypeID = 3, Title = "Replace for Lost License" },
                    ApplicationStatus = EnApplicationStatus.Completed,
                    LastStatusDate = new DateTime(2025, 11, 21),
                    PaidFees = 10.0f,
                    CreatedByUserID = 2,
                    CreatedByUserInfo = new User { UserID = 2, UserName = "User_FrontDesk" }
                }
            };
        }
        private List<ApplicationDto> GetfakeLocalAppsDto()
        {
            return new List<ApplicationDto>()
            {
                new ApplicationDto
                {
                    ApplicationID = 1,
                    ApplicantPersonID = 10,
                    ApplicantFullName = "Ahmed Mohammed Ali Mansour",
                    ApplicationDate = new DateTime(2025, 12, 1),
                    ApplicationTypeID = 1,
                    ApplicationTypeTitle = "New Local Driving License",
                    ApplicationStatus = (byte)EnApplicationStatus.New,
                    StatusText = "New",
                    LastStatusDate = new DateTime(2025, 12, 1),
                    PaidFees = 15.0f,
                    CreatedByUserID = 1,
                    CreatedByUserName = "Admin01"
                },
                new ApplicationDto
                {
                    ApplicationID = 2,
                    ApplicantPersonID = 25,
                    ApplicantFullName = "Sara Ibrahim Khalid",
                    ApplicationDate = new DateTime(2025, 11, 20),
                    ApplicationTypeID = 3,
                    ApplicationTypeTitle = "Replace for Lost License",
                    ApplicationStatus = (byte)EnApplicationStatus.Completed,
                    StatusText = "Completed",
                    LastStatusDate = new DateTime(2025, 11, 21),
                    PaidFees = 10.0f,
                    CreatedByUserID = 2,
                    CreatedByUserName = "User_FrontDesk"
                }
            };
        }
        private List<ApplicationCreateDto> GetfakeLocalAppsCreateDto()
        {
            return new List<ApplicationCreateDto>()
            {
                new ApplicationCreateDto
                {
                    ApplicantPersonID = 10,
                    ApplicationTypeID = 1
                },
                new ApplicationCreateDto
                {
                    ApplicantPersonID = 25,
                    ApplicationTypeID = 4
                },
                new ApplicationCreateDto
                {
                    ApplicantPersonID = 30,
                    ApplicationTypeID = 2
                }
            };
        }
        #endregion

        #region GetByIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetByIdAsync_ThrowsValidationException_WhenApplicationIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.GetByIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsResourceNotFoundException_WhenApplicationIsNotFound()
        {
            // Arrange
            _appRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync((Application)null!);

            // Act
            var result = _service.GetByIdAsync(100);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Application with ID ", exception.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsApplicationDto_WhenApplicationIsFound()
        {
            // Arrange
            var fakeLocalApps = GetfakeLocalApps()[0];
            var expectedFakeLocalAppDtos = GetfakeLocalAppsDto()[0];

            int applicationIDToTest = 1;

            _appRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(fakeLocalApps);

            _mapperMock.Setup(m => m.Map<ApplicationDto>(fakeLocalApps))
                .Returns((Application app) => expectedFakeLocalAppDtos);

            // Act
            var result = await _service.GetByIdAsync(applicationIDToTest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(applicationIDToTest, result.ApplicationID);
            Assert.Equal(expectedFakeLocalAppDtos.ApplicantFullName, result.ApplicantFullName);
        }

        #endregion

        #region GetActiveApplicationIdAsync
        [Theory]
        [InlineData(-1, 1)]
        [InlineData(1, -1)]
        [InlineData(0, 0)]
        public async Task GetActiveApplicationIdAsync_ThrowsValidationException_WhenPersonIDOrApplicationTypeIDIsLessOrEqualZero
            (int personID, int applicationTypeID)
        {
            // Arrange & Act
            var result = _service.GetActiveApplicationIdAsync(personID, applicationTypeID);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetActiveApplicationIdAsync_ThrowsResourceNotFoundException_WhenActiveApplicationIsNotFound()
        {
            int applicationTypeID = 1, personID = 1;
            // Arrange
            _appRepositoryMock.Setup(r => r.GetActiveApplicationIdAsync(
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(0);

            // Act
            var result = _service.GetActiveApplicationIdAsync(personID, applicationTypeID);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Application with PersonID: ", exception.Message);
        }

        [Fact]
        public async Task GetActiveApplicationIdAsync_ReturnsApplicationID_WhenActiveApplicationIsFound()
        {
            // Arrange
            int applicationTypeID = 1, personID = 1;

            _appRepositoryMock.Setup(r => r.GetActiveApplicationIdAsync(
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.GetActiveApplicationIdAsync(personID, applicationTypeID);

            // Assert
            Assert.True(result > 0);
        }
        #endregion

        #region GetActiveApplicationIdForLicenseClassAsync

        [Theory]
        [InlineData(-1, 1, 1)]
        [InlineData(1, -1, 1)]
        [InlineData(1, 1, -1)]
        [InlineData(0, 0, 0)]
        public async Task GetActiveApplicationIdForLicenseClassAsync_ThrowsValidationException_WhenParametersAreLessOrEqualZero
            (int personID, int applicationTypeID, int licenseClassID)
        {
            // Arrange & Act
            var result = _service.GetActiveApplicationIdForLicenseClassAsync(personID, applicationTypeID, licenseClassID);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetActiveApplicationIdForLicenseClassAsync_ReturnsZero_WhenActiveApplicationIsNotFound()
        {
            // Arrange
            int personID = 1, applicationTypeID = 1, licenseClassID = 3;

            _appRepositoryMock.Setup(r => r.GetActiveApplicationIdForLicenseClassAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(0);

            // Act
            var result =await _service.GetActiveApplicationIdForLicenseClassAsync(personID, applicationTypeID, licenseClassID);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetActiveApplicationIdForLicenseClassAsync_ReturnsApplicationID_WhenActiveApplicationIsFound()
        {
            // Arrange
            int personID = 1, applicationTypeID = 1, licenseClassID = 3, expectedAppId = 100;

            _appRepositoryMock.Setup(r => r.GetActiveApplicationIdForLicenseClassAsync(
                personID,
                applicationTypeID,
                licenseClassID))
                .ReturnsAsync(expectedAppId);

            // Act
            var result = await _service.GetActiveApplicationIdForLicenseClassAsync(personID, applicationTypeID, licenseClassID);

            // Assert
            Assert.Equal(expectedAppId, result);
            _appRepositoryMock.Verify(r => r.GetActiveApplicationIdForLicenseClassAsync(personID, applicationTypeID, licenseClassID), Times.Once);
        }

        #endregion

        #region AddApplicationAsync
        [Fact]
        public async Task AddApplicationAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            int currentUserId = 1;
            var fakeApplicationCreateDto = GetfakeLocalAppsCreateDto()[0];
            var expectedApplication = GetfakeLocalApps()[0];

            _appRepositoryMock.Setup(r => r.DoesPersonHaveActiveApplicationAsync(
                It.IsAny<int>(),
                It.IsAny<int>())).ReturnsAsync(false);

            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync( new ApplicationType());

            _mapperMock.Setup(m => m.Map<Application>(fakeApplicationCreateDto)).Returns(expectedApplication);

            _appRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Application>())).ThrowsAsync(new Exception("Database Error"));

            // Act
            var result = _service.AddApplicationAsync(fakeApplicationCreateDto, currentUserId);
            // Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => result);
            Assert.Contains("An error occurred while saving the Application record.", exception.Message);
            _appRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Application>()), Times.Once);
        }

        [Fact]
        public async Task AddApplicationAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange
            int currentUserId = 1;
            var fakeApplicationCreateDto = GetfakeLocalAppsCreateDto()[0];
            var expectedApplication = GetfakeLocalApps()[0];

            _appRepositoryMock.Setup(r => r.DoesPersonHaveActiveApplicationAsync(
                It.IsAny<int>(),
                It.IsAny<int>())).ReturnsAsync(false);

            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new ApplicationType { Fees = 15 });

            _mapperMock.Setup(m => m.Map<Application>(fakeApplicationCreateDto)).Returns(expectedApplication);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.AddApplicationAsync(fakeApplicationCreateDto, currentUserId);

            // Assert
            Assert.Null(result); // تم التعديل من False إلى Null
            _appRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Application>()), Times.Once);
        }

        [Fact]
        public async Task AddApplicationAsync_ShouldReturnDto_WhenApplicationIsAddedSuccessfully()
        {
            // Arrange
            int currentUserId = 1;
            int generatedAppId = 1;
            var fakeApplicationCreateDto = GetfakeLocalAppsCreateDto()[0];

            var expectedApplication = GetfakeLocalApps()[0];
            expectedApplication.ApplicationID = generatedAppId;

            var expectedDto = new ApplicationDto { ApplicationID = generatedAppId };

            _appRepositoryMock.Setup(r => r.DoesPersonHaveActiveApplicationAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationType { Fees = 15 });

            _mapperMock.Setup(m => m.Map<Application>(fakeApplicationCreateDto)).Returns(expectedApplication);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _appRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Application>()))
                .Callback<Application>(a => a.ApplicationID = generatedAppId)
                .Returns(Task.CompletedTask);

            _appRepositoryMock.Setup(r => r.FindAsync(
                    It.IsAny<Expression<Func<Application, bool>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(expectedApplication);

            _mapperMock.Setup(m => m.Map<ApplicationDto>(It.IsAny<Application>()))
                .Returns(expectedDto);

            // Act
            var result = await _service.AddApplicationAsync(fakeApplicationCreateDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(generatedAppId, result.ApplicationID);
            _appRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Application>()), Times.Once);
        }

        [Fact]
        public async Task AddApplicationAsync_ThrowsResourceNotFoundException_WhenApplicationTypeisNotFound()
        {
            // Arrange
            int currentUserId = 1;
            var fakeApplicationCreateDto = GetfakeLocalAppsCreateDto()[0];
            _appRepositoryMock.Setup(r => r.DoesPersonHaveActiveApplicationAsync(
                It.IsAny<int>(),
                It.IsAny<int>())).ReturnsAsync(false);

            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ApplicationType)null!);
            // Act
            var result = _service.AddApplicationAsync(fakeApplicationCreateDto, currentUserId);
            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Application Type not found", exception.Message);
            _appRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Application>()), Times.Never);
        }

        [Fact]
        public async Task AddApplicationAsync_ThrowsInvalidOperationException_WhenPersonAlreadyHasActiveApplication()
        {
            // Arrange
            int currentUserId = 1;
            var fakeApplicationCreateDto = GetfakeLocalAppsCreateDto()[0];
            _appRepositoryMock.Setup(r => r.DoesPersonHaveActiveApplicationAsync(
                It.IsAny<int>(),
                It.IsAny<int>())).ReturnsAsync(true);

            _applicationTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new ApplicationType());
            // Act
            var result = _service.AddApplicationAsync(fakeApplicationCreateDto, currentUserId);
            // Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => result);
            Assert.Contains("This person already has an active application of this type.", exception.Message);
            _appRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Application>()), Times.Never);
        }
        #endregion

        #region UpdateStatusAsync
        [Fact]
        public async Task UpdateStatusAsync_ThrowsResourceNotFoundException_WhenUserIsNotFound()
        {
            // Arrange
            var fakeUserCreateDto = new ApplicationUpdateDto() { ApplicationID = 100 };
            _appRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Application)null!);

            // Act
            var result = _service.UpdateStatusAsync(fakeUserCreateDto);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Cannot update: Application with ID", exception.Message);
            _appRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldReturnDto_WhenUpdatedSuccessfully()
        {
            // Arrange             
            var dto = new ApplicationUpdateDto() { ApplicationID = 100 };
            var existingInDb = new Application() { ApplicationID = 100 };
            var expectedDto = new ApplicationDto() { ApplicationID = 100 };

            _appRepositoryMock.Setup(r => r.GetByIdAsync(100))
                .ReturnsAsync(existingInDb);

            _mapperMock.Setup(m => m.Map(It.IsAny<ApplicationUpdateDto>(), It.IsAny<Application>()));

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _appRepositoryMock.Setup(r => r.FindAsync(
                    It.IsAny<Expression<Func<Application, bool>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(existingInDb);

            _mapperMock.Setup(m => m.Map<ApplicationDto>(It.IsAny<Application>()))
                .Returns(expectedDto);

            // Act
            var result = await _service.UpdateStatusAsync(dto);

            // Assert 
            Assert.NotNull(result);
            Assert.Equal(100, result.ApplicationID);

            _appRepositoryMock.Verify(r => r.Update(It.IsAny<Application>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_ReturnsNull_WhenFailedToUpdateInDatabase()
        {
            // Arrange             
            var dto = new ApplicationUpdateDto() { ApplicationID = 100 };
            var existingInDb = new Application() { ApplicationID = 100 };

            _appRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(existingInDb);

            _mapperMock.Setup(m => m.Map(It.IsAny<ApplicationUpdateDto>(), It.IsAny<Application>()));

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.UpdateStatusAsync(dto);

            // Assert 
            Assert.Null(result); // تم التعديل من False إلى Null
            _appRepositoryMock.Verify(r => r.Update(It.IsAny<Application>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var dto = new ApplicationUpdateDto() { ApplicationID = 100 };
            _appRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var result = _service.UpdateStatusAsync(dto);
            var exception = await Assert.ThrowsAsync<Exception>(() => result);
            Assert.Contains($"An error occurred while updating the Application record", exception.Message);
        }
        #endregion

        #region DeleteApplicationAsync
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteApplicationAsync_ThrowsValidationException_WhenIdIsLessOrEqualZero(int invalidId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.DeleteApplicationAsync(invalidId));

            Assert.Contains("Invalid ApplicationID provided for deletion.", exception.Message);
        }

        [Fact]
        public async Task DeleteApplicationAsync_ThrowsResourceNotFoundException_WhenApplicationDoesNotExist()
        {
            // Arrange
            int applicationID = 99;
            _appRepositoryMock.Setup(r => r.GetByIdAsync(applicationID)).ReturnsAsync((Application)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.DeleteApplicationAsync(applicationID));

            Assert.Contains("Application with ApplicationID", exception.Message);

            _appRepositoryMock.Verify(r => r.Delete(It.IsAny<Application>()), Times.Never);
        }

        [Fact]
        public async Task DeleteApplicationAsync_ReturnsTrue_WhenUserIsDeleted()
        {
            // Arrange
            int applicationID = 1;
            var application = new Application  { ApplicationID = applicationID };

            _appRepositoryMock.Setup(r => r.GetByIdAsync(applicationID)).ReturnsAsync(application);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteApplicationAsync(applicationID);

            // Assert
            Assert.True(result);
            _appRepositoryMock.Verify(r => r.Delete(application), Times.Once);
        }

        [Fact]
        public async Task DeleteApplicationAsync_ReturnsFalse_WhenNoRowsAffected()
        {
            // Arrange
            _appRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Application());
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.DeleteApplicationAsync(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteApplicationAsync_ThrowsConflictException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            int applicationID = 1;
            var application = new Application { ApplicationID = applicationID };
            _appRepositoryMock.Setup(r => r.GetByIdAsync(applicationID)).ReturnsAsync(application);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ThrowsAsync(new Exception("Foreign key violation"));

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteApplicationAsync(applicationID));
        }

        #endregion

    }
}
