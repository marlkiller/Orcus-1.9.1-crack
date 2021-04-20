using System;
using System.Windows.Forms;

namespace Orcus
{
    public partial class InstallationPromptForm : Form
    {
        public InstallationPromptForm()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            button2.Focus();
        }
    }
}