namespace Accounting.Contracts
{
    public class OperationResult
    {
        public OperationStatus Status { get; set; }

        public string ErrorMessage { get; set; }
    }
}
