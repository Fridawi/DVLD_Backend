using AutoMapper;
using DVLD.CORE.DTOs.Applications.LocalDrivingLicenseApplication;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
namespace DVLD.Services
{
    public class LocalDrivingLicenseApplicationService : ILocalDrivingLicenseApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LocalDrivingLicenseApplicationService> _logger;
        public LocalDrivingLicenseApplicationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<LocalDrivingLicenseApplicationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<PagedResultDto<LocalDrivingLicenseApplicationDto>> GetAllLocalDrivingLicenseApplicationsAsync(
            int pageNumber,
            int pageSize,
            string? filterColumn = null,
            string? filterValue = null)
        {
            _logger.LogInformation("Fetching Local Driving License Applications with pagination - PageNumber: {PageNumber}, PageSize: {PageSize}, FilterColumn: {FilterColumn}, FilterValue: {FilterValue}",
                pageNumber, pageSize, filterColumn, filterValue);

            Expression<Func<LocalDrivingLicenseApplication, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                string cleanValue = filterValue.Trim();

                filter = filterColumn.ToLower() switch
                {
                    "localappid" when int.TryParse(cleanValue, out int id)
                        => p => p.LocalDrivingLicenseApplicationID == id,
                    "applicationid" when int.TryParse(cleanValue, out int id)
                        => p => p.ApplicationID == id,
                    "classname" => p => p.LicenseClassInfo.ClassName.Contains(cleanValue),
                    "nationalno" => p => p.ApplicationInfo.PersonInfo.NationalNo.Contains(cleanValue),

                    "fullname" => p => (p.ApplicationInfo.PersonInfo.FirstName + " " +
                                        (p.ApplicationInfo.PersonInfo.SecondName ?? "") + " " +
                                        (p.ApplicationInfo.PersonInfo.ThirdName ?? "") + " " +
                                        p.ApplicationInfo.PersonInfo.LastName)
                                        .Contains(cleanValue),
                    "status" when Enum.GetNames(typeof(EnApplicationStatus))
                                      .Any(name => name.Equals(cleanValue, StringComparison.OrdinalIgnoreCase))
                                       => p => p.ApplicationInfo.ApplicationStatus == (EnApplicationStatus)Enum.Parse(typeof(EnApplicationStatus), cleanValue, true),
                    _ => p => true
                };
            }
            var result = await _unitOfWork.LocalDrivingLicenseApplications.GetPagedApplicationsAsync(filter, pageNumber, pageSize);

            return new PagedResultDto<LocalDrivingLicenseApplicationDto>
            {
                Data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

        }

        public async Task<LocalDrivingLicenseApplicationDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching Local Driving License Application with LocalAppID: {LocalAppID}", id);

            if (id <= 0) throw new ValidationException("Invalid ID");

            var localApp = await _unitOfWork.LocalDrivingLicenseApplications.FindAsync(
                l => l.LocalDrivingLicenseApplicationID == id,
                includes: ["ApplicationInfo.PersonInfo", "LicenseClassInfo"],
                tracked: false);

            if (localApp == null)
            {
                _logger.LogWarning("Local Driving License Application with ID {Id} was not found.", id);
                throw new ResourceNotFoundException($"Local Driving License Application with ID {id} was not found.");
            }
            var dto = _mapper.Map<LocalDrivingLicenseApplicationDto>(localApp);

            dto.PassedTestCount = await GetPassedTestCountAsync(dto.LocalDrivingLicenseApplicationID);

            return dto;
        }

        public async Task<LocalDrivingLicenseApplicationDto?> GetByApplicationIdAsync(int id)
        {
            _logger.LogInformation("Fetching Local Driving License Application with ApplicationID: {ApplicationID}", id);

            if (id <= 0) throw new ValidationException("Invalid ID");

            var localApp = await _unitOfWork.LocalDrivingLicenseApplications.FindAsync(
                l => l.ApplicationID == id,
                includes: ["ApplicationInfo.PersonInfo", "LicenseClassInfo"],
                tracked: false);

            if (localApp == null)
            {
                _logger.LogWarning("Local Driving License Application with ApplicationID {Id} was not found.", id);
                throw new ResourceNotFoundException($"Local Driving License Application with ApplicationID {id} was not found.");
            }
            var dto = _mapper.Map<LocalDrivingLicenseApplicationDto>(localApp);

            dto.PassedTestCount = await GetPassedTestCountAsync(dto.LocalDrivingLicenseApplicationID);

            return dto;
        }

        public async Task<LocalDrivingLicenseApplicationDto?> AddLocalDrivingLicenseApplicationAsync(LocalDrivingLicenseApplicationCreateDto createDto, int currentUserID)
        {
            _logger.LogInformation("Attempting to add a new Local Driving License Application with PersonID: {PersonID}",
                createDto.PersonID);

            await ValidateLocalDrivingLicenseApplicationDataAsync(createDto);

            var appType = await _unitOfWork.ApplicationTypes.GetByIdAsync(createDto.ApplicationTypeID);
            if (appType == null)
            {
                throw new ResourceNotFoundException($"Application Type with ID {createDto.ApplicationTypeID} not found.");
            }
            float fees = appType?.Fees ?? 15;

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {

                var app = new Application
                {
                    ApplicantPersonID = createDto.PersonID,
                    ApplicationDate = DateTime.UtcNow,
                    ApplicationTypeID = createDto.ApplicationTypeID,
                    ApplicationStatus = EnApplicationStatus.New,
                    LastStatusDate = DateTime.UtcNow,
                    PaidFees = fees,
                    CreatedByUserID = currentUserID
                };

                await _unitOfWork.Applications.AddAsync(app);

                var baseResult = await _unitOfWork.CompleteAsync();
                if (baseResult <= 0)
                {
                    _logger.LogError("Failed to save Local Driving License Application record to the database for ApplicantPersonID: {ApplicantPersonID}", app.ApplicantPersonID);
                    await transaction.RollbackAsync();
                    return null;
                }

                var localApp = new LocalDrivingLicenseApplication
                {
                    ApplicationID = app.ApplicationID,
                    LicenseClassID = createDto.LicenseClassID
                };

                await _unitOfWork.LocalDrivingLicenseApplications.AddAsync(localApp);

                var localAppResult = await _unitOfWork.CompleteAsync();
                if (localAppResult <= 0)
                {
                    _logger.LogError("Failed to save Local Driving License Application record to the database for ApplicantPersonID: {ApplicantPersonID}", app.ApplicantPersonID);
                    await transaction.RollbackAsync();
                    return null;
                }

                await transaction.CommitAsync();

                _logger.LogInformation("Successfully added Local Driving License Application with LocalDrivingLicenseApplicationID: {LocalDrivingLicenseApplicationID}",
                    localApp.LocalDrivingLicenseApplicationID);
                return await GetByApplicationIdAsync(localApp.LocalDrivingLicenseApplicationID);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Conflict error while adding Local Driving License Application: PersonID: {PersonID}", createDto.PersonID);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<LocalDrivingLicenseApplicationDto?> UpdateLocalDrivingLicenseApplicationAsync(LocalDrivingLicenseApplicationUpdateDto localAppDto)
        {
            _logger.LogInformation(
                "Attempting to update Local Driving License Application with with LocalDrivingLicenseApplicationID: {LocalDrivingLicenseApplicationID}",
                localAppDto.LocalDrivingLicenseApplicationID);
            try
            {
                if (!await _unitOfWork.LicenseClasses.IsExistAsync(lc => lc.LicenseClassID == localAppDto.LicenseClassID))
                {
                    _logger.LogWarning("Validation failed: License Class with ID {LicenseClassID} does not exist.",
                        localAppDto.LicenseClassID);
                    throw new ResourceNotFoundException($"The specified License Class {localAppDto.LicenseClassID} does not exist.");
                }

                var localAppInDb = await _unitOfWork.LocalDrivingLicenseApplications.GetByIdAsync(localAppDto.LocalDrivingLicenseApplicationID);

                if (localAppInDb == null)
                    throw new ResourceNotFoundException($"Cannot update: Local Driving License Application with ID {localAppDto.LocalDrivingLicenseApplicationID} not found.");

                if (await _unitOfWork.TestAppointments.IsExistAsync(ta => ta.LocalDrivingLicenseApplicationID == localAppDto.LocalDrivingLicenseApplicationID))
                {
                    throw new ValidationException("The license category cannot be changed after booking test appointments.");
                }


                localAppInDb.LicenseClassID = localAppDto.LicenseClassID;

                _unitOfWork.LocalDrivingLicenseApplications.Update(localAppInDb);

                var result = await _unitOfWork.CompleteAsync();
                if (result > 0)
                {
                    _logger.LogInformation("Successfully updated Local Driving License Application with with LocalDrivingLicenseApplicationID: {LocalDrivingLicenseApplicationID}", localAppDto.LocalDrivingLicenseApplicationID);
                    return await GetByApplicationIdAsync(localAppDto.LocalDrivingLicenseApplicationID);
                }

                _logger.LogError("Failed to update Local Driving License Application record to the database forLocalDrivingLicenseApplicationID: {LocalDrivingLicenseApplicationID}", localAppDto.LocalDrivingLicenseApplicationID);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validation or Conflict error while updated LocalDrivingLicenseApplicationID: {LocalDrivingLicenseApplicationID}", localAppDto.LocalDrivingLicenseApplicationID);

                if (ex is ResourceNotFoundException) throw;

                _logger.LogCritical(ex, "Unexpected error updated Application with LocalDrivingLicenseApplicationID: {LocalDrivingLicenseApplicationID}", localAppDto.LocalDrivingLicenseApplicationID);
                throw new Exception("An error occurred while updating the Local Driving License Application record. " + ex.Message);
            }
        }

        public async Task<bool> DeleteLocalDrivingLicenseApplicationAsync(int id)
        {
            _logger.LogInformation("Attempting to delete LocalAppID: {Id}", id);

            if (id <= 0) throw new ValidationException("Invalid ID provided.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var localApp = await _unitOfWork.LocalDrivingLicenseApplications.GetByIdAsync(id);
                if (localApp == null)
                    throw new ResourceNotFoundException($"Local Application {id} not found.");

                _unitOfWork.LocalDrivingLicenseApplications.Delete(localApp);

                var baseApp = await _unitOfWork.Applications.GetByIdAsync(localApp.ApplicationID);
                if (baseApp != null)
                {
                    _unitOfWork.Applications.Delete(baseApp);
                }

                var result = await _unitOfWork.CompleteAsync() > 0;

                if (result)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Successfully deleted LocalAppID: {Id} and its Base Application.", id);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting LocalAppID: {Id}", id);

                if (ex is ResourceNotFoundException) throw;

                throw new ConflictException("The request cannot be deleted because it is linked to other records such as tests or appointments.");
            }
        }

        public async Task<int> GetActiveApplicationIdForLicenseClassAsync(int personID, int applicationTypeID, int licenseClassID)
        {
            _logger.LogInformation("Fetching Active Local Driving License Application with PersonID: {personID}, ApplicationTypeID: {applicationTypeID}, LicenseClassID: {licenseClassID}",
                personID, applicationTypeID, licenseClassID);

            if (personID <= 0 || applicationTypeID <= 0 || licenseClassID <= 0)
                throw new ValidationException("Invalid ID");

            var activeApp = await _unitOfWork.LocalDrivingLicenseApplications.
                GetActiveApplicationIdForLicenseClassAsync(personID, applicationTypeID, licenseClassID);
            if (activeApp <= 0)
            {
                _logger.LogWarning("No active Local Driving License Application found for PersonID: {personID}, ApplicationTypeID: {applicationTypeID}, LicenseClassID: {licenseClassID}",
                    personID, applicationTypeID, licenseClassID);
                return -1;
            }
            return activeApp;
        }

        #region Private Helper Methods
        private async Task ValidateLocalDrivingLicenseApplicationDataAsync(LocalDrivingLicenseApplicationCreateDto createDto)
        {
            if (await _unitOfWork.LocalDrivingLicenseApplications.GetActiveApplicationIdForLicenseClassAsync(
                createDto.PersonID, createDto.ApplicationTypeID, createDto.LicenseClassID) > 0)
                throw new ConflictException("This person already has an active application for this type of license.");

            if (!await _unitOfWork.LicenseClasses.IsExistAsync(lc => lc.LicenseClassID == createDto.LicenseClassID))
            {
                _logger.LogWarning("Validation failed: License Class with ID {LicenseClassID} does not exist.",
                    createDto.LicenseClassID);
                throw new ResourceNotFoundException($"The specified License Class {createDto.LicenseClassID} does not exist.");
            }
        }

        public async Task<byte> GetPassedTestCountAsync(int localAppID)
        {
            var passedTests = await _unitOfWork.Tests.FindAllAsync(
                predicate: t => t.TestAppointmentInfo.LocalDrivingLicenseApplicationID == localAppID && t.TestResult == true,
                includes: ["TestAppointmentInfo"],
                tracked: false
            );

            return (byte)passedTests.Count();
        }
        #endregion
    }
}