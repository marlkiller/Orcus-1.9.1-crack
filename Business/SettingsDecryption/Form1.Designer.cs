namespace SettingsDecryption
{
    partial class Form1
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
            this.decryptButton = new System.Windows.Forms.Button();
            this.encryptedTextTextBox = new System.Windows.Forms.TextBox();
            this.encryptionKeyTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.showIpAddressesButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // decryptButton
            // 
            this.decryptButton.Location = new System.Drawing.Point(12, 92);
            this.decryptButton.Name = "decryptButton";
            this.decryptButton.Size = new System.Drawing.Size(75, 23);
            this.decryptButton.TabIndex = 0;
            this.decryptButton.Text = "Decrypt (Old)";
            this.decryptButton.UseVisualStyleBackColor = true;
            this.decryptButton.Click += new System.EventHandler(this.decryptButton_Click);
            // 
            // encryptedTextTextBox
            // 
            this.encryptedTextTextBox.Location = new System.Drawing.Point(12, 66);
            this.encryptedTextTextBox.Name = "encryptedTextTextBox";
            this.encryptedTextTextBox.Size = new System.Drawing.Size(502, 20);
            this.encryptedTextTextBox.TabIndex = 1;
            // 
            // encryptionKeyTextBox
            // 
            this.encryptionKeyTextBox.Location = new System.Drawing.Point(12, 25);
            this.encryptionKeyTextBox.Name = "encryptionKeyTextBox";
            this.encryptionKeyTextBox.Size = new System.Drawing.Size(502, 20);
            this.encryptionKeyTextBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Encryption Key";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Text";
            // 
            // showIpAddressesButton
            // 
            this.showIpAddressesButton.Location = new System.Drawing.Point(520, 64);
            this.showIpAddressesButton.Name = "showIpAddressesButton";
            this.showIpAddressesButton.Size = new System.Drawing.Size(111, 23);
            this.showIpAddressesButton.TabIndex = 5;
            this.showIpAddressesButton.Text = ">> To ip addresses";
            this.showIpAddressesButton.UseVisualStyleBackColor = true;
            this.showIpAddressesButton.Click += new System.EventHandler(this.showIpAddressesButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 293);
            this.Controls.Add(this.showIpAddressesButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.encryptionKeyTextBox);
            this.Controls.Add(this.encryptedTextTextBox);
            this.Controls.Add(this.decryptButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button decryptButton;
        private System.Windows.Forms.TextBox encryptedTextTextBox;
        private System.Windows.Forms.TextBox encryptionKeyTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button showIpAddressesButton;
    }
}

