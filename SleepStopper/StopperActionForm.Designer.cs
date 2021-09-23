
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
        private CheckBox keepOnSw;
        private Button actionButton;
        private bool activate;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StopperActionForm));
            this.keepOnSw = new System.Windows.Forms.CheckBox();
            this.actionButton = new System.Windows.Forms.Button();
            this.textBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // keepOnSw
            // 
            this.keepOnSw.AutoSize = true;
            this.keepOnSw.Location = new System.Drawing.Point(424, 99);
            this.keepOnSw.Margin = new System.Windows.Forms.Padding(2);
            this.keepOnSw.Name = "keepOnSw";
            this.keepOnSw.Size = new System.Drawing.Size(92, 24);
            this.keepOnSw.TabIndex = 0;
            this.keepOnSw.Text = "Activate";
            this.keepOnSw.UseVisualStyleBackColor = true;
            this.keepOnSw.CheckedChanged += new System.EventHandler(this.KeepOnCheckedChanged);
            // 
            // actionButton
            // 
            this.actionButton.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.actionButton.Location = new System.Drawing.Point(207, 37);
            this.actionButton.Name = "actionButton";
            this.actionButton.Size = new System.Drawing.Size(146, 69);
            this.actionButton.TabIndex = 1;
            this.actionButton.Text = "Activate";
            this.actionButton.UseVisualStyleBackColor = false;
            this.actionButton.Click += new System.EventHandler(this.Action_Form_Button_Click);
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(96, 138);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox.Size = new System.Drawing.Size(360, 102);
            this.textBox.TabIndex = 2;
            // 
            // ActionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 252);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.actionButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ActionForm";
            this.Text = "No Sleeping";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Action_Form_Closed);
            this.Load += new System.EventHandler(this.Action_Form_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox textBox;
    }
}

