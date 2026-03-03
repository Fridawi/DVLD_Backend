using AutoMapper;
using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.People;
using DVLD.CORE.Entities;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;

namespace DVLD.Services
{
    public class PersonService : IPersonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PersonService> _logger;
        public PersonService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService, IFileService fileService, IHttpContextAccessor httpContextAccessor, ILogger<PersonService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _fileService = fileService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<PagedResultDto<PersonDto>> GetAllPeoplePagedAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching people with pagination. PageNumber: {PageNumber}, PageSize: {PageSize}, FilterColumn: {FilterColumn}, FilterValue: {FilterValue}",
                pageNumber, pageSize, filterColumn, filterValue);

            Expression<Func<Person, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                filter = filterColumn.ToLower() switch
                {
                    "personid" when int.TryParse(filterValue, out int id) => p => p.PersonID == id,

                    "nationalno" => p => p.NationalNo.Contains(filterValue),

                    "name" => p => (p.FirstName + " " +
                                    p.SecondName + " " +
                                    (p.ThirdName != null ? p.ThirdName + " " : "") +
                                    p.LastName)
                                    .Replace("  ", " ") 
                                    .Contains(filterValue.Trim()),

                    "phone" => p => p.Phone.Contains(filterValue),

                    "nationality" => p => p.CountryInfo.CountryName.Contains(filterValue),
                    _ => p => true
                };
            }

            int skip = (pageNumber - 1) * pageSize;

            var people = await _unitOfWork.People.FindAllAsync(
                predicate: filter, 
                includes: ["CountryInfo"],
                tracked: false,
                orderBy: p => p.FirstName,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.People.CountAsync(filter);

            var mappedPeople = _mapper.Map<IEnumerable<PersonDto>>(people, opt => opt.Items["BaseUrl"] = GetBaseUrl());

            return new PagedResultDto<PersonDto>
            {
                Data = mappedPeople,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PersonDto> GetPersonByIdAsync(int id)
        {
            _logger.LogInformation("Fetching person with PersonID: {PersonID}", id);
            if (id <= 0) throw new ValidationException("Invalid ID");

            var person = await _unitOfWork.People.FindAsync(p => p.PersonID == id, ["CountryInfo"]);

            if (person == null)
            {
                _logger.LogWarning("Person with PersonID {PersonID} was not found.", id);
                throw new ResourceNotFoundException($"Person with PersonID {id} was not found.");
            }
            return _mapper.Map<PersonDto>(person, opt => opt.Items["BaseUrl"] = GetBaseUrl());
        }

        public async Task<PersonDto> GetPersonByNationalNoAsync(string nationalNo)
        {
            _logger.LogInformation("Fetching person with National No: {nationalNo}", nationalNo);
            if (string.IsNullOrWhiteSpace(nationalNo))
                throw new ValidationException("National number cannot be null or empty.");

            var person = await _unitOfWork.People.FindAsync(p => p.NationalNo == nationalNo.Trim(), ["CountryInfo"]);

            if (person == null)
            {
                _logger.LogWarning("Person with National No {nationalNo} was not found.", nationalNo);
                throw new ResourceNotFoundException($"Person with National Number '{nationalNo}' was not found.");
            }
            return _mapper.Map<PersonDto>(person, opt => opt.Items["BaseUrl"] = GetBaseUrl());
        }

        public async Task<PersonDto?> AddPersonAsync(PersonDto personDto, Stream? imageStream, string? originalFileName)
        {
            _logger.LogInformation("Attempting to add a new person with NationalNo: {NationalNo}", personDto.NationalNo);
            string? uploadedFileName = null;
            try
            {
                await ValidatePersonDataAsync(personDto);

                var person = _mapper.Map<Person>(personDto);

                person.ImagePath = await HandleImageUploadAsync(imageStream, originalFileName);
                uploadedFileName = person.ImagePath;

                await _unitOfWork.People.AddAsync(person);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully added person with PersonID: {PersonID} and NationalNo: {NationalNo}",
                        person.PersonID, person.NationalNo);

                    return await GetPersonByIdAsync(person.PersonID); 
                }

                _logger.LogError("Failed to save person record to the database for NationalNo: {NationalNo}",
                    personDto.NationalNo);

                CleanupFile(uploadedFileName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validation or Conflict error while adding person: {NationalNo}", personDto.NationalNo);
                CleanupFile(uploadedFileName);

                if (ex is ConflictException || ex is ValidationException) throw;

                _logger.LogCritical(ex, "Unexpected error adding person with NationalNo: {NationalNo}", personDto.NationalNo);
                throw new Exception("An error occurred while saving the person record. " + ex.Message);
            }
        }

        public async Task<PersonDto?> UpdatePersonAsync(PersonDto personDto, Stream? imageStream, string? originalFileName,
            int currentUserId, string currentUserRole)
        {
            _logger.LogInformation("Attempting to update person with PersonID: {PersonID}", personDto.PersonID);
            string? newFileName = null;
            try
            {
                var personInDb = await _unitOfWork.People.FindAsync(p => p.PersonID == personDto.PersonID);
                if (personInDb == null)
                    throw new ResourceNotFoundException($"Cannot update: Person with PersonID {personDto.PersonID} not found.");

                await CheckPermissionAsync(personDto, personInDb, currentUserId, currentUserRole);

                await ValidatePersonDataAsync(personDto, isUpdate: true);

                string? oldImagePath = personInDb.ImagePath;

                newFileName = await HandleImageUploadAsync(imageStream, originalFileName);

                if (newFileName != null)
                    personInDb.ImagePath = newFileName;
                else if (string.IsNullOrEmpty(personDto.ImageUrl) && !string.IsNullOrEmpty(oldImagePath))
                    personInDb.ImagePath = null;

                _mapper.Map(personDto, personInDb);
                _unitOfWork.People.Update(personInDb);

                var result = await _unitOfWork.CompleteAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Successfully updated person with PersonID: {PersonID} ", personDto.PersonID);
                    if (oldImagePath != personInDb.ImagePath)
                        CleanupFile(oldImagePath);
                    return await GetPersonByIdAsync(personDto.PersonID);
                }

                _logger.LogError("Failed to update person record in the database for PersonID: {PersonID}", personDto.PersonID);
                CleanupFile(newFileName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validation or Conflict error while updated PersonID: {PersonID}", personDto.PersonID);
                CleanupFile(newFileName);

                if (ex is ResourceNotFoundException || ex is ValidationException ||
                        ex is ConflictException || ex is UnauthorizedAccessException ||
                        ex is ForbiddenException) throw;

                _logger.LogCritical(ex, "Unexpected error updated person with PersonID: {PersonID}", personDto.PersonID);
                throw new Exception($"An error occurred while updating the person with PersonID {personDto.PersonID}. {ex.Message}");
            }
        }

        public async Task<bool> DeletePersonAsync(int id)
        {
            _logger.LogInformation("Attempting to delete person with PersonID: {PersonID}", id);
            if (id <= 0)
                throw new ValidationException("Invalid PersonID provided for deletion.");

            try
            {
                var person = await _unitOfWork.People.GetByIdAsync(id);

                if (person == null)
                    throw new ResourceNotFoundException($"Person with PersonID {id} not found.");

                string? imagePath = person.ImagePath;

                _unitOfWork.People.Delete(person);

                var result = await _unitOfWork.CompleteAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Successfully deleted person with PersonID: {PersonID}", id);
                    CleanupFile(imagePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting person with PersonID: {PersonID}. Person might be linked to other records.", id);

                if (ex is ResourceNotFoundException || ex is ValidationException) throw;
                throw new ConflictException("Cannot delete this person because they are linked to other records (like Users, Applications, or Licenses).");
            }
        }

        public async Task<bool> IsPersonExistByIdAsync(int id)
        {
            _logger.LogInformation("Checking existence of person with PersonID: {PersonID}", id);
            if (id <= 0)
                throw new ValidationException("Invalid ID");
            return await _unitOfWork.People.IsExistAsync(d => d.PersonID == id);
        }

        #region Private Helper Methods
        private string? GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return null;

            return $"{request.Scheme}://{request.Host}{request.PathBase}";
        }

        private void CleanupFile(string? fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                _fileService.DeleteFile(fileName, "people");
            }
        }

        private async Task ValidatePersonDataAsync(PersonDto personDto, bool isUpdate = false)
        {
            if (await _unitOfWork.People.IsExistAsync(p => p.NationalNo == personDto.NationalNo && (!isUpdate || p.PersonID != personDto.PersonID)))
                throw new ConflictException($"The National Number '{personDto.NationalNo}' is already registered for another person.");

            if (!await _unitOfWork.Countries.IsExistAsync(c => c.CountryID == personDto.NationalityCountryID))
                throw new ValidationException("The selected country is invalid or does not exist.");
        }

        private async Task CheckPermissionAsync(PersonDto personDto, Person personInDb, int currentUserId, string currentUserRole)
        {
            if (currentUserRole != UserRoles.Admin)
            {
                int loggedInPersonId = await _userService.GetPersonIdByUserIdAsync(currentUserId);

                bool isTargetUser = await _userService.IsPersonAlreadyUserAsync(personDto.PersonID);

                if (isTargetUser && personInDb.PersonID != loggedInPersonId)
                {
                    _logger.LogWarning("Unauthorized: User {UserId} tried to update another staff member {TargetId}", currentUserId, personDto.PersonID);
                    throw new ForbiddenException("You cannot edit other users' data.");
                }
            }
        }

        private async Task<string?> HandleImageUploadAsync(Stream? imageStream, string? originalFileName)
        {
            if (imageStream != null && !string.IsNullOrEmpty(originalFileName))
            {
                if (_fileService.IsImage(originalFileName))
                {
                    return await _fileService.SaveFileAsync(imageStream, originalFileName, "people");
                }

                throw new ValidationException("The uploaded file is not a valid image.");
            }
            return null;
        }
        #endregion
    }
}