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


        private void KeepOnCheckedChanged(object sender, EventArgs e) 
        {           
            Debug.WriteLine($"Action checkbox {activate}");
            keepOnSw.Text = !keepOnSw.Checked ? "Activate" : "Deactivate";
            DoAction(activate);
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
            activate = !activate;
            Debug.WriteLine($"Activation Status:: {activate.ToString().ToUpper()}");
        }
        private void Action_Form_Load(object sender, EventArgs e)
        {
            //keepOnSw.Checked = true;
            activate = false;
            actionButton.Text = "Activate";
            actionButton.BackColor = activate ? Color.Red : Color.Green;
            textBox.AppendText("Application started successfully......");
            //Debug.WriteLine($"Activation Status:: {activate.ToString().ToUpper()}");
        }

        private void Action_Form_Closed(object sender, FormClosedEventArgs e)
        {
            textBox.Text += "\nClosing application....";
            DoAction(false);
        }

        private void Action_Form_Button_Click(object sender, EventArgs e)
        {
            DoAction(!activate);
            var btn = (Button)sender;
            btn.Text = activate ? "Deactivate" : "Activate";
            actionButton.BackColor = activate ? Color.Red : Color.Green;
            var text = activate ? "Sleep Deactivated...." : "Sleep Activated....";
            textBox.Text += Environment.NewLine;
            textBox.AppendText(text);
        }

        private void MessageTextBoxTextChanged(object sender, EventArgs e)
        {
            //var textBox = (TextBox)sender;
            //textBox.Text = activate ? "Sleep Deactivated" : "Sleep Activated";
        }
    }
}
