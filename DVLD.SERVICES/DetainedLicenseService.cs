using AutoMapper;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Drivers;
using DVLD.CORE.DTOs.Licenses.DetainedLicenses;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DVLD.Services
{
    public class DetainedLicenseService : IDetainedLicenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DetainedLicenseService> _logger;

        public DetainedLicenseService(IUnitOfWork unitOfWork, ILogger<DetainedLicenseService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<DetainedLicenseDto>> GetAllDetainedLicensesAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching Detained Licenses with pagination - PageNumber: {PageNumber}, PageSize: {PageSize}, FilterColumn: {FilterColumn}, FilterValue: {FilterValue}",
                 pageNumber, pageSize, filterColumn, filterValue);

            Expression<Func<DetainedLicense, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                filter = filterColumn.ToLower() switch
                {
                    "detainid" when int.TryParse(filterValue, out int id) => p => p.DetainID == id,
                    "licenseid" when int.TryParse(filterValue, out int id) => p => p.LicenseID == id,
                    "nationalno" => p => p.LicenseInfo.ApplicationInfo.PersonInfo.NationalNo == filterValue,

                    "fullname" => p => (p.LicenseInfo.ApplicationInfo.PersonInfo.FirstName + " " +
                                  p.LicenseInfo.ApplicationInfo.PersonInfo.SecondName + " " +
                                 (p.LicenseInfo.ApplicationInfo.PersonInfo.ThirdName != null ? p.LicenseInfo.ApplicationInfo.PersonInfo.ThirdName + " " : "") +
                                  p.LicenseInfo.ApplicationInfo.PersonInfo.LastName)
                                    .Replace("  ", " ")
                                    .Contains(filterValue.Trim()),
                    "isreleased" when bool.TryParse(filterValue, out bool released) => p => p.IsReleased == released,
                    "releaseapplicationid" when int.TryParse(filterValue, out int id) => p => p.ReleaseApplicationID == id,

                    _ => p => true
                };
            }

            int skip = (pageNumber - 1) * pageSize;

            var detainedLicenses = await _unitOfWork.DetainedLicenses.FindAllAsync(
                predicate: filter,
                includes: ["CreatedByUserInfo", "ReleasedByUserInfo", "LicenseInfo.ApplicationInfo.PersonInfo"],
                tracked: false,
                orderBy: p => p.DetainDate,
                orderByDirection: EnOrderByDirection.Descending,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.DetainedLicenses.CountAsync(filter);

            var mappedDetainedLicenses = _mapper.Map<IEnumerable<DetainedLicenseDto>>(detainedLicenses);

            return new PagedResultDto<DetainedLicenseDto>
            {
                Data = mappedDetainedLicenses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<DetainedLicenseDto?> GetDetainedLicenseByIdAsync(int id)
        {
            _logger.LogInformation("Fetching DetainedLicense with DetainID: {DetainID}", id);

            if (id <= 0) throw new ValidationException("Invalid ID");

            var detainedLicense = await _unitOfWork.DetainedLicenses.FindAsync(
                d => d.DetainID == id,
                includes: ["CreatedByUserInfo", "ReleasedByUserInfo", "LicenseInfo.ApplicationInfo.PersonInfo"]
            );

            if (detainedLicense == null)
            {
                _logger.LogWarning("Detained License with ID {Id} was not found.", id);
                throw new ResourceNotFoundException($"Detained License with ID {id} was not found.");
            }

            return _mapper.Map<DetainedLicenseDto>(detainedLicense);
        }

        public async Task<DetainedLicenseDto?> GetDetainedLicenseByLicenseIdAsync(int licenseId)
        {
            _logger.LogInformation("Fetching DetainedLicense with LicenseID: {LicenseId}", licenseId);

            if (licenseId <= 0) throw new ValidationException("Invalid License ID");

            var detainedLicense = await _unitOfWork.DetainedLicenses.FindAsync(
                d => d.LicenseID == licenseId && !d.IsReleased,
                includes: ["CreatedByUserInfo", "ReleasedByUserInfo", "LicenseInfo.ApplicationInfo.PersonInfo"]
            );

            if (detainedLicense == null)
            {
                _logger.LogInformation("No active detention found for License ID {LicenseId}.", licenseId);
                return null;
            }

            return _mapper.Map<DetainedLicenseDto>(detainedLicense);
        }

        public async Task<DetainedLicenseDto?> DetainLicenseAsync(DetainLicenseCreateDto detainDto, int userId)
        {
            if (detainDto == null) throw new ValidationException("Detain data is null.");

            try
            {
                _logger.LogInformation("Attempting to detain license ID: {LicenseID} by User ID: {UserID}", detainDto.LicenseID, userId);

                if (detainDto.LicenseID <= 0) throw new ValidationException("Invalid License ID.");
                if (detainDto.FineFees < 0) throw new ValidationException("Fine fees cannot be negative.");


                if (!await _unitOfWork.Licenses.IsExistAsync(l => l.LicenseID == detainDto.LicenseID))
                {
                    _logger.LogWarning("License ID {LicenseID} not found.", detainDto.LicenseID);
                    throw new ResourceNotFoundException($"License with ID {detainDto.LicenseID} was not found.");
                }

                bool isAlreadyDetained = await _unitOfWork.DetainedLicenses.IsExistAsync(d => d.LicenseID == detainDto.LicenseID && !d.IsReleased);

                if (isAlreadyDetained)
                {
                    _logger.LogWarning("License ID {LicenseID} is already detained.", detainDto.LicenseID);
                    throw new ValidationException("This license is already detained and cannot be detained again until it is released.");
                }

                var detainedLicense = new DetainedLicense
                {
                    LicenseID = detainDto.LicenseID,
                    DetainDate = DateTime.UtcNow,
                    FineFees = detainDto.FineFees,
                    CreatedByUserID = userId,
                    IsReleased = false,
                };

                await _unitOfWork.DetainedLicenses.AddAsync(detainedLicense);

                var result = await _unitOfWork.CompleteAsync();

                if (result <= 0)
                {
                    _logger.LogError("Failed to save the detain record for LicenseID: {LicenseID}", detainDto.LicenseID);
                    return null;
                }

                _logger.LogInformation("License ID {LicenseID} has been detained successfully with DetainID: {DetainID}",
                    detainDto.LicenseID, detainedLicense.DetainID);

                return await GetDetainedLicenseByIdAsync(detainedLicense.DetainID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while detaining license ID: {LicenseID}", detainDto.LicenseID);

                if (ex is ResourceNotFoundException || ex is ValidationException) throw;
                throw new Exception("An unexpected error occurred during the detention process.");
            }
        }

        public async Task<DetainedLicenseDto?> ReleaseLicenseAsync(int licenseId, int userId)
        {
            _logger.LogInformation("Attempting to release license ID: {LicenseID}", licenseId);

            var detainedRecord = await _unitOfWork.DetainedLicenses.FindAsync(
                d => d.LicenseID == licenseId && !d.IsReleased,
                includes: ["LicenseInfo.DriverInfo"]);

            if (detainedRecord == null)
            {
                _logger.LogWarning("No active detention record found for License ID: {LicenseId}", licenseId);
                throw new ResourceNotFoundException($"No active detention record found for License ID {licenseId}.");
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var appTypeInfo = await _unitOfWork.ApplicationTypes.GetByIdAsync((int)EnApplicationType.ReleaseDetainedDrivingLicsense);
                if (appTypeInfo == null)
                    throw new Exception("Application type 'Release Detained License' not found in the system.");

                var application = new Application
                {
                    ApplicantPersonID = detainedRecord.LicenseInfo.DriverInfo.PersonID,
                    ApplicationDate = DateTime.UtcNow,
                    ApplicationTypeID = (int)EnApplicationType.ReleaseDetainedDrivingLicsense,
                    ApplicationStatus = EnApplicationStatus.Completed,
                    LastStatusDate = DateTime.UtcNow,
                    PaidFees = appTypeInfo.Fees,
                    CreatedByUserID = userId
                };

                await _unitOfWork.Applications.AddAsync(application);
                await _unitOfWork.CompleteAsync(); 

                detainedRecord.IsReleased = true;
                detainedRecord.ReleaseDate = DateTime.UtcNow;
                detainedRecord.ReleasedByUserID = userId;
                detainedRecord.ReleaseApplicationID = application.ApplicationID;

                _unitOfWork.DetainedLicenses.Update(detainedRecord);
                var result = await _unitOfWork.CompleteAsync();

                if (result <= 0)
                {
                    throw new Exception("Failed to update the detention record.");
                }

                await transaction.CommitAsync();

                _logger.LogInformation("License ID {LicenseId} has been released successfully.", licenseId);

                return await GetDetainedLicenseByIdAsync(detainedRecord.DetainID);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error while releasing license ID: {LicenseID}", licenseId);

                if (ex is ResourceNotFoundException || ex is ValidationException) throw;
                throw new Exception("An unexpected error occurred during the release process: " + ex.Message);
            }
        }

        public async Task<bool> IsLicenseDetainedAsync(int licenseId)
        {
            _logger.LogInformation("Checking if License ID {LicenseId} is currently detained.", licenseId);

            if (licenseId <= 0) return false;

            return await _unitOfWork.DetainedLicenses.IsExistAsync(d => d.LicenseID == licenseId && !d.IsReleased);
        }
    }
}