using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses.InternationalLicenses;
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
    public class InternationalLicensesController : ControllerBase
    {
        private readonly IInternationalLicenseService _intLicenseService;
        private readonly ILogger<InternationalLicensesController> _logger;

        public InternationalLicensesController(IInternationalLicenseService intLicenseService, ILogger<InternationalLicensesController> logger)
        {
            _intLicenseService = intLicenseService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<InternationalLicenseDto>>> GetAllInternationalLicenses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null)
        {
            return Ok(await _intLicenseService.GetAllInternationalLicensesAsync(pageNumber, pageSize, filterColumn, filterValue));
        }

        [HttpGet("Driver/{driverId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<InternationalLicenseDto>>> GetInternationalLicensesByDriverId(int driverId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null)
        {
            return Ok(await _intLicenseService.GetInternationalLicensesByDriverIdAsync(driverId, pageNumber, pageSize, filterColumn, filterValue));
        }


        [HttpGet("{id:int}", Name = "GetInternationalLicenseById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InternationalLicenseDto>> GetInternationalLicenseById(int id)
        {
            return Ok(await _intLicenseService.GetInternationalLicenseByIdAsync(id));
        }


        [HttpGet("Details/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DriverInternationalLicenseDto>> GetDetailedInternationalLicenseById(int id)
        {
            return Ok(await _intLicenseService.GetDriverInternationalLicenseByIdAsync(id));
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> IssueInternationalLicense([FromBody] InternationalLicenseCreateDto createDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized("User ID is missing or invalid in the token.");
            }

            _logger.LogInformation("Issuing International License for Local License ID: {LocalID}", createDto.LocalLicenseID);

            var result = await _intLicenseService.IssueInternationalLicenseAsync(createDto, currentUserId);

            return CreatedAtRoute("GetInternationalLicenseById", new { id = result!.InternationalLicenseID },
                new { message = "International License issued successfully", result });
        }


        [HttpGet("active-id/{driverId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int?>> GetActiveInternationalLicenseId(int driverId)
        {
            var activeId = await _intLicenseService.GetActiveInternationalLicenseIdByDriverIdAsync(driverId);
            return Ok(activeId);
        }


        [HttpGet("eligibility/{localLicenseId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CheckEligibility(int localLicenseId)
        {
            await _intLicenseService.IsDriverEligibleForInternationalLicenseAsync(localLicenseId);
            return Ok(new { eligible = true, message = "Driver is eligible for an international license." });
        }


        [HttpPatch("deactivate/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateInternationalLicense(int id)
        {
            _logger.LogInformation("Deactivating International License ID: {Id}", id);
            var result = await _intLicenseService.DeactivateInternationalLicenseAsync(id);

            if (result) return Ok(new { message = "License deactivated successfully." });
            return BadRequest("Could not deactivate license.");
        }
    }
}
