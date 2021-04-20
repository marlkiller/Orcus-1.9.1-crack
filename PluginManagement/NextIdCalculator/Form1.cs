using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NextIdCalculator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.SelectedPath = @"E:\Dokumente\Visual Studio 2015\Projects\Orcus\Source\Orcus\Commands";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                var dir = new DirectoryInfo(folderBrowser.SelectedPath);
                var list = new List<Tuple<int, string>>();
                foreach (var directory in dir.GetDirectories())
                {
                    list.AddRange(
                        directory.GetFiles("*.cs")
                            .Select(
                                x =>
                                    Regex.Match(File.ReadAllText(x.FullName),
                                        @"class (?<className>(.*?)) : Command.*protected override uint GetId\(\)\s*?{\s*?return (?<id>([0-9]{1,2}));\s*?}",
                                        RegexOptions.Singleline))
                            .Where(x => x.Success)
                            .Select(x => Tuple.Create(int.Parse(x.Groups["id"].Value), x.Groups["className"].Value)));
                }
                label1.Text = (list.OrderByDescending(x => x.Item1).First().Item1 + 1).ToString();
                richTextBox1.Text = list.OrderByDescending(x => x.Item1).Aggregate("", (x, y) => x + y.Item1 + " - " + y.Item2 + "\r\n");
            }
        }
    }
}