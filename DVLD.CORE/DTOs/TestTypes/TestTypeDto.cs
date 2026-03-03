using DVLD.CORE.Constants;
using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.TestTypes
{
    public class TestTypeDto
    {
        public int TestTypeID { set; get; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { set; get; } = null!;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500)]
        public string Description { set; get; } = null!;

        [Required]
        public float Fees { set; get; }
    }
}
