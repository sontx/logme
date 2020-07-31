using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LogMe.Supervisor
{
    public class RichTextBoxMessageDisplayer : IMessageDisplayer
    {
        private static readonly Dictionary<string, Color> colors = new Dictionary<string, Color>()
        {
            {"INFO:", Color.Green},
            {"ERROR:", Color.Red },
            {"DEBUG:", SystemColors.ControlText}
        };

        private readonly RichTextBox richTextBox;

        public int Count { get; private set; }

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
                Count++;
                var parts = msg.Split(new char[] { ' ' }, 4);// date time type msg
                var type = parts.Length == 4 ? parts[2] : "";
                var color = colors.ContainsKey(type)
                    ? colors[type]
                    : SystemColors.ControlText;
                AppendLine(msg, color);
                richTextBox.ScrollToCaret();
            }
        }

        private void AppendLine(string text, Color color)
        {
            text += Environment.NewLine;
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;

            richTextBox.SelectionColor = color;
            richTextBox.AppendText(text);
            richTextBox.SelectionColor = richTextBox.ForeColor;
        }

        public void Clear()
        {
            richTextBox.Clear();
            Count = 0;
        }
    }
}