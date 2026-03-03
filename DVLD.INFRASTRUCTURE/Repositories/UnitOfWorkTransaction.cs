using DVLD.CORE.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DVLD.INFRASTRUCTURE.Repositories
{
    public class UnitOfWorkTransaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public UnitOfWorkTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public async Task CommitAsync() => await _transaction.CommitAsync();
        public async Task RollbackAsync() => await _transaction.RollbackAsync();
        public void Dispose() => _transaction.Dispose();
    }
}
