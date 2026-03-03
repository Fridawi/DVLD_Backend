using AutoMapper;
using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.Entities;

namespace DVLD.Services.Mapping
{
    public class ApplicationMappingProfile : Profile
    {
        public ApplicationMappingProfile()
        {

            CreateMap<Application, ApplicationDto>()
                .ForMember(dest => dest.ApplicantFullName, opt => opt.MapFrom(src => src.PersonInfo.FullName))
                .ForMember(dest => dest.ApplicationTypeTitle, opt => opt.MapFrom(src => src.ApplicationTypeInfo.Title))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUserInfo.UserName))
                .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.StatusText));


            CreateMap<ApplicationCreateDto, Application>()
                .ForMember(dest => dest.ApplicationID, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastStatusDate, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationStatus, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserID, opt => opt.Ignore());


            CreateMap<ApplicationUpdateDto, Application>()
                .ForMember(dest => dest.ApplicationID, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationDate, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicantPersonID, opt => opt.Ignore())
                .ForMember(dest => dest.LastStatusDate, opt => opt.MapFrom(_ => DateTime.Now));
        }
    }
}
