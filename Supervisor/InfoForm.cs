using System;
using System.Drawing;
using System.Windows.Forms;

namespace LogMe.Supervisor
{
    public partial class InfoForm : Form
    {
        public InfoForm()
        {
            InitializeComponent();
        }

        public void SetVariant(Variant variant)
        {
            if (variant == Variant.Warning)
                richTextBox1.BackColor = Color.PaleGoldenrod;
            else if (variant == Variant.Info)
                richTextBox1.BackColor = Color.LightSteelBlue;
        }

        public void SetInfo(string info)
        {
            richTextBox1.Text = info;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        public enum Variant
        {
            Warning,
            Info
        }
    }
}