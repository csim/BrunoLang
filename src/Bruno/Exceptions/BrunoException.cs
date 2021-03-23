namespace Bruno.Exceptions
{
    using System;

    public class BrunoException : Exception
    {
        public BrunoException(string message) : base(message)
        {
        }

        public BrunoException(string message, Exception innerEx) : base(message, innerEx)
        {
        }
    }
}