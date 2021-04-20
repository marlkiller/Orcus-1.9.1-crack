namespace Orcus.Server.Forms
{
    partial class SettingsWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsWindow));
            this.AutostartCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.EnableUpdaterCheckBox = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.SslPasswordTextBox = new System.Windows.Forms.TextBox();
            this.CreateSslCertificate = new System.Windows.Forms.Button();
            this.OpenSslCertificateButton = new System.Windows.Forms.Button();
            this.SslPathTextBox = new System.Windows.Forms.TextBox();
            this.PluginsListBox = new System.Windows.Forms.ListBox();
            this.PluginSettingsPanel = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.Ip2LocationEmailTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.GeoIpCheckBox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.Ip2LocationPasswordTextBox = new System.Windows.Forms.TextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.SearchForUpdatesButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // AutostartCheckBox
            // 
            this.AutostartCheckBox.AutoSize = true;
            this.AutostartCheckBox.Location = new System.Drawing.Point(16, 35);
            this.AutostartCheckBox.Name = "AutostartCheckBox";
            this.AutostartCheckBox.Size = new System.Drawing.Size(68, 17);
            this.AutostartCheckBox.TabIndex = 20;
            this.AutostartCheckBox.Text = "Autostart";
            this.AutostartCheckBox.UseVisualStyleBackColor = true;
            this.AutostartCheckBox.CheckedChanged += new System.EventHandler(this.AutostartCheckBox_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 21);
            this.label2.TabIndex = 21;
            this.label2.Text = "Common";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Location = new System.Drawing.Point(12, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 21);
            this.label1.TabIndex = 22;
            this.label1.Text = "IP Updater";
            // 
            // EnableUpdaterCheckBox
            // 
            this.EnableUpdaterCheckBox.AutoSize = true;
            this.EnableUpdaterCheckBox.Location = new System.Drawing.Point(16, 91);
            this.EnableUpdaterCheckBox.Name = "EnableUpdaterCheckBox";
            this.EnableUpdaterCheckBox.Size = new System.Drawing.Size(100, 17);
            this.EnableUpdaterCheckBox.TabIndex = 23;
            this.EnableUpdaterCheckBox.Text = "Enable Updater";
            this.EnableUpdaterCheckBox.UseVisualStyleBackColor = true;
            this.EnableUpdaterCheckBox.CheckedChanged += new System.EventHandler(this.EnableIpUpdater_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label6.Location = new System.Drawing.Point(12, 220);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(110, 21);
            this.label6.TabIndex = 27;
            this.label6.Text = "SSL Certificate";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(16, 274);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 13);
            this.label7.TabIndex = 32;
            this.label7.Text = "Password:";
            // 
            // SslPasswordTextBox
            // 
            this.SslPasswordTextBox.BackColor = System.Drawing.Color.White;
            this.SslPasswordTextBox.Location = new System.Drawing.Point(81, 271);
            this.SslPasswordTextBox.Name = "SslPasswordTextBox";
            this.SslPasswordTextBox.Size = new System.Drawing.Size(545, 20);
            this.SslPasswordTextBox.TabIndex = 31;
            this.SslPasswordTextBox.UseSystemPasswordChar = true;
            // 
            // CreateSslCertificate
            // 
            this.CreateSslCertificate.Location = new System.Drawing.Point(570, 243);
            this.CreateSslCertificate.Name = "CreateSslCertificate";
            this.CreateSslCertificate.Size = new System.Drawing.Size(56, 23);
            this.CreateSslCertificate.TabIndex = 30;
            this.CreateSslCertificate.Text = "Create";
            this.CreateSslCertificate.UseVisualStyleBackColor = true;
            this.CreateSslCertificate.Click += new System.EventHandler(this.CreateSslCertificate_Click);
            // 
            // OpenSslCertificateButton
            // 
            this.OpenSslCertificateButton.Location = new System.Drawing.Point(533, 243);
            this.OpenSslCertificateButton.Name = "OpenSslCertificateButton";
            this.OpenSslCertificateButton.Size = new System.Drawing.Size(31, 23);
            this.OpenSslCertificateButton.TabIndex = 29;
            this.OpenSslCertificateButton.Text = "...";
            this.OpenSslCertificateButton.UseVisualStyleBackColor = true;
            this.OpenSslCertificateButton.Click += new System.EventHandler(this.OpenSslCertificateButton_Click);
            // 
            // SslPathTextBox
            // 
            this.SslPathTextBox.BackColor = System.Drawing.Color.White;
            this.SslPathTextBox.Location = new System.Drawing.Point(17, 245);
            this.SslPathTextBox.Name = "SslPathTextBox";
            this.SslPathTextBox.ReadOnly = true;
            this.SslPathTextBox.Size = new System.Drawing.Size(510, 20);
            this.SslPathTextBox.TabIndex = 28;
            // 
            // PluginsListBox
            // 
            this.PluginsListBox.FormattingEnabled = true;
            this.PluginsListBox.Location = new System.Drawing.Point(16, 110);
            this.PluginsListBox.Name = "PluginsListBox";
            this.PluginsListBox.Size = new System.Drawing.Size(132, 95);
            this.PluginsListBox.TabIndex = 33;
            this.PluginsListBox.SelectedIndexChanged += new System.EventHandler(this.PluginsListBox_SelectedIndexChanged);
            // 
            // PluginSettingsPanel
            // 
            this.PluginSettingsPanel.Location = new System.Drawing.Point(154, 110);
            this.PluginSettingsPanel.Name = "PluginSettingsPanel";
            this.PluginSettingsPanel.Size = new System.Drawing.Size(472, 95);
            this.PluginSettingsPanel.TabIndex = 34;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label3.Location = new System.Drawing.Point(12, 306);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 21);
            this.label3.TabIndex = 35;
            this.label3.Text = "IP2Location";
            // 
            // Ip2LocationEmailTextBox
            // 
            this.Ip2LocationEmailTextBox.Location = new System.Drawing.Point(16, 374);
            this.Ip2LocationEmailTextBox.Name = "Ip2LocationEmailTextBox";
            this.Ip2LocationEmailTextBox.Size = new System.Drawing.Size(332, 20);
            this.Ip2LocationEmailTextBox.TabIndex = 36;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 358);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 13);
            this.label4.TabIndex = 37;
            this.label4.Text = "Email Address";
            // 
            // GeoIpCheckBox
            // 
            this.GeoIpCheckBox.AutoSize = true;
            this.GeoIpCheckBox.Location = new System.Drawing.Point(16, 333);
            this.GeoIpCheckBox.Name = "GeoIpCheckBox";
            this.GeoIpCheckBox.Size = new System.Drawing.Size(65, 17);
            this.GeoIpCheckBox.TabIndex = 38;
            this.GeoIpCheckBox.Text = "Enabled";
            this.GeoIpCheckBox.UseVisualStyleBackColor = true;
            this.GeoIpCheckBox.CheckedChanged += new System.EventHandler(this.GeoIpCheckBox_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(354, 358);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 40;
            this.label5.Text = "Password";
            // 
            // Ip2LocationPasswordTextBox
            // 
            this.Ip2LocationPasswordTextBox.Location = new System.Drawing.Point(354, 374);
            this.Ip2LocationPasswordTextBox.Name = "Ip2LocationPasswordTextBox";
            this.Ip2LocationPasswordTextBox.Size = new System.Drawing.Size(272, 20);
            this.Ip2LocationPasswordTextBox.TabIndex = 39;
            this.Ip2LocationPasswordTextBox.UseSystemPasswordChar = true;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(109, 312);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(65, 13);
            this.linkLabel1.TabIndex = 41;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "register here";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // SearchForUpdatesButton
            // 
            this.SearchForUpdatesButton.Location = new System.Drawing.Point(494, 31);
            this.SearchForUpdatesButton.Name = "SearchForUpdatesButton";
            this.SearchForUpdatesButton.Size = new System.Drawing.Size(132, 23);
            this.SearchForUpdatesButton.TabIndex = 42;
            this.SearchForUpdatesButton.Text = "Search for updates";
            this.SearchForUpdatesButton.UseVisualStyleBackColor = true;
            this.SearchForUpdatesButton.Click += new System.EventHandler(this.SearchForUpdatesButton_Click);
            // 
            // SettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(638, 407);
            this.Controls.Add(this.SearchForUpdatesButton);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.Ip2LocationPasswordTextBox);
            this.Controls.Add(this.GeoIpCheckBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Ip2LocationEmailTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.PluginSettingsPanel);
            this.Controls.Add(this.PluginsListBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.SslPasswordTextBox);
            this.Controls.Add(this.CreateSslCertificate);
            this.Controls.Add(this.OpenSslCertificateButton);
            this.Controls.Add(this.SslPathTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.EnableUpdaterCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.AutostartCheckBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsWindow";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox AutostartCheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox EnableUpdaterCheckBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox SslPasswordTextBox;
        private System.Windows.Forms.Button CreateSslCertificate;
        private System.Windows.Forms.Button OpenSslCertificateButton;
        private System.Windows.Forms.TextBox SslPathTextBox;
        private System.Windows.Forms.ListBox PluginsListBox;
        private System.Windows.Forms.Panel PluginSettingsPanel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox Ip2LocationEmailTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox GeoIpCheckBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox Ip2LocationPasswordTextBox;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button SearchForUpdatesButton;
    }
}