using System.Security.Claims;

namespace DVLD.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static (int UserId, string? Role) GetUserInfo(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleClaim = user.FindFirstValue(ClaimTypes.Role);

            int.TryParse(idClaim, out int userId);

            return (userId, roleClaim);
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idClaim, out int userId) ? userId : 0;
        }
    }
}
