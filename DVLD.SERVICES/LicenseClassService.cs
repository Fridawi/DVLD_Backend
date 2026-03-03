using AutoMapper;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.DTOs.TestTypes;
using DVLD.CORE.Entities;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.CORE.Interfaces.Licenses;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DVLD.Services
{
    public class LicenseClassService : ILicenseClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LicenseClassService> _logger;

        public LicenseClassService(IUnitOfWork unitOfWork, ILogger<LicenseClassService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<LicenseClassDto>> GetAllLicenseClassesAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching License Classes with pagination - PageNumber: {PageNumber}, PageSize: {PageSize}, FilterColumn: {FilterColumn}, FilterValue: {FilterValue}",
                    pageNumber, pageSize, filterColumn, filterValue);

            Expression<Func<LicenseClass, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                filter = filterColumn.ToLower() switch
                {
                    "licenseclassid" when int.TryParse(filterValue, out int id) => p => p.LicenseClassID == id,
                    "classname" => p => p.ClassName.Contains(filterValue),
                    _ => p => true
                };
            }

            int skip = (pageNumber - 1) * pageSize;

            var licenseClasses = await _unitOfWork.LicenseClasses.FindAllAsync(
                predicate: filter,
                includes: null,
                tracked: false,
                orderBy: p => p.ClassName,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.LicenseClasses.CountAsync(filter);

            var mappedLicenseClasses = _mapper.Map<IEnumerable<LicenseClassDto>>(licenseClasses);

            return new PagedResultDto<LicenseClassDto>
            {
                Data = mappedLicenseClasses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<LicenseClassDto> GetLicenseClassByIdAsync(int id)
        {
            _logger.LogInformation("Fetching License Class with ID: {Id}", id);
            if (id <= 0) throw new ValidationException("Invalid ID");

            var licenseClass = await _unitOfWork.LicenseClasses.FindAsync(lc => lc.LicenseClassID == id);

            if (licenseClass == null)
            {
                _logger.LogWarning("License Class with LicenseClassID {LicenseClassID} was not found.", id);
                throw new ResourceNotFoundException($"License Class with LicenseClassID {id} was not found.");
            }
            return _mapper.Map<LicenseClassDto>(licenseClass);
        }

        public async Task<LicenseClassDto> GetLicenseClassByClassNameAsync(string className)
        {
            _logger.LogInformation("Fetching License Class with ClassName: {className}", className);

            if (string.IsNullOrWhiteSpace(className))
                throw new ValidationException("Class Name cannot be null or empty.");

            var licenseClass = await _unitOfWork.LicenseClasses.FindAsync(lc => lc.ClassName == className.Trim());

            if (licenseClass == null)
            {
                _logger.LogWarning("License Class with ClassName {className} was not found.", className);
                throw new ResourceNotFoundException($"License Class with ClassName {className} was not found.");
            }
            return _mapper.Map<LicenseClassDto>(licenseClass);
        }

        public async Task<LicenseClassDto?> AddLicenseClassAsync(LicenseClassDto licenseClassDto)
        {
            _logger.LogInformation("Attempting to add a new License Class with Class Name: {ClassName}", licenseClassDto.ClassName);
            try
            {
                await ValidateLicenseClassDataAsync(licenseClassDto);

                var licenseClass = _mapper.Map<LicenseClass>(licenseClassDto);

                await _unitOfWork.LicenseClasses.AddAsync(licenseClass);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully added License Class with LicenseClassID: {LicenseClassID}",
                        licenseClass.LicenseClassID);
                    licenseClassDto.LicenseClassID = licenseClass.LicenseClassID;
                    return licenseClassDto;
                }

                _logger.LogError("Failed to save License Class record to the database for Class Name: {ClassName}", licenseClassDto.ClassName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Conflict error while adding License Class: {ClassName}", licenseClassDto.ClassName);

                if (ex is ConflictException) throw;

                _logger.LogCritical(ex, "Unexpected error adding License Class with {ClassName}", licenseClassDto.ClassName);
                throw new Exception("An error occurred while saving the License Class record. " + ex.Message);
            }
        }

        public async Task<LicenseClassDto?> UpdateLicenseClassAsync(LicenseClassDto licenseClassDto)
        {
            _logger.LogInformation("Attempting to update a new License Class with Class Name: {ClassName}", licenseClassDto.ClassName);
            try
            {
                var licenseClassInDb = await _unitOfWork.LicenseClasses.FindAsync(lc =>
                lc.LicenseClassID == licenseClassDto.LicenseClassID);

                if (licenseClassInDb == null)
                    throw new ResourceNotFoundException($"Cannot update: License Class with LicenseClassID {licenseClassDto.LicenseClassID} not found.");

                await ValidateLicenseClassDataAsync(licenseClassDto, isUpdate: true);

                _mapper.Map(licenseClassDto, licenseClassInDb);

                _unitOfWork.LicenseClasses.Update(licenseClassInDb);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully updateed License Class with LicenseClassID: {licenseClassID}",
                        licenseClassInDb.LicenseClassID);
                    return licenseClassDto;
                }

                _logger.LogError("Failed to update License Class record to the database for Class Name: {ClassName}",
                    licenseClassInDb.ClassName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validation or Conflict error while updated LicenseClassID: {LicenseClassID}",
                    licenseClassDto.LicenseClassID);

                if (ex is ResourceNotFoundException || ex is ConflictException) throw;

                _logger.LogCritical(ex, "Unexpected error updated License Class with LicenseClassID {LicenseClassID}",
                    licenseClassDto.LicenseClassID);
                throw new Exception("An error occurred while updating the License Class record. " + ex.Message);
            }
        }

        #region Private Helper Methods
        private async Task ValidateLicenseClassDataAsync(LicenseClassDto licenseClass, bool isUpdate = false)
        {
            if (await _unitOfWork.LicenseClasses.IsExistAsync(lc => lc.ClassName == licenseClass.ClassName &&
            (!isUpdate || lc.LicenseClassID != licenseClass.LicenseClassID)))
                throw new ConflictException($"The License Class ClassName '{licenseClass.ClassName}' is already registered for another License Class.");
        }
        #endregion
    }
}