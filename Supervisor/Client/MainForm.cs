using Config4Net.Core;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Client
{
    public partial class MainForm : Form
    {
        private readonly AppSettings appSettings;
        private readonly IMessageDisplayer messageDisplayer;
        private ISupervisorClient supervisorClient;

        public MainForm()
        {
            InitializeComponent();
            messageDisplayer = new RichTextBoxMessageDisplayer(richTextBox1);
            appSettings = Config.Default.Get<AppSettings>();
            txtAppName.Text = appSettings.AppName;
            txtServerAddress.Text = appSettings.ServerAddress;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            messageDisplayer.Clear();
            UpdateLogCount();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "Connect")
            {
                DoConnect();
            }
            else
            {
                DoDisconnect();
            }
        }

        private async void DoConnect()
        {
            DoDisconnect();
            btnConnect.Enabled = false;
            var appName = txtAppName.Text.Trim();
            supervisorClient = new MqttSupervisorClient(appName, txtServerAddress.Text, $"{appName}/logs");
            supervisorClient.OnMessage = HandleMessage;
            try
            {
                await supervisorClient.StartAsync();
                labStatus.Text = $"Supervising {appName}";
                appSettings.AppName = txtAppName.Text;
                appSettings.ServerAddress = txtServerAddress.Text;
                txtAppName.Enabled = false;
                txtServerAddress.Enabled = false;
                btnConnect.Text = "Disconnect";
                messageDisplayer.Clear();
            }
            catch (Exception ex)
            {
                btnConnect_Click(null, null);
                labStatus.Text = $"Error: {ex.Message}";
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        private void HandleMessage(string msg)
        {
            messageDisplayer.Append(msg);
            UpdateLogCount();
        }

        private void UpdateLogCount()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { UpdateLogCount(); });
            }
            else
            {
                var count = richTextBox1.Lines.Count(line => !string.IsNullOrEmpty(line));
                labLogsCount.Text = $"{count} logs";
            }
        }

        private async void DoDisconnect()
        {
            btnConnect.Enabled = false;
            if (supervisorClient != null)
            {
                await supervisorClient?.StopAsync();
                supervisorClient = null;
            }
            labStatus.Text = string.Empty;
            txtAppName.Enabled = true;
            txtServerAddress.Enabled = true;
            btnConnect.Enabled = true;
            btnConnect.Text = "Connect";
        }

        private void txtServerAddress_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void txtAppName_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void ValidateInputs()
        {
            btnConnect.Enabled = !string.IsNullOrWhiteSpace(txtAppName.Text) && !string.IsNullOrWhiteSpace(txtServerAddress.Text);
        }
    }
}