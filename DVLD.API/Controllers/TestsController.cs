using DVLD.API.Extensions;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Tests;
using DVLD.CORE.Interfaces.Tests;
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
    public class TestsController : ControllerBase
    {
        private readonly ITestService _testService;
        private readonly ILogger<TestsController> _logger;

        public TestsController(ITestService testService, ILogger<TestsController> logger)
        {
            _testService = testService;
            _logger = logger;
        }


        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TestDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<TestDto>>> GetAllTests(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null) => Ok(await _testService.GetAllTestsAsync(pageNumber, pageSize, filterColumn, filterValue));


        [HttpGet("{id:int}", Name = "GetTestById")]
        [ProducesResponseType(typeof(TestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TestDto>> GetTestById(int id) => Ok(await _testService.GetTestByIdAsync(id));


        [HttpGet("last-test")]
        [ProducesResponseType(typeof(TestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TestDto>> GetLastTestPerPerson([FromQuery] int personId, [FromQuery] int licenseClassId, [FromQuery] int testTypeID)
        {
            _logger.LogInformation("Fetching last test for PersonID: {PersonId}, Class: {ClassId}, Type: {TypeId}", personId, licenseClassId, testTypeID);

            var test = await _testService.GetLastTestPerPersonAndLicenseClassAsync(personId, licenseClassId, testTypeID);

            if (test == null) return NotFound("No previous test found for the specified criteria.");

            return Ok(test);
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TestDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] 
        public async Task<ActionResult<TestDto>> AddTest([FromBody] TestCreateDto testCreateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (currentUserId, _) = User.GetUserInfo();

            if (currentUserId <= 0)
            {
                _logger.LogWarning("Unauthorized attempt to add test: User ID missing.");
                return Unauthorized("User ID not found or invalid in token.");
            }

            _logger.LogInformation("Adding new Test for AppointmentID: {AppID}", testCreateDto.TestAppointmentID);

            var createdTest = await _testService.AddTestAsync(testCreateDto, currentUserId);

            if (createdTest == null)
            {
                _logger.LogError("Failed to save Test for AppointmentID: {AppID}", testCreateDto.TestAppointmentID);
                return BadRequest("An unexpected error occurred while recording the test result.");
            }

            return CreatedAtAction(nameof(GetTestById), new { id = createdTest.TestID }, createdTest);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TestDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TestDto>> UpdateTest(int id, [FromBody] TestUpdateDto testUpdateDto)
        {
            if (id != testUpdateDto.TestID)
            {
                _logger.LogWarning("Update failed: ID mismatch (Path: {PathId}, Body: {BodyId})", id, testUpdateDto.TestID);
                return BadRequest("ID mismatch between URL and request body.");
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Updating TestID: {Id}", id);

            var updatedTest = await _testService.UpdateTestAsync(testUpdateDto);

            if (updatedTest == null)
            {
                _logger.LogWarning("Update executed but no changes were applied for TestID: {Id}", id);
                return BadRequest("Could not update the test record. It might have been modified by another process or no changes were provided.");
            }

            return Ok(updatedTest);
        }


        [HttpGet("check-passed-all/{localAppID:int}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> PassedAllTests(int localAppID)
        {
            _logger.LogInformation("Checking if LocalAppID: {localAppID} passed all tests", localAppID);
            var result = await _testService.PassedAllTestsAsync(localAppID);
            return Ok(result);
        }


        [HttpGet("passed-count/{localAppID:int}")]
        [ProducesResponseType(typeof(byte), StatusCodes.Status200OK)]
        public async Task<ActionResult<byte>> GetPassedTestCount(int localAppID)
        {
            _logger.LogInformation("Getting passed tests count for LocalAppID: {localAppID}", localAppID);
            var count = await _testService.GetPassedTestCountAsync(localAppID);
            return Ok(count);
        }

    }
}
