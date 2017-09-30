using System.Transactions;

namespace Accounting.Contracts.Data
{
    public interface IAccountUnitOfWorkFactory
    {
        IAccountUnitOfWork Create(IsolationLevel isolationLevel);
    }
}
