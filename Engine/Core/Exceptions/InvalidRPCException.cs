using System;

namespace SE.Core.Exceptions
{
    public class InvalidRPCException : Exception
    {
        public InvalidRPCException(string message) : base(message) { }
        public InvalidRPCException(string message, Exception innerException) : base(message, innerException) { }
    }
}
