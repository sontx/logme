using System;
using System.Windows.Forms;

namespace Client
{
    public class RichTextBoxMessageDisplayer : IMessageDisplayer
    {
        private readonly RichTextBox richTextBox;

        public RichTextBoxMessageDisplayer(RichTextBox richTextBox)
        {
            this.richTextBox = richTextBox;
        }

        public void Append(string msg)
        {
            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke((MethodInvoker)delegate { Append(msg); });
            }
            else
            {
                richTextBox.AppendText(msg + Environment.NewLine);
                richTextBox.ScrollToCaret();
            }
        }

        public void Clear()
        {
            richTextBox.Clear();
        }
    }
}