namespace DVLD.CORE.DTOs.Users
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; }
        public int PersonID { get; set; }
    }

}
