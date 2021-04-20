using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Orcus.StaticCommands.SystemLock
{
    partial class SystemLockForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.SuspendLayout();
            // 
            // SystemLockForm
            // 
            this.AutoScaleDimensions = new SizeF(11F, 25F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1177, 652);
            this.Font = new Font("Segoe UI Semibold", 20, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = FormBorderStyle.None;
            this.Margin = new Padding(6);
            this.Name = "SystemLockForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Text = "Orcus System Lock";
            this.ResumeLayout(false);

        }

        #endregion
    }
}