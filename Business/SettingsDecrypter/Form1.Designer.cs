namespace SettingsDecrypter
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
            this.settingsRichTextBox = new System.Windows.Forms.RichTextBox();
            this.signatureTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.decryptButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // settingsRichTextBox
            // 
            this.settingsRichTextBox.Location = new System.Drawing.Point(12, 12);
            this.settingsRichTextBox.Name = "settingsRichTextBox";
            this.settingsRichTextBox.Size = new System.Drawing.Size(840, 387);
            this.settingsRichTextBox.TabIndex = 0;
            this.settingsRichTextBox.Text = "";
            // 
            // signatureTextBox
            // 
            this.signatureTextBox.Location = new System.Drawing.Point(12, 418);
            this.signatureTextBox.Name = "signatureTextBox";
            this.signatureTextBox.Size = new System.Drawing.Size(743, 20);
            this.signatureTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 402);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Signature:";
            // 
            // decryptButton
            // 
            this.decryptButton.Location = new System.Drawing.Point(761, 416);
            this.decryptButton.Name = "decryptButton";
            this.decryptButton.Size = new System.Drawing.Size(91, 23);
            this.decryptButton.TabIndex = 3;
            this.decryptButton.Text = "Decrypt";
            this.decryptButton.UseVisualStyleBackColor = true;
            this.decryptButton.Click += new System.EventHandler(this.decryptButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(864, 455);
            this.Controls.Add(this.decryptButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.signatureTextBox);
            this.Controls.Add(this.settingsRichTextBox);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings Decrypter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox settingsRichTextBox;
        private System.Windows.Forms.TextBox signatureTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button decryptButton;
    }
}

