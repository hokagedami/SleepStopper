using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class StopperActionForm : Form
    {
        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        public StopperActionForm()
        {
            InitializeComponent();
        }

        
        private void DoAction(bool condition)
        {
            if (condition)
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            }
            else
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            }
            var text = activate ? "System Auto-Sleep Deactivated...." : "System Auto-Sleep Activated....";
            textBox.Text += Environment.NewLine;
            textBox.AppendText(text);
            activate = !activate;
            this.startStopToolStripMenuItem.Text = activate ? "DEACTIVATE" : "ACTIVATE";
        }
        private void ActionFormLoad(object sender, EventArgs e)
        {
            activate = false;
            actionButton.Text = "ACTIVATE";
            actionButton.BackColor = activate ? Color.Red : Color.Green;
            textBox.AppendText("Application started successfully....");
        }

        private void ActionFormClosed(object sender, FormClosedEventArgs e)
        {
            textBox.AppendText("Closing application....");
            DoAction(false);
        }

       
        private void ActionFormButtonClick(object sender, EventArgs e)
        {
            DoAction(!activate);
            //var btn = (Button)sender;
            ChangeBtnColorText(activate);
            //btn.Text = activate ? "STOP" : "START";
            //actionButton.BackColor = activate ? Color.Red : Color.Green;
        }

        private void StopperActionFormSizeChanged(object sender, EventArgs e)
        {
            bool mouseNotOnAppOnTaskBar = Screen.GetWorkingArea(this).Contains(Cursor.Position);
            if (mouseNotOnAppOnTaskBar 
                && this.WindowState == FormWindowState.Minimized)
            {
                //systemTrayIcon.Icon = SystemIcons.Application;
                this.ShowInTaskbar = false;
                systemTrayIcon.Visible = true;
                this.maximizeToolStripMenuItem.Text = "MAXIMIZE";
            }
        }

        private void SystemTrayIconMouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            if (this.WindowState == FormWindowState.Normal)
            {
                this.ShowInTaskbar = true;
                systemTrayIcon.Visible = false;
            }
        }

        private void StartStopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool start = this.startStopToolStripMenuItem.Text == "ACTIVATE" && !activate;
            DoAction(start);
            ChangeBtnColorText(start);
            this.startStopToolStripMenuItem.Text = start ? "DEACTIVATE" : "ACTIVATE";
        }

        private void MaximizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case FormWindowState.Normal:
                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;
                    systemTrayIcon.Visible = true;
                    this.maximizeToolStripMenuItem.Text = "MINIMIZE";
                    break;
                case FormWindowState.Minimized:
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
                    systemTrayIcon.Visible = false;
                    this.maximizeToolStripMenuItem.Text = "MAXIMIZE";
                    break;
                case FormWindowState.Maximized:
                default:                    
                    break;
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.AppendText("Closing application....");
            DoAction(false);
            this.Close();
        }

        private void ChangeBtnColorText(bool start)
        {
            actionButton.Text = start ? "DEACTIVATE" : "ACTIVATE";
            actionButton.BackColor = start ? Color.Red : Color.Green;
        }
    }
}
