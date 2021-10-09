
using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    partial class StopperActionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Button actionButton;
        private bool activate;
        private TextBox textBox;
        private NotifyIcon systemTrayIcon;
        private ContextMenuStrip systemTrayIconContextMenuStrip;
        private ToolStripMenuItem startStopToolStripMenuItem;
        private ToolStripMenuItem maximizeToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StopperActionForm));
            this.actionButton = new System.Windows.Forms.Button();
            this.textBox = new System.Windows.Forms.TextBox();
            this.systemTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.systemTrayIconContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.startStopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.maximizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.systemTrayIconContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            this.activate = false;
            // 
            // actionButton
            // 
            this.actionButton.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.actionButton.Font = new System.Drawing.Font("Gadugi", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.actionButton.ForeColor = System.Drawing.Color.Transparent;
            this.actionButton.Location = new System.Drawing.Point(106, 18);
            this.actionButton.Margin = new System.Windows.Forms.Padding(2);
            this.actionButton.Name = "actionButton";
            this.actionButton.Size = new System.Drawing.Size(138, 45);
            this.actionButton.TabIndex = 1;
            this.actionButton.Text = "ACTIVATE";
            this.actionButton.UseVisualStyleBackColor = false;
            this.actionButton.Click += new System.EventHandler(this.ActionFormButtonClick);
            // 
            // textBox
            // 
            this.textBox.BackColor = System.Drawing.SystemColors.WindowText;
            this.textBox.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox.ForeColor = System.Drawing.Color.White;
            this.textBox.Location = new System.Drawing.Point(40, 71);
            this.textBox.Margin = new System.Windows.Forms.Padding(2);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.Size = new System.Drawing.Size(270, 89);
            this.textBox.TabIndex = 2;
            // 
            // systemTrayIcon
            // 
            this.systemTrayIcon.ContextMenuStrip = this.systemTrayIconContextMenuStrip;
            this.systemTrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("systemTrayIcon.Icon")));
            this.systemTrayIcon.Text = "No Sleeping";
            this.systemTrayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.SystemTrayIconMouseDoubleClick);
            // 
            // systemTrayIconContextMenuStrip
            // 
            this.systemTrayIconContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startStopToolStripMenuItem,
            this.maximizeToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.systemTrayIconContextMenuStrip.Name = "contextMenuStrip1";
            this.systemTrayIconContextMenuStrip.Size = new System.Drawing.Size(128, 70);
            // 
            // startStopToolStripMenuItem
            // 
            this.startStopToolStripMenuItem.Name = "startStopToolStripMenuItem";
            this.startStopToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.startStopToolStripMenuItem.Text = "START";
            this.startStopToolStripMenuItem.Click += new System.EventHandler(this.StartStopToolStripMenuItem_Click);
            // 
            // maximizeToolStripMenuItem
            // 
            this.maximizeToolStripMenuItem.Name = "maximizeToolStripMenuItem";
            this.maximizeToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.maximizeToolStripMenuItem.Text = "MINIMIZE";
            this.maximizeToolStripMenuItem.Click += new System.EventHandler(this.MaximizeToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.exitToolStripMenuItem.Text = "EXIT";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // StopperActionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 164);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.actionButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(420, 203);
            this.Name = "StopperActionForm";
            this.Text = "No Sleeping";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ActionFormClosed);
            this.Load += new System.EventHandler(this.ActionFormLoad);
            this.SizeChanged += new System.EventHandler(this.StopperActionFormSizeChanged);
            this.systemTrayIconContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}

