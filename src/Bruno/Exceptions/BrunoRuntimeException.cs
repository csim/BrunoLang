namespace Bruno.Exceptions
{
    using System;

    internal class BrunoRuntimeException : Exception
    {
        public BrunoRuntimeException(string message) : base(message)
        {
        }
    }
}