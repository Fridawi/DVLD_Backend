using AutoMapper;
using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Licenses.InternationalLicenses;
using DVLD.CORE.DTOs.People;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq.Expressions;
using System.Text;

namespace DVLD.Tests.Services
{
    public class PersonServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<Person>> _personRepoMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IGenericRepository<Country>> _countryMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<PersonService>> _loggerMock;
        private readonly PersonService _service;

        public PersonServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _personRepoMock = new Mock<IGenericRepository<Person>>();
            _fileServiceMock = new Mock<IFileService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _countryMock = new Mock<IGenericRepository<Country>>();
            _userServiceMock = new Mock<IUserService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<PersonService>>();
            _unitOfWorkMock.Setup(u => u.People).Returns(_personRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Countries).Returns(_countryMock.Object);
            _service = new PersonService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _userServiceMock.Object,
                _fileServiceMock.Object,
                _httpContextAccessorMock.Object,
                _loggerMock.Object);
            SetupMockHttpContext();
        }

        #region Data 
        private void SetupMockHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("localhost", 7001);
            _httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(context);
        }
        private List<Person> GetFakePeople()
        {
            return new List<Person>
            {
                new Person
                {
                    PersonID = 1,
                    FirstName = "Ali",
                    SecondName = "Ahmed",
                    ThirdName = "Hassan",
                    LastName = "Omar",
                    NationalNo = "123456789",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gendor = 1,
                    Address = "Cairo",
                    Phone = "01000000001",
                    Email = "ali@example.com",
                    NationalityCountryID = 1,
                    CountryInfo = new Country { CountryID = 1, CountryName = "Egypt" },
                    ImagePath = null
                },
                new Person
                {
                    PersonID = 2,
                    FirstName = "Sara",
                    SecondName = "Mohamed",
                    ThirdName = null,
                    LastName = "Youssef",
                    NationalNo = "987654321",
                    DateOfBirth = new DateTime(1995, 5, 5),
                    Gendor = 2,
                    Address = "Alexandria",
                    Phone = "01000000002",
                    Email = "sara@example.com",
                    NationalityCountryID = 2,
                    CountryInfo = new Country { CountryID = 2, CountryName = "Jordan" },
                    ImagePath = "sara.jpg"
                }
            };
        }

        private List<PersonDto> GetFakePeopleDtos()
        {
            return new List<PersonDto>
            {
                new PersonDto
                {
                    PersonID = 1,
                    NationalNo = "123456789",
                    FullName = "Ali Ahmed Hassan Omar",
                    GenderName = "Male",
                    Gendor = 0,
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Email = "ali@example.com",
                    Phone = "01000000001",
                    Address = "Cairo",
                    CountryName = "Egypt",
                    NationalityCountryID = 1,
                },
                new PersonDto
                {
                    PersonID = 2,
                    NationalNo = "987654321",
                    FullName = "Sara Mohamed Youssef",
                    GenderName = "Female",
                    Gendor = 1,
                    DateOfBirth = new DateTime(1995, 5, 5),
                    Email = "sara@example.com",
                    Phone = "01000000002",
                    Address = "Alexandria",
                    CountryName = "Jordan",
                    NationalityCountryID = 2,
                }
            };
        }
        public static TheoryData<PersonDto> PersonTheoryData => new TheoryData<PersonDto>
        {
            new PersonServiceTests().GetFakePeopleDtos()[0],
            new PersonServiceTests().GetFakePeopleDtos()[1]
        };
        #endregion

        #region GetAllPeoplePagedAsync
        [Fact]
        public async Task GetAllPeoplePagedAsync_ReturnsPagedResultDto_WhenDataExists()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            var fakePeople = GetFakePeople();
            var fakePeopleDto = GetFakePeopleDtos();
            int expectedTotalCount = 50;

            _personRepoMock
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(), 
                    It.IsAny<string[]>(),                       
                    It.IsAny<bool>(),                           
                    It.IsAny<Expression<Func<Person, object>>>(),
                    It.IsAny<EnOrderByDirection>(),             
                    It.IsAny<int?>(),                           
                    It.IsAny<int?>()                           
                ))
                .ReturnsAsync(fakePeople);

            _personRepoMock
                .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(expectedTotalCount);

            _mapperMock.Setup(m => m.Map<IEnumerable<PersonDto>>(
                fakePeople,
                It.IsAny<Action<IMappingOperationOptions<object, IEnumerable<PersonDto>>>>()))
                .Returns(fakePeopleDto);

            // Act
            var result = await _service.GetAllPeoplePagedAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(fakePeopleDto.Count(), result.Data.Count());
            Assert.Equal(expectedTotalCount, result.TotalCount);
            Assert.Equal(pageNumber, result.PageNumber);
            Assert.Equal(pageSize, result.PageSize);

            Assert.Equal("Ali Ahmed Hassan Omar", result.Data.First().FullName);
            Assert.Equal("Egypt", result.Data.First().CountryName);

            _personRepoMock.Verify(r => r.FindAllAsync(
                It.IsAny<Expression<Func<Person, bool>>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<Person, object>>>(),
                It.IsAny<EnOrderByDirection>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()
            ), Times.Once);

            _personRepoMock.Verify(r => r.CountAsync(It.IsAny<Expression<Func<Person, bool>>>()), Times.Once);
        }
        #endregion

        #region GetPersonByIdAsync
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetPersonByIdAsync_ThrowsValidationException_WhenPersonIdIsLessOrEqualZero(int id)
        {
            // Arrange & Act
            var result = _service.GetPersonByIdAsync(id);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(()=> result);
            Assert.Contains("Invalid ID",exception.Message);
        }

        [Fact]
        public async Task GetPersonByIdAsync_ThrowsResourceNotFoundException_WhenPersonIsNotFound()
        {
            // Arrange
            _personRepoMock.Setup(r => r.FindAsync(
               It.IsAny<Expression<Func<Person, bool>>>(),
               It.IsAny<string[]>())).ReturnsAsync((Person)null!);

            // Act
            var result = _service.GetPersonByIdAsync(1);

            // Assert 
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);
            Assert.Contains("Person with PersonID", exception.Message);
        }

        [Fact]
        public async Task GetPersonByIdAsync_ReturnsPersonDto_WhenPersonIsFound()
        {
            // Arrange
            var expectedPerson = GetFakePeople()[0];
            var expectedPersonDto = GetFakePeopleDtos()[0];

            _personRepoMock.Setup(r => r.FindAsync(
               It.IsAny<Expression<Func<Person, bool>>>(),
               It.IsAny<string[]>())).ReturnsAsync(expectedPerson);

            _mapperMock.Setup(m => m.Map<PersonDto>(
                expectedPerson,
                It.IsAny<Action<IMappingOperationOptions<object, PersonDto>>>()))
                .Returns(expectedPersonDto);

            // Act
            var result = await _service.GetPersonByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPersonDto.PersonID, result.PersonID);
        }


        #endregion

        #region GetPersonByNationalNoAsync
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetPersonByNationalNoAsync_ThrowsValidationException_WhenNationalNoIsNullOrWhiteSpace(string nationalNo)
        {
            // Act & Assert
            var result = _service.GetPersonByNationalNoAsync(nationalNo);

            var exception = await Assert.ThrowsAsync<ValidationException>(() => result);

            Assert.Contains("National number cannot be null or empty.", exception.Message);
        }
        
        [Fact]
        public async Task GetPersonByNationalNoAsync_ThrowsResourceNotFoundException_WhenPersonIsNotFound()
        {
            // Arrange
            var nationalNo = "No.1";

            _personRepoMock.Setup(r => r.FindAsync(
               It.IsAny<Expression<Func<Person, bool>>>(),
               It.IsAny<string[]>())).ReturnsAsync((Person)null!);

            // Act
            var result = _service.GetPersonByNationalNoAsync(nationalNo);

            // Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => result);

            Assert.Contains("Person with National Number ", exception.Message);
        }

        [Fact]
        public async Task GetPersonByNationalNoAsync_ReturnsPersonDto_WhenNationalNoIsNotNullOrWhiteSpace()
        {
            // Arrange 
            var nationalNo = "No.1";
            var fakePerson = new Person { PersonID = 1, NationalNo = nationalNo };
            var expectedDto = new PersonDto { PersonID = 1, NationalNo = nationalNo };

            _personRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Person, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(fakePerson);

            _mapperMock.Setup(m => m.Map<PersonDto>(
                fakePerson,
                It.IsAny<Action<IMappingOperationOptions<object, PersonDto>>>()))
                .Returns(expectedDto);

            // Act
            var result = await _service.GetPersonByNationalNoAsync(nationalNo);

            // Assert 
            Assert.NotNull(result);
            Assert.Equal(nationalNo, result.NationalNo);
        }

        [Fact]
        public async Task GetPersonByNationalNoAsync_ShouldTrimNationalNo_BeforeSearching()
        {
            // Arrange
            var rawNo = "  12345  ";
            var trimmedNo = "12345";

            _personRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Person, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(new Person { NationalNo = trimmedNo, PersonID = 1 });

            // Act
            await _service.GetPersonByNationalNoAsync(rawNo);

            // Assert
            _personRepoMock.Verify(r => r.FindAsync(It.Is<Expression<Func<Person, bool>>>(exp =>
                CheckExpressionResult(exp, trimmedNo)), It.IsAny<string[]>()), Times.Once);
        }
        private bool CheckExpressionResult(Expression<Func<Person, bool>> exp, string expectedValue)
        {
            var compiled = exp.Compile();
            return compiled.Invoke(new Person { NationalNo = expectedValue });
        }
        #endregion

        #region Add Person Tests
        [Fact]
        public async Task AddPersonAsync_ShouldReturnDto_WhenPersonIsAddedWithoutImage()
        {
            // Arrange
            var personDto = GetFakePeopleDtos()[0];
            var fakePerson = GetFakePeople()[0];
            fakePerson.PersonID = 10;

            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(false);
            _countryMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Country, bool>>>())).ReturnsAsync(true);

            _mapperMock.Setup(m => m.Map<Person>(personDto)).Returns(fakePerson);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _personRepoMock.Setup(r => r.FindAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<string[]>()))
                    .ReturnsAsync(fakePerson);

            _mapperMock.Setup(m => m.Map<PersonDto>(fakePerson,
                    It.IsAny<Action<IMappingOperationOptions<object, PersonDto>>>()))
                    .Returns(personDto);

            // Act
            var result = await _service.AddPersonAsync(personDto, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PersonDto>(result);
            Assert.Equal(personDto.NationalNo, result.NationalNo);

            _personRepoMock.Verify(r => r.AddAsync(It.IsAny<Person>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddPersonAsync_ShouldReturnDto_WhenPersonIsAddedWithImage()
        {
            // Arrange
            var personDto = GetFakePeopleDtos()[0];
            var fakePerson = new Person { PersonID = 1, NationalNo = personDto.NationalNo };
            var stream = new MemoryStream();
            string fileName = "profile.jpg";
            string savedPath = "uploaded_name.jpg";

            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(false);
            _countryMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Country, bool>>>())).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<Person>(personDto)).Returns(fakePerson); // ضروري لتحويل الـ DTO لـ Entity

            _fileServiceMock.Setup(f => f.IsImage(fileName)).Returns(true);
            _fileServiceMock.Setup(f => f.SaveFileAsync(stream, fileName, "people")).ReturnsAsync(savedPath);

            _personRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Person, bool>>>(),
                It.IsAny<string[]>())) 
                .ReturnsAsync(fakePerson);

            _mapperMock.Setup(m => m.Map<PersonDto>(
                It.IsAny<Person>(),
                It.IsAny<Action<IMappingOperationOptions<object, PersonDto>>>()))
                .Returns(personDto);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.AddPersonAsync(personDto, stream, fileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(savedPath, fakePerson.ImagePath); 
            Assert.Equal(personDto.NationalNo, result.NationalNo);

            _personRepoMock.Verify(r => r.AddAsync(It.IsAny<Person>()), Times.Once);
            _fileServiceMock.Verify(f => f.SaveFileAsync(stream, fileName, "people"), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddPersonAsync_ThrowConflictException_WhenNationalNoIsExist()
        {
            // Arrange
            var personDto = GetFakePeopleDtos()[0];
            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _service.AddPersonAsync(personDto, null, null));
            Assert.Contains("The National Number ", exception.Message);
        }

        [Fact]
        public async Task AddPersonAsync_ThrowValidationException_WhenCountryDoesNotExist()
        {
            // Arrange
            var personDto = GetFakePeopleDtos()[0];
            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(false);
            _countryMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Country, bool>>>())).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.AddPersonAsync(personDto, null, null));
            Assert.Contains("The selected country is invalid or does not exist.", exception.Message);
        }

        [Fact]
        public async Task AddPersonAsync_ShouldReturnNullAndCleanupFile_WhenDatabaseSaveFails()
        {
            // Arrange
            var dto = GetFakePeopleDtos()[0];
            string tempPath = "temp_to_delete.jpg";

            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(false);
            _countryMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Country, bool>>>())).ReturnsAsync(true);

            _fileServiceMock.Setup(f => f.IsImage(It.IsAny<string>())).Returns(true);

            _fileServiceMock.Setup(f => f.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "people")).ReturnsAsync(tempPath);

            _mapperMock.Setup(m => m.Map<Person>(dto)).Returns(new Person());

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.AddPersonAsync(dto, new MemoryStream(), "test.jpg");

            // Assert
            Assert.Null(result);

            _fileServiceMock.Verify(f => f.DeleteFile(tempPath, "people"), Times.Once);
        }

        [Fact]
        public async Task AddPersonAsync_ShouldCleanupFile_WhenExceptionIsThrown()
        {
            // Arrange
            var dto = GetFakePeopleDtos()[0];
            string tempPath = "exception_cleanup.jpg";

            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(false);
            _countryMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Country, bool>>>())).ReturnsAsync(true);
            _fileServiceMock.Setup(f => f.IsImage(It.IsAny<string>())).Returns(true);
            _fileServiceMock.Setup(f => f.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "people")).ReturnsAsync(tempPath);

            _mapperMock.Setup(m => m.Map<Person>(dto)).Returns(new Person());

            _personRepoMock.Setup(r => r.AddAsync(It.IsAny<Person>())).ThrowsAsync(new Exception("Database Error"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AddPersonAsync(dto, new MemoryStream(), "test.jpg"));
            Assert.Contains("An error occurred", ex.Message);
            _fileServiceMock.Verify(f => f.DeleteFile(tempPath, "people"), Times.Once);
        }

        #endregion

        #region UpdatePersonAsync
        [Theory]
        [MemberData(nameof(PersonTheoryData))]
        public async Task UpdatePersonAsync_ThrowsResourceNotFoundException_WhenPersonIsNotFound(PersonDto person)
        {
            // Arrange
            _personRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync((Person)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.UpdatePersonAsync(person, null, null,0,""));
            Assert.Contains("Cannot update: Person with PersonID ", exception.Message);
        }

        [Theory]
        [MemberData(nameof(PersonTheoryData))]
        public async Task UpdatePersonAsync_ThrowsForbiddenException_WhenUserAttemptsToUpdateAnotherStaffMember(PersonDto person)
        {
            // Arrange
            int currentUserId = 10;
            string currentUserRole = UserRoles.User; // Not Admin
            var existingPerson = new Person { PersonID = person.PersonID };
            var anotherStaffPersonId = 99; // // Not The Target PersonID

            _personRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPerson);

            // When perosn is a user in the system
            _userServiceMock.Setup(s => s.IsPersonAlreadyUserAsync(person.PersonID))
                .ReturnsAsync(true);

            // when current user is not the same as the target person
            _userServiceMock.Setup(s => s.GetPersonIdByUserIdAsync(currentUserId))
                .ReturnsAsync(anotherStaffPersonId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ForbiddenException>(() =>
                _service.UpdatePersonAsync(person, null, null, currentUserId, currentUserRole));

            Assert.Contains("You cannot edit other users' data.", exception.Message);
        }

        [Fact]
        public async Task UpdatePersonAsync_Succeeds_WhenUserUpdatesTheirOwnData()
        {
            // Arrange
            var personDto = GetFakePeopleDtos()[0];
            int currentUserId = 10;
            string currentUserRole = UserRoles.User;

            var existingPersonInDb = new Person { PersonID = personDto.PersonID, NationalNo = personDto.NationalNo, ImagePath = "old.jpg" };

            _personRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPersonInDb);

            _userServiceMock.Setup(s => s.IsPersonAlreadyUserAsync(personDto.PersonID))
                .ReturnsAsync(true);
            _userServiceMock.Setup(s => s.GetPersonIdByUserIdAsync(currentUserId))
                .ReturnsAsync(personDto.PersonID);

            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(false);
            _countryMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Country, bool>>>()))
                .ReturnsAsync(true);

            _mapperMock.Setup(m => m.Map(personDto, existingPersonInDb)).Returns(existingPersonInDb);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _personRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Person, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(existingPersonInDb);

            _mapperMock.Setup(m => m.Map<PersonDto>(
                It.IsAny<Person>(),
                It.IsAny<Action<IMappingOperationOptions<object, PersonDto>>>()))
                .Returns(personDto);

            // Act
            var result = await _service.UpdatePersonAsync(personDto, null, null, currentUserId, currentUserRole);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(personDto.PersonID, result.PersonID);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _personRepoMock.Verify(r => r.Update(It.IsAny<Person>()), Times.Once);
        }

        [Theory]
        [MemberData(nameof(PersonTheoryData))]
        public async Task UpdatePersonAsync_ReturnsDto_WhenUpdateIsSuccessfulWithoutImage(PersonDto person)
        {
            // Arrange
            int currentUserId = 10;
            string currentUserRole = UserRoles.User;
            var existingPerson = new Person { PersonID = person.PersonID, NationalNo = person.NationalNo };

            _personRepoMock  .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(existingPerson);

            _userServiceMock.Setup(s => s.IsPersonAlreadyUserAsync(person.PersonID))
                .ReturnsAsync(true);

            _userServiceMock.Setup(s => s.GetPersonIdByUserIdAsync(currentUserId))
                .ReturnsAsync(person.PersonID);

            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(false);
            _countryMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Country, bool>>>())).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<Person>(person)).Returns(existingPerson);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _personRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Person, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(existingPerson);

            _mapperMock.Setup(m => m.Map<PersonDto>(
                It.IsAny<Person>(),
                It.IsAny<Action<IMappingOperationOptions<object, PersonDto>>>()))
                .Returns(person);
            // Act 
            var result = await _service.UpdatePersonAsync(person, null, null, currentUserId, currentUserRole);

            // Assert
            Assert.NotNull(result);
            _personRepoMock.Verify(r => r.Update(It.IsAny<Person>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Theory]
        [MemberData(nameof(PersonTheoryData))]
        public async Task UpdatePersonAsync_ReturnsDto_WhenUpdateIsSuccessfulWithImage(PersonDto person)
        {
            // Arrange
            int currentUserId = 10;
            string currentUserRole = UserRoles.User;
            var existingPerson = new Person { PersonID = person.PersonID, ImagePath = "old-image.jpg" };
            var imageStream = new MemoryStream(Encoding.UTF8.GetBytes("fake-image-content"));
            var fileName = "new-image.jpg";
            string savedPath = "new-unique-name.jpg";

            _personRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Person, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(existingPerson);

            _userServiceMock.Setup(s => s.IsPersonAlreadyUserAsync(person.PersonID))
                .ReturnsAsync(true);

            _userServiceMock.Setup(s => s.GetPersonIdByUserIdAsync(currentUserId))
                .ReturnsAsync(person.PersonID);

            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(false);
            _countryMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Country, bool>>>())).ReturnsAsync(true);

            _fileServiceMock.Setup(f => f.IsImage(fileName)).Returns(true);
            _fileServiceMock.Setup(f => f.SaveFileAsync(It.IsAny<Stream>(), fileName, "people"))
                .ReturnsAsync(savedPath);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _personRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Person, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(existingPerson);

            _mapperMock.Setup(m => m.Map<PersonDto>(
                It.IsAny<Person>(),
                It.IsAny<Action<IMappingOperationOptions<object, PersonDto>>>()))
                .Returns(person);
            // Act
            var result = await _service.UpdatePersonAsync(person, imageStream, fileName, currentUserId, currentUserRole);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(savedPath, existingPerson.ImagePath);
            _mapperMock.Verify(m => m.Map(person, existingPerson), Times.Once);
            _fileServiceMock.Verify(f => f.SaveFileAsync(imageStream, fileName, "people"), Times.Once);
            _fileServiceMock.Verify(f => f.DeleteFile("old-image.jpg", "people"), Times.Once);
        }

        [Theory]
        [MemberData(nameof(PersonTheoryData))]
        public async Task UpdatePersonAsync_ThrowsConflictException_WhenNationalNoExistsForAnotherPerson(PersonDto person)
        {
            // Arrange
            var existingPerson = new Person { PersonID = person.PersonID };
            _personRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPerson);

            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _service.UpdatePersonAsync(person, null, null,0,""));
            Assert.Contains("The National Number", exception.Message);
        }

        [Fact]
        public async Task UpdatePersonAsync_ShouldNotThrow_WhenNationalNoBelongsToSamePersonDuringUpdate()
        {
            // Arrange
            int currentUserId = 10;
            string currentUserRole = UserRoles.User;
            var dto = new PersonDto { PersonID = 1, NationalNo = "123" };
            var existingPerson = new Person { PersonID = 1, NationalNo = "123" };
            _personRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Person, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(existingPerson);

            // even if the person is already a user in the system
            _userServiceMock.Setup(s => s.IsPersonAlreadyUserAsync(dto.PersonID))
                .ReturnsAsync(true);

            // but the current user is the same as the target person (editing own data)
            _userServiceMock.Setup(s => s.GetPersonIdByUserIdAsync(currentUserId))
                .ReturnsAsync(dto.PersonID);

            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(false);

            _countryMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Country, bool>>>()))
                .ReturnsAsync(true);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _personRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Person, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(existingPerson);

            _mapperMock.Setup(m => m.Map<PersonDto>(
                It.IsAny<Person>(),
                It.IsAny<Action<IMappingOperationOptions<object, PersonDto>>>()))
                .Returns(dto);

            // Act
            var result = await _service.UpdatePersonAsync(dto, null, null, currentUserId, currentUserRole);

            // Assert
            Assert.NotNull(result);
            _personRepoMock.Verify(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>()), Times.Once);
        }

        [Theory]
        [MemberData(nameof(PersonTheoryData))]
        public async Task UpdatePersonAsync_ReturnsDto_WhenNationalNoIsSameAsCurrent(PersonDto person)
        {
            // Arrange
            int currentUserId = 10;
            string currentUserRole = UserRoles.User;
            var existingPerson = new Person
            {
                PersonID = person.PersonID,
                NationalNo = person.NationalNo
            };

            _personRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPerson);

            // even if the person is already a user in the system
            _userServiceMock.Setup(s => s.IsPersonAlreadyUserAsync(person.PersonID))
                .ReturnsAsync(true);

            // but the current user is the same as the target person (editing own data)
            _userServiceMock.Setup(s => s.GetPersonIdByUserIdAsync(currentUserId))
                .ReturnsAsync(person.PersonID);

            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(false);

            _countryMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Country, bool>>>())).ReturnsAsync(true);

            _mapperMock.Setup(m => m.Map<Person>(person)).Returns(existingPerson);

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _personRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Person, bool>>>(),
                It.IsAny<string[]>()))
                .ReturnsAsync(existingPerson);

            _mapperMock.Setup(m => m.Map<PersonDto>(
                It.IsAny<Person>(),
                It.IsAny<Action<IMappingOperationOptions<object, PersonDto>>>()))
                .Returns(person);
            // Act
            var result = await _service.UpdatePersonAsync(person, null, null, currentUserId, currentUserRole);

            // Assert
            Assert.NotNull(result);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Theory]
        [MemberData(nameof(PersonTheoryData))]
        public async Task UpdatePersonAsync_ThrowsValidationException_WhenCountryDoesNotExist(PersonDto person)
        {
            // Arrange
            int currentUserId = 10;
            string currentUserRole = UserRoles.User;
            var existingPerson = new Person
            {
                PersonID = person.PersonID,
                NationalNo = person.NationalNo
            };

            _personRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPerson);

            // even if the person is already a user in the system
            _userServiceMock.Setup(s => s.IsPersonAlreadyUserAsync(person.PersonID))
                .ReturnsAsync(true);

            // but the current user is the same as the target person (editing own data)
            _userServiceMock.Setup(s => s.GetPersonIdByUserIdAsync(currentUserId))
                .ReturnsAsync(person.PersonID);

            _personRepoMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(false);

            _countryMock.Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<Country, bool>>>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.UpdatePersonAsync(person, null, null, currentUserId, currentUserRole));
            Assert.Contains("The selected country is invalid or does not exist.", exception.Message);
        }

        [Theory]
        [MemberData(nameof(PersonTheoryData))]
        public async Task UpdatePersonAsync_ThrowsException_WhenDatabaseErrorOccurs(PersonDto person)
        {
            // Arrange
            _personRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.UpdatePersonAsync(person, null, null,0,""));
            Assert.Contains($"An error occurred while updating the person with PersonID {person.PersonID}", exception.Message);
        }
        #endregion

        #region DeletePersonAsync
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeletePersonAsync_ThrowsValidationException_WhenIdIsLessOrEqualZero(int invalidId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.DeletePersonAsync(invalidId));

            Assert.Contains("Invalid PersonID provided for deletion.", exception.Message);
        }

        [Fact]
        public async Task DeletePersonAsync_ReturnsTrue_WhenPersonIsDeletedSuccessfully()
        {
            // Arrange
            int personId = 1;
            var fakePerson = new Person { PersonID = personId };

            _personRepoMock.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync(fakePerson);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.DeletePersonAsync(personId);

            // Assert
            Assert.True(result);
            _personRepoMock.Verify(r => r.Delete(fakePerson), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeletePersonAsync_ReturnsTrue_WhenPersonIsDeletedSuccessfullyAddDeleteHisImage()
        {
            // Arrange
            int personId = 1;
            var fakePerson = new Person { PersonID = personId, ImagePath = "old-image.jpg" };

            _personRepoMock.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync(fakePerson);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.DeletePersonAsync(personId);

            // Assert
            Assert.True(result);
            _personRepoMock.Verify(r => r.Delete(fakePerson), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _fileServiceMock.Verify(f => f.DeleteFile("old-image.jpg", "people"), Times.Once);
        }

        [Fact]
        public async Task DeletePersonAsync_ReturnsTrue_WhenPersonIsDeletedSuccessfullyWithNoImage()
        {
            // Arrange
            int personId = 1;
            var fakePerson = new Person { PersonID = personId, ImagePath = null };

            _personRepoMock.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync(fakePerson);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.DeletePersonAsync(personId);

            // Assert
            Assert.True(result);
            _personRepoMock.Verify(r => r.Delete(fakePerson), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _fileServiceMock.Verify(f => f.DeleteFile(It.IsAny<string>(), "people"), Times.Never);
        }

        [Fact]
        public async Task DeletePersonAsync_ThrowsResourceNotFoundException_WhenPersonDoesNotExist()
        {
            // Arrange
            int personId = 99;
            _personRepoMock.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync((Person)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.DeletePersonAsync(personId));

            Assert.Contains("Person with PersonID", exception.Message);

            _personRepoMock.Verify(r => r.Delete(It.IsAny<Person>()), Times.Never);
        }

        [Fact]
        public async Task DeletePersonAsync_ReturnsFalse_WhenNoRowsAffected()
        {
            // Arrange
            _personRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Person());
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.DeletePersonAsync(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeletePersonAsync_ThrowsConflictException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            int personId = 1;
            _personRepoMock.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync(new Person());
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ThrowsAsync(new Exception("Foreign Key Constraint"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _service.DeletePersonAsync(personId));
            Assert.Contains("Cannot delete this person because they are linked to other records (like Users, Applications, or Licenses).", exception.Message);
        }
        #endregion
    }
}

