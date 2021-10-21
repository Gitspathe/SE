using System;

namespace SE.Core.Exceptions
{
    /// <summary>
    /// Thrown when an action is taken that isn't supported by the fully headless display mode.
    /// These typically occur when the graphics device is interacted with, or when resources such as textures or audio are loaded.
    /// </summary>
    public class HeadlessNotSupportedException : Exception
    {
        public HeadlessNotSupportedException(string message = null) : base(message) { }
        public HeadlessNotSupportedException(string message, Exception innerException) : base(message, innerException) { }
        public HeadlessNotSupportedException(Exception innerException) : base(null, innerException) { }
    }
}
