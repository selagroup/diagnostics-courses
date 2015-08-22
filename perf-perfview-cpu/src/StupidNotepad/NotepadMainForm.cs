using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace StupidNotepad
{
    public partial class NotepadMainForm : Form
    {
        public NotepadMainForm()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Stupid Notepad replacement.");
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select a file to open";
            ofd.InitialDirectory = Environment.CurrentDirectory;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string originalFilename = Path.GetFileName(ofd.FileName);
                mainText.Text = File.ReadAllText(originalFilename);
            }
        }

        private void NotepadMainForm_Load(object sender, EventArgs e)
        {
            File.WriteAllText(" MyFile ", "Hello World");
        }

        private SpellCheckService spellChecker = new SpellCheckService();

        private void mainText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!spellChecker.CheckSpelling(mainText.Text))
            {
                status.Text = "Spelling error";
            }
            else
            {
                status.Text = "OK";
            }
        }
    }
}
