using System;
using System.Diagnostics;

namespace LogMe
{
    public sealed class Logger
    {
        private static LogMe logMe;

        internal static void SetLogMe(LogMe logMe)
        {
            Logger.logMe = logMe;
        }

        private static string NormalizeMessage(string type, string message, string tag = null)
        {
            return string.IsNullOrEmpty(tag)
                ? $"{type}: {message}"
                : $"{type}: [{tag}] {message}";
        }

        public static void I(string tag, string message)
        {
            var normalizedMessage = NormalizeMessage("INFO", message, tag);
            Trace.TraceInformation(normalizedMessage);
            SendToSupervisor(normalizedMessage);
        }

        public static void D(string tag, string message)
        {
            var normalizedMessage = NormalizeMessage("DEBUG", message, tag);
            Trace.WriteLine(normalizedMessage);
            SendToSupervisor(normalizedMessage);
        }

        public static void E(string tag, string message, Exception exception)
        {
            var normalizedMessage = NormalizeMessage("ERROR", message, tag);
            if (exception != null)
            {
                normalizedMessage = $"{normalizedMessage}{Environment.NewLine}{exception}";
            }
            Trace.TraceError(normalizedMessage);
            SendToSupervisor(normalizedMessage);
        }

        private static async void SendToSupervisor(string message)
        {
            var logMe = Logger.logMe;
            if (logMe != null)
            {
                var now = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss,fff}";
                await logMe.SendAsync($"{now} {message}");
            }
        }

        public static void I(string message)
        {
            I(null, message);
        }

        public static void D(string message)
        {
            D(null, message);
        }

        public static void E(string message, Exception exception = null)
        {
            E(null, message, exception);
        }
    }
}