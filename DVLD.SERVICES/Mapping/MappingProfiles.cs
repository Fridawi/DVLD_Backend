using AutoMapper;
using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.DTOs.Countries;
using DVLD.CORE.DTOs.Drivers;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.DTOs.Licenses.DetainedLicenses;
using DVLD.CORE.DTOs.Tests;
using DVLD.CORE.DTOs.TestTypes;
using DVLD.CORE.DTOs.Users;
using DVLD.CORE.Entities;

namespace DVLD.Services.Mapping
{
    public partial class MappingProfiles : Profile
    {
        public MappingProfiles()
        {

            CreateMap<Country, CountryDto>().ReverseMap();
            CreateMap<TestType, TestTypeDto>().ReverseMap();
            CreateMap<LicenseClass, LicenseClassDto>().ReverseMap();
            CreateMap<ApplicationType, ApplicationTypeDto>().ReverseMap();

            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.Person, opt => opt.Ignore());

            CreateMap<UserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.UserID, opt => opt.Ignore()) 
                .ForMember(dest => dest.Person, opt => opt.Ignore()); 

            CreateMap<Driver, DriverDto>()
                .ForMember(dest => dest.NationalNo, opt => opt.MapFrom(src => src.PersonInfo.NationalNo))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    (src.PersonInfo.FirstName + " " +
                     src.PersonInfo.SecondName + " " +
                     (string.IsNullOrEmpty(src.PersonInfo.ThirdName) ? "" : src.PersonInfo.ThirdName + " ") +
                     src.PersonInfo.LastName).Replace("  ", " ").Trim()))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate));

            CreateMap<DriverCreateDto, Driver>();


            CreateMap<Test, TestDto>().ReverseMap();

            CreateMap<TestCreateDto, Test>()
                .ForMember(dest => dest.TestID, opt => opt.Ignore())
                .ForMember(dest => dest.TestAppointmentInfo, opt => opt.Ignore());

            CreateMap<TestUpdateDto, Test>()
                .ForMember(dest => dest.TestAppointmentID, opt => opt.Ignore())
                .ForMember(dest => dest.TestAppointmentInfo, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserID, opt => opt.Ignore());


            CreateMap<DetainedLicense, DetainedLicenseDto>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUserInfo != null ? src.CreatedByUserInfo.UserName : "Unknown"))
                .ForMember(dest => dest.ReleasedByUserName, opt => opt.MapFrom(src => src.ReleasedByUserInfo != null ? src.ReleasedByUserInfo.UserName : null))
                 .ForMember(dest => dest.NationalNo, opt => opt.MapFrom(src => src.LicenseInfo.ApplicationInfo.PersonInfo.NationalNo));
        }
    }
}
