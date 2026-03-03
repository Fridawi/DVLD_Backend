using AutoMapper;
using DVLD.CORE.DTOs.People;
using DVLD.CORE.Entities;

namespace DVLD.Services.Mapping
{
    public class PeopleMappingProfile : Profile
    {
        public PeopleMappingProfile()
        {
            // 1. From Entity To DTO
            CreateMap<Person, PersonDto>()
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.CountryInfo.CountryName))
                .ForMember(dest => dest.GenderName, opt => opt.MapFrom(src => src.Gendor == 0 ? "Male" : "Female"))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    $"{src.FirstName} {src.SecondName} {src.ThirdName} {src.LastName}".Replace("  ", " ").Trim()))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    if (string.IsNullOrEmpty(src.ImagePath)) return null;

                    if (context.Items.TryGetValue("BaseUrl", out var baseUrl))
                    {
                        return $"{baseUrl}/uploads/people/{src.ImagePath}";
                    }
                    return null;
                }));

            // 2. From DTO To Entity
            CreateMap<PersonDto, Person>()
                .ForMember(dest => dest.CountryInfo, opt => opt.Ignore())
                .ForMember(dest => dest.ImagePath, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    var names = src.FullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    dest.FirstName = names.Length > 0 ? names[0] : "";
                    dest.SecondName = names.Length > 1 ? names[1] : "";

                    if (names.Length == 1)
                    {
                        dest.LastName = names[0];
                    }
                    else if (names.Length == 2)
                    {
                        dest.LastName = names[1];
                    }
                    else if (names.Length == 3)
                    {
                        dest.ThirdName = "";
                        dest.LastName = names[2];
                    }
                    else if (names.Length >= 4)
                    {
                        dest.ThirdName = names[2];
                        dest.LastName = names[3];
                    }
                });

        }
    }
}
