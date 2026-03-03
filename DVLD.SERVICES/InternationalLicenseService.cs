using AutoMapper;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.DTOs.Licenses.InternationalLicenses;
using DVLD.CORE.DTOs.People;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Exceptions;
using DVLD.CORE.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace DVLD.Services
{
    public class InternationalLicenseService : IInternationalLicenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<InternationalLicenseService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public InternationalLicenseService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<InternationalLicenseService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PagedResultDto<InternationalLicenseDto>> GetAllInternationalLicensesAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching All International Licenses - Page: {PageNumber}, Filter: {FilterColumn}", pageNumber, filterColumn);

            Expression<Func<InternationalLicense, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                filter = filterColumn.ToLower() switch
                {
                    "internationallicenseid" when int.TryParse(filterValue, out int id) => p => p.InternationalLicenseID == id,
                    "applicationid" when int.TryParse(filterValue, out int id) => p => p.ApplicationID == id,
                    "driverid" when int.TryParse(filterValue, out int id) => p => p.DriverID == id,
                    "issuedusinglocallicenseid" when int.TryParse(filterValue, out int id) => p => p.IssuedUsingLocalLicenseID == id,
                    "isactive" when bool.TryParse(filterValue, out bool active) => p => p.IsActive == active,
                    _ => p => true
                };
            }

            int skip = (pageNumber - 1) * pageSize;

            var licenses = await _unitOfWork.InternationalLicenses.FindAllAsync(
                predicate: filter,
                includes: null,
                tracked: false,
                orderBy: l => l.ExpirationDate,
                orderByDirection: EnOrderByDirection.Descending,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.InternationalLicenses.CountAsync(filter);
            var mappedLicenses = _mapper.Map<IEnumerable<InternationalLicenseDto>>(licenses);

            return new PagedResultDto<InternationalLicenseDto>
            {
                Data = mappedLicenses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }


        public async Task<PagedResultDto<InternationalLicenseDto>> GetInternationalLicensesByDriverIdAsync(int driverId, int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null)
        {
            _logger.LogInformation("Fetching International Licenses for Driver ID: {DriverID}...", driverId);

            if (driverId <= 0) throw new ValidationException("Invalid Driver ID");

            Expression<Func<InternationalLicense, bool>> finalFilter = p => p.DriverID == driverId;

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                Expression<Func<InternationalLicense, bool>> additionalFilter = filterColumn.ToLower() switch
                {
                    "internationallicenseid" when int.TryParse(filterValue, out int id) => p => p.InternationalLicenseID == id,
                    "applicationid" when int.TryParse(filterValue, out int id) => p => p.ApplicationID == id,
                    "issuedusinglocallicenseid" when int.TryParse(filterValue, out int id) => p => p.IssuedUsingLocalLicenseID == id,
                    "isactive" when bool.TryParse(filterValue, out bool active) => p => p.IsActive == active,
                    _ => p => true 
                };

                if (additionalFilter != null)
                {
                    finalFilter = filterColumn.ToLower() switch
                    {
                        "internationallicenseid" when int.TryParse(filterValue, out int id) => p => p.DriverID == driverId && p.InternationalLicenseID == id,
                        "applicationid" when int.TryParse(filterValue, out int id) => p => p.DriverID == driverId && p.ApplicationID == id,
                        "isactive" when bool.TryParse(filterValue, out bool active) => p => p.DriverID == driverId && p.IsActive == active,
                        _ => p => p.DriverID == driverId
                    };
                }
            }

            int skip = (pageNumber - 1) * pageSize;

            var licenses = await _unitOfWork.InternationalLicenses.FindAllAsync(
                predicate: finalFilter,
                includes: null,
                tracked: false,
                orderBy: l => l.ExpirationDate,
                orderByDirection: EnOrderByDirection.Descending,
                skip: skip,
                take: pageSize
            );

            int totalCount = await _unitOfWork.InternationalLicenses.CountAsync(finalFilter);
            var mappedLicenses = _mapper.Map<IEnumerable<InternationalLicenseDto>>(licenses);

            return new PagedResultDto<InternationalLicenseDto>
            {
                Data = mappedLicenses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }


        public async Task<InternationalLicenseDto?> GetInternationalLicenseByIdAsync(int internationalLicenseId)
        {
            _logger.LogInformation("Fetching International License with ID: {ID}", internationalLicenseId);
            if (internationalLicenseId <= 0) throw new ValidationException("Invalid ID");
            var license = await _unitOfWork.InternationalLicenses.GetByIdAsync(internationalLicenseId);
            if (license == null)
                throw new ResourceNotFoundException($"International License with ID {internationalLicenseId} not found.");

            return _mapper.Map<InternationalLicenseDto>(license);
        }

        public async Task<DriverInternationalLicenseDto?> GetDriverInternationalLicenseByIdAsync(int internationalLicenseId)
        {
            _logger.LogInformation("Fetching Detailed International License with ID: {ID}", internationalLicenseId);
            if (internationalLicenseId <= 0) throw new ValidationException("Invalid ID");
            var license = await _unitOfWork.InternationalLicenses.FindAsync(
                il => il.InternationalLicenseID == internationalLicenseId,
                includes: ["DriverInfo.PersonInfo", "LocalLicenseInfo.LicenseClassInfo", "ApplicationInfo", "CreatedByUserInfo"],
                tracked: false);

            if (license == null)
                throw new ResourceNotFoundException($"International License with ID {internationalLicenseId} not found.");

            return _mapper.Map<DriverInternationalLicenseDto>(license, opt => opt.Items["BaseUrl"] = GetBaseUrl());
        }

        public async Task<InternationalLicenseDto?> IssueInternationalLicenseAsync(InternationalLicenseCreateDto createDto, int createdByUserId)
        {
            _logger.LogInformation("Attempting to issue International License for Local License ID: {LocalID}", createDto.LocalLicenseID);

            await IsDriverEligibleForInternationalLicenseAsync(createDto.LocalLicenseID);

            var localLicense = await _unitOfWork.Licenses.FindAsync(l=>l.LicenseID == createDto.LocalLicenseID, ["DriverInfo"],false);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var app = new Application
                {
                    ApplicantPersonID = localLicense!.DriverInfo.PersonID,
                    ApplicationDate = DateTime.UtcNow,
                    ApplicationTypeID = (int)EnApplicationType.NewInternationalLicense, 
                    ApplicationStatus = EnApplicationStatus.Completed, 
                    LastStatusDate = DateTime.UtcNow,
                    PaidFees = (await _unitOfWork.ApplicationTypes.GetByIdAsync((int)EnApplicationType.NewInternationalLicense))!.Fees, 
                    CreatedByUserID = createdByUserId
                };

                await _unitOfWork.Applications.AddAsync(app);
                await _unitOfWork.CompleteAsync();

                var activeLicenses = await _unitOfWork.InternationalLicenses.FindAllAsync(il => il.DriverID == localLicense.DriverID && il.IsActive);
                foreach (var oldLicense in activeLicenses)
                {
                    oldLicense.IsActive = false;
                }

                var intLicense = new InternationalLicense
                {
                    ApplicationID = app.ApplicationID,
                    DriverID = localLicense.DriverID,
                    IssuedUsingLocalLicenseID = createDto.LocalLicenseID,
                    IssueDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddYears(1),
                    IsActive = true,
                    CreatedByUserID = createdByUserId
                };

                await _unitOfWork.InternationalLicenses.AddAsync(intLicense);
                await _unitOfWork.CompleteAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("International License issued successfully with ID: {ID}", intLicense.InternationalLicenseID);
                return _mapper.Map<InternationalLicenseDto>(intLicense);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to issue International License.");
                throw new Exception("An error occurred while issuing the international license." + ex.Message);
            }
        }

        public async Task<int?> GetActiveInternationalLicenseIdByDriverIdAsync(int driverId)
        {
            _logger.LogInformation("Checking for active International License for Driver ID: {DriverID}", driverId);

            if (driverId <= 0) throw new ValidationException("Invalid ID");

            var license = await _unitOfWork.InternationalLicenses.FindAsync(
                il => il.DriverID == driverId && il.IsActive && il.ExpirationDate > DateTime.UtcNow,
                includes:null,
                tracked:false);

            return license?.InternationalLicenseID;
        }

        public async Task<bool> DeactivateInternationalLicenseAsync(int internationalLicenseId)
        {
            _logger.LogInformation("Deactivating International License with ID: {ID}", internationalLicenseId);

            if (internationalLicenseId <= 0) throw new ValidationException("Invalid ID");

            var license = await _unitOfWork.InternationalLicenses.GetByIdAsync(internationalLicenseId);
            if (license == null) return false;

            license.IsActive = false;
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> IsDriverEligibleForInternationalLicenseAsync(int localLicenseId)
        {
            if (localLicenseId <= 0) throw new ValidationException("Invalid ID");

            var localLicense = await _unitOfWork.Licenses.GetByIdAsync(localLicenseId);

            if (localLicense == null) throw new ResourceNotFoundException("Local License not found.");

            if (!localLicense.IsActive) throw new ValidationException("Local License is not active.");

            if (localLicense.ExpirationDate < DateTime.UtcNow) throw new ValidationException("Local License is expired.");

            if (localLicense.LicenseClassID != 3)
                throw new ValidationException("International License can only be issued for Class 3 (Ordinary Driving License) holders.");

            var activeId = await GetActiveInternationalLicenseIdByDriverIdAsync(localLicense.DriverID);

            if (activeId.HasValue)
                throw new ConflictException($"Driver already has an active International License (ID: {activeId}).");

            return true;
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