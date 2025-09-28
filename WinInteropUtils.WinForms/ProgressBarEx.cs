using Microsoft.VisualBasic.ApplicationServices;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FireBlade.WinInteropUtils.WinForms
{
    /// <summary>
    /// <para>Represents a Win32 progress bar control.</para>
    /// 
    /// ![Sample image](progressbars.png)
    /// </summary>
    [Description("Represents a Win32 progress bar control.")]
    public partial class ProgressBarEx : Control
    {
        #region Constants

        #region Styles
        private const int PBS_SMOOTH = 0x01;
        private const int PBS_SMOOTHREVERSE = 0x10;
        private const int PBS_MARQUEE = 0x08;
        private const int PBS_VERTICAL = 0x04;
        #endregion

        #region Messages
        private const int WM_USER = 0x0400;
        private const int PBM_SETPOS = WM_USER + 2;
        private const int PBM_SETRANGE32 = WM_USER + 6;
        private const int PBM_GETRANGE = WM_USER + 7;
        private const int PBM_GETPOS = WM_USER + 8;
        private const int PBM_GETSTATE = WM_USER + 17;
        private const int PBM_SETSTATE = WM_USER + 16;
        private const int PBM_GETSTEP = WM_USER + 13;
        private const int PBM_SETSTEP = WM_USER + 4;
        private const int PBM_STEPIT = WM_USER + 5; // nice name microsoft!
        private const int PBM_GETBKCOLOR = WM_USER + 14;
        private const int PBM_SETBKCOLOR = CCM_SETBKCOLOR;
        private const int PBM_GETBARCOLOR = WM_USER + 15;
        private const int PBM_SETBARCOLOR = WM_USER + 9;
        private const int PBM_SETMARQUEE = WM_USER + 10;
        private const int PBM_DELTAPOS = WM_USER + 3;

        private const int CCM_FIRST = 0x2000;
        private const int CCM_SETBKCOLOR = CCM_FIRST + 1;
        #endregion

        private const uint CLR_DEFAULT = 0xFFFFFFFF;

        #endregion

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassName = "msctls_progress32";

                switch (Type)
                {
                    case ProgressBarExType.Smooth:
                        cp.Style |= PBS_SMOOTH;
                        break;

                    case ProgressBarExType.Marquee:
                        cp.Style |= PBS_MARQUEE;
                        break;
                }

                if (ReverseOnBackward)
                    cp.Style |= PBS_SMOOTHREVERSE;

                if (Orientation == Orientation.Vertical)
                    cp.Style |= PBS_VERTICAL;

                return cp;
            }
        }

        private ProgressBarExType m_type = ProgressBarExType.Blocks;

        /// <summary>
        /// Specifies the type of the progress bar.
        /// </summary>
        [Description("Specifies the type of the progress bar.")]
        [DefaultValue(ProgressBarExType.Blocks)]
        [Category("Appearance")]
        public ProgressBarExType Type
        {
            get => m_type;
            set
            {
                m_type = value;

                _rmakeHwnd();
            }
        }

        private bool m_reverseOnBackward = false;

        /// <summary>
        /// Determines the animation behavior that the progress bar should use when moving backward (from a higher value to a lower value). If this is set,
        /// then a "smooth" transition will occur, otherwise the control will "jump" to the lower value.
        /// </summary>
        [Description("Determines the animation behavior that the progress bar should use when moving backward (from a higher value to a lower value). If this is set, then a \"smooth\" transition will occur, otherwise the control will \"jump\" to the lower value.")]
        [DefaultValue(false)]
        [Category("Behavior")]
        public bool ReverseOnBackward
        {
            get => m_reverseOnBackward;
            set
            {
                m_reverseOnBackward = value;

                _rmakeHwnd();
            }
        }

        private Orientation m_orientation = Orientation.Horizontal;

        /// <summary>
        /// Specifies the orientation of the progress bar.
        /// </summary>
        [Description("Specifies the orientation of the progress bar.")]
        [DefaultValue(Orientation.Horizontal)]
        public Orientation Orientation
        {
            get => m_orientation;
            set
            {
                m_orientation = value;

                _rmakeHwnd();
            }
        }

        /// <summary>
        /// Specifies the minimum value the progress bar represents.
        /// </summary>
        [Description("Specifies the minimum value the progress bar represents.")]
        [DefaultValue(0)]
        [Category("Value")]
        public int Minimum
        {
            get
            {
                if (IsHandleCreated)
                    return (int)User32.SendMessage(Handle, PBM_GETRANGE, true, nint.Zero);

                return _min;
            }
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, Maximum, nameof(value));

                if (IsHandleCreated)
                    User32.SendMessage(Handle, PBM_SETRANGE32, (nuint)value, Maximum);

                _min = value;
            }
        }
        /// <summary>
        /// Specifies the maximum value the progress bar represents.
        /// </summary>
        [Description("Specifies the maximum value the progress bar represents.")]
        [DefaultValue(100)]
        [Category("Value")]
        public int Maximum
        {
            get
            {
                if (IsHandleCreated)
                    return (int)User32.SendMessage(Handle, PBM_GETRANGE, false, nint.Zero);

                return _max;
            }
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, Minimum, nameof(value));

                if (IsHandleCreated)
                    User32.SendMessage(Handle, PBM_SETRANGE32, (nuint)Minimum, value);

                _max = value;
            }
        }

        /// <summary>
        /// Specifies the current value the progress bar displays.
        /// </summary>
        [Description("Specifies the current value the progress bar displays.")]
        [DefaultValue(0)]
        [Category("Value")]
        public int Value
        {
            get
            {
                if (IsHandleCreated)
                    return (int)User32.SendMessage(Handle, PBM_GETPOS, 0, 0);

                return _val;
            }
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, Minimum, nameof(value));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, Maximum, nameof(value));

                if (Type != ProgressBarExType.Marquee && IsHandleCreated)
                    User32.SendMessage(Handle, PBM_SETPOS, (nuint)value, 0);

                _val = value;
            }
        }

        /// <summary>
        /// Gets or sets the style of the progress bar.
        /// </summary>
        [Description("Gets or sets the style of the progress bar.")]
        [Category("Appearance")]
        [DefaultValue(ProgressBarExStyle.Normal)]
        public ProgressBarExStyle Style
        {
            get
            {
                if (IsHandleCreated)
                    return (ProgressBarExStyle)User32.SendMessage(Handle, PBM_GETSTATE, 0, 0);

                return _style;
            }
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, PBM_SETSTATE, (nuint)value, 0);

                _style = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount that a call to <see cref="PerformStep"/> increases the progress
        /// bar's current position by.
        /// </summary>
        [Description("Gets or sets the amount that a call to PerformStep increases the progress bar's current position by.")]
        [DefaultValue(10)]
        [Category("Behavior")]
        public int Step
        {
            get
            {
                if (IsHandleCreated)
                    return (int)User32.SendMessage(Handle, PBM_GETSTEP, 0, 0);

                return _step;
            }
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, PBM_SETSTEP, (nuint)value, 0);

                _step = value;
            }
        }

        private static Color FromCOLORREF(uint colorRef)
        {
            byte r = (byte)(colorRef & 0xFF);        // lowest byte = Red
            byte g = (byte)((colorRef >> 8) & 0xFF); // next byte = Green
            byte b = (byte)((colorRef >> 16) & 0xFF);// high byte = Blue

            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// If <see langword="true"/>, use the default progress bar background color.
        /// </summary>
        /// <remarks>Has no effect when visual styles are enabled.</remarks>
        [Description("If true, use the default progress bar background color. Has no effect when visual styles are enabled.")]
        [DefaultValue(true)]
        [Category("Appearance")]
        public bool UseDefaultBackColor
        {
            get
            {
                if (IsHandleCreated)
                    return ((uint)User32.SendMessage(Handle, PBM_GETBKCOLOR, 0, 0)) == CLR_DEFAULT;

                return _defBk;
            }
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, PBM_SETBKCOLOR, 0, value ? unchecked((nint)CLR_DEFAULT) : (nint)_bkColor);

                _defBk = value;
            }
        }

        /// <summary>
        /// If <see langword="true"/>, use the default progress bar bar color.
        /// </summary>
        /// <remarks>Has no effect when visual styles are enabled.</remarks>
        [Description("If true, use the default progress bar bar color. Has no effect when visual styles are enabled.")]
        [DefaultValue(true)]
        [Category("Appearance")]
        public bool UseDefaultForeColor
        {
            get
            {
                if (IsHandleCreated)
                    return ((uint)User32.SendMessage(Handle, PBM_GETBARCOLOR, 0, 0)) == CLR_DEFAULT;

                return _defBar;
            }
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, PBM_SETBARCOLOR, 0, value ? unchecked((nint)CLR_DEFAULT) : (nint)_barColor);

                _defBar = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color of the progress bar.
        /// </summary>
        /// <remarks>Has no effect when visual styles are enabled.</remarks>
        [Description("Gets or sets the background color of the progress bar. Has no effect when visual styles are enabled.")]
        public override Color BackColor
        {
            get
            {
                if (UseDefaultBackColor)
                    return SystemColors.Control;

                if (IsHandleCreated)
                    return FromCOLORREF((uint)User32.SendMessage(Handle, PBM_GETBKCOLOR, 0, 0));

                return FromCOLORREF(_bkColor);
            }
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, PBM_SETBKCOLOR, 0, (nint)ToCOLORREF(value));

                _bkColor = ToCOLORREF(value);
            }
        }

        /// <summary>
        /// Gets or sets the bar color of the progress bar.
        /// </summary>
        /// <remarks>Has no effect when visual styles are enabled.</remarks>
        [Description("Gets or sets the bar color of the progress bar. Has no effect when visual styles are enabled.")]
        public override Color ForeColor
        {
            get
            {
                if (UseDefaultBackColor)
                    return SystemColors.Highlight;

                if (IsHandleCreated)
                    return FromCOLORREF((uint)User32.SendMessage(Handle, PBM_GETBARCOLOR, 0, 0));

                return FromCOLORREF(_barColor);
            }
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, PBM_SETBARCOLOR, 0, (nint)ToCOLORREF(value));

                _barColor = ToCOLORREF(value);
            }
        }

        /// <summary>
        /// Specifies the speed, in milliseconds, of the marquee animation when <see cref="Type"/> is set to <see cref="ProgressBarExType.Marquee"/>.
        /// </summary>
        [DefaultValue(30)]
        [Description("Specifies the speed, in milliseconds, of the marquee animation when Type is set to Marquee.")]
        public int MarqueeSpeed
        {
            get => _marqueeSpeed;
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, PBM_SETMARQUEE, Type == ProgressBarExType.Marquee, value);

                _marqueeSpeed = value;
            }
        }

        #region Backing Fields
        private int _min = 0;
        private int _max = 100;
        private int _val = 0;
        private int _step = 0;
        private uint _bkColor = CLR_DEFAULT;
        private uint _barColor = CLR_DEFAULT;
        private ProgressBarExStyle _style = ProgressBarExStyle.Normal;
        private bool _defBk = true;
        private bool _defBar = true;
        private int _marqueeSpeed = 30;
        #endregion

        private void _rmakeHwnd()
        {
            if (IsHandleCreated)
                RecreateHandle();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            User32.SendMessage(Handle, PBM_SETRANGE32, (nuint)_min, _max);

            if (Type != ProgressBarExType.Marquee)
                User32.SendMessage(Handle, PBM_SETPOS, (nuint)_val, 0);

            User32.SendMessage(Handle, PBM_SETSTATE, (nuint)_style, 0);
            User32.SendMessage(Handle, PBM_SETBKCOLOR, 0, _defBk ? unchecked((nint)CLR_DEFAULT) : (nint)ToCOLORREF(BackColor));

            User32.SendMessage(Handle, PBM_SETBARCOLOR, 0, _defBar ? unchecked((nint)CLR_DEFAULT) : (nint)_barColor);

            User32.SendMessage(Handle, PBM_SETMARQUEE, Type == ProgressBarExType.Marquee, _marqueeSpeed);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressBarEx"/> class.
        /// </summary>
        public ProgressBarEx()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint | ControlStyles.UseTextForAccessibility | ControlStyles.Selectable, false);
        }

        private static uint ToCOLORREF(Color color)
        {
            // COLORREF layout is 0x00bbggrr
            uint dw = (uint)(color.R | (color.G << 8) | (color.B << 16));
            return dw;
        }

        /// <summary>
        /// Advances the current position of the <see cref="ProgressBarEx"/> by the amount of the
        /// <see cref="Step"/> property, and redraws the control to reflect the new position.
        /// </summary>
        /// <remarks>When the position exceeds the maximum range value, this method resets the current
        /// position so that the progress indicator starts over again from the beginning.</remarks>
        /// <exception cref="InvalidOperationException">A step cannot be performed on a marquee progress bar as it does not use a value.</exception>
        /// <exception cref="InvalidOperationException">Can't perform a step right now; the control handle has not been created yet.</exception>
        /// <returns>The previous position.</returns>
        public int PerformStep()
        {
            if (Type == ProgressBarExType.Marquee)
            {
                throw new InvalidOperationException("A step cannot be performed on a marquee progress bar as it does not use a value.");
            }

            if (!IsHandleCreated)
                throw new InvalidOperationException("Can't perform a step right now; the control handle has not been created yet.");

            return (int)User32.SendMessage(Handle, PBM_STEPIT, 0, 0);
        }

        /// <summary>
        /// Advances the current position of the progress bar by a specified increment and redraws the bar to reflect the new position.
        /// </summary>
        /// <param name="step">The amount to advance the position by.</param>
        /// <returns>The previous position.</returns>
        /// <remarks>If the increment results in a value outside the range of the control, the position is set to the nearest boundary.</remarks>
        /// <exception cref="InvalidOperationException">A step cannot be performed on a marquee progress bar as it does not use a value.</exception>
        /// <exception cref="InvalidOperationException">Can't perform a step right now; the control handle has not been created yet.</exception>
        public int StepBy(int step)
        {
            if (Type == ProgressBarExType.Marquee)
            {
                throw new InvalidOperationException("A step cannot be performed on a marquee progress bar as it does not use a value.");
            }

            if (!IsHandleCreated)
                throw new InvalidOperationException("Can't perform a step right now; the control handle has not been created yet.");

            return (int)User32.SendMessage(Handle, PBM_DELTAPOS, (nuint)step, 0);
        }
    }

    /// <summary>
    /// Specifies the <see cref="ProgressBarEx.Type"/> of a <see cref="ProgressBarEx"/>.
    /// </summary>
    public enum ProgressBarExType
    {
        /// <summary>
        /// The progress bar displays progress status in a smooth scrolling bar instead of the default segmented bar.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > This style is supported only in the <b>Windows Classic</b> theme. All other themes override this style.
        /// </remarks>
        Smooth,
        /// <summary>
        /// The progress bar displays progress status in blocks that fill up accross the bar. This is the default.
        /// </summary>
        Blocks,
        /// <summary>
        /// The progress indicator does not grow in size but instead moves repeatedly along the length of the bar,
        /// indicating activity without specifying what proportion of the progress is complete.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > Requires visual styles. Make sure that you include <see cref="Application.EnableVisualStyles()"/> inside your Program class
        /// > before starting your application.
        /// </remarks>
        Marquee
    }

    /// <summary>
    /// Specifies the style of a <see cref="ProgressBarEx"/> control.
    /// </summary>
    public enum ProgressBarExStyle
    {
        /// <summary>
        /// Shows a regular progress bar.
        /// </summary>
        Normal = 0x0001,
        /// <summary>
        /// Shows an error (red) progress bar.
        /// </summary>
        Error = 0x0002,
        /// <summary>
        /// Shows a paused (yellow) progress bar.
        /// </summary>
        Paused = 0x0003
    }
}
