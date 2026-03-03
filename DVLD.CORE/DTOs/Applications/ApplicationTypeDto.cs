using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.Applications
{
    public class ApplicationTypeDto
    {
        public int ApplicationTypeID { set; get; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(150)]
        public string Title { set; get; } = null!;

        [Required]
        public float Fees { set; get; }
    }
}
