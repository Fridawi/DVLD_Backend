using DVLD.CORE.Constants;

namespace DVLD.CORE.Entities
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IsActive { get; set; }
        public string Role { get; set; } = UserRoles.User;

        public int PersonID { get; set; }
        public Person Person { get; set; } = null!;
    }
}
