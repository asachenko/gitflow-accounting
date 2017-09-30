using System;

namespace Accounting.Contracts.Data
{
    public interface IAccountUnitOfWork : IDisposable
    {
        IAccountRepository AccountRepository { get; }

        void Commit();
    }
}
