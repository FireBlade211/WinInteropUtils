using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;

namespace FireBlade.WinInteropUtils.WinForms
{
    /// <summary>
    /// <para>Represents a control that allows the user to slide a slider along a track, commonly referred to as a track bar.</para>
    /// 
    /// ![Sample image](../images/sliders.png)
    /// </summary>
    [Designer("FireBlade.WinInteropUtils.WinForms.SliderDesigner, WinInteropUtils.WinForms")]
    public partial class Slider : Control
    {
        private const int WM_USER = 0x0400;
        private const int TBM_GETBUDDY = WM_USER + 33;
        private const int TBM_SETBUDDY = WM_USER + 32;
        private const int TBM_GETSELSTART = WM_USER + 17;
        private const int TBM_SETSELSTART = WM_USER + 11;
        private const int TBM_GETSELEND = WM_USER + 18;
        private const int TBM_SETSELEND = WM_USER + 12;
        private const int TBM_SETTICKFREQ = WM_USER + 20;
        private const int TBS_HORZ = 0x0000;
        private const int TBS_VERT = 0x0002;
        private const int TBS_REVERSED = 0x0200;
        private const int TBS_TRANSPARENTBKGND = 0x1000;
        private const int TBS_DOWNISLEFT = 0x0400;
        private const int TBS_NOTHUMB = 0x0080;
        private const int TBS_NOTIFYBEFOREMOVE = 0x0800;
        private const int TBS_ENABLESELRANGE = 0x0020;
        private const int TBS_TOPLEFT = 0x0004;
        private const int TBS_BOTH = 0x0008;
        private const int TBS_BOTTOMRIGHT = 0x0000;
        private const int TBS_NOTICKS = 0x0010;
        private const int TBS_TOOLTIPS = 0x0100;
        private const int TBM_CLEARSEL = WM_USER + 19;
        private const int TBM_SETTIPSIDE = WM_USER + 31;
        private const int TBM_GETPOS = WM_USER;
        private const int TBM_SETPOS = WM_USER + 5;
        private const int TBM_SETPOSNOTIFY = WM_USER + 34;
        private const int TRBN_FIRST = unchecked((int)4294965795);
        private const int TRBN_THUMBPOSCHANGING = TRBN_FIRST - 1;
        private const int WM_HSCROLL = 0x0114;
        private const int WM_VSCROLL = 0x0115;
        private const int WM_NOTIFY = 0x004E;
        private const int TBS_FIXEDLENGTH = 0x0040;
        private const int TBM_SETTHUMBLENGTH = WM_USER + 27;
        private const int TBM_GETTHUMBLENGTH = WM_USER + 28;
        private const int TBM_GETRANGEMAX = WM_USER + 2;
        private const int TBM_GETRANGEMIN = WM_USER + 1;
        private const int TBM_SETRANGEMAX = WM_USER + 8;
        private const int TBM_SETRANGEMIN = WM_USER + 7;
        private const int TBM_SETTIC = WM_USER + 4;
        private const int TBM_GETTIC = WM_USER + 3;
        private const int TBM_GETPTICS = WM_USER + 14;
        private const int TBM_GETNUMTICS = WM_USER + 16;
        private const int TBM_GETTHUMBRECT = WM_USER + 25;
        private const int TBM_CLEARTICS = WM_USER + 9;
        private const int TBM_GETLINESIZE = WM_USER + 24;
        private const int TBM_SETLINESIZE = WM_USER + 23;
        private const int TBM_SETPAGESIZE = WM_USER + 21;
        private const int TBM_GETPAGESIZE = WM_USER + 22;
        private const uint WM_REFLECT = WM_USER + 0x1C00;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassName = "msctls_trackbar32";
                cp.Style |= Orientation == Orientation.Horizontal ? TBS_HORZ : TBS_VERT;
                cp.Style |= TBS_NOTIFYBEFOREMOVE;

                if (Reversed)
                    cp.Style |= TBS_REVERSED;

                if (BackColor.A < 255)
                    cp.Style |= TBS_TRANSPARENTBKGND;

                if (ReverseInput)
                    cp.Style |= TBS_DOWNISLEFT;

                if (!IsThumbVisible)
                    cp.Style |= TBS_NOTHUMB;

                if (ShowSelectionRange)
                    cp.Style |= TBS_ENABLESELRANGE;

                cp.Style |= (int)TickMode;
                cp.Style |= TickStyle switch
                {
                    TickStyle.BottomRight => TBS_BOTTOMRIGHT,
                    TickStyle.TopLeft => TBS_TOPLEFT,
                    TickStyle.Both => TBS_BOTH,
                    _ => TBS_NOTICKS
                };

                if (ShowToolTip)
                    cp.Style |= TBS_TOOLTIPS;

                if (FixedLength)
                    cp.Style |= TBS_FIXEDLENGTH;

                return cp;
            }
        }

        /// <summary>
        /// Gets or sets the buddy to the left of the slider if the slider is horizontal, or above the slider if the slider is vertical.
        /// </summary>
        /// <exception cref="ArgumentException">The buddy can't be set to a Form.</exception>
        /// <exception cref="ArgumentException">The buddy can't be set to the same Slider that the buddy is being set on.</exception>
        [Description("Gets or sets the buddy to the left of the slider if the slider is horizontal, or above the slider if the slider is vertical.")]
        [DefaultValue(null)]
        public Control? LeftBuddy
        {
            get
            {
                if (IsHandleCreated)
                    if (User32.SendMessage(Handle, TBM_GETBUDDY, true, 0) is nint hCtrl)
                        if (FromHandle(hCtrl) is Control ctrl)
                            return ctrl;

                return _leftBuddy;
            }
            set
            {
                if (value is Form)
                    throw new ArgumentException("The buddy can't be set to a Form.", nameof(value));

                if (value == this)
                    throw new ArgumentException("The buddy can't be set to the same Slider that the buddy is being set on.", nameof(value));

                if (IsHandleCreated)
                    User32.SendMessage(Handle, TBM_SETBUDDY, true, value?.Handle ?? nint.Zero);

                _leftBuddy = value;
            }
        }

        /// <summary>
        /// Gets or sets the buddy to the right of the slider if the slider is horizontal, or below the slider if the slider is vertical.
        /// </summary>
        /// <exception cref="ArgumentException">The buddy can't be set to a Form.</exception>
        /// <exception cref="ArgumentException">The buddy can't be set to the same Slider that the buddy is being set on.</exception>
        [Description("Gets or sets the buddy to the right of the slider if the slider is horizontal, or below the slider if the slider is vertical.")]
        [DefaultValue(null)]
        public Control? RightBuddy
        {
            get
            {
                if (IsHandleCreated)
                    if (User32.SendMessage(Handle, TBM_GETBUDDY, false, 0) is nint hCtrl)
                        if (FromHandle(hCtrl) is Control ctrl)
                            return ctrl;

                return _rightBuddy;
            }
            set
            {
                if (value is Form)
                    throw new ArgumentException("The buddy can't be set to a Form.", nameof(value));

                if (value == this)
                    throw new ArgumentException("The buddy can't be set to the same Slider that the buddy is being set on.", nameof(value));

                if (IsHandleCreated)
                    User32.SendMessage(Handle, TBM_SETBUDDY, false, value?.Handle ?? nint.Zero);

                _rightBuddy = value;
            }
        }

        private void _rmakeHwnd()
        {
            if (IsHandleCreated)
                RecreateHandle();
        }

        /// <summary>
        /// Gets or sets the orientation of the slider.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(Orientation.Horizontal)]
        [Description("Gets or sets the orientation of the slider.")]
        public Orientation Orientation
        {
            get => _orient;
            set
            {
                if (_orient != value) // value changed?
                {
                    _orient = value;

                    _rmakeHwnd();

                    // Swap sizes
                    var w = Width;
                    var h = Height;

                    Width = h;
                    Height = w;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the slider is reversed, where smaller numbers mean higher and larger numbers mean lower.
        /// </summary>
        [DefaultValue(false)]
        [Category("Behavior")]
        [Description("Gets or sets whether the slider is reversed, where smaller numbers mean higher and larger numbers mean lower.")]
        public bool Reversed
        {
            get => _reverse;
            set
            {
                _reverse = value;

                _rmakeHwnd();
            }
        }

        /// <summary>
        /// Gets or sets whether input is reversed.
        /// </summary>
        /// <remarks>
        /// By default, the control uses down equal to right and up equal to left. Enable to reverse the default,
        /// making down equal left and up equal right.
        /// </remarks>
        [Description("Gets or sets whether input is reversed. By default, the control uses down equal to right and up equal to left. Enable to reverse the default, making down equal left and up equal right.")]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool ReverseInput
        {
            get => _reverseInput;
            set
            {
                _reverseInput = value;

                _rmakeHwnd();
            }
        }

        /// <summary>
        /// Gets or sets whether the slider thumb is visible.
        /// </summary>
        [Description("Gets or sets whether the slider thumb is visible.")]
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool IsThumbVisible
        {
            get => _thumbVis;
            set
            {
                _thumbVis = value;

                _rmakeHwnd();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the slider's selection range should be shown.
        /// </summary>
        /// <remarks>
        /// The slider's selection range does not affect its functionality in any way. It is up to the application to implement the range.
        /// You might do this in one of the following ways:
        /// <list type="bullet">
        /// <item>Use a selection range to enable the user to set maximum and minimum values for some parameter.
        /// For example, the user could move the slider to a position and then click a button labeled "Max". The application
        /// then sets the selection range to show the values chosen by the user.</item>
        /// <item>Limit the movement of the slider to a predetermined subrange within the control, by handling the <see cref="ValueChanging"/> event
        /// and disallowing any movement outside the selection range. You might do this, for example, if the range of values available to the user
        /// can change because of other choices the user has made, or according to available resources.</item>
        /// </list>
        /// </remarks>
        [Description("Gets or sets a value indicating whether the slider's selection range should be shown.")]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool ShowSelectionRange
        {
            get => _showSelRange;
            set
            {
                _showSelRange = value;

                _rmakeHwnd();
            }
        }

        /// <summary>
        /// Gets or sets the start of the slider's selection range.
        /// </summary>
        /// <remarks>
        /// The slider's selection range does not affect its functionality in any way. It is up to the application to implement the range.
        /// You might do this in one of the following ways:
        /// <list type="bullet">
        /// <item>Use a selection range to enable the user to set maximum and minimum values for some parameter.
        /// For example, the user could move the slider to a position and then click a button labeled "Max". The application
        /// then sets the selection range to show the values chosen by the user.</item>
        /// <item>Limit the movement of the slider to a predetermined subrange within the control, by handling the <see cref="ValueChanging"/> event
        /// and disallowing any movement outside the selection range. You might do this, for example, if the range of values available to the user
        /// can change because of other choices the user has made, or according to available resources.</item>
        /// </list>
        /// </remarks>
        [Description("Gets or sets the start of the slider's selection range.")]
        [Category("Appearance")]
        [DefaultValue(0)]
        public int SelectionRangeStart
        {
            get
            {
                if (IsHandleCreated)
                    if (User32.SendMessage(Handle, TBM_GETSELSTART, 0, 0) is nint start)
                        return (int)start;

                return _selRangeStart;
            }
            set
            {
                _selRangeStart = value;

                if (IsHandleCreated)
                    User32.SendMessage(Handle, TBM_SETSELSTART, true, value);
            }
        }

        /// <summary>
        /// Gets or sets the end of the slider's selection range.
        /// </summary>
        /// <remarks>
        /// The slider's selection range does not affect its functionality in any way. It is up to the application to implement the range.
        /// You might do this in one of the following ways:
        /// <list type="bullet">
        /// <item>Use a selection range to enable the user to set maximum and minimum values for some parameter.
        /// For example, the user could move the slider to a position and then click a button labeled "Max". The application
        /// then sets the selection range to show the values chosen by the user.</item>
        /// <item>Limit the movement of the slider to a predetermined subrange within the control, by handling the <see cref="ValueChanging"/> event
        /// and disallowing any movement outside the selection range. You might do this, for example, if the range of values available to the user
        /// can change because of other choices the user has made, or according to available resources.</item>
        /// </list>
        /// </remarks>
        [Description("Gets or sets the end of the slider's selection range.")]
        [Category("Appearance")]
        [DefaultValue(0)]
        public int SelectionRangeEnd
        {
            get
            {
                if (IsHandleCreated)
                    if (User32.SendMessage(Handle, TBM_GETSELEND, 0, 0) is nint start)
                        return (int)start;

                return _selRangeEnd;
            }
            set
            {
                _selRangeEnd = value;

                if (IsHandleCreated)
                    User32.SendMessage(Handle, TBM_SETSELEND, true, value);
            }
        }

        /// <summary>
        /// Specifies the types of ticks displayed on the slider.
        /// </summary>
        [Description("Specifies the types of ticks displayed on the slider.")]
        [Category("Appearance")]
        [DefaultValue(SliderTickMode.Auto)]
        public SliderTickMode TickMode
        {
            get => _tm;
            set
            {
                _tm = value;

                _rmakeHwnd();
            }
        }

        /// <summary>
        /// Gets or sets the frequency of ticks on the slider. Only applies
        /// when <see cref="TickMode"/> is <see cref="SliderTickMode.Custom"/>.
        /// </summary>
        [Description("Gets or sets the frequency of ticks on the slider. Only applies when TickMode is set to Custom.")]
        [DefaultValue(0)]
        [Category("Appearance")]
        public int TickFrequency
        {
            get => _tickFreq;
            set
            {
                _tickFreq = value;

                if (IsHandleCreated)
                    User32.SendMessage(Handle, TBM_SETTICKFREQ, (nuint)value, 0);
            }
        }
        /// <summary>
        /// Gets or sets the position of ticks in the slider.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(TickStyle.BottomRight)]
        [Description("Gets or sets the position of ticks in the slider.")]
        public TickStyle TickStyle
        {
            get => _ts;
            set
            {
                _ts = value;

                _rmakeHwnd();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show a tooltip showing the value of the slider.
        /// </summary>
        [Category("Tooltip")]
        [DefaultValue(false)]
        [Description("Gets or sets a value indicating whether to show a tooltip showing the value of the slider.")]
        public bool ShowToolTip
        {
            get => _showTooltip;
            set
            {
                _showTooltip = value;

                _rmakeHwnd();
            }
        }

        /// <summary>
        /// Gets or sets the side where the tooltip should be displayed.
        /// </summary>
        [Category("Tooltip")]
        [DefaultValue(SliderToolTipSide.Top)]
        [Description("Gets or sets the side where the tooltip should be displayed.")]
        public SliderToolTipSide ToolTipSide
        {
            get => _side;
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, TBM_SETTIPSIDE, (nuint)value, 0);

                _side = value;
            }
        }

        /// <summary>
        /// Gets or sets the currently selected value of the slider.
        /// </summary>
        [Description("Gets or sets the currently selected value of the slider.")]
        [Category("Behavior")]
        [DefaultValue(0)]
        public int Value
        {
            get
            {
                if (IsHandleCreated)
                    return (int)User32.SendMessage(Handle, TBM_GETPOS, 0, 0);

                return _val;
            }
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, TBM_SETPOSNOTIFY, true, value);

                _val = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the length of the slider thumb can be overriden via the <see cref="ThumbLength"/> property.
        /// </summary>
        [Description("Gets or sets a value indicating whether the length of the slider thumb can be overriden via the ThumbLength property.")]
        [DefaultValue(false)]
        [Category("Appearance")]
        public bool FixedLength
        {
            get => _fixedLength;
            set
            {
                _fixedLength = value;

                _rmakeHwnd();
            }
        }

        /// <summary>
        /// Gets or sets the length of the thumb in pixels. Only applies when <see cref="FixedLength"/> is set to <see langword="true"/>.
        /// </summary>
        [Description("Gets or sets the length of the thumb in pixels. Only applies when FixedLength is set to true.")]
        [Category("Appearance")]
        public int ThumbLength
        {
            get
            {
                if (IsHandleCreated)
                    return (int)User32.SendMessage(Handle, TBM_GETTHUMBLENGTH, 0, 0);

                return _thumbLength;
            }
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, TBM_SETTHUMBLENGTH, (nuint)value, 0);

                _thumbLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum value the slider can represent.
        /// </summary>
        [Description("Gets or sets the minimum value the slider can represent.")]
        [Category("Behavior")]
        [DefaultValue(0)]
        public int Minimum
        {
            get
            {
                if (IsHandleCreated)
                    return (int)User32.SendMessage(Handle, TBM_GETRANGEMIN, 0, 0);

                return _min;
            }
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, TBM_SETRANGEMIN, true, value);

                _min = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value the slider can represent.
        /// </summary>
        [Description("Gets or sets the maximum value the slider can represent.")]
        [Category("Behavior")]
        [DefaultValue(100)]
        public int Maximum
        {
            get
            {
                if (IsHandleCreated)
                    return (int)User32.SendMessage(Handle, TBM_GETRANGEMAX, 0, 0);

                return _max;
            }
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, TBM_SETRANGEMAX, true, value);

                _max = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;

            public readonly Rectangle ToRectangle() =>
                new(Left, Top, Right - Left, Bottom - Top);
        }

        /// <summary>
        /// Gets the size and position of the bounding rectangle for the slider.
        /// </summary>
        [Description("Gets the size and position of the bounding rectangle for the slider.")]
        [Category("Appearance")]
        public Rectangle ThumbRect
        {
            get
            {
                if (IsHandleCreated)
                {
                    unsafe
                    {
                        RECT rect;

                        User32.SendMessage(Handle, TBM_GETTHUMBRECT, 0, (nint)(&rect));

                        return rect.ToRectangle();
                    }
                }

                return new Rectangle();
            }
        }

        /// <summary>
        /// Gets or sets the amount of ticks that the slider will move when a small change occurs, such as moving the slider with the arrow keys.
        /// </summary>
        /// <remarks>Also commonly referred to as the "line size".</remarks>
        [Description("Gets or sets the amount of ticks that the slider will move when a small change occurs, such as moving the slider with the arrow keys.")]
        [Category("Behavior")]
        [DefaultValue(1)]
        public int SmallChange
        {
            get
            {
                if (IsHandleCreated)
                    return (int)User32.SendMessage(Handle, TBM_GETLINESIZE, 0, 0);

                return _smallChange;
            }
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, TBM_SETLINESIZE, 0, value);

                _smallChange = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of ticks that the slider will move when a large change occurs, such as moving the slider
        /// with the <c>PageUp</c>/<c>PageDown</c> keys.
        /// </summary>
        /// <remarks>Also commonly referred to as the "page size".</remarks>
        [Description("Gets or sets the amount of ticks that the slider will move when a large change occurs, such as moving the slider with the PageUp/PageDown keys.")]
        [Category("Behavior")]
        [DefaultValue(5)]
        public int LargeChange
        {
            get
            {
                if (IsHandleCreated)
                    return (int)User32.SendMessage(Handle, TBM_GETPAGESIZE, 0, 0);

                return _largeChange;
            }
            set
            {
                if (IsHandleCreated)
                    User32.SendMessage(Handle, TBM_SETPAGESIZE, 0, value);

                _largeChange = value;
            }
        }

        private Control? _rightBuddy;
        private Control? _leftBuddy;
        private Orientation _orient = Orientation.Horizontal;
        private bool _reverse = false;
        private bool _reverseInput = false;
        private bool _thumbVis = true;
        private bool _showSelRange = false;
        private int _selRangeStart = 0;
        private int _selRangeEnd = 0;
        private int _tickFreq = 0;
        private SliderTickMode _tm = SliderTickMode.Auto;
        private TickStyle _ts = TickStyle.BottomRight;
        private bool _showTooltip = false;
        private SliderToolTipSide _side = SliderToolTipSide.Top;
        private int _val = 0;
        private bool _fixedLength = false;
        private int _thumbLength = 16;
        private int _min = 0;
        private int _max = 100;
        private int _smallChange = 1;
        private int _largeChange = 5;

        /// <summary>
        /// Fires before the slider's value changes and allows validation.
        /// </summary>
        [Description("Fires before the slider's value changes and allows validation.")]
        [Category("Property Changed")]
        public event EventHandler<SliderValueChangingEventArgs>? ValueChanging;

        /// <summary>
        /// Fires when the slider scrolls horizontally.
        /// </summary>
        [Category("Scroll")]
        [Description("Fires when the slider scrolls horizontally.")]
        public event EventHandler<SliderScrollEventArgs>? HorizontalScroll;

        /// <summary>
        /// Fires when the slider scrolls vertically.
        /// </summary>
        [Category("Scroll")]
        [Description("Fires when the slider scrolls vertically.")]
        public event EventHandler<SliderScrollEventArgs>? VerticalScroll;

        public Slider()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint | ControlStyles.UseTextForAccessibility, false);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            User32.SendMessage(Handle, TBM_SETBUDDY, true, _leftBuddy?.Handle ?? nint.Zero);
            User32.SendMessage(Handle, TBM_SETBUDDY, false, _rightBuddy?.Handle ?? nint.Zero);

            User32.SendMessage(Handle, TBM_SETSELSTART, true, _selRangeStart);
            User32.SendMessage(Handle, TBM_SETSELEND, true, _selRangeEnd);

            User32.SendMessage(Handle, TBM_SETTICKFREQ, (nuint)_tickFreq, 0);

            User32.SendMessage(Handle, TBM_SETTIPSIDE, (nuint)_side, 0);

            User32.SendMessage(Handle, TBM_SETPOS, true, _val);

            User32.SendMessage(Handle, TBM_SETTHUMBLENGTH, (nuint)_thumbLength, 0);

            User32.SendMessage(Handle, TBM_SETRANGEMIN, true, _min);
            User32.SendMessage(Handle, TBM_SETRANGEMAX, true, _max);

            User32.SendMessage(Handle, TBM_SETLINESIZE, 0, _smallChange);
            User32.SendMessage(Handle, TBM_SETPAGESIZE, 0, _largeChange);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            if (BackColor.A < 255)
            {
                _rmakeHwnd();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg >= WM_REFLECT)
            {
                WmReflect(ref m);
            }

            base.WndProc(ref m);
        }

        private void WmReflect(ref Message m)
        {
            switch (m.Msg - WM_REFLECT)
            {
                case WM_NOTIFY:
                    var nmhdr = (NMHDR)m.GetLParam(typeof(NMHDR))!;

                    switch (nmhdr.code)
                    {
                        case TRBN_THUMBPOSCHANGING:
                            var args = new SliderValueChangingEventArgs((NMTRBTHUMBPOSCHANGING)m.GetLParam(typeof(NMTRBTHUMBPOSCHANGING))!);
                            ValueChanging?.Invoke(this, args);
                            
                            m.Result = args.Cancel ? 1 : 0;
                            break;
                    }

                    break;
                case WM_HSCROLL:
                    HorizontalScroll?.Invoke(this, new SliderScrollEventArgs(m.WParam, m.LParam));
                    break;
                case WM_VSCROLL:
                    VerticalScroll?.Invoke(this, new SliderScrollEventArgs(m.WParam, m.LParam));
                    break;
            }
        }

        /// <summary>
        /// Clears the slider's selection range and redraws the control.
        /// </summary>
        public void ClearSelectionRange()
        {
            if (IsHandleCreated)
                User32.SendMessage(Handle, TBM_CLEARSEL, true, 0);

            _selRangeStart = 0;
            _selRangeEnd = 0;
        }

        /// <summary>
        /// Clears the slider's selection range and optionally redraws the control.
        /// </summary>
        /// <param name="redraw">Whether to redraw the control.</param>
        public void ClearSelectionRange(bool redraw)
        {
            if (IsHandleCreated)
                User32.SendMessage(Handle, TBM_CLEARSEL, redraw, 0);

            _selRangeStart = 0;
            _selRangeEnd = 0;
        }
        
        /// <summary>
        /// Adds a tick to the <see cref="Slider"/> at the specified position.
        /// </summary>
        /// <param name="pos">The position of the tick.</param>
        public void AddTick(int pos)
        {
            if (IsHandleCreated)
                User32.SendMessage(Handle, TBM_SETTIC, 0, pos);
        }

        /// <summary>
        /// Retrieves the logical position of a tick mark in a slider. The logical position can be any of the integer
        /// values in the slider's range of minimum to maximum slider positions.
        /// </summary>
        /// <param name="idx">Zero-based index identifying a tick mark. Valid indexes are in the range from zero to two less than the
        /// tick count returned by the <see cref="GetTickCount"/> method.</param>
        /// <returns>The logical position of the specified tick mark, or -1 if <paramref name="idx"/> does not specify a valid index,
        /// or the handle of the slider hasn't been created.</returns>
        public int GetTickPosition(int idx)
        {
            if (IsHandleCreated)
                return (int)User32.SendMessage(Handle, TBM_GETTIC, (nuint)idx, 0);

            return -1;
        }

        /// <summary>
        /// Retrieves the number of tick marks in a slider.
        /// </summary>
        /// <remarks>
        /// The <see cref="GetTickCount"/> method counts all of the tick marks, including the first and last tick marks created by the slider.
        /// </remarks>
        /// <returns>If <see cref="TickMode"/> isn't set to <see cref="SliderTickMode.Custom"/>, returns 2 for the beginning and ending ticks. If it's set to
        /// <see cref="SliderTickMode.None"/>, returns zero. Otherwise, it takes the difference between the range minimum and maximum,
        /// divides by the tick frequency, and adds 2.</returns>
        public int? GetTickCount()
        {
            if (IsHandleCreated)
                return (int)User32.SendMessage(Handle, TBM_GETNUMTICS, 0, 0);

            return null;
        }

        /// <summary>
        /// Retrieves an array that contains the positions of the tick marks for a slider.
        /// </summary>
        /// <remarks>
        /// The number of elements in the array is two less than the tick count returned by the <see cref="GetTickCount"/> method. Note that
        /// the values in the array may include duplicate positions and may not be in sequential order. The returned
        /// array is valid until you change the slider's tick marks.
        /// </remarks>
        /// <returns>An <see cref="uint"/> array. The elements of the array specify the logical positions of the slider's tick marks,
        /// not including the first and last tick marks created by the slider. The logical positions can be any of the integer
        /// values in the slider's range of minimum to maximum slider positions.</returns>
        public uint[]? GetTickPositions()
        {
            if (IsHandleCreated)
            {
                var ptr = User32.SendMessage(Handle, TBM_GETPTICS, 0, 0);
                var length = ((int)GetTickCount()!) - 2;

                if (ptr == nint.Zero || length <= 0)
                    return [];

                // Allocate a uint array to hold the DWORDs
                uint[] temp = new uint[length];

                // Copy unmanaged memory to the uint array
                Marshal.Copy(ptr, (int[])(object)temp, 0, length);

                return temp;
            }

            return null;
        }

        /// <summary>
        /// Removes the current tick marks from the slider. This message does not remove
        /// the first and last tick marks, which are created automatically by the slider.
        /// </summary>
        public void ClearTicks()
        {
            if (IsHandleCreated)
                User32.SendMessage(Handle, TBM_CLEARTICS, true, 0);
        }
          
        private bool ShouldSerializeThumbLength() => FixedLength;
        private void ResetThumbLength() => FixedLength = false;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NMTRBTHUMBPOSCHANGING
    {
        public NMHDR hdr;

        public int dwPos;

        public int nReason;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NMHDR
    {
        public IntPtr hwndFrom;
        public IntPtr idFrom;
        public int code;
    }

    /// <summary>
    /// Specifies the type of ticks to use on a <see cref="Slider"/>.
    /// </summary>
    public enum SliderTickMode
    {
        /// <summary>
        /// No ticks.
        /// </summary>
        None = 0x0010,
        /// <summary>
        /// The control has a tick mark for each increment in its range of values.
        /// </summary>
        Auto = 0,
        /// <summary>
        /// The control has a custom amount of ticks dictated by the <see cref="Slider.TickFrequency"/> property.
        /// </summary>
        Custom = 0x0001
    }

    /// <summary>
    /// Defines event arguments for the <see cref="Slider.ValueChanging"/> event.
    /// </summary>
    public class SliderValueChangingEventArgs : CancelEventArgs
    {
        internal NMTRBTHUMBPOSCHANGING internalStruct;

        /// <summary>
        /// Gets the reason for the event.
        /// </summary>
        public SliderValueChangeReason Reason => (SliderValueChangeReason)internalStruct.nReason;

        /// <summary>
        /// Gets the new position of the thumb (value of the slider).
        /// </summary>
        public int NewValue => internalStruct.dwPos;

        internal SliderValueChangingEventArgs(NMTRBTHUMBPOSCHANGING str)
        {
            internalStruct = str;
        }
    }

    /// <summary>
    /// Specifies the reason that a <see cref="Slider"/>'s value may be changing.
    /// </summary>
    /// <seealso cref="Slider"/>
    public enum SliderValueChangeReason
    {
        /// <summary>
        /// The user has decreased the slider using the arrow keys.
        /// </summary>
        SmallDecrease = 1,
        /// <summary>
        /// The user has incremented the slider using the arrow keys.
        /// </summary>
        SmallIncrease = 0,
        /// <summary>
        /// The user has incremented the slider by clicking the track.
        /// </summary>
        LargeIncrease = 2,
        /// <summary>
        /// The user has decreased the value of the slider by clicking the track.
        /// </summary>
        LargeDecrease = 3,
        /// <summary>
        /// The user has finished dragging the thumb and released the mouse button.
        /// </summary>
        Thumb = 4,
        /// <summary>
        /// The user is currently dragging the thumb and moving it around. Fires continuously while the thumb is being dragged.
        /// </summary>
        ThumbDrag = 5,
        /// <summary>
        /// The user has set the slider to the minimum value, for example by pressing the <c>Home</c> key.
        /// </summary>
        Bottom = 7,
        /// <summary>
        /// The user has set the slider to the maximum value, for example by pressing the <c>End</c> key.
        /// </summary>
        Top = 6,
        /// <summary>
        /// The user released the thumb after dragging.
        /// </summary>
        /// <remarks>
        /// Use this reason only for cleanup, as it doesn't give you the actual moved-to value. For validation, please use the <see cref="Thumb"/> reason
        /// instead.
        /// </remarks>
        ThumbEnd = 8
    }

    public class SliderDesigner : ControlDesigner
    {
        public override DesignerActionListCollection ActionLists
        {
            get
            {
                var al = new DesignerActionListCollection
                {
                    new SliderActionList(Component)
                };

                return al;
            }
        }

        public override SelectionRules SelectionRules => SelectionRules.Visible | SelectionRules.Moveable | SelectionRules.AllSizeable;
    }

    public class SliderActionList(IComponent? comp) : DesignerActionList(comp)
    {
        public void ClearSelectionRange()
        {
            if (Component is Slider slider)
            {
                slider.ClearSelectionRange();
            }
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection
            {
                new DesignerActionMethodItem(
                    this,
                    nameof(ClearSelectionRange),
                    "Clear selection range",
                    "Actions",   // category in the smart tag menu
                    "Clears the selection range on this slider.",
                    true)      // include as designer verb
            };

            return items;
        }
    }

    /// <summary>
    /// Specifes the side where the tooltip is located on a <see cref="Slider"/>.
    /// </summary>
    public enum SliderToolTipSide
    {
        /// <summary>
        /// The tooltip control will be positioned above the slider.
        /// </summary>
        Top = 0,
        /// <summary>
        /// The tooltip control will be positioned to the left of the slider.
        /// </summary>
        Left = 1,
        /// <summary>
        /// The tooltip control will be positioned below the slider.
        /// </summary>
        Bottom = 2,
        /// <summary>
        /// The tooltip control will be positioned to the right of the trackbar.
        /// </summary>
        Right = 3
    }

    /// <summary>
    /// Provides event arguments for the <see cref="Slider.HorizontalScroll"/> and <see cref="Slider.VerticalScroll"/> events.
    /// </summary>
    public class SliderScrollEventArgs : EventArgs
    {
        internal nint wParam;
        internal nint lParam;

        /// <summary>
        /// Gets the reason for the event.
        /// </summary>
        public SliderValueChangeReason Reason => (SliderValueChangeReason)Macros.LowWord(wParam);

        /// <summary>
        /// Specifies the current position of the slider.
        /// </summary>
        /// <remarks>
        /// Specifies the current position of the slider if the <see cref="Reason"/> is <see cref="SliderValueChangeReason.Thumb"/>
        /// or <see cref="SliderValueChangeReason.ThumbDrag"/>. For all other <see cref="SliderValueChangeReason"/> values, the value of this property
        /// is zero; check the <see cref="Slider.Value"/> property to determine the slider position.
        /// </remarks>
        public int Value => Macros.HighWord(wParam);

        /// <summary>
        /// The <see cref="Slider"/> control that was the source of the event. You should rely on the <c>sender</c> parameter passed to your
        /// event handler function instead of using this property for a more reliable way of checking the event source.
        /// </summary>
        public Slider? Source => Control.FromHandle(lParam) as Slider;

        internal SliderScrollEventArgs(nint WParam, nint LParam)
        {
            wParam = WParam;
            lParam = LParam;
        }
    }
}
