using Accounting.Contracts.Models;

namespace Accounting.Contracts.Managers
{
    public interface IAccountingAdministrationManager
    {
        CreationResult<Account> Create(Account account);

        OperationStatus Edit(Account account);

        OperationStatus Delete(int accountId);
    }
}
