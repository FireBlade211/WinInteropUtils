using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FireBlade.WinInteropUtils.WinForms
{
    // TODO: Make the LinkClicked actually work to finish this control

    /// <summary>
    /// Represents a control that renders marked-up text, and notifies the application when users click its embedded hyperlinks.
    /// </summary>
    /// <remarks>
    /// <para>To use this control, set the text to a string containing a HTML anchor tag (<c>&lt;a&gt;</c>). For more info, see the HyperLink article.</para>
    /// 
    /// > [!NOTE]
    /// > Requires visual styles. Make sure that you include <see cref="Application.EnableVisualStyles()"/> inside your <c>Program</c> class
    /// > before starting your application.
    /// </remarks>
    //[Designer(typeof(HyperLinkDesigner))]
    [SupportedOSPlatform("windows5.1")] // WinXP (visual styles)
    internal partial class HyperLink : Control
    {
        private const int WM_USER = 0x0400;
        private const int LM_GETIDEALHEIGHT = WM_USER + 0x301;
        private const int LM_GETIDEALSIZE = LM_GETIDEALHEIGHT;
        private const uint WM_REFLECT = WM_USER + 0x1C00;
        private const int WM_NOTIFY = 0x004E;
        private const int NM_FIRST = 0;
        private const int NM_CLICK = NM_FIRST - 2;
        private const int LM_GETITEM = WM_USER + 0x303;
        private const int LWS_TRANSPARENT = 0x0001;
        private const int LWS_IGNORERETURN = 0x0002;
        private const int LWS_IGNOREPREFIX = 0x0004;
        private const int LWS_USEVISUALSTYLE = 0x0008;
        private const int LWS_RIGHT = 0x0020;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassName = "SysLink";

                if (BackColor.Equals(Color.Transparent))
                    cp.Style |= LWS_TRANSPARENT;

                if (IgnoreReturn)
                    cp.Style |= LWS_IGNORERETURN;

                if (!UseMnemonic)
                    cp.Style |= LWS_IGNOREPREFIX;

                if (UseVisualStyle)
                    cp.Style |= LWS_USEVISUALSTYLE;

                if (OperatingSystem.IsWindowsVersionAtLeast(6, 0) && RightAlign)
                    cp.Style |= LWS_RIGHT;

                return cp;
            }
        }

#pragma warning disable CS8765
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            { 
                base.Text = value;

                //AdjustSize();
            }
        }
#pragma warning restore
        /// <summary>
        /// Occurs when a link is clicked inside the <see cref="HyperLink"/> control.
        /// </summary>
        [Description("Occurs when a link is clicked inside the HyperLink control.")]
        [Category("Action")]
        public event EventHandler<HyperLinkLinkClickedEventArgs>? LinkClicked;

        ///// <summary>
        ///// Indicates whether the control is automatically resized to fit its contents.
        ///// </summary>
        //[Description("Gets or sets a value that indicates whether the control is automatically resized to fit its contents.")]
        //[DefaultValue(false)]
        //[Browsable(true)]
        //[RefreshProperties(RefreshProperties.All)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        //public override bool AutoSize
        //{
        //    get => base.AutoSize;
        //    set
        //    {
        //        if (AutoSize != value)
        //        {
        //            base.AutoSize = value;
        //            AdjustSize();
        //        }
        //    }
        //}

        //private void AdjustSize()
        //{
        //    // If width and/or height are constrained by anchoring, don't adjust control size
        //    // to fit around text, since this will cause us to lose the original anchored size.
        //    if (!AutoSize &&
        //        ((Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right) ||
        //         (Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom)))
        //    {
        //        return;
        //    }

        //    if (IsHandleCreated)
        //    {
        //        // Allocate unmanaged memory for the SIZE structure
        //        IntPtr pSize = Marshal.AllocHGlobal(Marshal.SizeOf<SIZE>());

        //        try
        //        {
        //            // Initialize structure (optional)
        //            SIZE size = new SIZE();
        //            Marshal.StructureToPtr(size, pSize, false);

        //            // Send the message
        //            User32.SendMessage(Handle, LM_GETIDEALSIZE, (nuint)MaximumSize.Width, pSize);

        //            // Read back the structure after the message is processed
        //            SIZE updated = Marshal.PtrToStructure<SIZE>(pSize);

        //            Size = new Size(updated.cx, updated.cy);
        //        }
        //        finally
        //        {
        //            // Free unmanaged memory
        //            Marshal.FreeHGlobal(pSize);
        //        }
        //    }
        //}

        public HyperLink()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, false);
            SetStyle(ControlStyles.Selectable | ControlStyles.StandardClick | ControlStyles.UseTextForAccessibility, true);
        }

        protected override void WndProc(ref Message m)
        {
            // Detect reflected messages
            if (m.Msg >= WM_REFLECT)
            {
                WmReflect(ref m); // handle message as if it went to the control itself
                return;
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
                        case NM_CLICK:
                            var nmlink = (NMLINK)m.GetLParam(typeof(NMLINK))!;

                            if (TryGetLink(nmlink.item.iLink, out var link))
                            {
                                LinkClicked?.Invoke(this, new HyperLinkLinkClickedEventArgs
                                {
                                    ClickedLink = link
                                });
                            }
                            break;
                    }
                    break;
            }
        }

        private bool _ignoreReturn = false;

        /// <summary>
        /// Gets or sets a value indicating whether presses of the Return (enter) key should be ignored and passed to the owner dialog box.
        /// </summary>
        [Description("Gets or sets a value indicating whether presses of the Return (enter) key should be ignored and passed to the owner dialog box.")]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool IgnoreReturn
        {
            get => _ignoreReturn;
            set
            {
                _ignoreReturn = value;

                _rmakeHwnd();
            }
        }

        private bool _useVs = false;

        /// <summary>
        /// Gets or sets a value indicating whether to use the current visual style to draw the <see cref="HyperLink"/> control.
        /// </summary>
        [Description("Gets or sets a value indicating whether to use the current visual style to draw the HyperLink control.")]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool UseVisualStyle
        {
            get => _useVs;
            set
            {
                _useVs = value;

                _rmakeHwnd();
            }
        }

        private void _rmakeHwnd()
        {
            if (IsHandleCreated)
                RecreateHandle();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            RecreateHandle();
        }

        /// <summary>
        /// Retrieves the states and attributes of an item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="link"></param>
        /// <returns><see langword="true"/> if the function succeeds in getting the values and attributes specified; otherwise, <see langword="false"/>.</returns>
        public bool TryGetLink(int index, [NotNullWhen(true)] out Link? link)
        {
            link = null;

            nint ptrLi = Marshal.AllocHGlobal(Marshal.SizeOf<LITEM>());
            
            try
            {
                var li = new LITEM();
                li.mask = 0x00000002 | 0x00000008 | 0x00000004 | 0x00000001; // LIF_STATE | LIF_URL | LIF_ITEMID | LIF_ITEMINDEX
                li.iLink = index;
                unsafe
                {
                    *li.szUrl = '\0';
                    *li.szID = '\0';
                }
                li.state = 0;
                li.stateMask = (uint)(HyperLinkLinkState.Enabled |
                    HyperLinkLinkState.Focused |
                    HyperLinkLinkState.Visited |
                    HyperLinkLinkState.HotTrack |
                    HyperLinkLinkState.DefaultColors);
                Marshal.StructureToPtr(li, ptrLi, false);

                var result = User32.SendMessage(Handle, LM_GETITEM, nuint.Zero, ptrLi) == 1;

                if (result)
                {
                    LITEM newLi = Marshal.PtrToStructure<LITEM>(ptrLi);

                    link = new Link
                    {
                        _state = (HyperLinkLinkState)newLi.state,
                        HRef = string.IsNullOrEmpty(newLi.GetUrl()) || newLi.GetUrl().Equals("\0", StringComparison.Ordinal) ? null : newLi.GetUrl(),
                        Id = string.IsNullOrEmpty(newLi.GetID()) || newLi.GetID().Equals("\0", StringComparison.Ordinal) ? null : newLi.GetID(),
                        ParentControl = this,
                        Index = newLi.iLink
                    };
                }

                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(ptrLi);
            }
        }

        private bool _mnemonic = false;

        /// <summary>
        /// Gets or sets a value indicating whether the character following the first ampersand (&amp;) in the text of the <see cref="HyperLink"/> control
        /// should be treated as a mnemonic instead of a literal character.
        /// </summary>
        [Description("Gets or sets a value indicating whether the character following the first ampersand (&&) in the text of the Hyperlink control" +
            " should be treated as a mnemonic instead of a literal character.")]
        [DefaultValue(false)]
        [Category("Behavior")]
        public bool UseMnemonic
        {
            get => _mnemonic;
            set
            {
                _mnemonic = value;

                _rmakeHwnd();
            }
        }

        private bool _rightAlign = false;

        /// <summary>
        /// Gets or sets a value indicating whether the text of the <see cref="HyperLink"/> control should be aligned to the right.
        /// </summary>
        [Description("Gets or sets a value indicating whether the text of the HyperLink control should be aligned to the right.")]
        [SupportedOSPlatform("windows6.0")]
        [DefaultValue(false)]
        [Category("Appearance")]
        public bool RightAlign
        {
            get => _rightAlign;
            set
            {
                _rightAlign = value;

                _rmakeHwnd();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int cx;
            public int cy;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4, Size = 4280)]
        internal unsafe struct LITEM
        {
            public uint mask;
            public int iLink;
            public fixed char szID[48];
            public fixed char szUrl[2084];
            public uint state;
            public uint stateMask;

            public string GetID()
            {
                fixed (char* ptr = szID)
                    return new string(ptr);
            }

            public string GetUrl()
            {
                fixed (char* ptr = szUrl)
                    return new string(ptr);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NMLINK
        {
            public NMHDR hdr;
            public LITEM item;
        }
    }

    //public class HyperLinkDesigner : ControlDesigner
    //{
    //    public HyperLink LinkControl => (HyperLink)Control;

    //    public override SelectionRules SelectionRules
    //    {
    //        get
    //        {
    //            return (LinkControl.AutoSize ? 0 : SelectionRules.AllSizeable) | SelectionRules.Moveable | SelectionRules.Visible;
    //        }
    //    }

    //    public HyperLinkDesigner() : base()
    //    {
    //        AutoResizeHandles = true;
    //    }
    //}


    /// <summary>
    /// Provides event data for the <see cref="HyperLink.LinkClicked"/> event.
    /// </summary>
    internal class HyperLinkLinkClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the link that was clicked.
        /// </summary>
        #pragma warning disable CS8618
        public Link ClickedLink { get; internal set; }
        #pragma warning restore
    }

    /// <summary>
    /// Represents a single hyperlink inside a <see cref="HyperLink"/> control.
    /// </summary>
    internal class Link
    {
        private const int WM_USER = 0x0400;
        private const int LM_SETITEM = WM_USER + 0x302;
        private const int LIF_STATE = 0x00000002;

        /// <summary>
        /// Gets the index of the hyperlink inside the <see cref="HyperLink"/> control.
        /// </summary>
        public int Index { get; internal set; }

        /// <summary>
        /// Gets the <see cref="HyperLink"/> control this <see cref="Link"/> is contained in, or <see langword="null"/>.
        /// </summary>
        public HyperLink? ParentControl { get; internal set; }

        internal HyperLinkLinkState _state;

        /// <summary>
        /// Gets or sets the state of the hyperlink. This is a combination of <see cref="HyperLinkLinkState"/> values.
        /// </summary>
        public HyperLinkLinkState State
        {
            get => _state;
            set
            {
                if (ParentControl != null)
                {
                    var li = new HyperLink.LITEM();
                    li.mask = LIF_STATE;
                    li.state = (uint)value;
                    li.stateMask = (uint)(_state ^ value);
                    li.iLink = Index;

                    _state = value;


                    // allocate unmanaged memory for LITEM
                    IntPtr ptrLi = Marshal.AllocHGlobal(Marshal.SizeOf<HyperLink.LITEM>());

                    bool result = true;

                    try
                    {
                        Marshal.StructureToPtr(li, ptrLi, false);

                        result = User32.SendMessage(ParentControl.Handle, LM_SETITEM, nuint.Zero, ptrLi) == 1;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptrLi);
                    }

                    if (!result)
                        throw new Exception("The Windows API returned a non-1 value. This means that an error occured setting the state of the item.");
                }
            }
        }

        /// <summary>
        /// Gets the value of the <c>id</c> attribute on the link inside the <see cref="HyperLink"/> control's original text.
        /// </summary>
        /// <example>
        /// When the following string is set as the text content of the <see cref="HyperLink"/> control, if the user clicks the first
        /// link, the <see cref="Id"/> member of the <see cref="Link"/> class will be equal to "my id":
        /// 
        /// This is some example &lt;a id="my id"&gt;hyperlink&lt;/a&gt; text content!
        /// </example>
        public string? Id { get; internal set; }

        /// <summary>
        /// Gets the value of the <c>href</c> attribute on the link inside the <see cref="HyperLink"/> control's original text.
        /// </summary>
        /// <example>
        /// When the following string is set as the text content of the <see cref="HyperLink"/> control, if the user clicks the first
        /// link, the <see cref="HRef"/> member of the <see cref="Link"/> class will be equal to the URL of the WinInteropUtils GitHub repository:
        /// 
        /// This is some example &lt;a href="https://github.com/fireblade211/wininteroputils"&gt;hyperlink&lt;/a&gt; text content!
        /// </example>
        public string? HRef { get; internal set; }
    }

    /// <summary>
    /// Describes the state(s) of a <see cref="Link"/>.
    /// </summary>
    [Flags]
    public enum HyperLinkLinkState
    {
        /// <summary>
        /// The link can respond to user input. This is the default unless the entire control is disabled. In this case, all links are disabled.
        /// </summary>
        Enabled = 0x00000002,
        /// <summary>
        /// The link has the keyboard focus. Pressing ENTER fires the <see cref="HyperLink.LinkClicked"/> event.
        /// </summary>
        Focused = 0x00000001,
        /// <summary>
        /// The link has been visited by the user. Changing the URL to one that has not been visited causes this flag to be cleared.
        /// </summary>
        Visited = 0x00000004,
        /// <summary>
        /// Indicates that the <see cref="HyperLink"/> control will highlight in
        /// a different color (<see cref="SystemColors.Highlight"/>) when the mouse hovers over the control.
        /// </summary>
        HotTrack = 0x00000008,
        /// <summary>
        /// Enable custom text colors to be used.
        /// </summary>
        DefaultColors = 0x00000010
    }
}
