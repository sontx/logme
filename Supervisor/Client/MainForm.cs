﻿using Config4Net.Core;
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

        protected override async void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            await supervisorClient?.StopAsync();
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
            await Task.Delay(1000);
            btnSystemInfo.Enabled = true;
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

                var infoForm = new InfoForm();
                infoForm.Text = $"{command} -> '{appName}'";
                infoForm.SetVariant(InfoForm.Variant.Info);
                infoForm.SetInfo(info.Trim());
                infoForm.Show(this);
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