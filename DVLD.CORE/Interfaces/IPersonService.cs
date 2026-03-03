using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.People;

namespace DVLD.CORE.Interfaces
{
    public interface IPersonService
    {
        Task<PagedResultDto<PersonDto>> GetAllPeoplePagedAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<PersonDto> GetPersonByIdAsync(int id);
        Task<PersonDto> GetPersonByNationalNoAsync(string nationalNo);
        Task<PersonDto?> AddPersonAsync(PersonDto personDto, Stream? imageStream, string? originalFileName);
        Task<PersonDto?> UpdatePersonAsync(PersonDto personDto, Stream? imageStream, string? originalFileName,int currentUserId, string currentUserRole);
        Task<bool> DeletePersonAsync(int id);
        Task<bool> IsPersonExistByIdAsync(int id);
    }
}
