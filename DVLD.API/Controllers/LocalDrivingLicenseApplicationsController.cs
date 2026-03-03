using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.DTOs.Applications.LocalDrivingLicenseApplication;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.Entities;
using DVLD.CORE.Interfaces;
using DVLD.Services;
using Microsoft.AspNetCore.Authorization;
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
    public class LocalDrivingLicenseApplicationsController : ControllerBase
    {
        private readonly ILocalDrivingLicenseApplicationService _localAppService;
        private readonly ILogger<LocalDrivingLicenseApplicationsController> _logger;

        public LocalDrivingLicenseApplicationsController(ILocalDrivingLicenseApplicationService localApp, ILogger<LocalDrivingLicenseApplicationsController> logger)
        {
            _localAppService = localApp;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<LocalDrivingLicenseApplicationDto>>> GetAllLocalDrivingLicenseApplications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null) =>
               Ok(await _localAppService.GetAllLocalDrivingLicenseApplicationsAsync(pageNumber, pageSize, filterColumn, filterValue));


        [HttpGet("{id:int}", Name = "GetLocalDrivingLicenseApplicationById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LocalDrivingLicenseApplicationDto>> GetLocalDrivingLicenseApplicationById(int id) =>
               Ok(await _localAppService.GetByIdAsync(id));


        [HttpGet("ApplicationId/{id:int}", Name = "GetLocalDrivingLicenseApplicationByApplicationId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LocalDrivingLicenseApplicationDto>> GetLocalDrivingLicenseApplicationByApplicationId(int id) =>
               Ok(await _localAppService.GetByApplicationIdAsync(id));


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(LocalDrivingLicenseApplicationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LocalDrivingLicenseApplicationDto>> AddLocalDrivingLicenseApplication([FromBody] LocalDrivingLicenseApplicationCreateDto localAppCreateDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized("User ID is missing or invalid in the token.");
            }

            _logger.LogInformation("Creating new Local App for PersonID: {PersonID}", localAppCreateDto.PersonID);

            var resultDto = await _localAppService.AddLocalDrivingLicenseApplicationAsync(localAppCreateDto, currentUserId);

            if (resultDto == null)
            {
                return BadRequest("Failed to create the application. Please try again.");
            }

            return CreatedAtRoute(
                "GetLocalDrivingLicenseApplicationById",
                new { id = resultDto.LocalDrivingLicenseApplicationID },
                resultDto
            );
        }


        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LocalDrivingLicenseApplicationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LocalDrivingLicenseApplicationDto>> UpdateLocalDrivingLicenseApplication(int id, [FromBody] LocalDrivingLicenseApplicationUpdateDto localAppUpdateDto)
        {
            if (id != localAppUpdateDto.LocalDrivingLicenseApplicationID)
            {
                _logger.LogWarning("Update failed: ID mismatch (Path: {PathId}, Body: {BodyId})", id, localAppUpdateDto.LocalDrivingLicenseApplicationID);
                return BadRequest("ID mismatch between URL and request body.");
            }

            _logger.LogInformation("Updating Local Driving License Application ID: {Id}", id);

            var updatedDto = await _localAppService.UpdateLocalDrivingLicenseApplicationAsync(localAppUpdateDto);

            if (updatedDto == null)
            {
                _logger.LogError("Update failed for Application ID: {Id} - no record was saved.", id);
                return BadRequest("Update failed. Please ensure the data is correct.");
            }

            return Ok(updatedDto);
        }


        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLocalDrivingLicenseApplication(int id)
        {
            _logger.LogInformation("Deletion requested for Local Driving License Application ID: {Id}", id);
            await _localAppService.DeleteLocalDrivingLicenseApplicationAsync(id);
            return Ok(new { Message = "Local Driving License Application deleted successfully." });
        }


        [HttpGet("active-id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetActiveApplicationId([FromQuery] int personId, [FromQuery] int appTypeId, [FromQuery] int licenseClassId)
        {
            var activeId = await _localAppService.GetActiveApplicationIdForLicenseClassAsync(personId, appTypeId, licenseClassId);
            return Ok(activeId);
        }
    }
}
