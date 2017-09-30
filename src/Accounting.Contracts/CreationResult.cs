namespace Accounting.Contracts
{
    public class CreationResult<T>
    {
        public CreationResult(OperationStatus status, T result)
        {
            Status = status;
            Result = result;
        }

        public OperationStatus Status { get; protected set; }

        public T Result { get; protected set; }
    }
}
