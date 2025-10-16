using System.Diagnostics;
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

        private void hotKeyBox2_HotKeyChanged(object sender, EventArgs e)
        {
            label4.Visible = !label4.Visible;
        }

        private void slider2_ValueChanging(object sender, SliderValueChangingEventArgs e)
        {
            if (e.NewValue < slider2.SelectionRangeStart || e.NewValue > slider2.SelectionRangeEnd) e.Cancel = true;
            Debug.WriteLine($"Cancelled: {e.Cancel}\nNew value: {e.NewValue}");
        }

        //private void hyperLink1_LinkClicked(object sender, FireBlade.WinInteropUtils.WinForms.HyperLinkLinkClickedEventArgs e)
        //{
        //    Debug.WriteLine("Link clicked!");

        //    if (e.ClickedLink.Index == 0)
        //        Process.Start(new ProcessStartInfo
        //        {
        //            UseShellExecute = true,
        //            FileName = e.ClickedLink.HRef
        //        });
        //    else
        //        if (e.ClickedLink.Id?.Equals("test") ?? false)
        //            MessageBox.Show("ID link clicked!");
        //}
    }
}
