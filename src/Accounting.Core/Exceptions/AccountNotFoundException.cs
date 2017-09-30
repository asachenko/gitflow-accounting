using System;

namespace Accounting.Core.Exceptions
{
    public class AccountNotFoundException : Exception
    {
        public AccountNotFoundException(string errorMessage) : base(errorMessage) {}
    }
}
