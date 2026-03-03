using DVLD.API.Extensions;
using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Users;
using DVLD.CORE.Interfaces;
using DVLD.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Mime;
using System.Security.Claims;

namespace DVLD.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("GeneralPolicy")]
    [Produces(MediaTypeNames.Application.Json)]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        #region Authentication
        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            _logger.LogInformation("Login attempt for user: {UserName}", loginRequest.UserName);

            var response = await _userService.AuthenticateAsync(loginRequest);

            if (!response.IsAuthenticated)
            {
                _logger.LogWarning("Authentication failed for user: {UserName}. Reason: {Message}", loginRequest.UserName, response.Message);
                return Unauthorized(new { detail = response.Message });
            }

            return Ok(response);
        }
        #endregion

        #region User Management
        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResultDto<UserDto>>> GetAllUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null)
        {
            _logger.LogInformation("Fetching all users.");
            var users = await _userService.GetAllUsersAsync(pageNumber, pageSize, filterColumn, filterValue);
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var (currentUserId, currentUserRole) = User.GetUserInfo();

            if (currentUserId != id && currentUserRole != UserRoles.Admin)
            {
                _logger.LogWarning("Unauthorized access attempt by User {Current} to fetch User {Target}", currentUserId, id);
                return Forbid();
            }

            _logger.LogInformation("Fetching UserID: {Id}", id);
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpGet("person/{id:int}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUserByPersonId(int id)
        {
            var (currentUserId, currentUserRole) = User.GetUserInfo();

            if (currentUserId != id && currentUserRole != UserRoles.Admin)
            {
                _logger.LogWarning("Unauthorized access attempt by User {Current} to fetch User with personId {Target}", currentUserId, id);
                return Forbid();
            }

            _logger.LogInformation("Fetching User With PersonID: {PersonID}", id);
            var user = await _userService.GetUserByPersonIdAsync(id);
            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserDto>> AddUser([FromBody] UserCreateDto userCreateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Attempting to add new user: {UserName}", userCreateDto.UserName);

            var createdUser = await _userService.AddUserAsync(userCreateDto);

            if (createdUser == null)
            {
                _logger.LogError("Failed to save user record for: {UserName}", userCreateDto.UserName);
                return BadRequest("An unexpected error occurred while creating the user.");
            }

            return CreatedAtAction(nameof(GetUserById),new { id = createdUser.UserID }, createdUser);
        }


        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UserDto userDto)
        {
            var (currentUserId, currentUserRole) = User.GetUserInfo();

            if (currentUserId != id && currentUserRole != UserRoles.Admin)
            {
                _logger.LogWarning("Unauthorized update attempt by User {Current} on User {Target}", currentUserId, id);
                return Forbid();
            }

            if (currentUserRole != UserRoles.Admin && !string.IsNullOrEmpty(userDto.Role))
            {
                _logger.LogWarning("Update restriction: Non-admin user {UserId} attempted to modify roles.", currentUserId);
                userDto.Role = null!;
            }

            if (id != userDto.UserID)
            {
                _logger.LogWarning("Update failed: ID mismatch (Path: {PathId}, Body: {BodyId})", id, userDto.UserID);
                return BadRequest("ID mismatch between URL and request body.");
            }

            _logger.LogInformation("Updating UserID: {Id}", id);

            var updatedUser = await _userService.UpdateUserAsync(userDto);

            if (updatedUser == null)
            {
                _logger.LogWarning("Update executed but no changes were applied for UserID: {Id}", id);
                return BadRequest("Could not update the user record. No changes detected or database error.");
            }

            return Ok(updatedUser);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation("Deletion requested for User ID: {Id}", id);

            await _userService.DeleteUserAsync(id);

            return Ok(new { Message = "User deleted successfully." });
        }

        [HttpPatch("{id:int}/toggle-status")]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            _logger.LogInformation("Toggling status for UserID: {Id}", id);
            var result = await _userService.ToggleUserStatusAsync(id);

            if (result) return Ok(new { message = "Status updated successfully" });
            return BadRequest("Operation failed.");
        }


        [HttpPatch("{id:int}/change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (currentUserId, currentUserRole) = User.GetUserInfo();

            if (currentUserId != id && currentUserRole != UserRoles.Admin)
            {
                _logger.LogWarning("Unauthorized access attempt by User {Current} to Change Password User {Target}", currentUserId, id);
                return Forbid();
            }

            _logger.LogInformation("Changing password for UserID: {Id}", id);
            var result = await _userService.ChangePasswordAsync(id, changePasswordDto.NewPassword);

            if (result) return Ok(new { message = "Password updated successfully" });
            return BadRequest("Password change failed.");
        }
        #endregion
    }
}
