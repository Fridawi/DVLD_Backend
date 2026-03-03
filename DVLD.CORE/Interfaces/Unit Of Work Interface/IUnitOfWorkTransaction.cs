namespace DVLD.CORE.Interfaces
{
    public interface IUnitOfWorkTransaction : IDisposable
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
}
