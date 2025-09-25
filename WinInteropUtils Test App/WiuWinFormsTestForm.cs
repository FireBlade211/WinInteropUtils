using FireBlade.WinInteropUtils.WinForms;

namespace WinInteropUtils_Test_App
{
    public partial class WiuWinFormsTestForm : Form
    {
        public WiuWinFormsTestForm()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBarEx1.PerformStep();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBarEx4.StepBy((int)numericUpDown1.Value);
        }
    }
}
