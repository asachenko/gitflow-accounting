using System;

namespace Accounting.Core.Exceptions
{
    public class AccountFrozenException : Exception
    {
        public AccountFrozenException(string errorMesage) : base(errorMesage) {}
    }
}
