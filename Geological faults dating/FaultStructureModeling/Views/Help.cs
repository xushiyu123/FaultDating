using System;
using System.Windows.Forms;

namespace FaultStructureModeling.Views
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
        }

        private void Help_Load(object sender, EventArgs e)
        {
            this.axAcroPDF1.LoadFile(Application.StartupPath + @"\help.pdf");
        }
    }
}
