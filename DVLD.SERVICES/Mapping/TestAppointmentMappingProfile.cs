using AutoMapper;
using DVLD.CORE.DTOs.TestAppointments;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;

namespace DVLD.Services.Mapping
{
    public class TestAppointmentMappingProfile : Profile
    {
        public TestAppointmentMappingProfile()
        {
            CreateMap<TestAppointment, TestAppointmentDto>()
                .ForMember(dest => dest.TestTypeName, opt => opt.MapFrom(src => GetTestTypeName(src.TestTypeID)))
                .ForMember(dest => dest.TestID, opt => opt.MapFrom(src => src.TestRecord != null ? src.TestRecord.TestID : -1))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.LocalAppInfo.LicenseClassInfo.ClassName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    $"{src.LocalAppInfo.ApplicationInfo.PersonInfo.FirstName} " +
                    $"{src.LocalAppInfo.ApplicationInfo.PersonInfo.SecondName} " +
                    $"{(string.IsNullOrWhiteSpace(src.LocalAppInfo.ApplicationInfo.PersonInfo.ThirdName) ? "" :
                    src.LocalAppInfo.ApplicationInfo.PersonInfo.ThirdName + " ")}" +
                    $"{src.LocalAppInfo.ApplicationInfo.PersonInfo.LastName}".Trim()));

            CreateMap<TestAppointmentCreateDto, TestAppointment>()
                .ForMember(dest => dest.TestTypeID, opt => opt.MapFrom(src => (EnTestType)src.TestTypeID))
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => false)); 


            CreateMap<TestAppointmentUpdateDto, TestAppointment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }

        private string GetTestTypeName(EnTestType testType)
        {
            return testType switch
            {
                EnTestType.VisionTest => "Vision Test",
                EnTestType.WrittenTest => "Written Test",
                EnTestType.StreetTest => "Street Test",
                _ => "Unknown Test"
            };
        }
    }
}
