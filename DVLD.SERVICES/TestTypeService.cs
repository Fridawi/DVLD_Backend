using AutoMapper;
using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.TestTypes;
using DVLD.CORE.Entities;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using DVLD.CORE.Interfaces.Tests;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using TestType = DVLD.CORE.Entities.TestType;

namespace DVLD.Services
{
    public class TestTypeService : ITestTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TestTypeService> _logger;

        public TestTypeService(IUnitOfWork unitOfWork, ILogger<TestTypeService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<TestTypeDto>> GetAllTestTypesAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching Test Types with pagination - PageNumber: {PageNumber}, PageSize: {PageSize}, FilterColumn: {FilterColumn}, FilterValue: {FilterValue}",
                    pageNumber, pageSize, filterColumn, filterValue);
            Expression<Func<TestType, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                filter = filterColumn.ToLower() switch
                {
                    "testtypeid" when int.TryParse(filterValue, out int id) => p => p.TestTypeID == id,
                    "title" => p => p.Title.Contains(filterValue),
                    _ => p => true
                };
            }

            int skip = (pageNumber - 1) * pageSize;

            var testTypes = await _unitOfWork.TestTypes.FindAllAsync(
                predicate: filter,
                includes: null,
                tracked: false,
                orderBy: p => p.Title,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.TestTypes.CountAsync(filter);

            var mappedTestTypes = _mapper.Map<IEnumerable<TestTypeDto>>(testTypes);

            return new PagedResultDto<TestTypeDto>
            {
                Data = mappedTestTypes,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

        }

        public async Task<TestTypeDto> GetTestTypeByIdAsync(int id)
        {
            _logger.LogInformation("Fetching TestType with TestTypeID: {TestTypeID}", id);
            if (id <= 0) throw new ValidationException("Invalid ID");

            var testType = await _unitOfWork.TestTypes.FindAsync(tt => tt.TestTypeID == id);

            if (testType == null)
            {
                _logger.LogWarning("TestType with ID {Id} was not found.", id);
                throw new ResourceNotFoundException($"TestType with ID {id} was not found.");
            }
            return _mapper.Map<TestTypeDto>(testType);
        }

        public async Task<TestTypeDto?> AddTestTypeAsync(TestTypeDto testTypeDto)
        {
            _logger.LogInformation("Attempting to add a new Test Type with Title: {Title}", testTypeDto.Title);
            try
            {
                await ValidateTestTypeDataAsync(testTypeDto);

                var testType = _mapper.Map<TestType>(testTypeDto);

                await _unitOfWork.TestTypes.AddAsync(testType);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully added Test Type with TestTypeID: {TestTypeID}", testType.TestTypeID);
                    testTypeDto.TestTypeID = testType.TestTypeID;
                    return testTypeDto;
                }

                _logger.LogError("Failed to save Test Type record to the database for Title: {Title}", testType.Title);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Conflict error while adding Test Type: {Title}", testTypeDto.Title);

                if (ex is ConflictException) throw;

                _logger.LogCritical(ex, "Unexpected error adding Test Type with {Title}", testTypeDto.Title);
                throw new Exception("An error occurred while saving the Test Type record. " + ex.Message);
            }
        }

        public async Task<TestTypeDto?> UpdateTestTypeAsync(TestTypeDto testTypeDto)
        {
            _logger.LogInformation("Attempting to update a new Test Type with Title: {Title}", testTypeDto.Title);
            try
            {
                var testTypeInDb = await _unitOfWork.TestTypes.FindAsync(tt => tt.TestTypeID == testTypeDto.TestTypeID);
                if (testTypeInDb == null)
                    throw new ResourceNotFoundException($"Cannot update: TestType with ID {testTypeDto.TestTypeID} not found.");

                await ValidateTestTypeDataAsync(testTypeDto, isUpdate: true);

                _mapper.Map(testTypeDto, testTypeInDb);

                _unitOfWork.TestTypes.Update(testTypeInDb);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully updateed Test Type with TestTypeID: {TestTypeID}", testTypeInDb.TestTypeID);
                    return testTypeDto;
                }

                _logger.LogError("Failed to update Test Type record to the database for Title: {Title}", testTypeInDb.Title);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validation or Conflict error while updated TestTypeID: {TestTypeID}", testTypeDto.TestTypeID);

                if (ex is ResourceNotFoundException || ex is ConflictException) throw;

                _logger.LogCritical(ex, "Unexpected error updated Test Type with TestTypeID {TestTypeID}", testTypeDto.TestTypeID);
                throw new Exception("An error occurred while updating the Test Type record. " + ex.Message);
            }
        }

        #region Private Helper Methods
        private async Task ValidateTestTypeDataAsync(TestTypeDto testType, bool isUpdate = false)
        {
            if (await _unitOfWork.TestTypes.IsExistAsync(tt => tt.Title == testType.Title && (!isUpdate || tt.TestTypeID != testType.TestTypeID)))
                throw new ConflictException($"The Test Type Title '{testType.Title}' is already registered for another Test Type.");
        }
        #endregion
    }
}