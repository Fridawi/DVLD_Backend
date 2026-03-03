using AutoMapper;
using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Users;
using DVLD.CORE.Entities;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace DVLD.Services
{
    public class ApplicationTypeService : IApplicationTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ApplicationTypeService> _logger;

        public ApplicationTypeService(IUnitOfWork unitOfWork, ILogger<ApplicationTypeService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<ApplicationTypeDto>> GetAllApplicationTypesAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching Application Types with pagination - PageNumber: {PageNumber}, PageSize: {PageSize}, FilterColumn: {FilterColumn}, FilterValue: {FilterValue}",
                pageNumber, pageSize, filterColumn, filterValue);

            Expression<Func<ApplicationType, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                filter = filterColumn.ToLower() switch
                {
                    "applicationtypeid" when int.TryParse(filterValue, out int id) => p => p.ApplicationTypeID == id,

                    "title" => p => p.Title.Contains(filterValue),

                    _ => p => true
                };
            }

            int skip = (pageNumber - 1) * pageSize;

            var applicationTypes = await _unitOfWork.ApplicationTypes.FindAllAsync(
                predicate: filter,
                includes: null,
                tracked: false,
                orderBy: p => p.Title,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.ApplicationTypes.CountAsync(filter);

            var mappedApplicationTypes = _mapper.Map<IEnumerable<ApplicationTypeDto>>(applicationTypes);

            return new PagedResultDto<ApplicationTypeDto> 
            {
                Data = mappedApplicationTypes,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ApplicationTypeDto> GetApplicationTypeByIdAsync(int id)
        {
            _logger.LogInformation("Fetching Application Type with ApplicationTypeID: {ApplicationTypeID}", id);
            if (id <= 0) throw new ValidationException("Invalid ID");

            var applicationType = await _unitOfWork.ApplicationTypes.FindAsync(at => at.ApplicationTypeID == id);

            if (applicationType == null)
            {
                _logger.LogWarning("Application Type with ApplicationTypeID {ApplicationTypeID} was not found.", id);
                throw new ResourceNotFoundException($"Application Type with ApplicationTypeID {id} was not found.");
            }
            return _mapper.Map<ApplicationTypeDto>(applicationType);
        }

        public async Task<ApplicationTypeDto?> AddApplicationTypeAsync(ApplicationTypeDto applicationTypeDto)
        {
            _logger.LogInformation("Attempting to add a new Application Type with Title: {Title}", applicationTypeDto.Title);
            try
            {
                await ValidateApplicationTypeDataAsync(applicationTypeDto);

                var applicationType = _mapper.Map<ApplicationType>(applicationTypeDto);

                await _unitOfWork.ApplicationTypes.AddAsync(applicationType);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully added Application Type with ApplicationTypeID: {ApplicationTypeID}",
                        applicationType.ApplicationTypeID);
                    applicationTypeDto.ApplicationTypeID = applicationType.ApplicationTypeID;
                    return applicationTypeDto;
                }

                _logger.LogError("Failed to save Application Type record to the database for Title: {Title}", applicationType.Title);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Conflict error while adding Application Type: {Title}", applicationTypeDto.Title);

                if (ex is ConflictException) throw;

                _logger.LogCritical(ex, "Unexpected error adding Application Type with {Title}", applicationTypeDto.Title);
                throw new Exception("An error occurred while saving the Application Type record. " + ex.Message);
            }
        }

        public async Task<ApplicationTypeDto?> UpdateApplicationTypeAsync(ApplicationTypeDto applicationTypeDto)
        {
            _logger.LogInformation("Attempting to update a new Application Type with Title: {Title}", applicationTypeDto.Title);
            try
            {
                var applicationTypeInDb = await _unitOfWork.ApplicationTypes.FindAsync(
                    at => at.ApplicationTypeID == applicationTypeDto.ApplicationTypeID);
                if (applicationTypeInDb == null)
                    throw new ResourceNotFoundException($"Cannot update: Application Type with ID {applicationTypeDto.ApplicationTypeID} not found.");

                await ValidateApplicationTypeDataAsync(applicationTypeDto, isUpdate: true);

                _mapper.Map(applicationTypeDto, applicationTypeInDb);

                _unitOfWork.ApplicationTypes.Update(applicationTypeInDb);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully updateed Application Type with ApplicationTypeID: {ApplicationTypeID}",
                        applicationTypeInDb.ApplicationTypeID);
                    return applicationTypeDto;
                }

                _logger.LogError("Failed to update Application Type record to the database for Title: {Title}", applicationTypeInDb.Title);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validation or Conflict error while updated ApplicationTypeID: {ApplicationTypeID}",
                    applicationTypeDto.ApplicationTypeID);

                if (ex is ResourceNotFoundException || ex is ConflictException) throw;

                _logger.LogCritical(ex, "Unexpected error updated Application Type with ApplicationTypeID {ApplicationTypeID}",
                    applicationTypeDto.ApplicationTypeID);
                throw new Exception("An error occurred while updating the Application Type record. " + ex.Message);
            }
        }

        #region Private Helper Methods
        private async Task ValidateApplicationTypeDataAsync(ApplicationTypeDto applicationType, bool isUpdate = false)
        {
            if (await _unitOfWork.ApplicationTypes.IsExistAsync(at => at.Title == applicationType.Title
                && (!isUpdate || at.ApplicationTypeID != applicationType.ApplicationTypeID)))
                throw new ConflictException($"The Application Type Title '{applicationType.Title}' is already registered for another Application Type.");
        }
        #endregion
    }
}