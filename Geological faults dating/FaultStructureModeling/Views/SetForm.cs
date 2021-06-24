using System;
using System.IO;
using FaultStructureModeling.Controllers;
using FaultStructureModeling.Entities;
using System.Windows.Forms;

namespace FaultStructureModeling.Views
{
    public partial class SetForm : Form
    {
        public SetForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("设置成功！");
            StreamWriter writer = new StreamWriter(Application.StartupPath + @"\config.txt", false, System.Text.Encoding.Default);
            writer.WriteLine(Parameters.Workspace);
            writer.WriteLine(Parameters.TempDirectory);
            writer.WriteLine(Parameters.Step);
            Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.textBox2.Text = Parameters.TempDirectory = AuxiliaryTools.FolderBrowser("");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = Parameters.Workspace = AuxiliaryTools.FolderBrowser("");
        }
    }
}
