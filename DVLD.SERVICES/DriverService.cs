using AutoMapper;
using DVLD.CORE.Constants;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Drivers;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace DVLD.Services
{
    public class DriverService : IDriverService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DriverService> _logger;

        public DriverService(IUnitOfWork unitOfWork, ILogger<DriverService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<DriverDto>> GetAllDriversAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching Drivers with pagination - PageNumber: {PageNumber}, PageSize: {PageSize}, FilterColumn: {FilterColumn}, FilterValue: {FilterValue}",
                   pageNumber, pageSize, filterColumn, filterValue);

            Expression<Func<Driver, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                filter = filterColumn.ToLower() switch
                {
                    "driverid" when int.TryParse(filterValue, out int id) => p => p.DriverID == id,
                    "personid" when int.TryParse(filterValue, out int id) => p => p.PersonID == id,
                    _ => p => true
                };
            }

            int skip = (pageNumber - 1) * pageSize;

            var drivers = await _unitOfWork.Drivers.FindAllAsync(
                predicate: filter,
                includes: ["PersonInfo"],
                tracked: false,
                orderBy: p => p.CreatedDate,
                orderByDirection:EnOrderByDirection.Descending,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.Drivers.CountAsync(filter);

            var mappedDrivers = _mapper.Map<IEnumerable<DriverDto>>(drivers);

            return new PagedResultDto<DriverDto>
            {
                Data = mappedDrivers,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<DriverDto> GetDriverByIdAsync(int id)
        {
            _logger.LogInformation("Fetching Driver with DriverID: {DriverID}", id);
            if (id <= 0) throw new ValidationException("Invalid ID");

            var driver = await _unitOfWork.Drivers.FindAsync(d => d.DriverID == id, ["PersonInfo"]);

            if (driver == null)
            {
                _logger.LogWarning("Driver with ID {Id} was not found.", id);
                throw new ResourceNotFoundException($"Driver with ID {id} was not found.");
            }
            return _mapper.Map<DriverDto>(driver);
        }

        public async Task<DriverDto?> GetDriverByPersonIdAsync(int personId)
        {
            _logger.LogInformation("Fetching Driver with PersonID: {PersonID}", personId);
            if (personId <= 0) throw new ValidationException("Invalid ID");

            var driver = await _unitOfWork.Drivers.FindAsync(d => d.PersonID == personId, ["PersonInfo"]);

            if (driver == null)
            {
                _logger.LogWarning("Driver with PersonID {PersonID} was not found.", personId);
                throw new ResourceNotFoundException($"Driver with PersonID {personId} was not found.");
            }
            return _mapper.Map<DriverDto>(driver);
        }

        public async Task<DriverDto?> AddDriverAsync(int personId, int currentUserId, string currentUserRole)
        {
            _logger.LogInformation("Attempting to add a new Driver with PersonID: {PersonID}", personId);
            try
            {
                if (currentUserRole != UserRoles.Admin && currentUserRole != UserRoles.User)
                {
                    _logger.LogWarning("Unauthorized: User with role {Role} tried to add a driver.", currentUserRole);
                    throw new UnauthorizedAccessException("You do not have permission to add drivers to the system.");
                }


                if (await IsDriverExistByPersonIdAsync(personId))
                    throw new ConflictException($"The Driver PersonID '{personId}' is already registered for another Driver.");


                var driver = new Driver
                {
                    PersonID = personId,
                    CreatedByUserID = currentUserId,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Drivers.AddAsync(driver);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully added Driver with DriverID: {DriverID}", driver.DriverID);
                    return await GetDriverByIdAsync(driver.DriverID);
                }

                _logger.LogError("Failed to save Driver record to the database for PersonID: {PersonID}", personId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Conflict error while adding Driver: PersonID: {PersonID}", personId);

                if (ex is ConflictException || ex is UnauthorizedAccessException) throw;

                _logger.LogCritical(ex, "Unexpected error adding Driver with PersonID: {PersonID}", personId);
                throw new Exception("An error occurred while saving the Driver record. " + ex.Message);
            }
        }

        public async Task<bool> IsDriverExistByPersonIdAsync(int personId)
        {
            return await _unitOfWork.Drivers.IsExistAsync(d => d.PersonID == personId);
        }

    }
}