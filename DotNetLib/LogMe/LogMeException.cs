using System;

namespace LogMe
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