﻿namespace Sample
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnWriteRandomLog = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnFireAnException = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnWriteRandomLog
            // 
            this.btnWriteRandomLog.Location = new System.Drawing.Point(12, 12);
            this.btnWriteRandomLog.Name = "btnWriteRandomLog";
            this.btnWriteRandomLog.Size = new System.Drawing.Size(154, 23);
            this.btnWriteRandomLog.TabIndex = 0;
            this.btnWriteRandomLog.Text = "Write Random Log";
            this.btnWriteRandomLog.UseVisualStyleBackColor = true;
            this.btnWriteRandomLog.Click += new System.EventHandler(this.btnWriteRandomLog_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 41);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(390, 288);
            this.textBox1.TabIndex = 1;
            this.textBox1.WordWrap = false;
            // 
            // btnFireAnException
            // 
            this.btnFireAnException.Location = new System.Drawing.Point(172, 12);
            this.btnFireAnException.Name = "btnFireAnException";
            this.btnFireAnException.Size = new System.Drawing.Size(139, 23);
            this.btnFireAnException.TabIndex = 2;
            this.btnFireAnException.Text = "Fire An Exception";
            this.btnFireAnException.UseVisualStyleBackColor = true;
            this.btnFireAnException.Click += new System.EventHandler(this.btnFireAnException_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 341);
            this.Controls.Add(this.btnFireAnException);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnWriteRandomLog);
            this.Name = "MainForm";
            this.Text = "DotNetLib Sample";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnWriteRandomLog;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnFireAnException;
    }
}

