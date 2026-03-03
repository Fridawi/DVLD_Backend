using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.Interfaces.Licenses;
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
    public class LicenseClassesController(ILicenseClassService licenseClassService, ILogger<LicenseClassesController> logger) : ControllerBase
    {
        private readonly ILicenseClassService _licenseClassService = licenseClassService;
        private readonly ILogger<LicenseClassesController> _logger = logger;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<LicenseClassDto>>> GetAllLicenseClasses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null)  =>
            Ok(await _licenseClassService.GetAllLicenseClassesAsync(pageNumber, pageSize, filterColumn, filterValue));


        [HttpGet("{id:int}", Name = "GetLicenseClassById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LicenseClassDto>> GetLicenseClassById(int id) =>
            Ok(await _licenseClassService.GetLicenseClassByIdAsync(id));

        [HttpGet("ByName/{className}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LicenseClassDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLicenseClassByClassName(string className) =>
            Ok(await _licenseClassService.GetLicenseClassByClassNameAsync(className));

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LicenseClassDto>> AddLicenseClass([FromBody] LicenseClassDto licenseClassDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Creating new License Class with ClassName: {ClassName}", licenseClassDto.ClassName);

            var result = await _licenseClassService.AddLicenseClassAsync(licenseClassDto);

            if (result == null)
                return BadRequest("Could not add License Class.");

            return CreatedAtRoute("GetLicenseClassById", new { id = result.LicenseClassID }, result);
        }


        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LicenseClassDto>> UpdateLicenseClass(int id, [FromBody] LicenseClassDto licenseClassDto)
        {
            if (id != licenseClassDto.LicenseClassID)
            {
                _logger.LogWarning("Update failed: ID mismatch. URL: {UrlId}, Body: {BodyId}", id, licenseClassDto.LicenseClassID);
                return BadRequest("ID mismatch between URL and body.");
            }

            _logger.LogInformation("Updating License Class ID: {Id}", id);

            var result = await _licenseClassService.UpdateLicenseClassAsync(licenseClassDto);

            if (result == null)
                return BadRequest("Update failed. No changes were saved.");

            return Ok(result);
        }

    }
}
