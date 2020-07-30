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
        private readonly WaitHandleCommands waitHandleCommands;
        private ISupervisorClient supervisorClient;

        public MainForm()
        {
            InitializeComponent();
            messageDisplayer = new RichTextBoxMessageDisplayer(richTextBox1);
            appSettings = Config.Default.Get<AppSettings>();
            waitHandleCommands = new WaitHandleCommands();
            waitHandleCommands.Add(Commands.GET_SYSTEM_INFO);
            waitHandleCommands.Add(Commands.TAKE_SCREENSHOT);
            txtAppName.Text = appSettings.AppName;
            txtServerAddress.Text = appSettings.ServerAddress;
        }

        protected override async void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            waitHandleCommands.Dispose();
            if (supervisorClient != null)
                await supervisorClient.StopAsync();
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

        private async void btnSystemInfo_Click(object sender, EventArgs e)
        {
            btnSystemInfo.Enabled = false;
            await supervisorClient.SendCommand(Commands.GET_SYSTEM_INFO);
            await waitHandleCommands.WaitAsync(Commands.GET_SYSTEM_INFO);
            btnSystemInfo.Enabled = true;
        }

        private async void btnScreenshot_Click(object sender, EventArgs e)
        {
            btnScreenshot.Enabled = false;
            await supervisorClient.SendCommand(Commands.TAKE_SCREENSHOT);
            await waitHandleCommands.WaitAsync(Commands.TAKE_SCREENSHOT);
            btnScreenshot.Enabled = true;
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
                btnSystemInfo.Enabled = true;
                btnScreenshot.Enabled = true;
                btnConnect.Text = "Disconnect";
                btnConnect.ImageKey = "stop.png";
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
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { HandleControlResponse(msg); });
            }
            else
            {
                var parts = msg.Split(new char[] { '\n' }, 3);
                var command = parts.Length == 3 ? parts[0] : "Unknown";
                var appName = parts.Length == 3 ? parts[1] : "Unknown";
                var info = parts.Length == 3 ? parts[2] : msg;

                command = command.ToUpper();

                waitHandleCommands.Set(command);

                if (command == Commands.TAKE_SCREENSHOT)
                {
                    var screenshotPreviewForm = new ScreenshotPreviewForm();
                    screenshotPreviewForm.Text = $"Screenshot from '{appName}'";
                    screenshotPreviewForm.SetScreenshot(info);
                    screenshotPreviewForm.Show(this);
                }
                else
                {
                    var infoForm = new InfoForm();
                    infoForm.Text = $"{command} -> '{appName}'";
                    infoForm.SetVariant(InfoForm.Variant.Info);
                    infoForm.SetInfo(info.Trim());
                    infoForm.Show(this);
                }
            }
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

                var infoForm = new InfoForm();
                infoForm.Text = $"Exception from '{appName}'";
                infoForm.SetVariant(InfoForm.Variant.Warning);
                infoForm.SetInfo(exception.Trim());
                infoForm.Show(this);
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
            btnSystemInfo.Enabled = false;
            btnScreenshot.Enabled = false;
            btnConnect.Text = "Connect";
            btnConnect.ImageKey = "start.png";
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

        private void chkTopMost_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = chkTopMost.Checked;
        }
    }
}