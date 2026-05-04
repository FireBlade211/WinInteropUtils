using FireBlade.WinInteropUtils;
using FireBlade.WinInteropUtils.ComponentObjectModel;
using FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces;
using FireBlade.WinInteropUtils.Memory;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static FireBlade.WinInteropUtils.Macros;

namespace WinInteropUtils_Test_App
{
    public partial class MemoryUtilsTestForm : Form
    {
        private MemoryString? _str;

        public MemoryUtilsTestForm()
        {
            InitializeComponent();

            // Create a STATIC control using WinInteropUtils.
            // We can't use a WinForms Label because it ignores
            // our text
            _label = new Window(0, "STATIC", "String...", WindowStyles.Child | WindowStyles.Visible, new Point(14, 43),
                new Size(188, 21), Window.FromHandle(Handle), null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _str?.Dispose();
            _str = new MemoryString(textBox1.Text);
        }

        private void MemoryUtilsTestForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _str?.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _str?.Dispose();
            _str = new MemoryString(textBox1.Text, MemoryStringEncoding.Ansi);
        }

        private Window _label;

        [LibraryImport("user32.dll", EntryPoint = "SetWindowTextW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetWindowTextW(nint hWnd, nint lpString);

        [LibraryImport("user32.dll", EntryPoint = "SetWindowTextA")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetWindowTextA(nint hWnd, nint lpString);

        private void button2_Click(object sender, EventArgs e)
        {
            if (_str != null)
            {
                if (_str.Encoding == MemoryStringEncoding.Unicode)
                    SetWindowTextW(_label.Handle, _str);
                else
                    SetWindowTextA(_label.Handle, _str);

                label2.Text = $"{_str.ByteCount} B";
            }
        }

        private void MemoryUtilsTestForm_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            HResult hr = COM.Initialize();
            if (Succeeded(hr))
            {
                try
                {
                    using (ComSmartPointer<IFileOpenDialog> sptr = new ComSmartPointer<IFileOpenDialog>(
                        new Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7"), null, COM.CreateInstanceContext.InprocServer))
                    {
                        sptr.Interface.SetTitle("Custom Dialog");

                        hr = sptr.Interface.Show(Handle);
                        if (Succeeded(hr))
                        {
                            hr = sptr.Interface.GetResult(out nint ppsi);
                            
                            if (Succeeded(hr))
                            {
                                using (ComSmartPointer<IShellItem> iptr = new ComSmartPointer<IShellItem>(
                                    (IShellItem)Marshal.GetTypedObjectForIUnknown(ppsi, typeof(IShellItem))))
                                {
                                    hr = iptr.Interface.GetDisplayName(SIGDN.SIGDN_NORMALDISPLAY, out nint psz);

                                    if (Succeeded(hr))
                                    {
                                        string? path = Marshal.PtrToStringUni(psz);

                                        if (path != null)
                                            MessageBox.Show($"You chose: {path}", "File Dialog Result", MessageBoxButtons.OK,
                                                MessageBoxIcon.Information);

                                        Marshal.FreeCoTaskMem(psz);
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
}
