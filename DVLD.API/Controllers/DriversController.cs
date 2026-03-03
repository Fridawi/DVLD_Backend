using DVLD.API.Extensions;
using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Drivers;
using DVLD.CORE.DTOs.TestTypes;
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
    public class DriversController : ControllerBase
    {
        private readonly IDriverService _driverService;
        private readonly ILogger<DriversController> _logger ;
        public DriversController(IDriverService driverService, ILogger<DriversController> logger)
        {
            _driverService = driverService;
            _logger = logger;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<DriverDto>>> GetAllDrivers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null) =>
                    Ok(await _driverService.GetAllDriversAsync(pageNumber, pageSize, filterColumn, filterValue));


        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DriverDto>> GetDriverTypeById(int id) =>
            Ok(await _driverService.GetDriverByIdAsync(id));


        [HttpGet("person/{personId:int}", Name = "GetDriverByPersonIdAsync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DriverDto>> GetDriverByPersonIdAsync(int personId) =>
            Ok(await _driverService.GetDriverByPersonIdAsync(personId));


        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(DriverDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<DriverDto>> AddDriverAsync([FromBody] DriverCreateDto driverDto)
        {
            var (currentUserId, currentUserRole) = User.GetUserInfo();

            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Creating new Driver with PersonID: {PersonID}", driverDto.PersonID);

            var newDriver = await _driverService.AddDriverAsync(driverDto.PersonID, currentUserId, currentUserRole!);

            if(newDriver == null)
            {
                _logger.LogWarning("Failed to create driver for PersonID: {PersonID}", driverDto.PersonID);
                return BadRequest("Driver could not be created.");
            }

            return CreatedAtRoute("GetDriverByPersonIdAsync", new { personId = driverDto.PersonID }, newDriver);
        }

        [HttpGet("exists/person/{personId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CheckIfDriverExists(int personId)
        {
            var exists = await _driverService.IsDriverExistByPersonIdAsync(personId);
            return Ok(exists);
        }
    }
}
