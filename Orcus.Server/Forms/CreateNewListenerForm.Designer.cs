namespace Orcus.Server.Forms
{
    partial class CreateNewListenerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateNewListenerForm));
            this.IpAddressComboBox = new System.Windows.Forms.ComboBox();
            this.PortNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.AddButton = new System.Windows.Forms.Button();
            this.AbortButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PortNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // IpAddressComboBox
            // 
            this.IpAddressComboBox.FormattingEnabled = true;
            this.IpAddressComboBox.Location = new System.Drawing.Point(12, 11);
            this.IpAddressComboBox.Name = "IpAddressComboBox";
            this.IpAddressComboBox.Size = new System.Drawing.Size(256, 21);
            this.IpAddressComboBox.TabIndex = 0;
            // 
            // PortNumericUpDown
            // 
            this.PortNumericUpDown.Location = new System.Drawing.Point(274, 12);
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
            this.PortNumericUpDown.Size = new System.Drawing.Size(91, 20);
            this.PortNumericUpDown.TabIndex = 1;
            this.PortNumericUpDown.Value = new decimal(new int[] {
            10134,
            0,
            0,
            0});
            // 
            // AddButton
            // 
            this.AddButton.Location = new System.Drawing.Point(263, 49);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(102, 23);
            this.AddButton.TabIndex = 2;
            this.AddButton.Text = "Add";
            this.AddButton.UseVisualStyleBackColor = true;
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // AbortButton
            // 
            this.AbortButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.AbortButton.Location = new System.Drawing.Point(155, 49);
            this.AbortButton.Name = "AbortButton";
            this.AbortButton.Size = new System.Drawing.Size(102, 23);
            this.AbortButton.TabIndex = 3;
            this.AbortButton.Text = "Cancel";
            this.AbortButton.UseVisualStyleBackColor = true;
            this.AbortButton.Click += new System.EventHandler(this.AbortButton_Click);
            // 
            // CreateNewListenerForm
            // 
            this.AcceptButton = this.AddButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.AbortButton;
            this.ClientSize = new System.Drawing.Size(377, 84);
            this.Controls.Add(this.AbortButton);
            this.Controls.Add(this.AddButton);
            this.Controls.Add(this.PortNumericUpDown);
            this.Controls.Add(this.IpAddressComboBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateNewListenerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Listener";
            ((System.ComponentModel.ISupportInitialize)(this.PortNumericUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox IpAddressComboBox;
        private System.Windows.Forms.NumericUpDown PortNumericUpDown;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.Button AbortButton;
    }
}