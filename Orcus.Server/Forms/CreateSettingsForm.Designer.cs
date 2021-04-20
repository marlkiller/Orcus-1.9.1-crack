namespace Orcus.Server.Forms
{
    partial class CreateSettingsForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateSettingsForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.IpAddressComboBox = new System.Windows.Forms.ComboBox();
            this.PortNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.SslPathTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.OpenSslCertificateButton = new System.Windows.Forms.Button();
            this.CreateSslCertificate = new System.Windows.Forms.Button();
            this.SslPasswordTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.generateSslCertificateRadioButton = new System.Windows.Forms.RadioButton();
            this.createSslCertificateRadioButton = new System.Windows.Forms.RadioButton();
            this.createSslCertPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.PortNumericUpDown)).BeginInit();
            this.createSslCertPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Welcome!";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(13, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(182, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Let\'s start to configure your server";
            // 
            // IpAddressComboBox
            // 
            this.IpAddressComboBox.FormattingEnabled = true;
            this.IpAddressComboBox.Location = new System.Drawing.Point(16, 85);
            this.IpAddressComboBox.Name = "IpAddressComboBox";
            this.IpAddressComboBox.Size = new System.Drawing.Size(351, 21);
            this.IpAddressComboBox.TabIndex = 2;
            // 
            // PortNumericUpDown
            // 
            this.PortNumericUpDown.Location = new System.Drawing.Point(373, 86);
            this.PortNumericUpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.PortNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.PortNumericUpDown.Name = "PortNumericUpDown";
            this.PortNumericUpDown.Size = new System.Drawing.Size(93, 20);
            this.PortNumericUpDown.TabIndex = 3;
            this.PortNumericUpDown.Value = new decimal(new int[] {
            10134,
            0,
            0,
            0});
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(13, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "IP-Address";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(13, 118);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Password";
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Location = new System.Drawing.Point(16, 134);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.Size = new System.Drawing.Size(450, 20);
            this.PasswordTextBox.TabIndex = 7;
            this.PasswordTextBox.UseSystemPasswordChar = true;
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(358, 301);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(108, 23);
            this.StartButton.TabIndex = 8;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // SslPathTextBox
            // 
            this.SslPathTextBox.BackColor = System.Drawing.Color.White;
            this.SslPathTextBox.Location = new System.Drawing.Point(16, 3);
            this.SslPathTextBox.Name = "SslPathTextBox";
            this.SslPathTextBox.ReadOnly = true;
            this.SslPathTextBox.Size = new System.Drawing.Size(351, 20);
            this.SslPathTextBox.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(13, 175);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "SSL Certificate";
            // 
            // OpenSslCertificateButton
            // 
            this.OpenSslCertificateButton.Location = new System.Drawing.Point(373, 1);
            this.OpenSslCertificateButton.Name = "OpenSslCertificateButton";
            this.OpenSslCertificateButton.Size = new System.Drawing.Size(31, 23);
            this.OpenSslCertificateButton.TabIndex = 11;
            this.OpenSslCertificateButton.Text = "...";
            this.OpenSslCertificateButton.UseVisualStyleBackColor = true;
            this.OpenSslCertificateButton.Click += new System.EventHandler(this.OpenSslCertificateButton_Click);
            // 
            // CreateSslCertificate
            // 
            this.CreateSslCertificate.Location = new System.Drawing.Point(410, 1);
            this.CreateSslCertificate.Name = "CreateSslCertificate";
            this.CreateSslCertificate.Size = new System.Drawing.Size(56, 23);
            this.CreateSslCertificate.TabIndex = 12;
            this.CreateSslCertificate.Text = "Create";
            this.CreateSslCertificate.UseVisualStyleBackColor = true;
            this.CreateSslCertificate.Click += new System.EventHandler(this.CreateSslCertificate_Click);
            // 
            // SslPasswordTextBox
            // 
            this.SslPasswordTextBox.BackColor = System.Drawing.Color.White;
            this.SslPasswordTextBox.Location = new System.Drawing.Point(78, 32);
            this.SslPasswordTextBox.Name = "SslPasswordTextBox";
            this.SslPasswordTextBox.Size = new System.Drawing.Size(386, 20);
            this.SslPasswordTextBox.TabIndex = 13;
            this.SslPasswordTextBox.UseSystemPasswordChar = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(13, 35);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Password:";
            // 
            // generateSslCertificateRadioButton
            // 
            this.generateSslCertificateRadioButton.AutoSize = true;
            this.generateSslCertificateRadioButton.Checked = true;
            this.generateSslCertificateRadioButton.Location = new System.Drawing.Point(16, 191);
            this.generateSslCertificateRadioButton.Name = "generateSslCertificateRadioButton";
            this.generateSslCertificateRadioButton.Size = new System.Drawing.Size(114, 17);
            this.generateSslCertificateRadioButton.TabIndex = 15;
            this.generateSslCertificateRadioButton.TabStop = true;
            this.generateSslCertificateRadioButton.Text = "Generate randomly";
            this.generateSslCertificateRadioButton.UseVisualStyleBackColor = true;
            // 
            // createSslCertificateRadioButton
            // 
            this.createSslCertificateRadioButton.AutoSize = true;
            this.createSslCertificateRadioButton.Location = new System.Drawing.Point(16, 214);
            this.createSslCertificateRadioButton.Name = "createSslCertificateRadioButton";
            this.createSslCertificateRadioButton.Size = new System.Drawing.Size(56, 17);
            this.createSslCertificateRadioButton.TabIndex = 16;
            this.createSslCertificateRadioButton.Text = "Create";
            this.createSslCertificateRadioButton.UseVisualStyleBackColor = true;
            this.createSslCertificateRadioButton.CheckedChanged += new System.EventHandler(this.createSslCertificateRadioButton_CheckedChanged);
            // 
            // createSslCertPanel
            // 
            this.createSslCertPanel.Controls.Add(this.SslPathTextBox);
            this.createSslCertPanel.Controls.Add(this.OpenSslCertificateButton);
            this.createSslCertPanel.Controls.Add(this.CreateSslCertificate);
            this.createSslCertPanel.Controls.Add(this.SslPasswordTextBox);
            this.createSslCertPanel.Controls.Add(this.label6);
            this.createSslCertPanel.Enabled = false;
            this.createSslCertPanel.Location = new System.Drawing.Point(0, 237);
            this.createSslCertPanel.Name = "createSslCertPanel";
            this.createSslCertPanel.Size = new System.Drawing.Size(479, 58);
            this.createSslCertPanel.TabIndex = 17;
            // 
            // CreateSettingsForm
            // 
            this.AcceptButton = this.StartButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(478, 336);
            this.Controls.Add(this.createSslCertPanel);
            this.Controls.Add(this.createSslCertificateRadioButton);
            this.Controls.Add(this.generateSslCertificateRadioButton);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.PasswordTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.PortNumericUpDown);
            this.Controls.Add(this.IpAddressComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Initialize Server";
            ((System.ComponentModel.ISupportInitialize)(this.PortNumericUpDown)).EndInit();
            this.createSslCertPanel.ResumeLayout(false);
            this.createSslCertPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox IpAddressComboBox;
        private System.Windows.Forms.NumericUpDown PortNumericUpDown;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.TextBox SslPathTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button OpenSslCertificateButton;
        private System.Windows.Forms.Button CreateSslCertificate;
        private System.Windows.Forms.TextBox SslPasswordTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton generateSslCertificateRadioButton;
        private System.Windows.Forms.RadioButton createSslCertificateRadioButton;
        private System.Windows.Forms.Panel createSslCertPanel;
    }
}