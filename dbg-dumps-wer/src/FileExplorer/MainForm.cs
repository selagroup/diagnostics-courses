using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace FileExplorer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private static readonly string RootPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        private bool _initializationComplete;

        private void MainForm_Load(object sender, EventArgs e)
        {
            TreeNode root = treeView1.Nodes.Add(RootPath, RootPath);
            RecursivelyFillTreeview(root, RootPath);
            _initializationComplete = true;
        }

        private void RecursivelyFillTreeview(TreeNode node, string path)
        {
            try
            {
                foreach (string subFolder in Directory.GetDirectories(path))
                {
                    TreeNode child = node.Nodes.Add(subFolder, Path.GetFileName(subFolder));
                    RecursivelyFillTreeview(child, subFolder);
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!_initializationComplete)
                return;

            TreeNode node = e.Node;
            listBox1.Items.Clear();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                foreach (string file in Directory.GetFiles(node.Name))
                {
                    listBox1.Items.Add(Path.GetFileName(file));
                }
            });
            node = null;
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            string file = listBox1.SelectedItem as string;
            if (String.IsNullOrEmpty(file))
                return;

            file = Path.Combine(treeView1.SelectedNode.Name, file);

            Process process = Process.Start(@"C:\windows\notepad.exe", file);
            process.WaitForExit();
        }
    }
}
