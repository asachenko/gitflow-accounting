using Accounting.Contracts;
using Accounting.Service.Models;

namespace Accounting.Service.Contracts
{
    public interface IAccountingAdministrationService
    {
        OperationCreationResult<AccountModel> Create(Login login, AccountModel account);

        OperationResult Edit(Login login, AccountModel account);

        OperationResult Delete(Login login, int accountId);
    }
}