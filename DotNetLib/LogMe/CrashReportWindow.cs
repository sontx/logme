using Blackcat.OS;
using System;

namespace LogMeLib
{
    internal class CrashReportWindow : IReportWindow
    {
        public static AppCrash AppCrash { get; set; }
        public static Action<string> OnReport { get; set; }

        public void Initialize(Exception exception, string productName, string devMail)
        {
            var report = AppCrash?.GenerateReport(exception, productName);
            if (!string.IsNullOrEmpty(report))
                OnReport?.Invoke(report);
        }

        public void Show()
        {
        }
    }
}