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
    public partial class VisualStyleExControl : Control
    {
        private bool _useVs;

        public bool UseVisualStyle
        {
            get => _useVs;
            set
            {
                _useVs = value;
                Refresh();
            }
        }

        public VisualStyleExControl()
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

            //try
            //{
            //    using (VisualStyle vs = VisualStyle.OpenThemeData(Window.FromHandle(Handle), "TEXTSTYLE"))
            //    {
            //        Rectangle rc = new Rectangle(0, 0, Width, Height);
            //        ThemeTextAdvancedOptions adv = new ThemeTextAdvancedOptions();
            //        adv.Flags = ThemeTextAdvancedOptionsFlags.GlowSize | ThemeTextAdvancedOptionsFlags.Callback;
            //        adv.GlowSize = 5;
            //        adv.Callback = int (Graphics gh, string text, int cchText, ref Rectangle rc, ThemeTextAdvancedOptionsFlags flags, object? param) =>
            //        {
            //            gh.FillEllipse(Brushes.Aqua, rc);
            //            return 0;
            //        };

            //        // Part ID 1 = main instruction
            //        vs.DrawThemeText(pe.Graphics, 1, 0, Text, -1, ThemeTextOptions.SingleLine | ThemeTextOptions.AlignCenter | ThemeTextOptions.AlignMiddle,
            //            ref rc, adv);

            //    }
            //}
            //catch
            //{

            //}

            if (!UseVisualStyle)
            {
                Rectangle rc = new Rectangle(0, 0, Width, Height);
                DrawTextAdvancedOptions dtadv = new DrawTextAdvancedOptions();
                dtadv.LeftMargin = 15;

                VisualStyle.DrawText(pe.Graphics, Text, ref rc, ThemeTextOptions.AlignLeft | ThemeTextOptions.AlignTop, ref dtadv);
            }
        }
    }
}
