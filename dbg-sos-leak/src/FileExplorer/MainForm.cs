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

        class FileInformation
        {
            public static event EventHandler FileInformationNeedsRefresh;

            public FileInformation(string fullPath)
            {
                Path = fullPath;
                Name = System.IO.Path.GetFileName(Path);
                FirstFewLines = File.ReadAllLines(Path).Take(100).ToArray();
                FileInformationNeedsRefresh += FileInformation_FileInformationNeedsRefresh;
            }

            private void FileInformation_FileInformationNeedsRefresh(object sender, EventArgs e)
            {
                Name = System.IO.Path.GetFileName(Path);
                FirstFewLines = File.ReadAllLines(Path).Take(100).ToArray();
            }

            public string Path { get; set; }
            public string Name { get; set; }
            public string[] FirstFewLines { get; private set; }

            public override string ToString()
            {
                return Name;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            TreeNode root = treeView1.Nodes.Add(RootPath);
            RecursivelyFillTreeview(root, RootPath);
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
            if (!String.IsNullOrEmpty(e.Node.Name))
            {
                listBox1.Items.Clear();
                foreach (string file in Directory.GetFiles(e.Node.Name))
                {
                    listBox1.Items.Add(new FileInformation(file));
                }
            }
        }

        private static void InvokeNotepadAsync(string file, ManualResetEvent whenDone)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    Process process = Process.Start(@"C:\Windows\notepad.exe", file);
                    process.WaitForExit();
                    whenDone.Set();
                }
                catch { }
            });
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            FileInformation file = listBox1.SelectedItem as FileInformation;
            if (file == null)
                return;

            ManualResetEvent waitEvent = new ManualResetEvent(false);
            InvokeNotepadAsync(file.Path, waitEvent);
            waitEvent.WaitOne();
        }
    }
}
