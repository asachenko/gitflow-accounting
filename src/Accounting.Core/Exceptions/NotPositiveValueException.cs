using System;

namespace Accounting.Core.Exceptions
{
    public class NotPositiveValueException : Exception
    {
        public NotPositiveValueException(string errorMessage) : base(errorMessage) {}
    }
}
