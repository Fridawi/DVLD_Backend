using DVLD.CORE.DTOs.Countries;

namespace DVLD.CORE.Interfaces
{
    public interface ICountryService
    {
        Task<IEnumerable<CountryDto>> GetAllCountryAsync();
        Task<CountryDto?> GetCountryByIdAsync(int id);
        Task<CountryDto?> GetCountryByNameAsync(string name);
    }
}
