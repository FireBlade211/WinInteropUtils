using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FireBlade.WinInteropUtils.WinForms.Explorer;

namespace WinInteropUtils_Test_App
{
    public partial class WiuWinFormsShellControlsTestForm : Form
    {
        public WiuWinFormsShellControlsTestForm()
        {
            InitializeComponent();

            //foreach (var item in Enum.GetValues<ShellViewMode>())
            //    toolStripComboBox1.Items.Add(item);

            //toolStripComboBox1.SelectedIndex = (int)ShellViewMode.Details - 1;
        }

        private void WiuWinFormsShellControlsTestForm_Load(object sender, EventArgs e)
        {
            //shellView1.Navigate("C:\\");
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (toolStripComboBox1.SelectedItem is ShellViewMode mode)
                //shellView1.View = mode;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //shellView1.Path = toolStripTextBox1.Text;
        }
    }
}
