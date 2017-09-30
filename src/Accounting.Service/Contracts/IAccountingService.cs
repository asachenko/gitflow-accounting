using Accounting.Contracts;
using Accounting.Service.Models;

namespace Accounting.Service.Contracts
{
    public interface IAccountingService
    {
        OperationResult Debit(Login login, int accountId, decimal value);

        OperationResult Credit(Login login, int accountId, decimal value);

        OperationResult Transfer(Login login, int sourceAccountId, int destinationAccountId, decimal value);

        OperationResult Freeze(Login login, int accountId);

        OperationResult AddIntrest(Login login, int accountId);
    }
}