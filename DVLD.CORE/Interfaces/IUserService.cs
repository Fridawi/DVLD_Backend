using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Users;

namespace DVLD.CORE.Interfaces
{
    public interface IUserService
    {
        Task<AuthResponseDto> AuthenticateAsync(LoginRequestDto loginRequest);
        Task<UserDto> GetUserByIdAsync(int id);
        Task<UserDto> GetUserByPersonIdAsync(int personId);
        Task<int> GetPersonIdByUserIdAsync(int personId);
        Task<PagedResultDto<UserDto>> GetAllUsersAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<UserDto?> AddUserAsync(UserCreateDto userCreateDto);
        Task<UserDto?> UpdateUserAsync( UserDto userDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, string newPassword);
        Task<bool> ToggleUserStatusAsync(int userId); 
        Task<bool> IsUserNameExistAsync(string userName);
        Task<bool> IsPersonAlreadyUserAsync(int personId);
    }
}
