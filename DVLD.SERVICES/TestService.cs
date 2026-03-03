using AutoMapper;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Tests;
using DVLD.CORE.DTOs.TestTypes;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.CORE.Interfaces.Tests;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace DVLD.Services
{
    public class TestService : ITestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TestService> _logger;
        public TestService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TestService> _logger)
        {
            _unitOfWork = unitOfWork;
            this._mapper = mapper;
            this._logger = _logger;
        }

        public async Task<PagedResultDto<TestDto>> GetAllTestsAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching Tests with pagination - PageNumber: {PageNumber}, PageSize: {PageSize}, FilterColumn: {FilterColumn}, FilterValue: {FilterValue}",
           pageNumber, pageSize, filterColumn, filterValue);
            Expression<Func<Test, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                filter = filterColumn.ToLower() switch
                {
                    "testid" when int.TryParse(filterValue, out int id)
                        => p => p.TestID == id,

                    "testappointmentid" when int.TryParse(filterValue, out int appointmentId)
                        => p => p.TestAppointmentID == appointmentId,

                    "testresult" when bool.TryParse(filterValue, out bool locked)
                        => p => p.TestResult == locked,

                    _ => p => true
                };
            }

            int skip = (pageNumber - 1) * pageSize;

            var tests = await _unitOfWork.Tests.FindAllAsync(
                predicate: filter,
                includes: ["TestAppointmentInfo"],
                tracked: false,
                orderBy: t => t.TestID,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.Tests.CountAsync(filter);

            var mappedTests = _mapper.Map<IEnumerable<TestDto>>(tests);

            return new PagedResultDto<TestDto>
            {
                Data = mappedTests,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<TestDto?> GetTestByIdAsync(int id)
        {
            _logger.LogInformation("Fetching Test with TestID: {TestID}", id);

            if (id <= 0) throw new ValidationException("Invalid ID");

            var test = await _unitOfWork.Tests.FindAsync(
                t => t.TestID == id,
                includes: ["TestAppointmentInfo"],
                tracked: false);

            if (test == null)
            {
                _logger.LogWarning("Test with ID {Id} was not found.", id);
                throw new ResourceNotFoundException($"Test with ID {id} was not found.");
            }

            return _mapper.Map<TestDto>(test);
        }

        public async Task<TestDto?> GetLastTestPerPersonAndLicenseClassAsync(int personId, int licenseClassId, int testTypeID)
        {
            _logger.LogInformation("Fetching Last Test Per PersonID: {PersonID}, LicenseClassID: {LicenseClassID}, TestTypeID: {TestTypeID}", personId, licenseClassId, testTypeID);

            if (personId <= 0 || licenseClassId <= 0 || testTypeID <= 0)
                throw new ValidationException("Invalid ID");


            var tests = await _unitOfWork.Tests.FindAllAsync(
                predicate: t => t.TestAppointmentInfo.LocalAppInfo.ApplicationInfo.ApplicantPersonID == personId &&
                                t.TestAppointmentInfo.LocalAppInfo.LicenseClassID == licenseClassId &&
                                (int)t.TestAppointmentInfo.TestTypeID == testTypeID,
                includes: ["TestAppointmentInfo.LocalAppInfo.ApplicationInfo"],
                tracked: false,
                orderBy: t => t.TestAppointmentID,
                orderByDirection: EnOrderByDirection.Descending,
                take: 1
            );

            var lastTest = tests.FirstOrDefault();

            if (lastTest == null)
            {
                _logger.LogInformation("No previous test found for the specified criteria.");
                return null;
            }

            return _mapper.Map<TestDto>(lastTest);
        }

        public async Task<TestDto?> AddTestAsync(TestCreateDto testCreateDto, int currentUserId)
        {
            _logger.LogInformation("Attempting to add a new Test with TestAppointmentID: {TestAppointmentID}", testCreateDto.TestAppointmentID);

            if (await _unitOfWork.Tests.IsExistAsync(t => t.TestAppointmentID == testCreateDto.TestAppointmentID))
                throw new ConflictException($"A Test record for Test Appointment ID '{testCreateDto.TestAppointmentID}' already exists.");

            var test = _mapper.Map<Test>(testCreateDto);
            test.CreatedByUserID = currentUserId;

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.Tests.AddAsync(test);

                var appointment = await _unitOfWork.TestAppointments.GetByIdAsync(testCreateDto.TestAppointmentID);
                if (appointment == null) throw new ResourceNotFoundException("Test Appointment not found.");

                appointment.IsLocked = true;
                _unitOfWork.TestAppointments.Update(appointment);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully added Test with TestID: {TestID} and locked AppointmentID: {AppID}", test.TestID, appointment.TestAppointmentID);
                    return await GetTestByIdAsync(test.TestID);
                }

                _logger.LogError("Failed to save Test record to the database for TestAppointmentID: {TestAppointmentID}", testCreateDto.TestAppointmentID);
                return null;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogWarning(ex, "Error while adding Test: TestAppointmentID: {TestAppointmentID}", testCreateDto.TestAppointmentID);

                if (ex is ConflictException || ex is ResourceNotFoundException) throw;

                _logger.LogCritical(ex, "Unexpected error adding Test with TestAppointmentID: {TestAppointmentID}", testCreateDto.TestAppointmentID);
                throw new Exception("An error occurred while saving the Test record. " + ex.Message);
            }
        }

        public async Task<TestDto?> UpdateTestAsync(TestUpdateDto testUpdateDto)
        {
            _logger.LogInformation("Attempting to update Test record with ID: {TestID}", testUpdateDto.TestID);
            try
            {
                var test = await _unitOfWork.Tests.GetByIdAsync(testUpdateDto.TestID);

                if (test == null)
                {
                    _logger.LogWarning("Update failed: Test ID {TestID} not found.", testUpdateDto.TestID);
                    throw new ResourceNotFoundException($"Test with ID {testUpdateDto.TestID} was not found.");
                }

                test.TestResult = testUpdateDto.TestResult;
                test.Notes = testUpdateDto.Notes;

                _unitOfWork.Tests.Update(test);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully updated Test ID: {TestID}", testUpdateDto.TestID);
                    return await GetTestByIdAsync(test.TestID);
                }

                _logger.LogWarning("Update executed but no rows were affected for Test ID: {TestID}", testUpdateDto.TestID);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while updating Test ID: {TestID}", testUpdateDto.TestID);

                if (ex is ResourceNotFoundException) throw;

                _logger.LogCritical(ex, "Unexpected server error updating Test ID: {TestID}", testUpdateDto.TestID);
                throw new Exception("An error occurred while updating the Test record. " + ex.Message);
            }
        }

        public async Task<bool> PassedAllTestsAsync(int localAppID)
        {
            _logger.LogInformation("Fetching Passed Test Count for LocalAppID: {LocalAppID}", localAppID);

            if (localAppID <= 0) throw new ValidationException("Invalid ID");

            byte count = await GetPassedTestCountAsync( localAppID);

            _logger.LogInformation("Found {Count} passed tests for LocalAppID: {LocalAppID}", count, localAppID);

            return count == 3;
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

        public async Task<bool> HasPassedAsync(int localAppID, int testTypeID)
        {
            _logger.LogInformation("Checking if LocalAppID {LocalAppID} has passed TestType {TestTypeID}", localAppID, testTypeID);

            return await _unitOfWork.Tests.IsExistAsync(
                t => t.TestAppointmentInfo.LocalDrivingLicenseApplicationID == localAppID
                && t.TestAppointmentInfo.TestTypeID == (EnTestType)testTypeID   
                && t.TestResult == true,
                includes: ["TestAppointmentInfo"]);
        }

        public async Task<bool> RequiresRetakeAsync(int localAppID, int testTypeId)
        {
            return await _unitOfWork.Tests.IsExistAsync(t =>
                t.TestAppointmentInfo.LocalDrivingLicenseApplicationID == localAppID &&
                t.TestAppointmentInfo.TestTypeID == (EnTestType)testTypeId,
                includes: ["TestAppointmentInfo"]); 
        }

    }
}