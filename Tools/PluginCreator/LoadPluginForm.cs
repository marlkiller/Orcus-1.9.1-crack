using System;
using System.Windows.Forms;

namespace PluginCreator
{
    public partial class LoadPluginForm : Form
    {
        public LoadPluginForm(Settings settings)
        {
            InitializeComponent();
            PluginListBox.DataSource = settings.PluginData;
            PluginListBox.DisplayMember = "Name";
            PluginListBox.MouseDoubleClick += PluginListBox_MouseDoubleClick;
        }

        public PluginData SelectedPluginData { get; private set; }

        private void PluginListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = PluginListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                SelectedPluginData = (PluginData) PluginListBox.Items[index];
                DialogResult = DialogResult.OK;
            }
        }

        private void LoadPluginForm_Load(object sender, EventArgs e)
        {
        }
    }
}