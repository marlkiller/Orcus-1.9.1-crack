namespace ServerStressTest
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.StartButton = new System.Windows.Forms.Button();
            this.IpTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ClientsCount = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.StopButton = new System.Windows.Forms.Button();
            this.PortNumber = new System.Windows.Forms.NumericUpDown();
            this.StatusProgressbar = new System.Windows.Forms.ProgressBar();
            this.StatusLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ClientsCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(566, 12);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(133, 23);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "Start Connecting";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // IpTextBox
            // 
            this.IpTextBox.Location = new System.Drawing.Point(15, 33);
            this.IpTextBox.Name = "IpTextBox";
            this.IpTextBox.Size = new System.Drawing.Size(245, 20);
            this.IpTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Server-IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Port";
            // 
            // ClientsCount
            // 
            this.ClientsCount.Location = new System.Drawing.Point(15, 149);
            this.ClientsCount.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.ClientsCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ClientsCount.Name = "ClientsCount";
            this.ClientsCount.Size = new System.Drawing.Size(140, 20);
            this.ClientsCount.TabIndex = 5;
            this.ClientsCount.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 133);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Clients To Connect";
            // 
            // StopButton
            // 
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(566, 41);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(133, 23);
            this.StopButton.TabIndex = 7;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // PortNumber
            // 
            this.PortNumber.Location = new System.Drawing.Point(15, 90);
            this.PortNumber.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.PortNumber.Name = "PortNumber";
            this.PortNumber.Size = new System.Drawing.Size(140, 20);
            this.PortNumber.TabIndex = 9;
            this.PortNumber.Value = new decimal(new int[] {
            10134,
            0,
            0,
            0});
            // 
            // StatusProgressbar
            // 
            this.StatusProgressbar.Location = new System.Drawing.Point(316, 156);
            this.StatusProgressbar.Name = "StatusProgressbar";
            this.StatusProgressbar.Size = new System.Drawing.Size(383, 23);
            this.StatusProgressbar.TabIndex = 10;
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(313, 133);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(30, 13);
            this.StatusLabel.TabIndex = 12;
            this.StatusLabel.Text = "0 / 0";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(711, 191);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.StatusProgressbar);
            this.Controls.Add(this.PortNumber);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ClientsCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.IpTextBox);
            this.Controls.Add(this.StartButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Server Stress Test";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ClientsCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortNumber)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.TextBox IpTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown ClientsCount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.NumericUpDown PortNumber;
        private System.Windows.Forms.ProgressBar StatusProgressbar;
        private System.Windows.Forms.Label StatusLabel;
    }
}

