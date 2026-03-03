using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.TestTypes;
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
    public class TestTypesController(ITestTypeService testTypeService, ILogger<TestTypesController> logger) : ControllerBase
    {
        private readonly ITestTypeService _testTypeService = testTypeService;
        private readonly ILogger<TestTypesController> _logger = logger;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<TestTypeDto>>> GetAllTestTypes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null) =>
            Ok(await _testTypeService.GetAllTestTypesAsync(pageNumber, pageSize, filterColumn, filterValue));


        [HttpGet("{id:int}", Name = "GetTestTypeById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TestTypeDto>> GetTestTypeById(int id) =>
            Ok(await _testTypeService.GetTestTypeByIdAsync(id));


        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TestTypeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<TestTypeDto>> AddTestType([FromBody] TestTypeDto testTypeDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Creating new TestType with Title: {Title}", testTypeDto.Title);

            var createdTestType = await _testTypeService.AddTestTypeAsync(testTypeDto);

            if (createdTestType == null)
            {
                _logger.LogError("Failed to create TestType for Title: {Title}", testTypeDto.Title);
                return BadRequest("An error occurred while saving the Test Type record.");
            }

            return CreatedAtRoute("GetTestTypeById", new { id = createdTestType.TestTypeID }, createdTestType);
        }


        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TestTypeDto))] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TestTypeDto>> UpdateTestType(int id, [FromBody] TestTypeDto testTypeDto)
        {
            if (id != testTypeDto.TestTypeID)
            {
                _logger.LogWarning("Update failed: ID mismatch. URL: {UrlId}, Body: {BodyId}", id, testTypeDto.TestTypeID);
                return BadRequest("ID mismatch between URL and body.");
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Updating TestTypeID: {Id}", id);

            var updatedTestType = await _testTypeService.UpdateTestTypeAsync(testTypeDto);

            if (updatedTestType == null)
            {
                _logger.LogWarning("Update executed but no changes applied for TestTypeID: {Id}", id);
                return BadRequest("Could not update the test type. No changes were applied.");
            }

            return Ok(updatedTestType);
        }
    }
}
