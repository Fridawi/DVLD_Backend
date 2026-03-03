using AutoMapper;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.CORE.Interfaces.Tests;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace DVLD.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITestService _testService;
        private readonly IMapper _mapper;
        private readonly ILogger<LicenseService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LicenseService(IUnitOfWork unitOfWork,ITestService testService, IMapper mapper, ILogger<LicenseService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _testService = testService;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<PagedResultDto<LicenseDto>> GetAllLicensesAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching All Licenses - Page: {PageNumber}, Filter: {FilterColumn}", pageNumber, filterColumn);

            Expression<Func<License, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                filter = filterColumn.ToLower() switch
                {
                    "licenseid" when int.TryParse(filterValue, out int id) => p => p.LicenseID == id,
                    "applicationid" when int.TryParse(filterValue, out int id) => p => p.ApplicationID == id,
                    "driverid" when int.TryParse(filterValue, out int id) => p => p.DriverID == id,
                    "isactive" when bool.TryParse(filterValue, out bool active) => p => p.IsActive == active,
                    "licenseclassname" => p => p.LicenseClassInfo.ClassName.Contains(filterValue),
                    "isdetained" when bool.TryParse(filterValue, out bool detained) =>
                        p => p.DetainedRecords.Any(d => !d.IsReleased) == detained,
                    _ => p => true
                };
            }

            int skip = (pageNumber - 1) * pageSize;

            var licenses = await _unitOfWork.Licenses.FindAllAsync(
                predicate: filter,
                includes: ["LicenseClassInfo", "DetainedRecords"],
                tracked: false,
                orderBy: l => l.LicenseID,
                orderByDirection: EnOrderByDirection.Descending,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.Licenses.CountAsync(filter);
            var mappedLicenses = _mapper.Map<IEnumerable<LicenseDto>>(licenses);

            return new PagedResultDto<LicenseDto>
            {
                Data = mappedLicenses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        
        public async Task<PagedResultDto<DriverLicenseDto>> GetLicensesByDriverIdAsync(int driverId, int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching Licenses for DriverID: {driverId}", driverId);

            if (driverId <= 0) throw new ValidationException("Invalid Driver ID");

            Expression<Func<License, bool>> filter = p => p.DriverID == driverId;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                if (filterColumn.ToLower() == "licenseid" && int.TryParse(filterValue, out int id))
                    filter = p => p.DriverID == driverId && p.LicenseID == id;
                else if (filterColumn.ToLower() == "isactive" && bool.TryParse(filterValue, out bool active))
                    filter = p => p.DriverID == driverId && p.IsActive == active;
            }

            int skip = (pageNumber - 1) * pageSize;

            var licenses = await _unitOfWork.Licenses.FindAllAsync(
                predicate: filter,
                includes: ["LicenseClassInfo", "DetainedRecords", "ApplicationInfo.PersonInfo"],
                tracked: false,
                orderBy: l => l.IssueDate,
                orderByDirection: EnOrderByDirection.Descending,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.Licenses.CountAsync(filter);

            var mappedData = _mapper.Map<IEnumerable<DriverLicenseDto>>(licenses, opt => opt.Items["BaseUrl"] = GetBaseUrl());

            return new PagedResultDto<DriverLicenseDto>
            {
                Data = mappedData,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
       
        public async Task<DriverLicenseDto?> GetDriverLicensesByIdAsync(int licenseId)
        {
            _logger.LogInformation("Fetching License with LicenseID: {licenseID}", licenseId);

            if (licenseId <= 0) throw new ValidationException("Invalid ID");

            var license = await _unitOfWork.Licenses.FindAsync(
                t => t.LicenseID == licenseId,
                includes: ["LicenseClassInfo", "ApplicationInfo.PersonInfo", "DetainedRecords"],
                tracked: false);

            if (license == null)
            {
                _logger.LogWarning("License with LicenseID {licenseID} was not found.", licenseId);
                return null;
            }

            return _mapper.Map<DriverLicenseDto>(license, opt => opt.Items["BaseUrl"] = GetBaseUrl());
        }

        public async Task<LicenseDto?> GetActiveLicenseByPersonIDAndLicenseClassID(int personID, int licenseClassID)
        {
            _logger.LogInformation("Fetching Active License with PersonID: {PersonID}, LicenseClassID: {LicenseClassID}", personID, licenseClassID);
            if (personID <= 0 || licenseClassID <= 0) throw new ValidationException("Invalid ID");

            var license = await _unitOfWork.Licenses.FindAsync(
                l => l.ApplicationInfo.ApplicantPersonID == personID &&
                     l.LicenseClassID == licenseClassID &&
                     l.IsActive,
                includes: ["LicenseClassInfo", "ApplicationInfo", "DetainedRecords"]
            );


            if (license == null)
            {
                _logger.LogWarning("Active License with PersonID {PersonID} and LicenseClassID {LicenseClassID} was not found.", personID, licenseClassID);
                return null;
            }

            return _mapper.Map<LicenseDto>(license);
        }

        public async Task<LicenseDto?> GetLicenseByIdAsync(int id)
        {
            _logger.LogInformation("Fetching License with ID: {ID}", id);
            if (id <= 0) throw new ValidationException("Invalid ID");

            var license = await _unitOfWork.Licenses.FindAsync(
                l => l.LicenseID == id,
                includes: ["LicenseClassInfo", "DetainedRecords"]
            );


            if (license == null)
            {
                _logger.LogWarning("License with ID {Id} was not found.", id);
                return null;
            }

            return _mapper.Map<LicenseDto>(license);
        }

        public async Task<LicenseDto?> IssueFirstTimeLicenseAsync(LicenseCreateDto dto, int currentUserID)
        {
            _logger.LogInformation("Issuing first-time license for Application: {AppID}", dto.LocalDrivingLicenseApplicationID);

            var localApp = await _unitOfWork.LocalDrivingLicenseApplications.FindAsync(
                l => l.LocalDrivingLicenseApplicationID == dto.LocalDrivingLicenseApplicationID,
                includes: ["ApplicationInfo", "LicenseClassInfo"]);

            if (localApp == null)
                throw new ResourceNotFoundException("Local Application not found.");

             if (!await _testService.PassedAllTestsAsync(dto.LocalDrivingLicenseApplicationID))
                throw new ValidationException("Person has not passed all required tests.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                DateTime expirationDate = DateTime.UtcNow.AddYears(localApp.LicenseClassInfo.DefaultValidityLength);
                float fees = (float)localApp.LicenseClassInfo.ClassFees;

                var result = await IssueLicenseAsync(localApp, expirationDate, fees, EnIssueReason.FirstTime, dto.Notes, currentUserID);

                if (result == null)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to issue first-time license for AppID: {AppID}", dto.LocalDrivingLicenseApplicationID);
                throw;
            }
        }

        public async Task<LicenseDto?> RenewLicenseAsync(int oldLicenseId, string? notes, int currentUserID)
        {
            _logger.LogInformation("Renewing License: {LicenseID}", oldLicenseId);

            var oldLicense = await _unitOfWork.Licenses.FindAsync(
                l => l.LicenseID == oldLicenseId,
                includes: ["DriverInfo.PersonInfo", "LicenseClassInfo"]);

            if (oldLicense == null || !oldLicense.IsActive)
                throw new ValidationException("Cannot renew an inactive or non-existent license.");

            if (oldLicense.ExpirationDate > DateTime.UtcNow) 
            {
                throw new ValidationException("License is not yet expired.");
            }

            var appTypeInfo = await _unitOfWork.ApplicationTypes.GetByIdAsync((int)EnApplicationType.RenewDrivingLicense);

            if (appTypeInfo == null)
                throw new ResourceNotFoundException("Application type for renewal not found.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var application = new Application
                {
                    ApplicantPersonID = oldLicense.DriverInfo.PersonID,
                    ApplicationDate = DateTime.UtcNow,
                    ApplicationTypeID = (int)EnApplicationType.RenewDrivingLicense,
                    ApplicationStatus = EnApplicationStatus.Completed,
                    LastStatusDate = DateTime.UtcNow,
                    PaidFees = appTypeInfo.Fees,
                    CreatedByUserID = currentUserID
                };
                await _unitOfWork.Applications.AddAsync(application);
                await _unitOfWork.CompleteAsync();

                oldLicense.IsActive = false;

                var localApp = new LocalDrivingLicenseApplication
                {
                    ApplicationID = application.ApplicationID,
                    ApplicationInfo = application,
                    LicenseClassID = oldLicense.LicenseClassID,
                    LicenseClassInfo = oldLicense.LicenseClassInfo
                };

                DateTime newExpirationDate = DateTime.UtcNow.AddYears(oldLicense.LicenseClassInfo.DefaultValidityLength);
                float licenseFees = (float)oldLicense.LicenseClassInfo.ClassFees;

                var result = await IssueLicenseAsync(localApp, newExpirationDate, licenseFees, EnIssueReason.Renew, notes, currentUserID);

                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error while renewing license {ID}", oldLicenseId);
                throw;
            }
        }

        public async Task<LicenseDto?> ReplaceLicenseAsync(int oldLicenseId, EnIssueReason issueReason, int currentUserID)
        {
            _logger.LogInformation("Replacing License {ID} for reason {Reason}", oldLicenseId, issueReason);

            var oldLicense = await _unitOfWork.Licenses.FindAsync(l => l.LicenseID == oldLicenseId, includes: ["DriverInfo", "LicenseClassInfo"]);

            if (oldLicense == null)
                throw new ResourceNotFoundException($"License with ID {oldLicenseId} was not found.");

            if (!oldLicense.IsActive)
                throw new ValidationException("License is not active and cannot be replaced.");

            var appType = (issueReason == EnIssueReason.DamagedReplacement)
                            ? EnApplicationType.ReplaceDamagedDrivingLicense
                            : EnApplicationType.ReplaceLostDrivingLicense;

            var appTypeInfo = await _unitOfWork.ApplicationTypes.GetByIdAsync((int)appType);
            if (appTypeInfo == null)
                throw new ResourceNotFoundException("Application type for renewal not found.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var application = new Application
                {
                    ApplicantPersonID = oldLicense.DriverInfo.PersonID,
                    ApplicationDate = DateTime.UtcNow,
                    ApplicationTypeID = (int)appType,
                    ApplicationStatus = EnApplicationStatus.Completed,
                    LastStatusDate = DateTime.UtcNow,
                    PaidFees = appTypeInfo.Fees,
                    CreatedByUserID = currentUserID
                };

                await _unitOfWork.Applications.AddAsync(application);
                await _unitOfWork.CompleteAsync();

                oldLicense.IsActive = false;

                var localApp = new LocalDrivingLicenseApplication
                {
                    ApplicationID = application.ApplicationID,
                    ApplicationInfo = application,
                    LicenseClassID = oldLicense.LicenseClassID,
                    LicenseClassInfo = oldLicense.LicenseClassInfo
                };

                var result = await IssueLicenseAsync(localApp, oldLicense.ExpirationDate, 0, issueReason, oldLicense.Notes, currentUserID);

                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<LicenseDto?> IssueLicenseAsync(LocalDrivingLicenseApplication localApp, DateTime expirationDate, float fees, EnIssueReason reason, string? notes, int currentUserID)
        {
            try
            {
                var driver = await _unitOfWork.Drivers.FindAsync(d => d.PersonID == localApp.ApplicationInfo.ApplicantPersonID);

                if (driver == null)
                {
                    _logger.LogInformation("Person {PersonID} is not a driver yet. Creating a new driver record.", localApp.ApplicationInfo.ApplicantPersonID);

                    driver = new Driver
                    {
                        PersonID = localApp.ApplicationInfo.ApplicantPersonID,
                        CreatedByUserID = currentUserID,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _unitOfWork.Drivers.AddAsync(driver);

                    await _unitOfWork.CompleteAsync();
                }

                var license = new License
                {
                    ApplicationID = localApp.ApplicationID,
                    DriverID = driver.DriverID, 
                    LicenseClassID = localApp.LicenseClassID,
                    IssueDate = DateTime.UtcNow,
                    ExpirationDate = expirationDate,
                    Notes = notes,
                    PaidFees = fees,
                    IsActive = true,
                    IssueReason = reason,
                    CreatedByUserID = currentUserID
                };

                await _unitOfWork.Licenses.AddAsync(license);

                localApp.ApplicationInfo.ApplicationStatus = EnApplicationStatus.Completed;
                localApp.ApplicationInfo.LastStatusDate = DateTime.UtcNow;

                var result = await _unitOfWork.CompleteAsync();

                if (result <= 0)
                {
                    _logger.LogError("Failed to save the new license record for ApplicationID: {AppID}", localApp.ApplicationID);
                    return null;
                }

                _logger.LogInformation("License issued successfully with ID: {LicenseID}", license.LicenseID);

                license.LicenseClassInfo = localApp.LicenseClassInfo;
                return _mapper.Map<LicenseDto>(license);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Internal IssueLicenseAsync for AppID: {AppID}", localApp.ApplicationID);
                throw;
            }
        }
        public async Task<bool> DeactivateLicense(int licenseID)
        {
            _logger.LogInformation("Deactivating License with ID: {Id}", licenseID);
            try
            {
                if (licenseID <= 0) throw new ValidationException("Invalid ID");

                var license = await _unitOfWork.Licenses.GetByIdAsync(licenseID);

                if (license == null)
                {
                    _logger.LogWarning("License with ID {Id} was not found for deactivation.", licenseID);
                    throw new ResourceNotFoundException("License not found");
                }

                license.IsActive = false;

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("License with ID: {Id} successfully deactivated.", licenseID);
                    return true;
                }

                _logger.LogWarning("Failed to deactivate License with ID: {Id}. No changes were saved.", licenseID);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while deactivate License with ID: {Id}. No changes were saved.", licenseID);

                if (ex is ResourceNotFoundException || ex is ValidationException) throw;

                _logger.LogCritical(ex, "Unexpected server error deactivate License with ID: {Id}. No changes were saved.", licenseID);
                throw new Exception("An error occurred while deactivate the License record. " + ex.Message);
            }
        }

        #region Private Helper Methods
        private string? GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return null;

            return $"{request.Scheme}://{request.Host}{request.PathBase}";
        }
        #endregion
    }
}