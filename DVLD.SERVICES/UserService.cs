using AutoMapper;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.People;
using DVLD.CORE.DTOs.Users;
using DVLD.CORE.Entities;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.CORE.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace DVLD.SERVICES
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IJwtProvider _jwtProvider;
        private readonly JwtSettings _jwtSettings;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger, IJwtProvider jwtProvider, IOptions<JwtSettings> jwtOptions)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _jwtProvider = jwtProvider;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<AuthResponseDto> AuthenticateAsync(LoginRequestDto loginRequest)
        {
            _logger.LogInformation("Login attempt for user: {UserName}", loginRequest.UserName);

            var userInDb = await _unitOfWork.Users.FindAsync(u => u.UserName == loginRequest.UserName, null, false);


            if (userInDb == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, userInDb.Password.Trim()))
            {
                _logger.LogWarning("Invalid login attempt for user: {UserName}", loginRequest.UserName);
                return new AuthResponseDto { IsAuthenticated = false, Message = "Invalid UserName or Password" };
            }

            if (!userInDb.IsActive)
            {
                _logger.LogWarning("Deactivated account login attempt: {UserName}", loginRequest.UserName);
                return new AuthResponseDto { IsAuthenticated = false, Message = "Account is deactivated. Please contact admin." };
            }

            var token = _jwtProvider.Generate(userInDb);

            return new AuthResponseDto
            {
                IsAuthenticated = true,
                Token = token,
                ExpiresOn = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                UserID = userInDb.UserID,
                UserName = userInDb.UserName,
                Role = userInDb.Role,
                Message = "Success"
            };
        }
        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            _logger.LogInformation("Attempting to Change Password with UserID: {UserID}", userId);
            try
            {
                var userInDb = await _unitOfWork.Users.GetByIdAsync(userId);
                if (userInDb == null)
                    throw new ResourceNotFoundException($"Cannot Change Password: User with ID {userId} not found.");

                userInDb.Password = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);

                _unitOfWork.Users.Update(userInDb);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully Changed Password with UserID: {UserID}", userInDb.UserID);
                    return true;
                }

                _logger.LogError("Failed to Changed Password to the database for UserName: {UserName}", userInDb.UserName);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validation error while Changed Password UserID: {UserID}", userId);

                if (ex is ResourceNotFoundException) throw;

                _logger.LogCritical(ex, "Unexpected error Changed Password with UserID {UserID}", userId);
                throw new Exception("An error occurred while Changed Password record. " + ex.Message);
            }
        }

        public async Task<PagedResultDto<UserDto>> GetAllUsersAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching all Users records.");

            Expression<Func<User, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                filter = filterColumn.ToLower() switch
                {
                    "userid" when int.TryParse(filterValue, out int id) => p => p.UserID == id,

                    "personid" when int.TryParse(filterValue, out int id) => p => p.PersonID == id,

                    "username" => p => p.UserName.Contains(filterValue),

                    "isactive" => p => p.IsActive ==Convert.ToBoolean( filterValue),

                    "role" => p => p.Role.Contains(filterValue),
                    _ => p => true
                };
            }

            int skip = (pageNumber - 1) * pageSize;

            var users = await _unitOfWork.Users.FindAllAsync(
                predicate: filter,
                includes: null,
                tracked: false,
                orderBy: p => p.UserName,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.Users.CountAsync(filter);

            var mappedUsers = _mapper.Map<IEnumerable<UserDto>>(users);

            return new PagedResultDto<UserDto>
            {
                Data = mappedUsers,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            _logger.LogInformation("Fetching User with UserID: {UserID}", userId);
            if (userId <= 0) throw new ValidationException("Invalid ID");

            var user = await _unitOfWork.Users.FindAsync(u => u.UserID == userId, null, false);

            if (user == null)
            {
                _logger.LogWarning("User with ID {Id} was not found.", userId);
                throw new ResourceNotFoundException($"User with ID {userId} was not found.");
            }
            return _mapper.Map<UserDto>(user);
        }
        public async Task<UserDto> GetUserByPersonIdAsync(int personId)
        {
            _logger.LogInformation("Fetching User with PersonID: {personId}", personId);
            if (personId <= 0) throw new ValidationException("Invalid ID");

            var user = await _unitOfWork.Users.FindAsync(u => u.PersonID == personId, null, false);

            if (user == null)
            {
                _logger.LogWarning("User with PersonID {personId} was not found.", personId);
                throw new ResourceNotFoundException($"User with PersonID {personId} was not found.");
            }
            return _mapper.Map<UserDto>(user);
        }
        public async Task<int> GetPersonIdByUserIdAsync(int userId)
        {
            var personId = await _unitOfWork.Users.GetProjectedByIdAsync(
                u => u.UserID == userId,
                u => u.PersonID  
            );

            if (personId == 0) throw new ResourceNotFoundException("User not found.");

            return personId;
        }
        public async Task<UserDto?> AddUserAsync(UserCreateDto userCreateDto)
        {
            _logger.LogInformation("Attempting to add a new User with UserName: {UserName}", userCreateDto.UserName);
            try
            {
                if (await IsPersonAlreadyUserAsync(userCreateDto.PersonID))
                    throw new ConflictException($"The PersonID '{userCreateDto.PersonID}' is already registered for another User.");

                await ValidateUserDataAsync(userCreateDto.PersonID, userCreateDto.UserName);

                userCreateDto.Password = BCrypt.Net.BCrypt.HashPassword(userCreateDto.Password, 12);

                var user = _mapper.Map<User>(userCreateDto);

                await _unitOfWork.Users.AddAsync(user);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully added User with UserID: {UserID}", user.UserID);
                    return _mapper.Map<UserDto>(user);
                }

                _logger.LogError("Failed to save User record to the database for UserName: {UserName}", userCreateDto.UserName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Conflict error while adding User: {UserName}", userCreateDto.UserName);

                if (ex is ConflictException) throw;

                _logger.LogCritical(ex, "Unexpected error adding User with {UserName}", userCreateDto.UserName);
                throw new Exception("An error occurred while saving the User record. " + ex.Message);
            }
        }
        public async Task<UserDto?> UpdateUserAsync(UserDto userDto)
        {
            _logger.LogInformation("Attempting to update a new User with UserName: {UserName}", userDto.UserName);
            try
            {
                var userInDb = await _unitOfWork.Users.GetByIdAsync(userDto.UserID);
                if (userInDb == null)
                    throw new ResourceNotFoundException($"Cannot update: User with ID {userDto.UserID} not found.");

                await ValidateUserDataAsync(userDto.UserID, userDto.UserName, isUpdate: true);
                _mapper.Map(userDto, userInDb);

                _unitOfWork.Users.Update(userInDb);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully updated User with UserID: {UserID}", userInDb.UserID);
                    return _mapper.Map<UserDto>(userInDb);
                }

                _logger.LogError("Failed to update User record to the database for UserName: {UserName}", userInDb.UserName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validation or Conflict error while updated UserID: {UserID}", userDto.UserID);

                if (ex is ResourceNotFoundException || ex is ConflictException) throw;

                _logger.LogCritical(ex, "Unexpected error updated User with UserID {UserID}", userDto.UserID);
                throw new Exception("An error occurred while updating the User record. " + ex.Message);
            }
        }
        public async Task<bool> IsPersonAlreadyUserAsync(int personId)
        {
            return await _unitOfWork.Users.IsExistAsync(u => u.PersonID == personId);
        }
        public async Task<bool> IsUserNameExistAsync(string userName)
        {
            return await _unitOfWork.Users.IsExistAsync(u => u.UserName == userName);
        }
        public async Task<bool> ToggleUserStatusAsync(int userId)
        {
            _logger.LogInformation("Attempting to Toggle User Status with UserID: {UserID}", userId);
            try
            {
                var userInDb = await _unitOfWork.Users.GetByIdAsync(userId);
                if (userInDb == null)
                    throw new ResourceNotFoundException($"Cannot Toggle User Status: User with ID {userId} not found.");

                userInDb.IsActive = !userInDb.IsActive;

                _unitOfWork.Users.Update(userInDb);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully Toggling User Status with UserID: {UserID}", userInDb.UserID);
                    return true;
                }

                _logger.LogError("Failed to Toggling User Status record to the database for UserName: {UserName}", userInDb.UserName);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validation error while Toggling User Status UserID: {UserID}", userId);

                if (ex is ResourceNotFoundException) throw;

                _logger.LogCritical(ex, "Unexpected error Toggling User Status with UserID {UserID}", userId);
                throw new Exception("An error occurred while Toggling User Status record. " + ex.Message);
            }
        }
        public async Task<bool> DeleteUserAsync(int userId)
        {
            _logger.LogInformation("Attempting to delete User with UserID: {UserID}", userId);

            if (userId <= 0)
                throw new ValidationException("Invalid UserID provided for deletion.");

            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                    throw new ResourceNotFoundException($"User with UserID {userId} not found.");

                _unitOfWork.Users.Delete(user);

                var result = await _unitOfWork.CompleteAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Successfully deleted User with UserID: {UserID}", userId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting User with UserID: {UserID}. User might be linked to other records.", userId);

                if (ex is ResourceNotFoundException || ex is ValidationException) throw;
                throw new ConflictException("Cannot delete this User because they are linked to other records (like Applications, or Licenses ...).");
            }
        }

        #region Private Helper Methods
        private async Task ValidateUserDataAsync(int userId, string userName, bool isUpdate = false)
        {
            if (await _unitOfWork.Users.IsExistAsync(u => u.UserName == userName && (!isUpdate || u.UserID != userId)))
                throw new ConflictException($"The User UserName '{userName}' is already registered for another User.");
        }
        #endregion
    }
}