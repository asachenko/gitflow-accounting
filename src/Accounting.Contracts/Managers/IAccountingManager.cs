namespace Accounting.Contracts.Managers
{
    public interface IAccountingManager
    {
        OperationStatus Debit(int accountId, decimal value);

        OperationStatus Credit(int accountId, decimal value);

        OperationStatus Transfer(int sourceAccountId, int destinationAccountId, decimal value);

        OperationStatus Freeze(int accountId);

        OperationStatus AddIntrest(int accountId);
    }
}
