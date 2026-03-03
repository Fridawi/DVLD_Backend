using AutoMapper;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.DTOs.TestAppointments;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.CORE.Interfaces.Tests;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace DVLD.Services
{
    public class TestAppointmentService : ITestAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITestService _testService;
        private readonly IMapper _mapper;
        private readonly ILogger<TestAppointmentService> _logger;
        public TestAppointmentService(IUnitOfWork unitOfWork, ITestService testService, ILogger<TestAppointmentService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _testService = testService;
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<PagedResultDto<TestAppointmentDto>> GetAllTestAppointmentsAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching Test Appointments with pagination - PageNumber: {PageNumber}, PageSize: {PageSize}, FilterColumn: {FilterColumn}, FilterValue: {FilterValue}",
                 pageNumber, pageSize, filterColumn, filterValue);

            Expression<Func<TestAppointment, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                filter = filterColumn.ToLower() switch
                {
                    "testappointmentid" when int.TryParse(filterValue, out int id) => p => p.TestAppointmentID == id,

                    "localdrivinglicenseapplicationid" when int.TryParse(filterValue, out int id) => p => p.LocalDrivingLicenseApplicationID == id,

                    "fullname" => p => p.LocalAppInfo.ApplicationInfo.PersonInfo.FullName.Contains(filterValue),

                    "testtypename" => GetTestTypeFilter(filterValue),

                    "islocked" when bool.TryParse(filterValue, out bool locked) => p => p.IsLocked == locked,

                    _ => p => true
                };
            }

            int skip = (pageNumber - 1) * pageSize;

            var testAppointments = await _unitOfWork.TestAppointments.FindAllAsync(
                predicate: filter,
                includes: [
                    "LocalAppInfo.LicenseClassInfo",
                    "LocalAppInfo.ApplicationInfo.PersonInfo",
                    "TestRecord"
                ],
                tracked: false,
                orderBy: ta => ta.AppointmentDate,
                orderByDirection: EnOrderByDirection.Descending,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.TestAppointments.CountAsync(filter);
            var mappedTestAppointments = _mapper.Map<IEnumerable<TestAppointmentDto>>(testAppointments);

            return new PagedResultDto<TestAppointmentDto>
            {
                Data = mappedTestAppointments,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultDto<TestAppointmentDto>> GetApplicationTestAppointmentsPerTestTypeAsync(
            int localAppID,
            int testTypeID,
            int pageNumber,
            int pageSize,
            string? filterColumn = null,
            string? filterValue = null)
        {
            _logger.LogInformation("Fetching Test Appointments for LocalAppID: {LocalAppID}, TestType: {TestTypeID}", localAppID, testTypeID);

            if (localAppID <= 0 || testTypeID <= 0) throw new ValidationException("Invalid ID");

            Expression<Func<TestAppointment, bool>> filter = ta =>
                ta.LocalDrivingLicenseApplicationID == localAppID &&
                ta.TestTypeID == (EnTestType)testTypeID;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                if (filterColumn.ToLower() == "islocked" && bool.TryParse(filterValue, out bool locked))
                {
                    filter = ta => ta.LocalDrivingLicenseApplicationID == localAppID &&
                                   ta.TestTypeID == (EnTestType)testTypeID &&
                                   ta.IsLocked == locked;
                }
                else if (filterColumn.ToLower() == "testappointmentid" && int.TryParse(filterValue, out int id))
                {
                    filter = ta => ta.LocalDrivingLicenseApplicationID == localAppID &&
                                   ta.TestTypeID == (EnTestType)testTypeID &&
                                   ta.TestAppointmentID == id;
                }
            }

            int skip = (pageNumber - 1) * pageSize;

            var testAppointments = await _unitOfWork.TestAppointments.FindAllAsync(
                predicate: filter,
                includes: [
                    "LocalAppInfo.LicenseClassInfo",
                    "LocalAppInfo.ApplicationInfo.PersonInfo",
                    "TestRecord"
                ],
                tracked: false,
                orderBy: ta => ta.AppointmentDate,
                orderByDirection: EnOrderByDirection.Descending,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.TestAppointments.CountAsync(filter);

            var mappedData = _mapper.Map<IEnumerable<TestAppointmentDto>>(testAppointments);

            return new PagedResultDto<TestAppointmentDto>
            {
                Data = mappedData,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        public async Task<TestAppointmentDto?> GetTestAppointmentByIdAsync(int id)
        {
            _logger.LogInformation("Fetching TestAppointment with TestAppointmentID: {TestAppointmentID}", id);

            if (id <= 0) throw new ValidationException("Invalid ID");

            var testAppointment = await _unitOfWork.TestAppointments.FindAsync(ta => ta.TestAppointmentID == id,
                includes: [
                    "LocalAppInfo.LicenseClassInfo",
                    "LocalAppInfo.ApplicationInfo.PersonInfo",
                    "TestRecord"
                ],
                tracked: false);

            if (testAppointment == null)
            {
                _logger.LogWarning("Test Appointment with ID {Id} was not found.", id);
                throw new ResourceNotFoundException($"Test Appointment with ID {id} was not found.");
            }

            return _mapper.Map<TestAppointmentDto>(testAppointment);
        }

        public async Task<TestAppointmentDto?> GetLastTestAppointmentAsync(int localAppID, int testTypeID)
        {
            _logger.LogInformation("Fetching last TestAppointment for LocalDrivingLicenseApplicationID: {LocalAppID} and TestTypeID: {TestTypeID}", localAppID, testTypeID);

            if (localAppID <= 0 || testTypeID <= 0) throw new ValidationException("Invalid ID");

            var testAppointment = await _unitOfWork.TestAppointments.FindAllAsync(
                    predicate: ta => ta.LocalDrivingLicenseApplicationID == localAppID && ta.TestTypeID == (EnTestType)testTypeID,
                    includes: [
                    "LocalAppInfo.LicenseClassInfo",
                    "LocalAppInfo.ApplicationInfo.PersonInfo",
                    "TestRecord"
                    ],
                    tracked: false,
                    orderBy: ta => ta.AppointmentDate,
                    orderByDirection: EnOrderByDirection.Descending,
                    take: 1
                );

            var lastAppointment = testAppointment.FirstOrDefault();

            if (lastAppointment == null)
            {
                _logger.LogInformation("No previous appointments found for LocalAppID: {LocalAppID} and TestTypeID: {TestTypeID}", localAppID, testTypeID);
                return null;
            }

            return _mapper.Map<TestAppointmentDto>(lastAppointment);
        }

        public async Task<TestAppointmentDto?> AddTestAppointmentAsync(TestAppointmentCreateDto testAppointmentCreateDto, int currentUserId)
        {
            _logger.LogInformation("Attempting to add a new Test Appointment for LocalAppID: {LocalAppID}", testAppointmentCreateDto.LocalDrivingLicenseApplicationID);

            await ValidateTestAppointmentAsync(testAppointmentCreateDto);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                int? retakeApplicationId = null;

                var totalTrials = await _testService.RequiresRetakeAsync(testAppointmentCreateDto.LocalDrivingLicenseApplicationID, testAppointmentCreateDto.TestTypeID);

                if (totalTrials)
                {
                    _logger.LogInformation("Retake detected. Handling Retake Application Creation.");

                    var localApp = await _unitOfWork.LocalDrivingLicenseApplications.FindAsync(
                                            l => l.LocalDrivingLicenseApplicationID == testAppointmentCreateDto.LocalDrivingLicenseApplicationID, includes: ["ApplicationInfo"]);

                    var retakeAppType = await _unitOfWork.ApplicationTypes.GetByIdAsync((int)EnApplicationType.RetakeTest);
                    float retakeFees = retakeAppType?.Fees ?? 5;

                    var retakeApp = new Application
                    {
                        ApplicantPersonID = localApp!.ApplicationInfo.ApplicantPersonID,
                        ApplicationDate = DateTime.UtcNow,
                        ApplicationTypeID = (int)EnApplicationType.RetakeTest,
                        ApplicationStatus = EnApplicationStatus.Completed,
                        LastStatusDate = DateTime.UtcNow,
                        PaidFees = retakeFees,
                        CreatedByUserID = currentUserId
                    };

                    await _unitOfWork.Applications.AddAsync(retakeApp);
                    var appResult = await _unitOfWork.CompleteAsync();

                    if (appResult <= 0)
                    {
                        await transaction.RollbackAsync();
                        return null;
                    }

                    retakeApplicationId = retakeApp.ApplicationID;
                }

                var testAppointment = _mapper.Map<TestAppointment>(testAppointmentCreateDto);

                testAppointment.CreatedByUserID = currentUserId;
                testAppointment.IsLocked = false;
                testAppointment.RetakeTestApplicationID = retakeApplicationId;

                await _unitOfWork.TestAppointments.AddAsync(testAppointment);
                var appointmentResult = await _unitOfWork.CompleteAsync();

                if (appointmentResult <= 0)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                await transaction.CommitAsync();

                return await GetTestAppointmentByIdAsync(testAppointment.TestAppointmentID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding Test Appointment with Transaction.");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<TestAppointmentDto?> UpdateTestAppointmentAsync(TestAppointmentUpdateDto updateDto)
        {
            _logger.LogInformation("Attempting to update appointment date for ID: {ID}", updateDto.TestAppointmentID);
            try
            {
                var appointment = await _unitOfWork.TestAppointments.GetByIdAsync(updateDto.TestAppointmentID);

                if (appointment == null)
                {
                    _logger.LogWarning("Update failed: Appointment ID {ID} not found.", updateDto.TestAppointmentID);
                    throw new ResourceNotFoundException($"Appointment with ID {updateDto.TestAppointmentID} was not found.");
                }

                if (appointment.IsLocked)
                {
                    _logger.LogWarning("Update forbidden: Appointment ID {ID} is locked.", updateDto.TestAppointmentID);
                    throw new InvalidOperationException("This appointment is locked and cannot be updated (the test has already been conducted).");
                }

                if (updateDto.AppointmentDate < DateTime.Now)
                {
                    throw new InvalidOperationException("Cannot set an appointment date in the past.");
                }

                appointment.AppointmentDate = updateDto.AppointmentDate;

                _unitOfWork.TestAppointments.Update(appointment);
                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully updated Appointment ID: {ID}", updateDto.TestAppointmentID);
                    return await GetTestAppointmentByIdAsync(appointment.TestAppointmentID);
                }

                _logger.LogWarning("No changes were saved for Appointment ID: {ID}", updateDto.TestAppointmentID);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while updating Test Appointment ID: {ID}", updateDto.TestAppointmentID);

                if (ex is ResourceNotFoundException || ex is InvalidOperationException) throw;

                _logger.LogCritical(ex, "Unexpected server error updating Test Appointment ID: {ID}", updateDto.TestAppointmentID);
                throw new Exception("An error occurred while updating the Test Appointment record. " + ex.Message);
            }
        }

        public async Task<bool> HasActiveAppointmentAsync(int localAppID, int testTypeID)
        {
            _logger.LogInformation("Checking for active appointment for LocalDrivingLicenseApplicationID: {LocalAppID} and TestTypeID: {TestTypeID}", localAppID, testTypeID);

            if (localAppID <= 0 || testTypeID <= 0) throw new ValidationException("Invalid ID");

            return await _unitOfWork.TestAppointments.IsExistAsync(
                ta => ta.LocalDrivingLicenseApplicationID == localAppID &&
                      ta.TestTypeID == (EnTestType)testTypeID &&
                      !ta.IsLocked
            );
        }

        public async Task<bool> HasAnyAppointmentsAsync(int localAppID)
        {
            return await _unitOfWork.TestAppointments.IsExistAsync(ta => ta.LocalDrivingLicenseApplicationID == localAppID);
        }

        #region Private Helper Methods
        private async Task ValidateTestAppointmentAsync(TestAppointmentCreateDto testAppointmentCreateDto)
        {
            if (await HasActiveAppointmentAsync(testAppointmentCreateDto.LocalDrivingLicenseApplicationID, testAppointmentCreateDto.TestTypeID))
                throw new InvalidOperationException("The applicant already has an active appointment for this test.");

            if (await _testService.HasPassedAsync(testAppointmentCreateDto.LocalDrivingLicenseApplicationID, testAppointmentCreateDto.TestTypeID))
                throw new InvalidOperationException("Applicant already passed this test.");

            if (testAppointmentCreateDto.TestTypeID == (int)EnTestType.WrittenTest)
            {
                if (!await _testService.HasPassedAsync(testAppointmentCreateDto.LocalDrivingLicenseApplicationID, (int)EnTestType.VisionTest))
                    throw new InvalidOperationException("Cannot schedule Written Test before passing Vision Test.");
            }
            else if (testAppointmentCreateDto.TestTypeID == (int)EnTestType.StreetTest)
            {
                if (!await _testService.HasPassedAsync(testAppointmentCreateDto.LocalDrivingLicenseApplicationID, (int)EnTestType.WrittenTest))
                    throw new InvalidOperationException("Cannot schedule Street Test before passing Written Test.");
            }
        }

        private Expression<Func<TestAppointment, bool>> GetTestTypeFilter(string filterValue)
        {
            if (Enum.TryParse(typeof(EnTestType), filterValue, true, out var testType))
            {
                var typeValue = (EnTestType)testType;
                return p => p.TestTypeID == typeValue;
            }

            return p => true;
        }
        #endregion
    }
}