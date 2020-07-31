using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Supervisor
{
    public partial class ScreenshotPreviewForm : Form
    {
        public ScreenshotPreviewForm()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (pictureBox1.Image is Bitmap bitmap)
            {
                bitmap.Dispose();
            }
        }

        public void SetScreenshot(string encoded)
        {
            var bytes = Convert.FromBase64String(encoded);
            pictureBox1.Image = new Bitmap(new MemoryStream(bytes));
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                var fileName = saveFileDialog1.FileName;
                pictureBox1.Image.Save(fileName, ImageFormat.Png);
            }
        }
    }
}