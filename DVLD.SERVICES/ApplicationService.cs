using AutoMapper;
using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using Microsoft.Extensions.Logging;

namespace DVLD.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ApplicationService> _logger;

        public ApplicationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ApplicationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ApplicationDto> GetByIdAsync(int applicationID)
        {
            _logger.LogInformation("Fetching Application with ApplicationID: {ApplicationID}", applicationID);

            if (applicationID <= 0) throw new ValidationException("Invalid ID");

            var application = await _unitOfWork.Applications.FindAsync(
                a => a.ApplicationID == applicationID,
                includes: ["PersonInfo", "ApplicationTypeInfo", "CreatedByUserInfo"],
                tracked: false);

            if (application == null)
            {
                _logger.LogWarning("Application with ID {Id} was not found.", applicationID);
                throw new ResourceNotFoundException($"Application with ID {applicationID} was not found.");
            }
            return _mapper.Map<ApplicationDto>(application);
        }

        public async Task<int> GetActiveApplicationIdAsync(int personID, int applicationTypeID)
        {
            _logger.LogInformation("Fetching Active Application with PersonID: {personID} And ApplicationTypeID {applicationTypeID}",
                personID, applicationTypeID);

            if (personID <= 0 || applicationTypeID <= 0) throw new ValidationException("Invalid ID");

            var applicationID = await _unitOfWork.Applications.GetActiveApplicationIdAsync(personID, applicationTypeID);

            if (applicationID <= 0)
            {
                _logger.LogWarning("Application with PersonID: {personID} And ApplicationTypeID {applicationTypeID} was not found.",
                    personID, applicationTypeID);
                throw new ResourceNotFoundException($"Application with PersonID: {personID} And ApplicationTypeID {applicationTypeID} was not found");
            }
            return applicationID;
        }

        public async Task<int> GetActiveApplicationIdForLicenseClassAsync(int personID, int applicationTypeID, int licenseClassID)
        {
            _logger.LogInformation("Checking for active application - PersonID: {personID}, Type: {applicationTypeID}, Class: {licenseClassID}",
                personID, applicationTypeID, licenseClassID);

            if (personID <= 0 || applicationTypeID <= 0 || licenseClassID <= 0)
                throw new ValidationException("Invalid ID");

            var applicationID = await _unitOfWork.Applications.GetActiveApplicationIdForLicenseClassAsync(personID, applicationTypeID, licenseClassID);

            if (applicationID > 0)
            {
                _logger.LogInformation("Active application found with ID: {applicationID}", applicationID);
            }
            else
            {
                _logger.LogInformation("No active application found for PersonID: {personID} in this license class.", personID);
            }

            return applicationID; 
        }

        public async Task<ApplicationDto?> AddApplicationAsync(ApplicationCreateDto applicationDto, int currentUserId)
        {
            _logger.LogInformation("Attempting to add a new Application with ApplicantPersonID: {ApplicantPersonID}",
                applicationDto.ApplicantPersonID);
            try
            {
                if (await _unitOfWork.Applications.DoesPersonHaveActiveApplicationAsync(applicationDto.ApplicantPersonID,
                    applicationDto.ApplicationTypeID))
                {
                    _logger.LogWarning("Validation failed: Person {PersonID} already has an active application for type {TypeID}",
                        applicationDto.ApplicantPersonID, applicationDto.ApplicationTypeID);
                    throw new ConflictException("This person already has an active application of this type.");
                }

                var appType = await _unitOfWork.ApplicationTypes.GetByIdAsync(applicationDto.ApplicationTypeID);
                if (appType == null) throw new ResourceNotFoundException("Application Type not found");


                var application = _mapper.Map<Application>(applicationDto);

                application.PaidFees = appType.Fees;
                application.ApplicationDate = DateTime.UtcNow;
                application.LastStatusDate = DateTime.UtcNow;
                application.ApplicationStatus = EnApplicationStatus.New;
                application.CreatedByUserID = currentUserId;

                await _unitOfWork.Applications.AddAsync(application);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully added Application with ApplicationID: {ApplicationID}", application.ApplicationID);
                    return await GetByIdAsync(application.ApplicationID);
                }

                _logger.LogError("Failed to save Application record to the database for ApplicantPersonID: {ApplicantPersonID}", applicationDto.ApplicantPersonID);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Conflict error while adding Application with ApplicantPersonID: {ApplicantPersonID}", applicationDto.ApplicantPersonID);

                if (ex is ConflictException || ex is ResourceNotFoundException) throw;

                _logger.LogCritical(ex, "Unexpected error adding Application with {ApplicantPersonID}", applicationDto.ApplicantPersonID);
                throw new Exception("An error occurred while saving the Application record. " + ex.Message);
            }
        }

        public async Task<ApplicationDto?> UpdateStatusAsync(ApplicationUpdateDto applicationDto)
        {
            _logger.LogInformation("Attempting to update Application with ApplicationID: {ApplicationID}", applicationDto.ApplicationID);
            try
            {
                var applicationInDb = await _unitOfWork.Applications.GetByIdAsync(applicationDto.ApplicationID);

                if (applicationInDb == null)
                    throw new ResourceNotFoundException($"Cannot update: Application with ID {applicationDto.ApplicationID} not found.");

                _mapper.Map(applicationDto, applicationInDb);

                applicationInDb.LastStatusDate = DateTime.UtcNow;

                _unitOfWork.Applications.Update(applicationInDb);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully updateed Application with ApplicationID: {ApplicationID}", applicationInDb.ApplicationID);
                    return await GetByIdAsync(applicationDto.ApplicationID);
                }

                _logger.LogError("Failed to update Application record to the database for ApplicationID: {ApplicationID}", applicationInDb.ApplicationID);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validation or Conflict error while updated ApplicationID: {ApplicationID}", applicationDto.ApplicationID);

                if (ex is ResourceNotFoundException) throw;

                _logger.LogCritical(ex, "Unexpected error updated Application with ApplicationID {ApplicationID}", applicationDto.ApplicationID);
                throw new Exception("An error occurred while updating the Application record. " + ex.Message);
            }
        }

        public async Task<bool> DeleteApplicationAsync(int applicationID)
        {
            _logger.LogInformation("Attempting to delete Application with ApplicationID: {applicationID}", applicationID);

            if (applicationID <= 0)
                throw new ValidationException("Invalid ApplicationID provided for deletion.");

            try
            {
                var application = await _unitOfWork.Applications.GetByIdAsync(applicationID);

                if (application == null)
                    throw new ResourceNotFoundException($"Application with ApplicationID {applicationID} not found.");

                _unitOfWork.Applications.Delete(application);

                var result = await _unitOfWork.CompleteAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Successfully deleted Application with ApplicationID: {applicationID}", applicationID);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Application with ApplicationID: {applicationID}", applicationID);

                if (ex is ResourceNotFoundException || ex is ValidationException) throw;
                throw new ConflictException("Cannot delete this Application because they are linked to other records (like Licenses ...).");
            }
        }

        public async Task<bool> DoesPersonHaveActiveApplicationAsync(int personID, int applicationTypeID)
        {
            return await _unitOfWork.Applications.DoesPersonHaveActiveApplicationAsync(personID, applicationTypeID);
        }

        public async Task<bool> IsApplicationExistAsync(int applicationID)
        {
            return await _unitOfWork.Applications.IsExistAsync(u => u.ApplicationID == applicationID);
        }
    }
}