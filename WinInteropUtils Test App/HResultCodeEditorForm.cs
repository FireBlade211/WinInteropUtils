using FireBlade.WinInteropUtils;
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
    public partial class HResultCodeEditorForm : UserControl
    {
        public HResultCodeEditorForm()
        {
            InitializeComponent();
        }

        private void HResultCodeEditorForm_Load(object sender, EventArgs e)
        {
            comboBox1.BeginUpdate();

            foreach (var facility in Enum.GetValues<Facility>())
                comboBox1.Items.Add(facility);

            comboBox1.EndUpdate();
        }
    }

}
