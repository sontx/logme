using Config4Net.Core;
using System;
using System.Threading.Tasks;
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

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "Connect")
            {
                await DoConnect();
            }
            else
            {
                await DoDisconnect();
            }
        }

        private async Task DoConnect()
        {
            await DoDisconnect();
            btnConnect.Enabled = false;
            var appName = txtAppName.Text.Trim();
            supervisorClient = new MqttSupervisorClient(appName, txtServerAddress.Text);
            supervisorClient.OnLog = HandleLog;
            supervisorClient.OnException = HandleException;
            supervisorClient.OnControlResponse = HandleControlResponse;
            try
            {
                await supervisorClient.StartAsync();
                labStatus.Text = $"Supervising";
                appSettings.AppName = txtAppName.Text;
                appSettings.ServerAddress = txtServerAddress.Text;
                txtAppName.ReadOnly = true;
                txtServerAddress.ReadOnly = true;
                btnConnect.Text = "Disconnect";
                messageDisplayer.Clear();
            }
            catch (Exception ex)
            {
                await DoDisconnect();
                labStatus.Text = $"Error: {ex.Message}";
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        private void HandleControlResponse(string msg)
        {
            throw new NotImplementedException();
        }

        private void HandleException(string msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { HandleException(msg); });
            }
            else
            {
                var parts = msg.Split(new char[] { '\n' }, 2);
                var appName = parts.Length == 2 ? parts[0] : "Unknown";
                var exception = parts.Length == 2 ? parts[1] : msg;

                var exceptionForm = new ExceptionForm();
                exceptionForm.Text = $"Exception from '{appName}'";
                exceptionForm.SetException(exception.Trim());
                exceptionForm.Show(this);
            }
        }

        private void HandleLog(string msg)
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
                labLogsCount.Text = $"{messageDisplayer.Count} logs";
            }
        }

        private async Task DoDisconnect()
        {
            btnConnect.Enabled = false;
            if (supervisorClient != null)
            {
                await supervisorClient?.StopAsync();
                supervisorClient = null;
            }
            labStatus.Text = string.Empty;
            txtAppName.ReadOnly = false;
            txtServerAddress.ReadOnly = false;
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