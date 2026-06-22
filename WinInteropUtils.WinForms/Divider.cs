using System.ComponentModel;
using Windows.Win32;

namespace FireBlade.WinInteropUtils.WinForms
{
    /// <summary>
    /// Represents a control that can act as a horizontal or vertical separator.
    /// </summary>
    public class Divider : Control
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassName = "Static";
                cp.Style |= _orient == Orientation.Horizontal ? SS_ETCHEDHORZ : SS_ETCHEDVERT;

                return cp;
            }
        }

        // WHY ARE THESE DEFINED IN WINUSER.H NOT COMMCTRL.H OF ALL PLACES WHY
        private const int SS_ETCHEDHORZ = 0x00000010;
        private const int SS_ETCHEDVERT = 0x00000011;

        private Orientation _orient = Orientation.Horizontal;

        /// <summary>
        /// Gets or sets the orientation of the <see cref="Divider"/>.
        /// </summary>
        [DefaultValue(Orientation.Horizontal)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Gets or sets the orientation of the Divider.")]
        public Orientation Orientation
        {
            get => _orient;
            set
            {
                _orient = value;
                if (IsHandleCreated) RecreateHandle();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Text => base.Text;

        protected override Size DefaultSize => new Size(80, 2);

        public Divider()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.Selectable, false);
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.FixedHeight, true);
        }
    }
}
