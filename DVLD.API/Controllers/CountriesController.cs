using DVLD.CORE.DTOs.Countries;
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
    public class CountriesController : ControllerBase
    {
        private readonly ICountryService _countryService;

        public CountriesController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CountryDto>))]
        public async Task<IActionResult> GetAll() => Ok(await _countryService.GetAllCountryAsync());


        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CountryDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id) => Ok(await _countryService.GetCountryByIdAsync(id));


        [HttpGet("ByName/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CountryDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByName(string name) => Ok(await _countryService.GetCountryByNameAsync(name));

    }
}