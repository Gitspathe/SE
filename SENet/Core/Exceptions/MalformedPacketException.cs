using System;

namespace DeeZ.Core.Exceptions
{
    public class MalformedPacketException : Exception
    {
        public MalformedPacketException(string message) : base(message) { }
        public MalformedPacketException(string message, Exception innerException) : base(message, innerException) { }
    }
}
