using AutoMapper;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;

namespace DVLD.Services.Mapping
{
    public class LicenseMappingProfile : Profile
    {
        public LicenseMappingProfile()
        {
            CreateMap<License, LicenseDto>()
                .ForMember(dest => dest.LicenseClassName, opt => opt.MapFrom(src => src.LicenseClassInfo.ClassName))
                .ForMember(dest => dest.IssueReasonText, opt => opt.MapFrom(src => GetIssueReasonText(src.IssueReason)))
                .ForMember(dest => dest.IsDetained, opt => opt.MapFrom(src => IsLicenseCurrentlyDetained(src)))
                .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.ExpirationDate < DateTime.UtcNow))
                .ReverseMap();

            CreateMap<License, DriverLicenseDto>()
                .ForMember(dest => dest.LicenseClassName, opt => opt.MapFrom(src => src.LicenseClassInfo.ClassName))
                .ForMember(dest => dest.DriverFullName, opt => opt.MapFrom(src => src.ApplicationInfo.PersonInfo.FullName))
                .ForMember(dest => dest.NationalNo, opt => opt.MapFrom(src => src.ApplicationInfo.PersonInfo.NationalNo))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.ApplicationInfo.PersonInfo.Gendor))
                .ForMember(dest => dest.GenderText, opt => opt.MapFrom(src => src.ApplicationInfo.PersonInfo.Gendor == 0 ? "Male" : "Female"))
                .ForMember(dest => dest.IssueReasonText, opt => opt.MapFrom(src => GetIssueReasonText(src.IssueReason)))
                .ForMember(dest => dest.IsDetained, opt => opt.MapFrom(src => IsLicenseCurrentlyDetained(src)))
                .ForMember(dest => dest.DriverBirthDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.ApplicationInfo.PersonInfo.DateOfBirth)))
                .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.ExpirationDate < DateTime.UtcNow))
                .ForMember(dest=>dest.DriverImageUrl, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    if (string.IsNullOrEmpty(src.ApplicationInfo.PersonInfo.ImagePath)) return null;

                    if (context.Items.TryGetValue("BaseUrl", out var baseUrl))
                    {
                        return $"{baseUrl}/uploads/people/{src.ApplicationInfo.PersonInfo.ImagePath}";
                    }
                    return null;
                }));

        }
        private bool IsLicenseCurrentlyDetained(License src)
        {
            if (src.DetainedRecords == null || !src.DetainedRecords.Any())
                return false;

            return src.DetainedRecords.Any(d => !d.IsReleased);
        }
        private string GetIssueReasonText(EnIssueReason reason)
        {
            return reason switch
            {
                EnIssueReason.FirstTime => "First Time",
                EnIssueReason.Renew => "Renew",
                EnIssueReason.DamagedReplacement => "Replacement for Damaged",
                EnIssueReason.LostReplacement => "Replacement for Lost",
                _ => "First Time"
            };
        }
    }
}
