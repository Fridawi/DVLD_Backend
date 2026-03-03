using AutoMapper;
using DVLD.CORE.DTOs.Licenses.InternationalLicenses;
using DVLD.CORE.Entities;

namespace DVLD.Services.Mapping
{
    public class InternationalLicenseProfile : Profile
    {
        public InternationalLicenseProfile()
        {
            CreateMap<InternationalLicense, InternationalLicenseDto>();

            CreateMap<InternationalLicense, DriverInternationalLicenseDto>()
                 .ForMember(dest => dest.DriverFullName, opt => opt.MapFrom(src =>
                    (src.DriverInfo.PersonInfo.FirstName + " " +
                     src.DriverInfo.PersonInfo.SecondName + " " +
                     (string.IsNullOrEmpty(src.DriverInfo.PersonInfo.ThirdName) ? "" : src.DriverInfo.PersonInfo.ThirdName + " ") +
                     src.DriverInfo.PersonInfo.LastName).Replace("  ", " ").Trim()))
                .ForMember(dest => dest.NationalNo, opt => opt.MapFrom(src => src.DriverInfo.PersonInfo.NationalNo))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.DriverInfo.PersonInfo.Gendor))
                .ForMember(dest => dest.GenderText, opt => opt.MapFrom(src => src.DriverInfo.PersonInfo.Gendor == 0 ? "Male" : "Female"))
                .ForMember(dest => dest.DriverBirthDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.DriverInfo.PersonInfo.DateOfBirth)))
                .ForMember(dest => dest.LicenseClassName, opt => opt.MapFrom(src => src.LocalLicenseInfo.LicenseClassInfo.ClassName))
                .ForMember(dest => dest.IssueReasonText, opt => opt.MapFrom(src => "First Time"))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.LocalLicenseInfo.Notes))
                .ForMember(dest => dest.LicenseID, opt => opt.MapFrom(src => src.IssuedUsingLocalLicenseID))
                .ForMember(dest => dest.DriverImageUrl, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    if (string.IsNullOrEmpty(src.DriverInfo.PersonInfo.ImagePath)) return null;

                    if (context.Items.TryGetValue("BaseUrl", out var baseUrl))
                    {
                        return $"{baseUrl}/uploads/people/{src.DriverInfo.PersonInfo.ImagePath}";
                    }
                    return null;
                })); ;
        }
    }
}
