using AutoMapper;
using Castle.Core.Logging;
using DVLD.CORE.DTOs.Countries;
using DVLD.CORE.Entities;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DVLD.Tests.Services
{
    public class CountryServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<CountryService>> _loggerMock;
        private readonly CountryService _countryService;

        public CountryServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<CountryService>>();
            _countryService = new CountryService(_unitOfWorkMock.Object, _mapperMock.Object,_loggerMock.Object);
        }

        #region GetAllCountryAsync 

        [Fact]
        public async Task GetAllCountryAsync_ShouldReturnListOfCountries_WhenDataExists()
        {
            // Arrange
            var countries = new List<Country> { new Country { CountryID = 1, CountryName = "Jordan" } };
            var countryDtos = new List<CountryDto> { new CountryDto { CountryID = 1, CountryName = "Jordan" } };

            _unitOfWorkMock.Setup(u => u.Countries.GetAllAsync(null, false))
                .ReturnsAsync(countries);

            _mapperMock.Setup(m => m.Map<IEnumerable<CountryDto>>(countries)).Returns(countryDtos);

            // Act
            var result = await _countryService.GetAllCountryAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Jordan", result.First().CountryName);
        }

        #endregion

        #region GetCountryByIdAsync 
        [Fact]
        public async Task GetCountryByIdAsync_ShouldReturnCountryDto_WhenIdExists()
        {
            // Arrange
            int countryId = 1;
            var country = new Country { CountryID = countryId, CountryName = "Jordan" };
            var countryDto = new CountryDto { CountryID = countryId, CountryName = "Jordan" };

            _unitOfWorkMock.Setup(u => u.Countries.GetByIdAsync(countryId)).ReturnsAsync(country);
            _mapperMock.Setup(m => m.Map<CountryDto>(country)).Returns(countryDto);

            // Act
            var result = await _countryService.GetCountryByIdAsync(countryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Jordan", result.CountryName);
        }

        [Fact]
        public async Task GetCountryByIdAsync_ThrowsValidationException_WhenIdIsInvalid()
        {
            // Arrange & Act 
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _countryService.GetCountryByIdAsync(0));

            // Assert 
            Assert.Contains("Invalid ID", exception.Message);
        }

        [Fact]
        public async Task GetCountryByIdAsync_ThrowsResourceNotFoundException_WhenCountryDoesNotExist()
        {
            // Arrange
            int countryId = 1;
            _unitOfWorkMock.Setup(u => u.Countries.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Country)null!);

            //  Act 
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => 
                                _countryService.GetCountryByIdAsync(countryId));

            // Assert 
            Assert.Contains("Country with CountryID ", exception.Message);
        }

        #endregion

        #region GetCountryByName 
        [Fact]
        public async Task GetCountryByName_ShouldReturnCountryDto_WhenNameExists()
        {
            // Arrange
            string countryName = "Jordan";
            var country = new Country { CountryID = 1, CountryName = countryName };
            var countryDto = new CountryDto { CountryID = 1, CountryName = countryName };

            _unitOfWorkMock.Setup(u => u.Countries.FindAsync(It.IsAny<Expression<Func<Country, bool>>>(), null, false))
                .ReturnsAsync(country);
            _mapperMock.Setup(m => m.Map<CountryDto>(country)).Returns(countryDto);

            // Act
            var result = await _countryService.GetCountryByNameAsync(countryName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(countryName, result.CountryName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task GetCountryByName_ThrowsValidationException_WhenNameIsNullOrWhiteSpace(string? name)
        {
            // Arrange & Act 
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                                        _countryService.GetCountryByNameAsync(name!));
            // Assert 
            Assert.Contains("Country name cannot be null or empty.", exception.Message);
        }

        [Fact]
        public async Task GetCountryByName_ThrowsResourceNotFoundException_WhenCountryDoesNotExist()
        {
            // Arrange
            string validName = "ssssss";
            _unitOfWorkMock.Setup(u => u.Countries.FindAsync(It.IsAny<Expression<Func<Country,bool>>>(),
                It.IsAny<string[]>(),It.IsAny<bool>())).ReturnsAsync((Country)null!);

            //  Act 
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
                                        _countryService.GetCountryByNameAsync(validName));

            // Assert 
            Assert.Contains("Country with Name ", exception.Message);
        }

        #endregion
    }
}
