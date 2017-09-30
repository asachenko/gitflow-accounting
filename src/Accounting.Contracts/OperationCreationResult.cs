namespace Accounting.Contracts
{
    public class OperationCreationResult<T> : OperationResult
    {
        public T Result { get; set; }
    }
}
