namespace Accounting.Contracts
{
    public enum OperationStatus
    {
        Unknown = 0,
        Success = 1,
        Failure = 2,
        AccountNotFound = 3,
        AccountFrozen = 4,
        InsufficientFunds = 5,
        AccessDenied = 6,
        InvalidArgument = 7
    }
}