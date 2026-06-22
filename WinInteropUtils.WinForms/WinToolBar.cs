//using FireBlade.WinInteropUtils;
//using System.Collections;
//using System.ComponentModel;
//using System.Drawing.Design;
//using System.Runtime.InteropServices;

//namespace FireBlade.WinInteropUtils.WinForms
//{
//    public class WinToolBar : WinInteropUtilsControlBase
//    {
//        private const int WM_USER               = 0x0400;
//        private const int TB_SETIMAGELIST       = (WM_USER + 48);
//        private const int TB_BUTTONSTRUCTSIZE   = (WM_USER + 30);
//        private const int TB_SETMAXTEXTROWS     = (WM_USER + 60);

//        private const int TBSTYLE_FLAT          = 0x0800;
//        private const int TBSTYLE_LIST          = 0x1000;
//        private const int TBSTYLE_TRANSPARENT   = 0x8000;
//        private const int TBSTYLE_CUSTOMERASE   = 0x2000;
//        private const int TBSTYLE_TOOLTIPS      = 0x0100;
//        private const int TBSTYLE_REGISTERDROP  = 0x4000;

//        private const int NM_FIRST              = 0;
//        private const int NM_CUSTOMDRAW         = (NM_FIRST - 12);

//        private const int WM_NOTIFY             = 0x004E;

//        // prevent it from using the default top docking behavior
//        private const int CCS_NOPARENTALIGN     = 0x00000008;
//        private const int CCS_NORESIZE          = 0x00000004;

//        protected override CreateParams CreateParams
//        {
//            get
//            {
//                CreateParams cp = base.CreateParams;
//                cp.ClassName = "ToolbarWindow32";
//                cp.Style |= CCS_NOPARENTALIGN | CCS_NORESIZE;

//                if (Mode != WinToolBarMode.Default)
//                    cp.Style |= Mode switch
//                    {
//                        WinToolBarMode.List => TBSTYLE_LIST,
//                        WinToolBarMode.Flat => TBSTYLE_FLAT,
//                        _ => 0
//                    };

//                if (CustomErase && !DesignMode)
//                    cp.Style |= TBSTYLE_CUSTOMERASE;

//                if (ToolTips)
//                    cp.Style |= TBSTYLE_TOOLTIPS;

//                if (RegisterDrop)
//                    cp.Style |= TBSTYLE_REGISTERDROP;

//                //if (BackColor.A == 0)
//                //    cp.Style |= TBSTYLE_TRANSPARENT;

//                return cp;
//            }
//        }

//        [DefaultValue(DockStyle.Top)]
//        public override DockStyle Dock { get => base.Dock; set => base.Dock = value; }

//        /// <summary>
//        /// Gets the collection of toolbar buttons.
//        /// </summary>
//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
//        [Description("Gets the collection of toolbar buttons.")]
//        [Category("Appearance")]
//        public WinToolBarButtonCollection Buttons { get; }

//        private ImageList? _iml;

//        /// <summary>
//        /// Gets or sets the image list used for button images.
//        /// </summary>
//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
//        [Description("Gets or sets the image list used for button images.")]
//        [Category("Appearance")]
//        public ImageList? ImageList
//        {
//            get => _iml;
//            set
//            {
//                if (_iml != null)
//                    _iml.RecreateHandle -= OnImageListHandleCreated;

//                _iml = value;

//                if (value != null)
//                    value.RecreateHandle += OnImageListHandleCreated;

//                if (value != null)
//                    Window.SendMessage(TB_SETIMAGELIST, 0, value.Handle);
//            }
//        }

//        private void OnImageListHandleCreated(object? sender, EventArgs e)
//        {
//            if (sender is ImageList iml && IsHandleCreated)
//                Window.SendMessage(TB_SETIMAGELIST, 0, iml.Handle);
//        }

//        private WinToolBarMode _mode = WinToolBarMode.Default;

//        /// <summary>
//        /// Gets or sets a value that indicates the style of the toolbar control.
//        /// </summary>
//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
//        [DefaultValue(WinToolBarMode.Default)]
//        [Category("Appearance")]
//        [Description("Gets or sets a value that indicates the style of the toolbar control.")]
//        public WinToolBarMode Mode
//        {
//            get => _mode;
//            set
//            {
//                _mode = value;

//                if (IsHandleCreated)
//                    RecreateHandle();
//            }
//        }

//        private bool _customErase = false;

//        /// <summary>
//        /// Gets or sets a value that indicates whether the application is responsible for drawing the toolbar background.
//        /// </summary>
//        /// <remarks>The application must handle the <see cref="PaintBackground"/> event.</remarks>
//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
//        [DefaultValue(false)]
//        [Description("Gets or sets a value that indicates whether the application is responsible for drawing the toolbar background. " +
//            "The application must handle the PaintBackground event.")]
//        [Category("Appearance")]
//        public bool CustomErase
//        {
//            get => _customErase;
//            set
//            {
//                _customErase = value;

//                if (IsHandleCreated)
//                    RecreateHandle();
//            }
//        }

//        private int _maxText = 1;

//        /// <summary>
//        /// Gets or sets the maximum amount of text rows for toolbar buttons.
//        /// </summary>
//        [DefaultValue(1)]
//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
//        [Description("Gets or sets the maximum amount of text rows for toolbar buttons.")]
//        [Category("Appearance")]
//        public int MaxTextRows
//        {
//            get => _maxText;
//            set
//            {
//                _maxText = value;

//                Window.SendMessage(TB_SETMAXTEXTROWS, (nuint)value, 0);
//            }
//        }

//        private bool _tooltips = false;

//        /// <summary>
//        /// Gets or sets a value that indicates whether tooltips are shown in the toolbar.
//        /// </summary>
//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
//        [DefaultValue(false)]
//        [Category("Behavior")]
//        public bool ToolTips
//        {
//            get => _tooltips;
//            set
//            {
//                _tooltips = value;

//                if (IsHandleCreated)
//                    RecreateHandle();
//            }
//        }

//        private bool _regDrop = false;

//        /// <summary>
//        /// Gets or sets a value that indicates whether the <see cref="ObjectDropped"/> event is fired
//        /// to request drop target objects when the cursor passes over toolbar buttons.
//        /// </summary>
//        [Category("Behavior")]
//        [Description("Gets or sets a value that indicates whether the ObjectDropped event is fired" +
//            " to request drop target objects when the cursor passes over toolbar buttons.")]
//        [DefaultValue(false)]
//        public bool RegisterDrop
//        {
//            get => _regDrop;
//            set
//            {
//                _regDrop = value;

//                if (IsHandleCreated)
//                    RecreateHandle();
//            }
//        }

//        /// <summary>
//        /// Fires whenever the toolbar requests a drop target object when the pointer passes over one of its buttons.
//        /// </summary>
//        /// <remarks>
//        /// <para>When handling this event, you must set <see cref="WinToolBarObjectDroppedEventArgs.Interface"/> to a valid
//        /// COM object, and <see cref="WinToolBarObjectDroppedEventArgs.HResult"/> to a COM result code.</para>
//        /// </remarks>
//        [Category("Drag Drop")]
//        [Description("Fires whenever the toolbar requests a drop target object when the pointer passes over one of its buttons.")]
//        public event EventHandler<WinToolBarObjectDroppedEventArgs>? ObjectDropped;

//        public WinToolBar()
//        {
//            Dock = DockStyle.Top;
//            Buttons = new(this);

//            SetStyle(ControlStyles.UserPaint, false);
//            //SetStyle(ControlStyles.SupportsTransparentBackColor, true);
//        }

//        protected override void OnHandleCreated(EventArgs e)
//        {
//            base.OnHandleCreated(e);

//            Window.SendMessage(TB_BUTTONSTRUCTSIZE, (nuint)Marshal.SizeOf<TBBUTTON>(), 0);

//            if (_iml != null)
//                Window.SendMessage(TB_SETIMAGELIST, 0, _iml.Handle);

//            Window.SendMessage(TB_SETMAXTEXTROWS, (nuint)MaxTextRows, 0);
//            Buttons.Refresh();
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        internal struct NMHDR
//        {
//            public IntPtr hwndFrom;
//            public IntPtr idFrom;
//            public int code; 
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        private struct RECT
//        {
//            public int left;
//            public int top;
//            public int right;
//            public int bottom;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        private struct NMCUSTOMDRAW
//        {
//            public NMHDR hdr;

//            public uint dwDrawStage;
//            public IntPtr hdc;
//            public RECT rc;
//            public IntPtr dwItemSpec;
//            public uint uItemState;
//            public IntPtr lItemlParam;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        private struct NMTBCUSTOMDRAW
//        {
//            public NMCUSTOMDRAW nmcd;
//            public int clrText;
//            public int clrMark;
//        }

//        private const int CDDS_PREERASE = 0x00000003;

//        /// <summary>
//        /// Fires when the <see cref="CustomErase"/> property is <see langword="true"/> and the toolbar needs to
//        /// paint its background.
//        /// </summary>
//        [Description("Fires when the CustomErase property is true and the toolbar needs to paint its background.")]
//        [Category("Paint")]
//        public event PaintEventHandler? PaintBackground;

//        private const int CDRF_SKIPDEFAULT = 0x00000004;
//        private const int WM_REFLECT = WM_USER + 0x1C00;

//        private const int TBN_FIRST     = -700;
//        private const int TBN_GETOBJECT = (TBN_FIRST - 12);

//        [StructLayout(LayoutKind.Sequential)]
//        internal unsafe struct NMOBJECTNOTIFY
//        {
//            public NMHDR hdr;
//            public int iItem;

//            public Guid* piid;
//            public nint pObject;

//            public int hResult;
//            public uint dwFlags;
//        }

//        protected override void WndProc(ref Message m)
//        {
//            base.WndProc(ref m);

//            if (m.Msg == WM_NOTIFY + WM_REFLECT)
//            {
//                unsafe
//                {
//                    NMHDR* nmhdr = (NMHDR*)m.LParam;

//                    switch (nmhdr->code)
//                    {
//                        case NM_CUSTOMDRAW:
//                            NMTBCUSTOMDRAW* pnmtbcd = (NMTBCUSTOMDRAW*)m.LParam;
                            
//                            if (pnmtbcd->nmcd.dwDrawStage == CDDS_PREERASE && CustomErase && !DesignMode)
//                            {
//                                using Graphics gh = Graphics.FromHdc(pnmtbcd->nmcd.hdc);
//                                Rectangle      rc = Rectangle.FromLTRB(pnmtbcd->nmcd.rc.left,
//                                                                        pnmtbcd->nmcd.rc.top,
//                                                                        pnmtbcd->nmcd.rc.right,
//                                                                        pnmtbcd->nmcd.rc.bottom);

//                                PaintEventArgs pe = new PaintEventArgs(gh, rc);

//                                PaintBackground?.Invoke(this, pe);

//                                m.Result = CDRF_SKIPDEFAULT;
//                            }
//                            break;
//                        case TBN_GETOBJECT:
//                            NMOBJECTNOTIFY* pnmon = (NMOBJECTNOTIFY*)m.LParam;
//                            WinToolBarObjectDroppedEventArgs args = new WinToolBarObjectDroppedEventArgs(*pnmon, this, *pnmon->piid);

//                            ObjectDropped?.Invoke(this, args);

//                            pnmon->hResult = args.HResult.FullCode;

//                            if (args.Interface != null)
//                            {
//                                nint ppv;
//                                nint pUnk = Marshal.GetIUnknownForObject(args.Interface);

//                                try
//                                {
//                                    HResult hr = Marshal.QueryInterface(
//                                        pUnk,
//                                        ref *pnmon->piid,
//                                        out ppv);

//                                    if (Macros.Succeeded(hr))
//                                        pnmon->pObject = ppv;
//                                }
//                                finally
//                                {
//                                    Marshal.Release(pUnk);
//                                }
//                            }

//                            m.Result = 0;
//                            break;
//                    }
//                }
//            }
//        }
//    }

//    /// <summary>
//    /// Represents the mode of a <see cref="WinToolBar"/>.
//    /// </summary>
//    public enum WinToolBarMode
//    {
//        /// <summary>
//        /// The default toolbar appearance.
//        /// </summary>
//        Default,
//        /// <summary>
//        /// Represents the flat toolbar appearance with button text to the right of the bitmap.
//        /// </summary>
//        List,
//        /// <summary>
//        /// Represents the flat toolbar appearance. In a flat toolbar, both the toolbar and the buttons
//        /// are transparent and hot-tracking is enabled. Button text appears under button bitmaps.
//        /// </summary>
//        Flat
//    }

//    public class WinToolBarButton
//    {
//        internal WinToolBar _tb = null!;
//        internal int _i = -1;

//        private const int TBIF_IMAGE = 0x00000001;
//        private const int TBIF_TEXT = 0x00000002;

//        private const int WM_USER = 0x0400;
//        private const int TB_SETBUTTONINFOW = (WM_USER + 64);

//        [Browsable(false)]
//        public WinToolBar ToolBar => _tb;

//        [Editor($"System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0," +
//            $" Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
//        public int ImageIndex
//        {
//            get => _img;
//            set
//            {
//                _img = value;

//                if (_i != -1)
//                {
//                    TBBUTTONINFO tbbi = new();
//                    tbbi.cbSize = (uint)Marshal.SizeOf<TBBUTTONINFO>();
//                    tbbi.dwMask = TBIF_IMAGE;
//                    tbbi.iImage = value;

//                    nint ptr = Marshal.AllocHGlobal(Marshal.SizeOf<TBBUTTONINFO>());
//                    Marshal.StructureToPtr(tbbi, ptr, false);

//                    _tb.Window.SendMessage(TB_SETBUTTONINFOW, (uint)_i + 12000u, ptr);

//                    Marshal.FreeHGlobal(ptr);
//                }
//            }
//        }

//        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
//        private struct TBBUTTONINFO
//        {
//            public uint cbSize;
//            public uint dwMask;
//            public int idCommand;
//            public int iImage;

//            public byte fsState;
//            public byte fsStyle;
//            public ushort cx;

//            public IntPtr lParam;
//            public IntPtr pszText;

//            public int cchText;
//        }

//        //[Editor($"System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0," +
//        //    $" Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
//        //public string ImageKey { get; set; } = string.Empty;

//        internal int _img;
//        private string _label = string.Empty;

//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
//        public string Label
//        {
//            get => _label;
//            set
//            {
//                _label = value;

//                if (_i != -1)
//                {
//                    TBBUTTONINFO tbbi = new();
//                    tbbi.cbSize = (uint)Marshal.SizeOf<TBBUTTONINFO>();
//                    tbbi.dwMask = TBIF_TEXT;
                    
//                    nint pszText = Marshal.StringToHGlobalUni(value);

//                    tbbi.pszText = pszText;
//                    tbbi.cchText = value.Length + 1;

//                    nint ptr = Marshal.AllocHGlobal(Marshal.SizeOf<TBBUTTONINFO>());
//                    Marshal.StructureToPtr(tbbi, ptr, false);

//                    _tb.Window.SendMessage(TB_SETBUTTONINFOW, (uint)_i + 12000u, ptr);

//                    Marshal.FreeHGlobal(pszText);
//                    Marshal.FreeHGlobal(ptr);
//                }
//            }
//        }

//        public override string ToString() => Label ?? "Button";
//    }

//    [StructLayout(LayoutKind.Sequential)]
//    internal struct TBBUTTON
//    {
//        public int iBitmap;
//        public int idCommand;
//        public byte fsState;
//        public byte fsStyle;
//        public byte bReserved1;
//        public byte bReserved2;
//        public IntPtr dwData;
//        public IntPtr iString;
//    }

//    [ListBindable(false)]
//    public class WinToolBarButtonCollection : CollectionBase
//    {
//        internal WinToolBar _tb;

//        private const int WM_USER = 0x0400;
//        private const int TB_INSERTBUTTON = (WM_USER + 21);
//        private const byte TBSTATE_ENABLED = 0x04;

//        public WinToolBarButton? this[int i] => (WinToolBarButton?)List[i];

//        public WinToolBarButtonCollection(WinToolBar tb)
//        {
//            _tb = tb;
//        }

//        private void InsertButton(int index, WinToolBarButton btn)
//        {
//            btn._tb = _tb;
//            btn._i = index;

//            TBBUTTON tbb = new();
//            tbb.iBitmap = btn._img;
//            tbb.idCommand = index + 12000;
//            tbb.fsState = TBSTATE_ENABLED;
//            tbb.fsStyle = 0;

//            tbb.iString = IntPtr.Zero;

//            nint ptr = Marshal.AllocHGlobal(Marshal.SizeOf<TBBUTTON>());
//            Marshal.StructureToPtr(tbb, ptr, false);

//            _tb.Window.SendMessage(TB_INSERTBUTTON, (nuint)index, ptr);

//            Marshal.FreeHGlobal(ptr);

//            // Reset the text.
//            // This is neccessary, as setting iString directly causes string truncation
//            // a.k.a makes it so that the toolbar only uses the first char of the label
//            btn.Label = btn.Label;
//        }

//        protected override void OnInsert(int index, object? value)
//        {
//            base.OnInsert(index, value);

//            if (value is WinToolBarButton btn)
//                InsertButton(index, btn);
//        }

//        public void Add(WinToolBarButton button)
//        {
//            List.Add(button);
//        }

//        private const int TB_DELETEBUTTON = (WM_USER + 22);
//        private const int TB_BUTTONCOUNT = (WM_USER + 24);

//        protected override void OnClear()
//        {
//            base.OnClear();

//            int count = (int)_tb.Window.SendMessage(TB_BUTTONCOUNT, 0, 0);

//            for (int i = count - 1; i >= 0; i--)
//                _tb.Window.SendMessage(TB_DELETEBUTTON, (nuint)i, 0);
//        }

//        internal void Refresh()
//        {
//            int count = (int)_tb.Window.SendMessage(TB_BUTTONCOUNT, 0, 0);

//            for (int i = count - 1; i >= 0; i--)
//                _tb.Window.SendMessage(TB_DELETEBUTTON, (nuint)i, 0);

//            int ii = 0;
//            foreach (var button in List)
//            {
//                InsertButton(ii, (WinToolBarButton)button);
//                ii++;
//            }
//        }

//        public override string ToString() => $"Collection ({Count})";
//    }


//    /// <summary>
//    /// Provides event data for the <see cref="WinToolBar.ObjectDropped"/> event.
//    /// </summary>
//    public class WinToolBarObjectDroppedEventArgs : EventArgs
//    {
//        private WinToolBarButton _btn = null!;

//        /// <summary>
//        /// Gets or sets the button whose object is requested.
//        /// </summary>
//        public WinToolBarButton Button => _btn;

//        private Guid _iid;

//        /// <summary>
//        /// Gets the <see cref="Guid"/> of the interface being requested.
//        /// </summary>
//        public Guid RequestedGuid => _iid;

//        /// <summary>
//        /// An interface representing the object being dropped.
//        /// </summary>
//        /// <remarks>The application handling the <see cref="WinToolBar.ObjectDropped"/>
//        /// must set this to the interface that should be used.</remarks>
//        public object? Interface { get; set; }

//        /// <summary>
//        /// Gets or sets the COM result code of the operation.
//        /// </summary>
//        public HResult HResult { get; set; } = HResult.E_NOTIMPL;

//        internal WinToolBarObjectDroppedEventArgs(WinToolBar.NMOBJECTNOTIFY nmon, WinToolBar wtb, Guid iid)
//        {
//            for (int i = 0; i < wtb.Buttons.Count; i++)
//                if (i == nmon.iItem - 12000 && wtb.Buttons[i] is WinToolBarButton b)
//                {
//                    _btn = b;
//                    break;
//                }

//            _iid = iid;
//        }

//        /// <summary>
//        /// Creates a new instance of the <see cref="WinToolBarObjectDroppedEventArgs"/> class for the specified toolbar button with
//        /// the specified <see cref="Guid"/>.
//        /// </summary>
//        /// <param name="btn">The toolbar button for which the event is fired.</param>
//        /// <param name="iid">The <see cref="Guid"/> of the interface requested.</param>
//        public WinToolBarObjectDroppedEventArgs(WinToolBarButton btn, Guid iid)
//        {
//            _btn = btn;
//            _iid = iid;
//        }
//    }
//}
