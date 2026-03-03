using AutoMapper;
using DVLD.CORE.DTOs.Countries;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using Microsoft.Extensions.Logging;

namespace DVLD.Services
{
    public class CountryService : ICountryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CountryService> _logger;
        public CountryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CountryService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<IEnumerable<CountryDto>> GetAllCountryAsync()
        {
            _logger.LogInformation("Fetching all countries from the database.");
            var countries = await _unitOfWork.Countries.GetAllAsync(null, false);
            return _mapper.Map<IEnumerable<CountryDto>>(countries);
        }

        public async Task<CountryDto?> GetCountryByIdAsync(int id)
        {
            _logger.LogInformation("Attempting to fetch country with CountryID: {CountryID}", id);
            if (id <= 0) throw new ValidationException("Invalid ID");

            var country = await _unitOfWork.Countries.GetByIdAsync(id);

            if (country == null)
            {
                _logger.LogWarning("Country with CountryID {CountryID} was not found.", id);
                throw new ResourceNotFoundException($"Country with CountryID {id} was not found.");
            }
            return _mapper.Map<CountryDto>(country);
        }

        public async Task<CountryDto?> GetCountryByNameAsync(string name)
        {
            _logger.LogInformation("Attempting to fetch country with Name: {Name}", name);
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Country name cannot be null or empty.");

            var country = await _unitOfWork.Countries.FindAsync(c => c.CountryName == name.Trim(), null, false);

            if (country == null)
            {
                _logger.LogWarning("Country with Name '{Name}' was not found.", name);
                throw new ResourceNotFoundException($"Country with Name {name} was not found.");
            }
            return  _mapper.Map<CountryDto>(country);
        }
    }
}
