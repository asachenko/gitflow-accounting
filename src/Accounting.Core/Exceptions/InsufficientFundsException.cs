using System;

namespace Accounting.Core.Exceptions
{
    public class InsufficientFundsException : Exception
    {
        public InsufficientFundsException() : this("Insufficient funds on the account.") {}

        public InsufficientFundsException(string errorMessage) : base(errorMessage) { }
    }
}
