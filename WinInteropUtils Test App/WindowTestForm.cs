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
    public partial class WindowTestForm : Form
    {
        private Window? Wnd;

#pragma warning disable CS8618
        public WindowTestForm()
        {
            InitializeComponent();
        }
#pragma warning restore

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            Wnd = Window.FromHandle(Handle);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label1.Text = Wnd?.Text;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            label2.Text = Wnd?.State.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (Wnd != null)
                Wnd.Text = textBox1.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var className = "WinInteropUtils Test Window Class";

            try
            {
                var wc = new WindowClass();
                wc.ClassName = className;
                wc.WindowProcedure = MyWindowProc;

                wc.Register();
            }
            catch { }

            var wnd = new Window(0, className, "Test Window", WindowStyles.OverlappedWindow | WindowStyles.Visible,
                Window.WindowDefaultLocation, Window.WindowDefaultSize);
        }

        private const int WM_CREATE = 0x0001;

        private nint MyWindowProc(Window wnd, uint uMsg, nuint wParam, object lParam)
        {
            switch (uMsg)
            {
                case WM_CREATE:
                    var button = new Window(0, "BUTTON", "Click me!", WindowStyles.Child | WindowStyles.Visible,
                        new Point(24, 24), new Size(120, 30), wnd);
                    break;
            }

            return WindowClass.DefaultWndProc(wnd, uMsg, wParam, lParam);
        }

        private const int BS_AUTORADIOBUTTON = 0x0009;

        private void button2_Click(object sender, EventArgs e)
        {
            var button = new Window(0, "BUTTON", "Win32 Radio", WindowStyles.Child | WindowStyles.Visible | BS_AUTORADIOBUTTON,
    new Point(24, 0), new Size(100, 20), (Window)Handle);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Window.FromHandle(Handle)?.Destroy();
        }
    }
}
