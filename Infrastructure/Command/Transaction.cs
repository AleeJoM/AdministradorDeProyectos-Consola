using Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Command
{
    public class Transaction : ITransaction
    {
        private readonly IDbContextTransaction _transaction;
        private bool _disposed;

        public Transaction(IDbContextTransaction transaction)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public async Task CommitAsync()
        {
            if (_disposed) 
                throw new ObjectDisposedException(nameof(Transaction));
                
            await _transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            if (_disposed) 
                throw new ObjectDisposedException(nameof(Transaction));
                
            await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _transaction?.Dispose();
                _disposed = true;
            }
        }
    }
}
