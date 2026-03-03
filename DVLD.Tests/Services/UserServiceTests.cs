using AutoMapper;
using DVLD.CORE.Constants;
using DVLD.CORE.DTOs;
using DVLD.CORE.DTOs.Users;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.CORE.Settings;
using DVLD.SERVICES;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq.Expressions;

namespace DVLD.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<User>> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly Mock<IJwtProvider> _jwtProviderMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userRepoMock = new Mock<IGenericRepository<User>>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _jwtProviderMock = new Mock<IJwtProvider>();

            var jwtSettings = new JwtSettings
            {
                Key = "SuperSecretKeyForTestingOnly123456",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                DurationInMinutes = 60
            };

            var options = Options.Create(jwtSettings);

            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);

            _service = new UserService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _jwtProviderMock.Object,
                options 
            );
        }

        #region Data 
        private List<User> GetFakeUsers()
        {
            var passwordHash = "$2a$12$V.vR8Vv/5eFm1pB.vH6vOuP0XQkY/vS4L1fD6vE6A7L0n5S3J6R7K"; // 12345678

            return new List<User>()
            {
                new User { UserID = 1, UserName = "Ahmed_Admin", Password = passwordHash, Role = "Admin", IsActive = true, PersonID = 1 },
                new User { UserID = 2, UserName = "Ali_User",    Password = passwordHash, Role = "User",  IsActive = true, PersonID = 2 },
                new User { UserID = 3, UserName = "Hussein_G",  Password = passwordHash, Role = "User",  IsActive = true, PersonID = 3 },
                new User { UserID = 4, UserName = "Omar_Admin",  Password = passwordHash, Role = "Admin", IsActive = false, PersonID = 4 },
                new User { UserID = 5, UserName = "Sara_User",   Password = passwordHash, Role = "User",  IsActive = true, PersonID = 5 }
            };
        }
        private List<UserDto> GetFakeUsersDto()
        {
            return new List<UserDto>()
            {
                new UserDto { UserID = 1, UserName = "Ahmed_Admin", Role = "Admin", IsActive = true , PersonID = 1 },
                new UserDto { UserID = 2, UserName = "Ali_User",    Role = "User",  IsActive = true , PersonID = 2 },
                new UserDto { UserID = 3, UserName = "Hussein_G",   Role = "User",  IsActive = true , PersonID = 3 },
                new UserDto { UserID = 4, UserName = "Omar_Admin",  Role = "Admin", IsActive = false, PersonID = 4 },
                new UserDto { UserID = 5, UserName = "Sara_User",   Role = "User",  IsActive = true , PersonID = 5 }
            };
        }
        private List<UserCreateDto> GetFakeUserCreateDtos()
        {
            return new List<UserCreateDto>()
            {
                new UserCreateDto
                {
                    PersonID = 1,
                    UserName = "Ahmed99",
                    Password = "SecurePassword123",
                    Role = UserRoles.Admin,
                    IsActive = true
                },
                new UserCreateDto
                {
                    PersonID = 2,
                    UserName = "Ali_Hassan",
                    Password = "MySecretPass789",
                    Role = UserRoles.User,
                    IsActive = true
                },
                new UserCreateDto
                {
                    PersonID = 3,
                    UserName = "Hussein_Dev",
                    Password = "StrongDevPass!@#",
                    Role = UserRoles.User,
                    IsActive = true
                },
                new UserCreateDto
                {
                    PersonID = 4,
                    UserName = "System_Manager",
                    Password = "AdminAccessOnly2026",
                    Role = UserRoles.Admin,
                    IsActive = false
                },
                new UserCreateDto
                {
                    PersonID = 5,
                    UserName = "Sara_Editor",
                    Password = "EditorPassWord55",
                    Role = UserRoles.User,
                    IsActive = true
                }
            };
        }
        #endregion

        #region AuthenticateAsync 
        [Fact]
        public async Task AuthenticateAsync_ReturnsAuthResponse_WhenCredentialsAreValid()
        {
            // Arrange
            var password = "12345678";

            var validHash = BCrypt.Net.BCrypt.HashPassword(password, 12);

            var loginRequest = new LoginRequestDto { UserName = "Ahmed_Admin", Password = password };

            var fakeUser = new User
            {
                UserID = 1,
                UserName = "Ahmed_Admin",
                Password = validHash,
                Role = "Admin",
                IsActive = true
            };

            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), null, false))
                .ReturnsAsync(fakeUser);

            _jwtProviderMock.Setup(j => j.Generate(fakeUser)).Returns("fake-jwt-token");

            // Act
            var result = await _service.AuthenticateAsync(loginRequest);

            // Assert
            Assert.True(result.IsAuthenticated); 
            Assert.Equal("fake-jwt-token", result.Token);
            Assert.Equal("Success", result.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsUnauthenticated_WhenPasswordIsWrong()
        {
            // Arrange
            var fakeUser = new User { UserName = "Ahmed_Admin", Password = BCrypt.Net.BCrypt.HashPassword("correct_pass") };
            var loginRequest = new LoginRequestDto { UserName = "Ahmed_Admin", Password = "wrong_pass" };

            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), null, false))
                .ReturnsAsync(fakeUser);

            // Act
            var result = await _service.AuthenticateAsync(loginRequest);

            // Assert
            Assert.False(result.IsAuthenticated);
            Assert.Equal("Invalid UserName or Password", result.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsUnauthenticated_WhenUserIsNotFound()
        {
            // Arrange
            var loginRequest = new LoginRequestDto { UserName = "Ahmed_Admin", Password = "wrong_pass" };

            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), null, false))
                .ReturnsAsync((User)null!);

            // Act
            var result = await _service.AuthenticateAsync(loginRequest);

            // Assert
            Assert.False(result.IsAuthenticated);
            Assert.Equal("Invalid UserName or Password", result.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsUnauthenticated_WhenUserIsNotActive()
        {
            // Arrange
            var password = "12345678";

            var validHash = BCrypt.Net.BCrypt.HashPassword(password, 12);

            var loginRequest = new LoginRequestDto { UserName = "Ahmed_Admin", Password = password };

            var fakeUser = new User
            {
                UserID = 1,
                UserName = "Ahmed_Admin",
                Password = validHash,
                Role = "Admin",
                IsActive = false
            };

            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), null, false))
                .ReturnsAsync(fakeUser);

            // Act
            var result = await _service.AuthenticateAsync(loginRequest);

            // Assert
            Assert.False(result.IsAuthenticated);
            Assert.Equal("Account is deactivated. Please contact admin.", result.Message);
        }

        #endregion

        #region ChangePasswordAsync 
        [Fact]
        public async Task ChangePasswordAsync_ThrowsResourceNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            int userId = 999;
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => 
                _service.ChangePasswordAsync(userId, "anyPassword"));

            Assert.Contains($"User with ID {userId} not found", exception.Message);
            _userRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsTrue_WhenPasswordIsChangedSuccessfully()
        {
            // Arrange
            int userId = 1;
            string newPassword = "NewSecurePassword123";
            var existingUser = new User { UserID = userId, UserName = "Hussein", Password = "OldHash" };

            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.ChangePasswordAsync(userId, newPassword);

            // Assert
            Assert.True(result);
            Assert.NotEqual(newPassword, existingUser.Password);
            Assert.True(BCrypt.Net.BCrypt.Verify(newPassword, existingUser.Password));

            _userRepoMock.Verify(r => r.Update(existingUser), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsFalse_WhenDatabaseUpdateFails()
        {
            // Arrange
            int userId = 1;
            var existingUser = new User { UserID = userId, Password = "OldHash" };

            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.ChangePasswordAsync(userId, "NewPass123");

            // Assert
            Assert.False(result);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            int userId = 1;
            _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var result = _service.ChangePasswordAsync(userId, "NewPass123");
            var exception = await Assert.ThrowsAsync<Exception>(() => result);
            Assert.Contains($"An error occurred while Changed Password record. ", exception.Message);
        }
        #endregion

        #region GetAllUsersAsync
        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers_WhenDataExists()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            var fakeUsers = GetFakeUsers();
            var fackUsersDto = GetFakeUsersDto();
            int expectedTotalCount = 50;

            _userRepoMock
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<User, object>>>(),
                    It.IsAny<EnOrderByDirection>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()
                )
                ).ReturnsAsync(fakeUsers);

            _userRepoMock
                .Setup(r => r.CountAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(expectedTotalCount);

            _mapperMock.Setup(m => m.Map<IEnumerable<UserDto>>(fakeUsers)).Returns(fackUsersDto);

            // Act
            var result = await _service.GetAllUsersAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(fackUsersDto.Count(), result.Data.Count());
            Assert.Equal(expectedTotalCount, result.TotalCount);
            Assert.Equal(pageNumber, result.PageNumber);
            Assert.Equal(pageSize, result.PageSize);

            Assert.Equal("Ahmed_Admin", result.Data.First().UserName);
            Assert.True( result.Data.First().IsActive);

            _userRepoMock.Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<User, object>>>(),
                    It.IsAny<EnOrderByDirection>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()
                ), Times.Once);

            _userRepoMock.Verify(r => r.CountAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        }
        #endregion

        #region GetUserByIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetUserByIdAsync_ThrowsValidationException_WhenUserIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.GetUserByIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetUserByIdAsync_ThrowsResourceNotFoundException_WhenUserIsNotFound()
        {
            // Arrange
            _userRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync((User)null!);

            // Act
            var result = _service.GetUserByIdAsync(100);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("User with ID ", exception.Message);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUserDto_WhenUserIsFound()
        {
            // Arrange
            var expectedUser = GetFakeUsers()[0];
            var expectedUserDto = GetFakeUsersDto()[0];

            _userRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(expectedUser);

            _mapperMock.Setup(m => m.Map<UserDto>(expectedUser)).Returns(expectedUserDto);

            // Act
            var result = await _service.GetUserByIdAsync(100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUserDto.UserID, result.UserID);
        }
        #endregion

        #region GetUserByPersonIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetUserByPersonIdAsync_ThrowsValidationException_WhenPersonIDIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.GetUserByPersonIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetUserByPersonIdAsync_ThrowsResourceNotFoundException_WhenUserIsNotFound()
        {
            // Arrange
            _userRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync((User)null!);

            // Act
            var result = _service.GetUserByPersonIdAsync(100);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("User with PersonID ", exception.Message);
        }

        [Fact]
        public async Task GetUserByPersonIdAsync_ReturnsUserDto_WhenUserIsFound()
        {
            // Arrange
            var expectedUser = GetFakeUsers()[0];
            var expectedUserDto = GetFakeUsersDto()[0];

            _userRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
                .ReturnsAsync(expectedUser);

            _mapperMock.Setup(m => m.Map<UserDto>(expectedUser)).Returns(expectedUserDto);

            // Act
            var result = await _service.GetUserByPersonIdAsync(100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUserDto.UserID, result.UserID);
        }
        #endregion

        #region GetPersonIdByUserIdAsync
        [Fact]
        public async Task GetPersonIdByUserIdAsync_ReturnPersonId_WhenUserExists()
        {
            // Arrange 
            int userId = 1;
            int expectedPersonId = 50;


            _unitOfWorkMock.Setup(uow => uow.Users.GetProjectedByIdAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, int>>>()))
                .ReturnsAsync(expectedPersonId);

            // Act 
            var result = await _service.GetPersonIdByUserIdAsync(userId);

            // Assert 
            Assert.Equal(expectedPersonId, result);
        }

        [Fact]
        public async Task GetPersonIdByUserIdAsync_ThrowsResourceNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            int userId = 999;

            _unitOfWorkMock.Setup(uow => uow.Users.GetProjectedByIdAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, int>>>()))
                .ReturnsAsync(0);

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
                _service.GetPersonIdByUserIdAsync(userId));
        }
        #endregion

        #region AddUserAsync
        [Fact]
        public async Task AddUserAsync_ThrowsConflictException_WhenPersonAlreadyUser()
        {
            // Arrange
            var fakeUserCreateDto = GetFakeUserCreateDtos()[0];
            _userRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(true);
            // Act
            var result = _service.AddUserAsync(fakeUserCreateDto);
            // Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => result);
            Assert.Contains("The PersonID ", exception.Message);
            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }
        [Fact]
        public async Task AddUserAsync_ThrowsConflictException_WhenUserNameIsExist()
        {
            // Arrange
            var fakeUserCreateDto = GetFakeUserCreateDtos()[0];
            _userRepoMock.SetupSequence(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false)
                .ReturnsAsync(true);
            // Act
            var result = _service.AddUserAsync(fakeUserCreateDto);
            // Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => result);
            Assert.Contains("The User UserName ", exception.Message);
            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AddUserAsync_ReturnsDto_WhenUserIsAdded()
        {
            // Arrange
            var fakeUserCreateDto = GetFakeUserCreateDtos()[0];
            var fakeUserDto = GetFakeUsersDto()[0];
            var expectedUser = GetFakeUsers()[0];

            _userRepoMock.SetupSequence(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false)
                .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<User>(fakeUserCreateDto)).Returns(expectedUser);
            _mapperMock.Setup(m => m.Map<UserDto>(expectedUser)).Returns(fakeUserDto);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            // Act
            var result = await _service.AddUserAsync(fakeUserCreateDto);
            // Assert
            Assert.NotNull(result);
            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task AddUserAsync_ReturnsNull_WhenDatabaseSaveFails()
        {
            // Arrange
            var fakeUserCreateDto = GetFakeUserCreateDtos()[0];
            var expectedUser = GetFakeUsers()[0];

            _userRepoMock.SetupSequence(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false)
                .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<User>(fakeUserCreateDto)).Returns(expectedUser);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);
            // Act
            var result = await _service.AddUserAsync(fakeUserCreateDto);
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddUserAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var fakeUserCreateDto = GetFakeUserCreateDtos()[0];
            var expectedUser = GetFakeUsers()[0];

            _userRepoMock.SetupSequence(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false)
                .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<User>(fakeUserCreateDto)).Returns(expectedUser);

            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ThrowsAsync(new Exception("Database Error"));
            // Act
            var result = _service.AddUserAsync(fakeUserCreateDto);
            // Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => result);
            Assert.Contains("An error occurred while saving the User record.", exception.Message);
            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }
        #endregion

        #region UpdateUserAsync
        [Fact]
        public async Task UpdateUserAsync_ThrowsResourceNotFoundException_WhenUserIsNotFound()
        {
            // Arrange
            var fakeUserCreateDto = new UserDto() { UserID = 100 };
            _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null!);

            // Act
            var result = _service.UpdateUserAsync(fakeUserCreateDto);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Cannot update: User with ID", exception.Message);
            _userRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsConflictException_WhenUserNameExistsForAnotherUser()
        {
            // Arrange
            var dto = new UserDto { UserID = 1, UserName = "New UserName" };
            var existingInDb = new User { UserID = 1, UserName = "Old UserName" };

            _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(existingInDb);

            _userRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(true);
            // Act
            var result = _service.UpdateUserAsync(dto);
            // Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => result);
            Assert.Contains("The User UserName ", exception.Message);

            _userRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _userRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsDto_WhenUserNameIsSameAsCurrentRecord()
        {
            // Arrange 
            var dto = new UserDto { UserID = 1, UserName = "Vision UserName" };
            var existingInDb = new User { UserID = 1, UserName = "Vision UserName" };

            _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(existingInDb);

            _userRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                             .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<UserDto>(existingInDb)).Returns(dto);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.UpdateUserAsync(dto);

            // Assert 
            Assert.NotNull(result);

            _userRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsNull_WhenFaildToUpdateInDatabase()
        {
            // Arrange 
            var dto = new UserDto { UserID = 1, UserName = "Vision UserName" };
            var existingInDb = new User { UserID = 1, UserName = "Vision UserName" };

            _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(existingInDb);

            _userRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                             .ReturnsAsync(false);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.UpdateUserAsync(dto);

            // Assert 
            Assert.Null(result);

            _userRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var dto = new UserDto { UserID = 1, UserName = "Vision UserName" };
            _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var result = _service.UpdateUserAsync(dto);
            var exception = await Assert.ThrowsAsync<Exception>(() => result);
            Assert.Contains($"An error occurred while updating the User record", exception.Message);
        }
        #endregion

        #region DeleteUserAsync
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteUserAsync_ThrowsValidationException_WhenIdIsLessOrEqualZero(int invalidId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.DeleteUserAsync(invalidId));

            Assert.Contains("Invalid UserID provided for deletion.", exception.Message);
        }

        [Fact]
        public async Task DeleteUserAsync_ThrowsResourceNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            int userId = 99;
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.DeleteUserAsync(userId));

            Assert.Contains("User with UserID", exception.Message);

            _userRepoMock.Verify(r => r.Delete(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserAsync_ReturnsTrue_WhenUserIsDeleted()
        {
            // Arrange
            int userId = 1;
            var user = new User { UserID = userId };
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteUserAsync(userId);

            // Assert
            Assert.True(result);
            _userRepoMock.Verify(r => r.Delete(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ReturnsFalse_WhenNoRowsAffected()
        {
            // Arrange
            _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new User());
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.DeleteUserAsync(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteUserAsync_ThrowsConflictException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            int userId = 1;
            var user = new User { UserID = userId };
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ThrowsAsync(new Exception("Foreign key violation"));

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteUserAsync(userId));
        }
        #endregion

        #region ToggleUserStatusAsync 
        [Theory]
        [InlineData(true, false)] 
        [InlineData(false, true)]
        public async Task ToggleUserStatusAsync_ChangesStatus_WhenUserExists(bool initialStatus, bool expectedStatus)
        {
            // Arrange
            int userId = 1;
            var fakeUser = new User { UserID = userId, IsActive = initialStatus };

            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(fakeUser);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.ToggleUserStatusAsync(userId);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedStatus, fakeUser.IsActive);
            _userRepoMock.Verify(r => r.Update(fakeUser), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ToggleUserStatusAsync_ThrowsResourceNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            int userId = 99;
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.ToggleUserStatusAsync(userId));

            Assert.Contains($"User with ID {userId} not found", exception.Message);
            _userRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ToggleUserStatusAsync_ReturnsFalse_WhenDatabaseSaveFails()
        {
            // Arrange
            int userId = 1;
            var fakeUser = new User { UserID = userId, IsActive = true };

            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(fakeUser);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.ToggleUserStatusAsync(userId);

            // Assert
            Assert.False(result);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ToggleUserStatusAsync_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            int userId = 1;
            _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var result = _service.ToggleUserStatusAsync(userId);
            var exception = await Assert.ThrowsAsync<Exception>(() => result);
            Assert.Contains($"An error occurred while Toggling User Status record. ", exception.Message);
        }
        #endregion
    }
}
