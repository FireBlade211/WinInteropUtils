using System.Diagnostics;

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

        private void button2_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Add click!");

            slider4.AddTick((int)numericUpDown2.Value);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Clear click!");

            slider4.ClearTicks();
        }
    }
}
