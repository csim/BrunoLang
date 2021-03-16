namespace Bruno.Exceptions
{
    using System;

    public class BrunoRuntimeException : Exception
    {
        public BrunoRuntimeException(string message) : base(message)
        {
        }
    }
}