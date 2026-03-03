using DVLD.API.Extensions;
using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.People;
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
    public class PeopleController(IPersonService personService, ILogger<PeopleController> logger) : ControllerBase
    {
        private readonly IPersonService _personService = personService;
        private readonly ILogger<PeopleController> _logger = logger;


        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<PersonDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<PersonDto>>> GetAllPeople(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterValue = null)
        {
            var result = await _personService.GetAllPeoplePagedAsync(pageNumber, pageSize, filterColumn, filterValue);
            return Ok(result);
        }

        [HttpGet("{id:int}", Name = "GetPersonById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PersonDto>> GetPersonById(int id) => Ok(await _personService.GetPersonByIdAsync(id));

        [HttpGet("{nationalNo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PersonDto>> GetPersonByNationalNo(string nationalNo) => Ok(await _personService.GetPersonByNationalNoAsync(nationalNo));

        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.User}")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PersonDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PersonDto>> AddPerson([FromForm] PersonDto personDto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Creating new person with NationalNo: {NationalNo}", personDto.NationalNo);

            using var imageStream = imageFile?.OpenReadStream();

            var newPerson = await _personService.AddPersonAsync(personDto, imageStream, imageFile?.FileName);

            if (newPerson != null)
            {
                _logger.LogInformation("Person created successfully with ID: {PersonID}", newPerson.PersonID);
                return CreatedAtRoute("GetPersonById", new { id = newPerson.PersonID }, newPerson);
            }

            _logger.LogWarning("Person creation failed for NationalNo: {NationalNo}", personDto.NationalNo);
            return BadRequest("Person could not be saved. Please check the data and try again.");
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.User}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PersonDto>> UpdatePerson(int id, [FromForm] PersonDto personDto, IFormFile? imageFile)
        {
            var (currentUserId, currentUserRole) = User.GetUserInfo();
            if (id != personDto.PersonID)
            {
                _logger.LogWarning("Update failed: ID mismatch. URL: {UrlId}, Body: {BodyId}", id, personDto.PersonID);
                return BadRequest("ID mismatch between URL and body.");
            }

            _logger.LogInformation("User {UserId} (Role: {Role}) is attempting to update Person {PersonId}",
                            currentUserId, currentUserRole, id);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            using var imageStream = imageFile?.OpenReadStream();

            var result =  await _personService.UpdatePersonAsync(personDto, imageStream, imageFile?.FileName, currentUserId, currentUserRole!);

            if (result != null)
            {
                _logger.LogInformation("Person ID: {PersonID} updated successfully by User ID: {UserID}", id, currentUserId);
                return Ok(result);
            }
            _logger.LogWarning("Update failed for Person ID: {PersonID} by User ID: {UserID}", id, currentUserId);
            return BadRequest("Person could not be updated. Please check the data and try again.");
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<bool>> DeletePerson(int id)
        {
            _logger.LogInformation("Deletion requested for Person ID: {Id}", id);
            await _personService.DeletePersonAsync(id);
            return Ok(new { Message = "Person deleted successfully." });
        }


        [HttpGet("DoesPersonExist/{id:int}")]
        public async Task<ActionResult<bool>> DoesPersonExist(int id)
        {
            _logger.LogInformation("Checking existence for Person ID: {Id}", id);
            return Ok(await _personService.IsPersonExistByIdAsync(id));
        }
    }
}