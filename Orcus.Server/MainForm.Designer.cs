namespace Orcus.Server
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SchnorchelsLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.AdministrationsLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.IsRunningLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.ButtonStop = new System.Windows.Forms.Button();
            this.AddListenerButton = new System.Windows.Forms.Button();
            this.RemoveListenerButton = new System.Windows.Forms.Button();
            this.ListenersListBox = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.ButtonDisconnectAll = new System.Windows.Forms.Button();
            this.ButtonDisconnectSchnorchels = new System.Windows.Forms.Button();
            this.ButtonDisconnectAdministrations = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.ChangePasswordButton = new System.Windows.Forms.Button();
            this.ShowPasswordCheckBox = new System.Windows.Forms.CheckBox();
            this.SettingsButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.logProgressBar = new System.Windows.Forms.ProgressBar();
            this.LogRichTextBox = new System.Windows.Forms.RichTextBox();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.SchnorchelsLabel);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.AdministrationsLabel);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.IsRunningLabel);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(508, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 100);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Status";
            // 
            // SchnorchelsLabel
            // 
            this.SchnorchelsLabel.AutoSize = true;
            this.SchnorchelsLabel.Location = new System.Drawing.Point(89, 55);
            this.SchnorchelsLabel.Name = "SchnorchelsLabel";
            this.SchnorchelsLabel.Size = new System.Drawing.Size(13, 13);
            this.SchnorchelsLabel.TabIndex = 5;
            this.SchnorchelsLabel.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 55);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Clients:";
            // 
            // AdministrationsLabel
            // 
            this.AdministrationsLabel.AutoSize = true;
            this.AdministrationsLabel.Location = new System.Drawing.Point(89, 40);
            this.AdministrationsLabel.Name = "AdministrationsLabel";
            this.AdministrationsLabel.Size = new System.Drawing.Size(13, 13);
            this.AdministrationsLabel.TabIndex = 3;
            this.AdministrationsLabel.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Administrations:";
            // 
            // IsRunningLabel
            // 
            this.IsRunningLabel.AutoSize = true;
            this.IsRunningLabel.Location = new System.Drawing.Point(89, 25);
            this.IsRunningLabel.Name = "IsRunningLabel";
            this.IsRunningLabel.Size = new System.Drawing.Size(29, 13);
            this.IsRunningLabel.TabIndex = 1;
            this.IsRunningLabel.Text = "false";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Is running:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label2.Location = new System.Drawing.Point(8, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 21);
            this.label2.TabIndex = 1;
            this.label2.Text = "Server";
            // 
            // ButtonStart
            // 
            this.ButtonStart.Location = new System.Drawing.Point(12, 120);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(75, 23);
            this.ButtonStart.TabIndex = 2;
            this.ButtonStart.Text = "Start";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // ButtonStop
            // 
            this.ButtonStop.Location = new System.Drawing.Point(93, 120);
            this.ButtonStop.Name = "ButtonStop";
            this.ButtonStop.Size = new System.Drawing.Size(75, 23);
            this.ButtonStop.TabIndex = 3;
            this.ButtonStop.Text = "Stop";
            this.ButtonStop.UseVisualStyleBackColor = true;
            this.ButtonStop.Click += new System.EventHandler(this.ButtonStop_Click);
            // 
            // AddListenerButton
            // 
            this.AddListenerButton.Location = new System.Drawing.Point(508, 271);
            this.AddListenerButton.Name = "AddListenerButton";
            this.AddListenerButton.Size = new System.Drawing.Size(75, 23);
            this.AddListenerButton.TabIndex = 5;
            this.AddListenerButton.Text = "Add";
            this.AddListenerButton.UseVisualStyleBackColor = true;
            this.AddListenerButton.Click += new System.EventHandler(this.AddListenerButton_Click);
            // 
            // RemoveListenerButton
            // 
            this.RemoveListenerButton.Location = new System.Drawing.Point(633, 271);
            this.RemoveListenerButton.Name = "RemoveListenerButton";
            this.RemoveListenerButton.Size = new System.Drawing.Size(75, 23);
            this.RemoveListenerButton.TabIndex = 6;
            this.RemoveListenerButton.Text = "Remove";
            this.RemoveListenerButton.UseVisualStyleBackColor = true;
            this.RemoveListenerButton.Click += new System.EventHandler(this.RemoveListenerButton_Click);
            // 
            // ListenersListBox
            // 
            this.ListenersListBox.FormattingEnabled = true;
            this.ListenersListBox.Location = new System.Drawing.Point(508, 139);
            this.ListenersListBox.Name = "ListenersListBox";
            this.ListenersListBox.Size = new System.Drawing.Size(200, 121);
            this.ListenersListBox.TabIndex = 7;
            this.ListenersListBox.SelectedIndexChanged += new System.EventHandler(this.ListenersListBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label4.Location = new System.Drawing.Point(504, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 21);
            this.label4.TabIndex = 8;
            this.label4.Text = "Listeners";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label6.Location = new System.Drawing.Point(8, 163);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(96, 21);
            this.label6.TabIndex = 9;
            this.label6.Text = "Connections";
            // 
            // ButtonDisconnectAll
            // 
            this.ButtonDisconnectAll.Location = new System.Drawing.Point(12, 188);
            this.ButtonDisconnectAll.Name = "ButtonDisconnectAll";
            this.ButtonDisconnectAll.Size = new System.Drawing.Size(146, 23);
            this.ButtonDisconnectAll.TabIndex = 10;
            this.ButtonDisconnectAll.Text = "Disconnect all";
            this.ButtonDisconnectAll.UseVisualStyleBackColor = true;
            this.ButtonDisconnectAll.Click += new System.EventHandler(this.ButtonDisconnectAll_Click);
            // 
            // ButtonDisconnectSchnorchels
            // 
            this.ButtonDisconnectSchnorchels.Location = new System.Drawing.Point(164, 188);
            this.ButtonDisconnectSchnorchels.Name = "ButtonDisconnectSchnorchels";
            this.ButtonDisconnectSchnorchels.Size = new System.Drawing.Size(146, 23);
            this.ButtonDisconnectSchnorchels.TabIndex = 11;
            this.ButtonDisconnectSchnorchels.Text = "Disconnect clients";
            this.ButtonDisconnectSchnorchels.UseVisualStyleBackColor = true;
            this.ButtonDisconnectSchnorchels.Click += new System.EventHandler(this.ButtonDisconnectSchnorchels_Click);
            // 
            // ButtonDisconnectAdministrations
            // 
            this.ButtonDisconnectAdministrations.Location = new System.Drawing.Point(316, 188);
            this.ButtonDisconnectAdministrations.Name = "ButtonDisconnectAdministrations";
            this.ButtonDisconnectAdministrations.Size = new System.Drawing.Size(146, 23);
            this.ButtonDisconnectAdministrations.TabIndex = 12;
            this.ButtonDisconnectAdministrations.Text = "Disconnect administrations";
            this.ButtonDisconnectAdministrations.UseVisualStyleBackColor = true;
            this.ButtonDisconnectAdministrations.Click += new System.EventHandler(this.ButtonDisconnectAdministrations_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(8, 230);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 21);
            this.label7.TabIndex = 13;
            this.label7.Text = "Password";
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Location = new System.Drawing.Point(12, 254);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.Size = new System.Drawing.Size(254, 20);
            this.PasswordTextBox.TabIndex = 14;
            this.PasswordTextBox.UseSystemPasswordChar = true;
            // 
            // ChangePasswordButton
            // 
            this.ChangePasswordButton.Location = new System.Drawing.Point(272, 252);
            this.ChangePasswordButton.Name = "ChangePasswordButton";
            this.ChangePasswordButton.Size = new System.Drawing.Size(75, 23);
            this.ChangePasswordButton.TabIndex = 15;
            this.ChangePasswordButton.Text = "Change";
            this.ChangePasswordButton.UseVisualStyleBackColor = true;
            this.ChangePasswordButton.Click += new System.EventHandler(this.ChangePasswordButton_Click);
            // 
            // ShowPasswordCheckBox
            // 
            this.ShowPasswordCheckBox.AutoSize = true;
            this.ShowPasswordCheckBox.Location = new System.Drawing.Point(12, 280);
            this.ShowPasswordCheckBox.Name = "ShowPasswordCheckBox";
            this.ShowPasswordCheckBox.Size = new System.Drawing.Size(53, 17);
            this.ShowPasswordCheckBox.TabIndex = 17;
            this.ShowPasswordCheckBox.Text = "Show";
            this.ShowPasswordCheckBox.UseVisualStyleBackColor = true;
            this.ShowPasswordCheckBox.CheckedChanged += new System.EventHandler(this.ShowPasswordCheckBox_CheckedChanged);
            // 
            // SettingsButton
            // 
            this.SettingsButton.Location = new System.Drawing.Point(174, 120);
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(112, 23);
            this.SettingsButton.TabIndex = 19;
            this.SettingsButton.Text = "Settings";
            this.SettingsButton.UseVisualStyleBackColor = true;
            this.SettingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Black;
            this.panel1.Controls.Add(this.logProgressBar);
            this.panel1.Controls.Add(this.LogRichTextBox);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(490, 77);
            this.panel1.TabIndex = 18;
            // 
            // logProgressBar
            // 
            this.logProgressBar.Location = new System.Drawing.Point(0, 64);
            this.logProgressBar.Name = "logProgressBar";
            this.logProgressBar.Size = new System.Drawing.Size(489, 13);
            this.logProgressBar.TabIndex = 20;
            this.logProgressBar.Visible = false;
            // 
            // LogRichTextBox
            // 
            this.LogRichTextBox.BackColor = System.Drawing.Color.White;
            this.LogRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LogRichTextBox.Location = new System.Drawing.Point(1, 1);
            this.LogRichTextBox.Name = "LogRichTextBox";
            this.LogRichTextBox.ReadOnly = true;
            this.LogRichTextBox.Size = new System.Drawing.Size(488, 75);
            this.LogRichTextBox.TabIndex = 0;
            this.LogRichTextBox.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(720, 306);
            this.Controls.Add(this.SettingsButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.ShowPasswordCheckBox);
            this.Controls.Add(this.ChangePasswordButton);
            this.Controls.Add(this.PasswordTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.ButtonDisconnectAdministrations);
            this.Controls.Add(this.ButtonDisconnectSchnorchels);
            this.Controls.Add(this.ButtonDisconnectAll);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ListenersListBox);
            this.Controls.Add(this.RemoveListenerButton);
            this.Controls.Add(this.AddListenerButton);
            this.Controls.Add(this.ButtonStop);
            this.Controls.Add(this.ButtonStart);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Orcus Server";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label IsRunningLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label SchnorchelsLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label AdministrationsLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.Button ButtonStop;
        private System.Windows.Forms.Button AddListenerButton;
        private System.Windows.Forms.Button RemoveListenerButton;
        private System.Windows.Forms.ListBox ListenersListBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button ButtonDisconnectAll;
        private System.Windows.Forms.Button ButtonDisconnectSchnorchels;
        private System.Windows.Forms.Button ButtonDisconnectAdministrations;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.Button ChangePasswordButton;
        private System.Windows.Forms.CheckBox ShowPasswordCheckBox;
        private System.Windows.Forms.Button SettingsButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox LogRichTextBox;
        private System.Windows.Forms.ProgressBar logProgressBar;
    }
}

