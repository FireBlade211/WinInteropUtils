using System.Runtime.InteropServices;

namespace WinInteropUtils_Test_App
{
    public partial class WindowPickerForm : Form
    {
        public nint? Hwnd = null;

        public WindowPickerForm()
        {
            InitializeComponent();

        }

        // May be included in next version of WinInteropUtils!
        [LibraryImport("user32.dll")]
        private static partial short GetAsyncKeyState(int vKey);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetCursorPos(out POINT lpPoint);

        [LibraryImport("user32.dll")]
        private static partial IntPtr WindowFromPoint(POINT Point);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        public static nint GetHWnd(IWin32Window? owner = null)
        {
            var dlg = new WindowPickerForm();
            dlg.ShowDialog(owner);

            while (true)
            {
                Thread.Sleep(50);
                if (dlg.Hwnd != null)
                {
                    return (nint)dlg.Hwnd;
                }
            }
        }

        private void WindowPickerForm_Shown(object sender, EventArgs e)
        {
            //Thread.Sleep(600);
        }

        private void WindowPickerForm_Load(object sender, EventArgs e)
        {
            var co = new CursorOverlay();
            co.Show();

            while (true)
            {
                Thread.Sleep(50);
                // VK_LBUTTON = 0x01
                if ((GetAsyncKeyState(0x01) & 0x8000) != 0)
                {
                    break;
                }

                if (GetAsyncKeyState(0x1B) != 0)
                {
                    co.Close();
                    Close();
                    return;
                }
            }

            if (!co.IsDisposed)
            {
                co.Close();
            }
            Thread.Sleep(100); // Allow time for the click to register

            if (GetCursorPos(out POINT pt))
            {
                IntPtr hwnd = WindowFromPoint(pt);
                if (hwnd == Handle && ParentForm != null)
                {
                    Hwnd = ParentForm.Handle;
                }

                Hwnd = hwnd;
            }

            Close();
        }
    }

    public class CursorOverlay : Form
    {
        public CursorOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Bounds = Screen.PrimaryScreen?.Bounds ?? Bounds;
            TopMost = true;
            Opacity = 0.5;
            BackColor = Color.Black;
            ShowInTaskbar = false;
            KeyPreview = true;
            KeyUp += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Close();
                }
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Capture all mouse clicks even outside the client area
            Capture = true;
            Cursor = Cursors.Cross;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // User clicked anywhere — exit overlay
            Close();
        }

        // Prevent Alt+Tab from showing the form
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80; // WS_EX_TOOLWINDOW
                return cp;
            }
        }
    }
}
