namespace GenerateHardwareId
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
            this.HardwareIdTextBox = new System.Windows.Forms.TextBox();
            this.HowToCalculateLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // HardwareIdTextBox
            // 
            this.HardwareIdTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HardwareIdTextBox.Location = new System.Drawing.Point(125, 126);
            this.HardwareIdTextBox.Name = "HardwareIdTextBox";
            this.HardwareIdTextBox.ReadOnly = true;
            this.HardwareIdTextBox.Size = new System.Drawing.Size(360, 29);
            this.HardwareIdTextBox.TabIndex = 0;
            this.HardwareIdTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // HowToCalculateLabel
            // 
            this.HowToCalculateLabel.Location = new System.Drawing.Point(97, 158);
            this.HowToCalculateLabel.Name = "HowToCalculateLabel";
            this.HowToCalculateLabel.Size = new System.Drawing.Size(416, 23);
            this.HowToCalculateLabel.TabIndex = 1;
            this.HowToCalculateLabel.Text = "label1";
            this.HowToCalculateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(611, 311);
            this.Controls.Add(this.HowToCalculateLabel);
            this.Controls.Add(this.HardwareIdTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hardware ID Generator";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox HardwareIdTextBox;
        private System.Windows.Forms.Label HowToCalculateLabel;
    }
}

