using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.Interfaces;
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
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly ILogger<ApplicationsController> _logger;

        public ApplicationsController(IApplicationService applicationService, ILogger<ApplicationsController> logger)
        {
            _applicationService = applicationService;
            _logger = logger;
        }


        [HttpGet("active-id")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> GetActiveApplicationIdAsync([FromQuery] int personID, [FromQuery] int applicationTypeID)
        {
            _logger.LogInformation("Fetching Active Application for PersonID: {personID} and Type: {applicationTypeID}", personID, applicationTypeID);

            var applicationId = await _applicationService.GetActiveApplicationIdAsync(personID, applicationTypeID);
            return Ok(applicationId);
        }


        [HttpGet("active-id-for-license-class")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> GetActiveApplicationIdForLicenseClassAsync([FromQuery] int personID, [FromQuery] int applicationTypeID, [FromQuery] int licenseClassID)
        {
            _logger.LogInformation("Fetching Active Application for PersonID: {personID}, Type: {type}, Class: {class}",
                personID, applicationTypeID, licenseClassID);

            var applicationId = await _applicationService.GetActiveApplicationIdForLicenseClassAsync(personID, applicationTypeID, licenseClassID);

            if (applicationId <= 0)
            {
                _logger.LogWarning("No active application found for PersonID: {personID} in License Class: {class}", personID, licenseClassID);
                return NotFound(0);
            }

            return Ok(applicationId);
        }


        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationDto>> GetApplicationById(int id)
        {
            _logger.LogInformation("Fetching ApplicationID: {Id}", id);
            var application = await _applicationService.GetByIdAsync(id);
            return Ok(application);
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApplicationDto>> AddApplication([FromBody] ApplicationCreateDto applicationCreateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int currentUserId))
            {
                return Unauthorized("User ID not found in token.");
            }

            _logger.LogInformation("Adding new Application for PersonID: {ApplicantPersonID}", applicationCreateDto.ApplicantPersonID);

            var result = await _applicationService.AddApplicationAsync(applicationCreateDto, currentUserId);

            if (result == null)
                return BadRequest("Could not add Application.");
            return CreatedAtAction(nameof(GetApplicationById), new { id = result.ApplicationID }, result);
        }


        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationDto>> UpdateStatus(int id, [FromBody] ApplicationUpdateDto applicationUpdateDto)
        {
            if (id != applicationUpdateDto.ApplicationID)
            {
                _logger.LogWarning("Update failed: ID mismatch (Path: {PathId}, Body: {BodyId})", id, applicationUpdateDto.ApplicationID);
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating ApplicationID: {Id}", id);

            var result = await _applicationService.UpdateStatusAsync(applicationUpdateDto);

            if (result == null)
                return BadRequest("Update failed. No changes were saved.");

            return Ok(result);
        }


        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            _logger.LogInformation("Deletion requested for Application ID: {Id}", id);
            await _applicationService.DeleteApplicationAsync(id);
            return Ok(new { Message = "Application deleted successfully." });
        }


        [HttpGet("check-active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> DoesPersonHaveActiveApplication([FromQuery] int personID, [FromQuery] int applicationTypeID)
        {
            _logger.LogInformation("Check Dese Person have Active Application for PersonID: {personID} and Type: {applicationTypeID}", personID, applicationTypeID);

            var exists = await _applicationService.DoesPersonHaveActiveApplicationAsync(personID, applicationTypeID);
            return Ok(exists);
        }


    }
}
