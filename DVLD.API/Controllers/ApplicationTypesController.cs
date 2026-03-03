using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.People;
using DVLD.CORE.Interfaces;
using DVLD.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class ApplicationTypesController(IApplicationTypeService applicationTypeService, ILogger<ApplicationTypesController> logger) : ControllerBase
    {
        private readonly IApplicationTypeService _applicationTypeService = applicationTypeService;
        private readonly ILogger<ApplicationTypesController> _logger = logger;

        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<ApplicationTypeDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<ApplicationTypeDto>>> GetAllApplicationTypes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null) =>
            Ok(await _applicationTypeService.GetAllApplicationTypesAsync(pageNumber, pageSize, filterColumn, filterValue));


        [HttpGet("{id:int}", Name = "GetApplicationTypeById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationTypeDto>> GetApplicationTypeById(int id) =>
            Ok(await _applicationTypeService.GetApplicationTypeByIdAsync(id));


        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApplicationTypeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApplicationTypeDto>> AddApplicationType([FromBody] ApplicationTypeDto applicationTypeDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Creating new ApplicationType with Title: {Title}", applicationTypeDto.Title);

            var newAppType = await _applicationTypeService.AddApplicationTypeAsync(applicationTypeDto);

            if (newAppType == null)
            {
                _logger.LogWarning("ApplicationType creation failed for Title: {Title}", applicationTypeDto.Title);
                return BadRequest("Could not create the application type. Please verify the data.");
            }

            return CreatedAtRoute("GetApplicationTypeById", new { id = applicationTypeDto.ApplicationTypeID }, newAppType);
        }


        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationTypeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationTypeDto>> UpdateApplicationType(int id, [FromBody] ApplicationTypeDto applicationTypeDto)
        {
            if (id != applicationTypeDto.ApplicationTypeID)
            {
                _logger.LogWarning("Update failed: ID mismatch. URL: {UrlId}, Body: {BodyId}", id, applicationTypeDto.ApplicationTypeID);
                return BadRequest("ID mismatch between URL and body.");
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updatedAppType = await _applicationTypeService.UpdateApplicationTypeAsync(applicationTypeDto);

            if (updatedAppType == null)
            {
                _logger.LogWarning("Update failed for ApplicationTypeID: {Id}", id);
                return BadRequest("Could not update the application type. No changes were saved.");
            }

            return Ok(updatedAppType);
        }
    }
}
