using LogMeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sample
{
    static class Program
    {
        private static LogMe logMe;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            StartLogMe();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            StopLogMe();
        }

        private static async void StartLogMe()
        {
            logMe = new LogMe("ws://mqtt.eclipse.org:80/mqtt");
            await logMe.StartAsync();
        }

        private static async void StopLogMe()
        {
            await logMe.StopAsync();
        }
    }
}
