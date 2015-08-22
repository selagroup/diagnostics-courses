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
        private readonly object _mainLock = "MainLock";
        private readonly object _secondaryLock = "SecondaryLock";

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

            listBox1.Items.Clear();
            foreach (string file in Directory.GetFiles(e.Node.Name))
            {
                listBox1.Items.Add(Path.GetFileName(file));
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            string file = listBox1.SelectedItem as string;
            if (String.IsNullOrEmpty(file))
                return;

            file = Path.Combine(treeView1.SelectedNode.Name, file);

            lock (_mainLock)
            {
                Thread thread = new Thread(LaunchNotepad);
                thread.Start(file);
                
                Thread.Sleep(200);
                lock (_secondaryLock)
                {
                    thread.Join();
                }
            }
        }

        private void LaunchNotepad(object state)
        {
            string file = (string)state;
            lock (_secondaryLock)
            {
                Process process = Process.Start(@"C:\windows\notepad.exe", file);
                lock (_mainLock)
                {
                    this.BeginInvoke((MethodInvoker)(() => 
                        {this.Text = "Launched secondary process";}));
                }
            }
        }
    }
}
