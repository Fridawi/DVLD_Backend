using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.Licenses
{
    public class LicenseClassDto
    {
        public int LicenseClassID { set; get; }

        [Required(ErrorMessage = "Class Name is required")]
        [StringLength(50)]
        public string ClassName { set; get; } = null!;

        [Required(ErrorMessage = "Class Description is required")]
        [StringLength(500)]
        public string ClassDescription { set; get; } = null!;

        [Required]
        public byte MinimumAllowedAge { set; get; }

        [Required]
        public byte DefaultValidityLength { set; get; }

        [Required]
        public float ClassFees { set; get; }
    }


}
