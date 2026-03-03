using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.TestAppointments;
using DVLD.CORE.Interfaces.Tests;
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
    public class TestAppointmentsController(ITestAppointmentService testAppointmentService, ILogger<TestAppointmentsController> logger) : ControllerBase
    {
        private readonly ITestAppointmentService _testAppointmentService = testAppointmentService;
        private readonly ILogger<TestAppointmentsController> _logger = logger;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<TestAppointmentDto>>> GetAllTestAppointments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null) =>
            Ok(await _testAppointmentService.GetAllTestAppointmentsAsync(pageNumber, pageSize, filterColumn, filterValue));


        [HttpGet("{id:int}", Name = "GetTestAppointmentById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TestAppointmentDto>> GetTestAppointmentById(int id) =>
            Ok(await _testAppointmentService.GetTestAppointmentByIdAsync(id));


        [HttpGet("localApp/{localAppID:int}/test-type/{testTypeID:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<TestAppointmentDto>>> GetAppointmentsByLocalAppAndTestType(
            int localAppID,
            int testTypeID,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null) =>
            Ok(await _testAppointmentService.GetApplicationTestAppointmentsPerTestTypeAsync(localAppID, testTypeID, pageNumber, pageSize, filterColumn, filterValue));


        [HttpGet("last-appointment/localApp/{localAppID:int}/test-type/{testTypeID:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TestAppointmentDto>> GetLastTestAppointment(int localAppID, int testTypeID)
        {
            var appointment = await _testAppointmentService.GetLastTestAppointmentAsync(localAppID, testTypeID);
            return appointment == null ? NotFound("No appointments found for this application and test type.") : Ok(appointment);
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TestAppointmentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TestAppointmentDto>> AddTestAppointment([FromBody] TestAppointmentCreateDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int currentUserId))
            {
                return Unauthorized("User ID not found in token.");
            }

            _logger.LogInformation("Creating new Test Appointment for LocalAppID: {LocalAppID}", createDto.LocalDrivingLicenseApplicationID);

            var createdAppointment = await _testAppointmentService.AddTestAppointmentAsync(createDto, currentUserId);

            if (createdAppointment == null)
            {
                return BadRequest("An unexpected error occurred while saving the appointment.");
            }

            return CreatedAtAction(
                nameof(GetTestAppointmentById),
                new { id = createdAppointment.TestAppointmentID },
                createdAppointment);
        }


        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TestAppointmentDto))] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TestAppointmentDto>> UpdateTestAppointment(int id, [FromBody] TestAppointmentUpdateDto updateDto)
        {
            if (id != updateDto.TestAppointmentID)
            {
                _logger.LogWarning("Update failed: ID mismatch. URL: {UrlId}, Body: {BodyId}", id, updateDto.TestAppointmentID);
                return BadRequest("ID mismatch between URL and body.");
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Updating Test Appointment ID: {Id}", id);

            var updatedAppointment = await _testAppointmentService.UpdateTestAppointmentAsync(updateDto);

            if (updatedAppointment == null)
            {
                _logger.LogWarning("Update attempt resulted in no changes for ID: {Id}", id);
                return BadRequest("Could not update the appointment. No changes were applied or a database error occurred.");
            }

            return Ok(updatedAppointment);
        }


        [HttpGet("has-active/localApp/{localAppID:int}/test-type/{testTypeID:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> HasActiveAppointment(int localAppID, int testTypeID) =>
            Ok(await _testAppointmentService.HasActiveAppointmentAsync(localAppID, testTypeID));
    }

}
