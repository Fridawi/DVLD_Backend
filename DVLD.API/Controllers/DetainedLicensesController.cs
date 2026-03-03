using DVLD.API.Extensions;
using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses.DetainedLicenses;
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
    public class DetainedLicensesController : ControllerBase
    {
        private readonly IDetainedLicenseService _detainedLicenseService;
        private readonly ILogger<DetainedLicensesController> _logger;

        public DetainedLicensesController(IDetainedLicenseService detainedLicenseService, ILogger<DetainedLicensesController> logger)
        {
            _detainedLicenseService = detainedLicenseService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<DetainedLicenseDto>>> GetAllDetainedLicenses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null) =>
            Ok(await _detainedLicenseService.GetAllDetainedLicensesAsync(pageNumber, pageSize, filterColumn, filterValue));


        [HttpGet("{id:int}", Name = "GetDetainedLicenseById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DetainedLicenseDto>> GetDetainedLicenseById(int id) =>
            Ok(await _detainedLicenseService.GetDetainedLicenseByIdAsync(id));


        [HttpGet("license/{licenseId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DetainedLicenseDto>> GetDetainedLicenseByLicenseId(int licenseId)
        {
            var result = await _detainedLicenseService.GetDetainedLicenseByLicenseIdAsync(licenseId);
            return result == null ? NotFound($"No active detention found for License ID {licenseId}") : Ok(result);
        }


        [HttpPost("detain")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(DetainedLicenseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DetainedLicenseDto>> DetainLicense([FromBody] DetainLicenseCreateDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int currentUserId = User.GetUserId();
            _logger.LogInformation("User {UserId} is detaining License {LicenseID}", currentUserId, createDto.LicenseID);

            var result = await _detainedLicenseService.DetainLicenseAsync(createDto, currentUserId);

            if (result == null) return BadRequest("Failed to detain the license.");

            return CreatedAtRoute("GetDetainedLicenseById", new { id = result.DetainID }, result);
        }


        [HttpPost("release/{licenseId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DetainedLicenseDto>> ReleaseLicense(int licenseId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int currentUserId = User.GetUserId();

            _logger.LogInformation("User {UserId} is releasing License {LicenseID}",currentUserId, licenseId);

            var released = await _detainedLicenseService.ReleaseLicenseAsync(licenseId, currentUserId);

            if (released == null)
            {
                return BadRequest("Could not release the license. Please ensure the data is correct.");
            }

            return Ok(released);
        }


        [HttpGet("is-detained/{licenseId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CheckIfLicenseIsDetained(int licenseId)
        {
            return Ok(await _detainedLicenseService.IsLicenseDetainedAsync(licenseId));
        }
    }
}
