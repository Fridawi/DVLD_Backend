using DVLD.API.Extensions;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.Enums;
using DVLD.CORE.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Mime;

namespace DVLD.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("GeneralPolicy")]
    [Produces(MediaTypeNames.Application.Json)]
    public class LicensesController(ILicenseService licenseService, ILogger<LicensesController> logger) : ControllerBase
    {
        private readonly ILicenseService _licenseService = licenseService;
        private readonly ILogger<LicensesController> _logger = logger;


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<LicenseDto>>> GetAllLicenses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null) =>
                     Ok(await _licenseService.GetAllLicensesAsync(pageNumber, pageSize, filterColumn, filterValue));


        [HttpGet("driver/{driverId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<DriverLicenseDto>>> GetLicensesByDriverId(int driverId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null) =>
             Ok(await _licenseService.GetLicensesByDriverIdAsync(driverId, pageNumber, pageSize, filterColumn, filterValue));


        [HttpGet("{id:int}/detail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DriverLicenseDto>> GetDriverLicenseDetail(int id)
        {
            var license = await _licenseService.GetDriverLicensesByIdAsync(id);
            return license == null ? NotFound($"No detailed license found for ID {id}") : Ok(license);
        }


        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LicenseDto>> GetLicenseById(int id)
        {
            var license = await _licenseService.GetLicenseByIdAsync(id);
            return license == null ? NotFound($"License with ID {id} not found") : Ok(license);
        }


        [HttpGet("active-check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LicenseDto>> GetActiveLicense([FromQuery] int personID, [FromQuery] int licenseClassID)
        {
            _logger.LogInformation("Checking active license for PersonID: {PersonID}, LicenseClassID: {LicenseClassID}", personID, licenseClassID);
            var license = await _licenseService.GetActiveLicenseByPersonIDAndLicenseClassID(personID, licenseClassID);
            return license == null ? NotFound("No active license found for this class.") : Ok(license);
        }


        [HttpPost("first-time")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LicenseDto>> IssueFirstTimeLicense([FromBody] LicenseCreateDto createDto)
        {
            _logger.LogInformation("Issuing first-time license for ApplicationID: {AppID}", createDto.LocalDrivingLicenseApplicationID);

            int currentUserId = User.GetUserId();
            if (currentUserId == 0) return Unauthorized("Invalid user identification in token.");

            var result = await _licenseService.IssueFirstTimeLicenseAsync(createDto, currentUserId);

            if (result == null)
            {
                _logger.LogWarning("Failed to issue first-time license for ApplicationID: {AppID}", createDto.LocalDrivingLicenseApplicationID);
                return BadRequest("Failed to issue license. Ensure all tests are passed.");
            }

            return CreatedAtAction(nameof(GetLicenseById), new { id = result.LicenseID }, result);
        }


        [HttpPost("{oldLicenseId}/renew")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LicenseDto>> RenewLicense(int oldLicenseId, [FromBody] string? notes)
        {
            _logger.LogInformation("Request to renew License ID: {ID}", oldLicenseId);

            int currentUserId = User.GetUserId();
            if (currentUserId == 0) return Unauthorized();

            var result = await _licenseService.RenewLicenseAsync(oldLicenseId, notes, currentUserId);

            if (result == null) return BadRequest("Renewal failed.");

            return CreatedAtAction(nameof(GetLicenseById), new { id = result.LicenseID }, result);
        }


        [HttpPost("{oldLicenseId}/replace")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LicenseDto>> ReplaceLicense(int oldLicenseId, [FromQuery] EnIssueReason reason)
        {
            if (reason != EnIssueReason.DamagedReplacement && reason != EnIssueReason.LostReplacement)
            {
                return BadRequest("Invalid replacement reason.");
            }

            _logger.LogInformation("Replacing License ID: {ID} for reason: {Reason}", oldLicenseId, reason);

            int currentUserId = User.GetUserId();
            if (currentUserId == 0) return Unauthorized();

            var result = await _licenseService.ReplaceLicenseAsync(oldLicenseId, reason, currentUserId);

            if (result == null) return BadRequest("Replacement failed.");

            return CreatedAtAction(nameof(GetLicenseById), new { id = result.LicenseID }, result);
        }


        [HttpPatch("{id:int}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Deactivate(int id)
        {
            _logger.LogInformation("Deactivating license with ID: {LicenseID}", id);
            var success = await _licenseService.DeactivateLicense(id);
            return success ? Ok(new { message = "License deactivated successfully." })
                           : BadRequest("Failed to deactivate license.");
        }

    }
}
