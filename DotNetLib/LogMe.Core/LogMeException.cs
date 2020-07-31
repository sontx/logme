using System;

namespace LogMe.Core
{
    public class LogMeException : Exception
    {
        public LogMeException(string message, Exception cause)
            : base(message, cause)
        {
        }

        public LogMeException(String message)
            : base(message)
        {
        }

        public LogMeException() : base()
        {
        }
    }
}