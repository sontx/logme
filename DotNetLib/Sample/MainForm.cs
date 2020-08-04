using LogMeLib;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Sample
{
    public partial class MainForm : Form
    {
        private readonly Random random = new Random(DateTime.Now.Millisecond);
        private int counter;

        public MainForm()
        {
            InitializeComponent();
            txtAppName.Text = Application.ProductName;
            txtServerAddress.Text = "ws://mqtt.eclipse.org:80/mqtt";
        }

        private void btnWriteRandomLog_Click(object sender, EventArgs e)
        {
            string msg = "";
            switch (random.Next(3))
            {
                case 0:
                    msg = "Random info message " + Interlocked.Increment(ref counter);
                    Logger.I(msg);
                    break;

                case 1:
                    msg = "Random debug message " + Interlocked.Increment(ref counter);
                    Logger.D(msg);
                    break;

                case 2:
                    msg = "Random error message " + Interlocked.Increment(ref counter);
                    Logger.E(msg, new Exception("This is a bullshit exception"));
                    break;
            }

            textBox1.AppendText(msg + Environment.NewLine);
        }

        private void btnFireAnException_Click(object sender, EventArgs e)
        {
            throw new Exception("This is an exception from " + Text);
        }
    }
}