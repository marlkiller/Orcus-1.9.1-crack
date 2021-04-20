namespace EmergencyShutdown
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.ipTextBox = new System.Windows.Forms.TextBox();
            this.portNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TakeDownButton = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.encryptionPasswordTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.selectDatabaseFileButton = new System.Windows.Forms.Button();
            this.databaseFileLabel = new System.Windows.Forms.Label();
            this.decryptDatabaseButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.portNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // ipTextBox
            // 
            this.ipTextBox.Location = new System.Drawing.Point(12, 33);
            this.ipTextBox.Name = "ipTextBox";
            this.ipTextBox.Size = new System.Drawing.Size(314, 20);
            this.ipTextBox.TabIndex = 0;
            this.ipTextBox.Text = "127.0.0.1";
            // 
            // portNumericUpDown
            // 
            this.portNumericUpDown.Location = new System.Drawing.Point(332, 33);
            this.portNumericUpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.portNumericUpDown.Name = "portNumericUpDown";
            this.portNumericUpDown.Size = new System.Drawing.Size(101, 20);
            this.portNumericUpDown.TabIndex = 1;
            this.portNumericUpDown.Value = new decimal(new int[] {
            10134,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Hostname";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(329, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port";
            // 
            // TakeDownButton
            // 
            this.TakeDownButton.Location = new System.Drawing.Point(439, 31);
            this.TakeDownButton.Name = "TakeDownButton";
            this.TakeDownButton.Size = new System.Drawing.Size(108, 23);
            this.TakeDownButton.TabIndex = 4;
            this.TakeDownButton.Text = "Take Down!";
            this.TakeDownButton.UseVisualStyleBackColor = true;
            this.TakeDownButton.Click += new System.EventHandler(this.TakeDownButton_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 59);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(530, 221);
            this.richTextBox1.TabIndex = 5;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(8, 294);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Decrypt Database";
            // 
            // encryptionPasswordTextBox
            // 
            this.encryptionPasswordTextBox.Location = new System.Drawing.Point(11, 336);
            this.encryptionPasswordTextBox.Name = "encryptionPasswordTextBox";
            this.encryptionPasswordTextBox.Size = new System.Drawing.Size(314, 20);
            this.encryptionPasswordTextBox.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 320);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Password";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(328, 320);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Database File";
            // 
            // selectDatabaseFileButton
            // 
            this.selectDatabaseFileButton.Location = new System.Drawing.Point(331, 336);
            this.selectDatabaseFileButton.Name = "selectDatabaseFileButton";
            this.selectDatabaseFileButton.Size = new System.Drawing.Size(36, 20);
            this.selectDatabaseFileButton.TabIndex = 10;
            this.selectDatabaseFileButton.Text = "...";
            this.selectDatabaseFileButton.UseVisualStyleBackColor = true;
            this.selectDatabaseFileButton.Click += new System.EventHandler(this.selectDatabaseFileButton_Click);
            // 
            // databaseFileLabel
            // 
            this.databaseFileLabel.AutoSize = true;
            this.databaseFileLabel.Location = new System.Drawing.Point(373, 340);
            this.databaseFileLabel.Name = "databaseFileLabel";
            this.databaseFileLabel.Size = new System.Drawing.Size(0, 13);
            this.databaseFileLabel.TabIndex = 11;
            // 
            // decryptDatabaseButton
            // 
            this.decryptDatabaseButton.Location = new System.Drawing.Point(11, 362);
            this.decryptDatabaseButton.Name = "decryptDatabaseButton";
            this.decryptDatabaseButton.Size = new System.Drawing.Size(118, 23);
            this.decryptDatabaseButton.TabIndex = 12;
            this.decryptDatabaseButton.Text = "Decrypt";
            this.decryptDatabaseButton.UseVisualStyleBackColor = true;
            this.decryptDatabaseButton.Click += new System.EventHandler(this.decryptDatabaseButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 394);
            this.Controls.Add(this.decryptDatabaseButton);
            this.Controls.Add(this.databaseFileLabel);
            this.Controls.Add(this.selectDatabaseFileButton);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.encryptionPasswordTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.TakeDownButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.portNumericUpDown);
            this.Controls.Add(this.ipTextBox);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Takedown";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.portNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ipTextBox;
        private System.Windows.Forms.NumericUpDown portNumericUpDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button TakeDownButton;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox encryptionPasswordTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button selectDatabaseFileButton;
        private System.Windows.Forms.Label databaseFileLabel;
        private System.Windows.Forms.Button decryptDatabaseButton;
    }
}

