using AutoMapper;
using DVLD.CORE.DTOs.Applications.LocalDrivingLicenseApplication;
using DVLD.CORE.Entities;

namespace DVLD.Services.Mapping
{
    public class LocalDrivingLicenseApplicationMappingProfile : Profile
    {
        public LocalDrivingLicenseApplicationMappingProfile()
        {
            // 1. From Entity to DTO 
            CreateMap<LocalDrivingLicenseApplication, LocalDrivingLicenseApplicationDto>()
                .ForMember(dest => dest.LocalDrivingLicenseApplicationID, opt => opt.MapFrom(src => src.LocalDrivingLicenseApplicationID))
                .ForMember(dest => dest.ApplicationID, opt => opt.MapFrom(src => src.ApplicationID))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.LicenseClassInfo.ClassName))
                .ForMember(dest => dest.NationalNo, opt => opt.MapFrom(src => src.ApplicationInfo.PersonInfo.NationalNo))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.ApplicationInfo.PersonInfo.FullName))
                .ForMember(dest => dest.ApplicationDate, opt => opt.MapFrom(src => src.ApplicationInfo.ApplicationDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.ApplicationInfo.ApplicationStatus.ToString()))
                .ForMember(dest => dest.PassedTestCount, opt => opt.Ignore());
        }
    }
}
