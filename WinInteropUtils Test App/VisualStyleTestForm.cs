using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinInteropUtils_Test_App
{
    public partial class VisualStyleTestForm : Form
    {
        public VisualStyleTestForm()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            headingTextControl1.Text = textBox1.Text;
            visualStyleExControl1.Text = textBox1.Text;
        }
    }
}
