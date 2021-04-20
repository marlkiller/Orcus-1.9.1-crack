using System;
using System.Windows.Forms;

namespace PluginIdGenerator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            unchecked
            {
                textBox1.Text = ((uint) (DateTime.Now - new DateTime(2015, 7, 1)).TotalSeconds).ToString();
            }
        }
    }
}