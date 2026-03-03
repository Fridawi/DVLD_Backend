using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.DTOs.People;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DVLD.CORE.Interfaces
{
    public interface IApplicationRepository : IGenericRepository<Application>
    {
        Task<int> GetActiveApplicationIdAsync(int personID, int applicationTypeID);
        Task<int> GetActiveApplicationIdForLicenseClassAsync(int personID, int applicationTypeID, int licenseClassID);
        Task<bool> DoesPersonHaveActiveApplicationAsync(int personID, int applicationTypeID);
        Task<bool> UpdateStatusAsync(int applicationID, EnApplicationStatus newStatus);
    }
}
