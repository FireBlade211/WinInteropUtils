using FireBlade.WinInteropUtils;
using FireBlade.WinInteropUtils.WinForms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using static WinInteropUtils_Test_App.WiuWinFormsTestForm.IDropTarget;

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

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Custom icon menu item clicked!", "Click");
        }

        [ComImport]
        [Guid("00000122-0000-0000-C000-000000000046")] // IID_IDropTarget
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDropTarget
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int x;
                public int y;
            }

            void DragEnter(nint pDataObj, uint grfKeyState, POINT pt, ref uint pdwEffect);
            void DragOver(uint grfKeyState, POINT pt, ref uint pdwEffect);
            void DragLeave();
            void Drop(nint pDataObj, uint grfKeyState, POINT pt, ref uint pdwEffect);
        }

        [ComImport]
        [Guid("0000010e-0000-0000-C000-000000000046")] // IID_IDataObject
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDataObject
        {
            void GetData(ref FORMATETC format, out STGMEDIUM medium);
            void GetDataHere(ref FORMATETC format, ref STGMEDIUM medium);
            int QueryGetData(ref FORMATETC format);
            int GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut);
            int SetData(ref FORMATETC formatIn, ref STGMEDIUM medium, bool release);
            IEnumFORMATETC EnumFormatEtc(uint direction);
            int DAdvise(ref FORMATETC pFormatetc, uint advf, IAdviseSink adviseSink, out uint connection);
            void DUnadvise(uint connection);
            int EnumDAdvise(out IEnumSTATDATA enumAdvise);
        }

        public partial class ToolbarTestDropTarget : IDropTarget
        {
            public void DragEnter(nint pDataObj, uint grfKeyState, POINT pt, ref uint pdwEffect)
            {
                pdwEffect = 1; // DROPEFFECT_COPY
            }

            public void DragOver(uint grfKeyState, POINT pt, ref uint pdwEffect)
            {
            }

            public void DragLeave()
            {
            }

            [LibraryImport("kernel32.dll")]
            private static partial nint GlobalLock(nint hMem);

            [LibraryImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static partial bool GlobalUnlock(nint hMem);

            [LibraryImport("kernel32.dll")]
            private static partial nint GlobalFree(nint hMem);

            public void Drop(nint pDataObj, uint grfKeyState, POINT pt, ref uint pdwEffect)
            {
                var dataObj = (IDataObject)Marshal.GetObjectForIUnknown(pDataObj);
                FORMATETC fmt = new FORMATETC
                {
                    cfFormat = (short)DataFormats.GetFormat(DataFormats.UnicodeText).Id,
                    dwAspect = DVASPECT.DVASPECT_CONTENT,
                    lindex = -1,
                    tymed = TYMED.TYMED_HGLOBAL
                };

                STGMEDIUM medium;
                dataObj.GetData(ref fmt, out medium);

                nint hGlobal = medium.unionmember;
                nint pText = GlobalLock(hGlobal);
                string text = Marshal.PtrToStringUni(pText)!;

                MessageBox.Show("Dropped: " + text);

                GlobalUnlock(hGlobal);

                if (medium.pUnkForRelease == null)
                    GlobalFree(hGlobal);
            }
        }

        private void winToolBar1_ObjectDropped(object sender, WinToolBarObjectDroppedEventArgs e)
        {
            if (e.Button != null && e.Button.ToolBar.Buttons.Cast<WinToolBarButton>().ToList().IndexOf(e.Button) != 1)
                return;

            e.Interface = new ToolbarTestDropTarget();
            e.HResult = HResult.S_OK;
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
