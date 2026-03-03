using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DVLD.CORE.DTOs.People
{
    public class PersonDto
    {
        public int PersonID { get; set; }

        [Required(ErrorMessage = "National Number is required")]
        [StringLength(20)]
        public string NationalNo { get; set; } = null!;

        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; } = null!;
        public string? GenderName { get; set; } = null!;

        [Range(0, 1, ErrorMessage = "Gender must be 0 (Male) or 1 (Female)")]
        public short Gendor { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        [Required, Phone]
        public string Phone { get; set; } = null!;

        [Required, StringLength(500)]
        public string Address { get; set; } = null!;
        public string? CountryName { get; set; } = null!;

        [Required]
        public int NationalityCountryID { get; set; }
        public string? ImageUrl { get; set; }
    }
}
