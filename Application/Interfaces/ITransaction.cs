using System;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITransaction : IDisposable
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
}
