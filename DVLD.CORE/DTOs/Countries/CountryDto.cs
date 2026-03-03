using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.Countries
{
    public class CountryDto
    {
        public int CountryID { get; set; }

        [Required(ErrorMessage = "Country Name is required")]
        [StringLength(50)]
        public string CountryName { get; set; } = null!;
    }
}
