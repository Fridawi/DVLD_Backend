using DVLD.CORE.Entities;

namespace DVLD.CORE.Interfaces
{
    public interface IJwtProvider
    {
        public string Generate(User user);
    }
}
