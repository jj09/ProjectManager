using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SampleApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.InitialDirectory = ".";
            dialog.Title = "Select a text file";
            if (dialog.ShowDialog() == DialogResult.OK)
                path.Text = dialog.FileName;

        }

        private void start_Click(object sender, EventArgs e)
        {
            //put algorithm code here
        }
    }
}
