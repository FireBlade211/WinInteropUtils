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
    public partial class HeadingTextControl : Control
    {
        public HeadingTextControl()
        {
            InitializeComponent();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            try
            {
                var wnd = Window.FromHandle(Handle);

                if (wnd != null)
                {
                    using (VisualStyle vs = VisualStyle.OpenThemeData(wnd, "TEXTSTYLE"))
                    {
                        // Part ID 1 = main instruction
                        vs.DrawThemeText(pe.Graphics, 1, 0, Text, -1, ThemeTextOptions.SingleLine | ThemeTextOptions.AlignCenter | ThemeTextOptions.AlignMiddle,
                            new Rectangle(0, 0, Width, Height));
                    }
                }
            }
            catch
            {

            }
        }
    }
}
