using FireBlade.WinInteropUtils.ComponentObjectModel;
using FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static FireBlade.WinInteropUtils.Macros;

// sorry for this mess of a control but its com
namespace FireBlade.WinInteropUtils.WinForms.Explorer
{
    /// <summary>
    /// Represents an Explorer view.
    /// </summary>
    internal partial class ShellView : Control
    {
        private IExplorerBrowser? _explorerBrowser = null;

        public ShellView()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, false);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHCreateItemFromParsingName(
            string pszPath,
            IntPtr pbc,
            ref Guid riid,
            out IShellItem ppv);

        private uint _eventCookie;

        [DllImport("shell32.dll")]
        private static extern int SHCreateItemFromIDList(
            nint pidl,
            ref Guid riid,
            out IShellItem ppv);

        private static string PidlToPath(nint pidl)
        {
            if (pidl == nint.Zero)
                return string.Empty;

            // we don't use SHGetPathFromIDListW because this can allocate the buffer of the right length
            // for us
            Guid iid = typeof(IShellItem).GUID;

            int hr = SHCreateItemFromIDList(pidl, ref iid, out IShellItem item);
            if (hr == 0 && item != null)
            {
                try
                {
                    if (item.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEPARSING, out nint psz) == 0)
                    {
                        string result = Marshal.PtrToStringUni(psz) ?? string.Empty;
                        Marshal.FreeCoTaskMem(psz);
                        return result;
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(item);
                }
            }

            return string.Empty;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            try
            {
                HResult hr = COM.Initialize(COM.COMInitOptions.ApartmentThreaded);
                if (!Succeeded(hr))
                    throw hr;

                IExplorerBrowser? browser =
                    COM.CreateInstance<IExplorerBrowser>(
                        new Guid("71F96385-DDD6-48D3-A0C1-AE06E8B055FB"),
                        null,
                        COM.CreateInstanceContext.InprocServer);

                // we have to do this to shut the warnings up
                // even though we know that it won't be null
                // because if it is then CreateInstance will throw an exception
                // and won't reach here
                if (browser is not null)
                {
                    _explorerBrowser = browser;
                    
                    RECT rc = new RECT
                    {
                        left = 0,
                        top = 0,
                        right = ClientSize.Width,
                        bottom = ClientSize.Height
                    };

                    FOLDERSETTINGS fs = new FOLDERSETTINGS
                    {
                        ViewMode = View,
                        fFlags = FOLDERFLAGS.FWF_NOWEBVIEW
                    };

                    _sink = new ShellViewEventSink();
                    _sink.ViewCreated += OnViewCreated;
                    _sink.NavigationFailed += OnNavigationFailed;
                    _sink.NavigationComplete += OnNavigationComplete;
                    _sink.NavigationPending += OnNavigationPending;

                    _explorerBrowser.Advise(_sink, out _eventCookie);

                    _explorerBrowser.Initialize(Handle, ref rc, ref fs);

                    //_explorerBrowser.SetOptions(
    //EXPLORER_BROWSER_OPTIONS.EBO_SHOWFRAMES);

                    _explorerBrowser.BrowseToObject(_lastItem!, 0);

                    //IShellItem item;
                    //Guid iid = typeof(IShellItem).GUID;

                    //hr = SHCreateItemFromParsingName(
                    //    "C:\\",
                    //    IntPtr.Zero,
                    //    ref iid,
                    //    out item);

                    //if (Succeeded(hr))
                    //{
                    //    _explorerBrowser.BrowseToObject(item, 0);
                    //}
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Fires when a navigation operation is about to occur in the shell view.
        /// </summary>
        [Category("Navigation")]
        [Description("Fires when a navigation operation is about to occur in the shell view.")]
        public event EventHandler<ShellNavigationEventArgs>? NavigationPending;
        /// <summary>
        /// Fires when a navigation operation has completed successfully in the shell view.
        /// </summary>
        [Category("Navigation")]
        [Description("Fires when a navigation operation has completed successfully in the shell view.")]
        public event EventHandler<ShellNavigationEventArgs>? NavigationComplete;
        /// <summary>
        /// Occurs when a navigation request fails.
        /// </summary>
        /// <remarks>This event is raised when an attempt to navigate to a new location does not succeed.
        /// The caller can use the event arguments to determine the cause of the failure and respond
        /// appropriately.</remarks>
        [Category("Navigation")]
        [Description("Occurs when a navigation request fails.")]
        public event EventHandler<ShellNavigationEventArgs>? NavigationFailed;
        /// <summary>
        /// Occurs when a shell view is created.
        /// </summary>
        /// <remarks>Subscribe to this event to perform additional initialization or respond when a view
        /// is instantiated. The event is raised after the view has been successfully created, but before it is
        /// displayed to the user.</remarks>
        [Category("View")]
        [Description("Occurs when a shell view is created.")]
        public event EventHandler? ViewCreated;

        private void OnNavigationPending(object? sender, nint e)
        {
            var args = new ShellNavigationEventArgs(e, PidlToPath(e));

            NavigationPending?.Invoke(this, args);
        }

        private void OnNavigationComplete(object? sender, nint e)
        {
            var args = new ShellNavigationEventArgs(e, PidlToPath(e));

            NavigationComplete?.Invoke(this, args);
        }

        private void OnNavigationFailed(object? sender, nint e)
        {
            var args = new ShellNavigationEventArgs(e, PidlToPath(e));

            NavigationFailed?.Invoke(this, args);
        }

        private void OnViewCreated(object? sender, EventArgs e)
        {
            ViewCreated?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);

            if (_explorerBrowser != null)
            {
                if (_sink != null)
                {
                    _sink.ViewCreated -= OnViewCreated;
                    _sink.NavigationFailed -= OnNavigationFailed;
                    _sink.NavigationComplete -= OnNavigationComplete;
                    _sink.NavigationPending -= OnNavigationPending;

                    _explorerBrowser.Unadvise(_eventCookie);
                    _sink = null;
                }

                _explorerBrowser.Destroy();
                Marshal.FinalReleaseComObject(_explorerBrowser);

                _explorerBrowser = null;
            }
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);

            if (_explorerBrowser != null)
            {
                RECT rc = new RECT
                {
                    left = 0,
                    top = 0,
                    right = ClientSize.Width,
                    bottom = ClientSize.Height
                };

                unsafe
                {
                    _explorerBrowser.SetRect(nint.Zero, rc);
                }
            }
        }

        private IShellItem? _lastItem = null;
        private ShellViewEventSink? _sink;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text => base.Text;

        /// <summary>
        /// Gets or sets the current path displayed in the Explorer view.
        /// </summary>
        /// <remarks>Note that if the path is not a standard folder, such as a special
        /// folder view, this property may return an empty string. You can get a pointer
        /// to an item ID list through the <see cref="GetPIDL"/> function instead.</remarks>
        [Category("Behavior")]
        [Description("Gets or sets the current path displayed in the Explorer view.")]
        public string Path
        {
            get
            {
                if (_explorerBrowser == null)
                    return string.Empty;

                Guid iid = typeof(IFolderView).GUID;
                nint pView;

                if (Failed(_explorerBrowser.GetCurrentView(ref iid, out pView)) || pView == 0)
                    return string.Empty;

                var folderView = (IFolderView)Marshal.GetObjectForIUnknown(pView);

                Guid iidShellItem = typeof(IShellItem).GUID;
                nint pItem;

                if (Failed(folderView.GetFolder(ref iidShellItem, out pItem)) || pItem == 0)
                {
                    Marshal.Release(pView);
                    return string.Empty;
                }

                var shellItem = (IShellItem)Marshal.GetObjectForIUnknown(pItem);

                if (Succeeded(shellItem.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out nint pszName)))
                {
                    string path = Marshal.PtrToStringUni(pszName) ?? string.Empty;
                    Marshal.FreeCoTaskMem(pszName);

                    Marshal.Release(pItem);
                    Marshal.Release(pView);

                    return path;
                }

                Marshal.Release(pItem);
                Marshal.Release(pView);

                return string.Empty;
            }

            set
            {
                Guid iid = typeof(IShellItem).GUID;
                IShellItem item = null!;

                if (Succeeded(SHCreateItemFromParsingName(
                    value,
                    IntPtr.Zero,
                    ref iid,
                    out item)))
                    _explorerBrowser?.BrowseToObject(item, 0);

                if (_lastItem != null)
                {
                    Marshal.ReleaseComObject(_lastItem);
                    _lastItem = null;
                }

                if (item != null)
                    _lastItem = item;

            }
        }

        // iunknown is not supported in source-generated pinvoke
        [DllImport("shell32.dll")]
        private static extern int SHGetIDListFromObject(
            [MarshalAs(UnmanagedType.IUnknown)] object punk,
            out nint ppidl);

        /// <summary>
        /// Retrieves a pointer to the Item ID List of the currently displayed folder
        /// in the shell view. This is useful for scenarios where the current folder is not a standard file system
        /// folder, such as a special folder view, and the <see cref="Path"/> property returns an empty string.
        /// </summary>
        /// <returns>A pointer to the Item ID List of the currently displayed
        /// folder, or <see cref="nint.Zero"/> if the pointer couldn't be retrieved.</returns>
        public nint GetPIDL()
        {
            if (_explorerBrowser == null)
                return nint.Zero;

            Guid iid = typeof(IFolderView).GUID;
            nint pView;

            if (Failed(_explorerBrowser.GetCurrentView(ref iid, out pView)) || pView == 0)
                return nint.Zero;

            var folderView = (IFolderView)Marshal.GetObjectForIUnknown(pView);

            Guid iidShellItem = typeof(IShellItem).GUID;
            nint pItem;

            if (Failed(folderView.GetFolder(ref iidShellItem, out pItem)) || pItem == 0)
            {
                Marshal.Release(pView);
                return nint.Zero;
            }

            var shellItem = (IShellItem)Marshal.GetObjectForIUnknown(pItem);

            HResult hr = SHGetIDListFromObject(shellItem, out nint pidl);
            if (Succeeded(hr))
            {
                Marshal.Release(pItem);
                Marshal.Release(pView);

                return pidl;
            }

            Marshal.Release(pItem);
            Marshal.Release(pView);

            return nint.Zero;
        }

        /// <summary>
        /// Navigates to the folder pointed to by the specified pointer to an Item ID List. This is useful
        /// for scenarios where the target folder is not a standard file system item, such as a shell folder.
        /// </summary>
        /// <param name="pidl">A pointer to the Item ID List of the item to navigate to.</param>
        /// <returns>A pointer to the Item ID List of the previous folder.</returns>
        public nint SetPIDL(nint pidl)
        {
            if (_explorerBrowser == null)
                return nint.Zero;

            nint oldPidl = GetPIDL();
            HResult hr = _explorerBrowser.BrowseToIDList(pidl, 0);

            if (Failed(hr))
                return nint.Zero;

            return oldPidl;
        }

        private ShellViewMode _view = ShellViewMode.Details;

        /// <summary>
        /// Gets or sets the current view mode of the shell view.
        /// </summary>
        [Description("Gets or sets the current view mode of the shell view.")]
        [Category("Appearance")]
        public ShellViewMode View
        {
            get => _view;
            set
            {
                _view = value;

                if (_explorerBrowser != null)
                {
                    FOLDERSETTINGS fs = new FOLDERSETTINGS
                    {
                        ViewMode = _view,
                        fFlags = FOLDERFLAGS.FWF_NOWEBVIEW
                    };
                    _explorerBrowser.SetFolderSettings(ref fs);
                }
            }
        }

        #region Interop Declarations
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FOLDERSETTINGS
        {
            public ShellViewMode ViewMode;
            public FOLDERFLAGS fFlags;
        }

        [Flags]
        private enum FOLDERFLAGS : uint
        {
            FWF_NONE = 0x00000000,
            FWF_AUTOARRANGE = 0x00000001,
            FWF_ABBREVIATEDNAMES = 0x00000002,
            FWF_SNAPTOGRID = 0x00000004,
            FWF_OWNERDATA = 0x00000008,
            FWF_BESTFITWINDOW = 0x00000010,
            FWF_DESKTOP = 0x00000020,
            FWF_SINGLESEL = 0x00000040,
            FWF_NOSUBFOLDERS = 0x00000080,
            FWF_TRANSPARENT = 0x00000100,
            FWF_NOCLIENTEDGE = 0x00000200,
            FWF_NOSCROLL = 0x00000400,
            FWF_ALIGNLEFT = 0x00000800,
            FWF_NOICONS = 0x00001000,
            FWF_SHOWSELALWAYS = 0x00002000,
            FWF_NOVISIBLE = 0x00004000,
            FWF_SINGLECLICKACTIVATE = 0x00008000,
            FWF_NOWEBVIEW = 0x00010000,
            FWF_HIDEFILENAMES = 0x00020000,
            FWF_CHECKSELECT = 0x00040000
        }

        [Flags]
        private enum EXPLORER_BROWSER_OPTIONS : uint
        {
            EBO_NONE = 0x0000,
            EBO_NAVIGATEONCE = 0x0001,
            EBO_SHOWFRAMES = 0x0002,
            EBO_ALWAYSNAVIGATE = 0x0004,
            EBO_NOTRAVELLOG = 0x0008,
            EBO_NOWRAPPERWINDOW = 0x0010,
            EBO_HTMLSHAREPOINTVIEW = 0x0020
        }

        [Flags]
        private enum EXPLORER_BROWSER_FILL_FLAGS : uint
        {
            EBF_NONE = 0x0000,
            EBF_SELECTFROMDATAOBJECT = 0x0001,
            EBF_NODROPTARGET = 0x0002,
            EBF_NOGLOBALOBJECTS = 0x0004,
            EBF_SELECTFROMDATAOBJECTEX = 0x0008
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("DFD3B6B5-C10C-4BE9-85F6-A66969F402F6")]
        private interface IExplorerBrowser
        {
            [PreserveSig]
            HResult Initialize(
                nint hwndParent,
                ref RECT prc,
                ref FOLDERSETTINGS pfs
            );

            [PreserveSig]
            HResult Destroy();

            [PreserveSig]
            HResult SetRect(
                nint phdwp,
                RECT rcBrowser
            );

            [PreserveSig]
            HResult SetPropertyBag(
                [MarshalAs(UnmanagedType.LPWStr)] string pszPropertyBag
            );

            [PreserveSig]
            HResult SetEmptyText(
                [MarshalAs(UnmanagedType.LPWStr)] string pszEmptyText
            );

            [PreserveSig]
            HResult SetFolderSettings(
                ref FOLDERSETTINGS pfs
            );

            [PreserveSig]
            HResult Advise(
                [MarshalAs(UnmanagedType.Interface)] object psbe,
                out uint pdwCookie
            );

            [PreserveSig]
            HResult Unadvise(uint dwCookie);

            [PreserveSig]
            HResult SetOptions(EXPLORER_BROWSER_OPTIONS dwFlag);

            [PreserveSig]
            HResult GetOptions(out EXPLORER_BROWSER_OPTIONS pdwFlag);

            [PreserveSig]
            HResult BrowseToIDList(
                nint pidl,
                uint uFlags
            );

            [PreserveSig]
            HResult BrowseToObject(
                [MarshalAs(UnmanagedType.IUnknown)] object punk,
                uint uFlags
            );

            [PreserveSig]
            HResult FillFromObject(
                [MarshalAs(UnmanagedType.IUnknown)] object punk,
                EXPLORER_BROWSER_FILL_FLAGS dwFlags
            );

            [PreserveSig]
            HResult RemoveAll();

            [PreserveSig]
            HResult GetCurrentView(
                ref Guid riid,
                out nint ppv
            );
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("cde725b0-ccc9-4519-917e-325d72fab4ce")]
        private interface IFolderView
        {
            [PreserveSig]
            HResult GetCurrentViewMode(out uint pViewMode);

            [PreserveSig]
            HResult SetCurrentViewMode(uint ViewMode);

            [PreserveSig]
            HResult GetFolder(
                ref Guid riid,
                out nint ppv
            );

            [PreserveSig]
            HResult Item(int iItemIndex, out nint ppidl);

            [PreserveSig]
            HResult ItemCount(uint uFlags, out int pcItems);

            [PreserveSig]
            HResult Items(
                uint uFlags,
                ref Guid riid,
                out nint ppv
            );

            [PreserveSig]
            HResult GetSelectionMarkedItem(out int piItem);

            [PreserveSig]
            HResult GetFocusedItem(out int piItem);

            [PreserveSig]
            HResult GetItemPosition(nint pidl, out POINT ppt);

            [PreserveSig]
            HResult GetSpacing(out POINT ppt);

            [PreserveSig]
            HResult GetDefaultSpacing(out POINT ppt);

            [PreserveSig]
            HResult GetAutoArrange();

            [PreserveSig]
            HResult SelectItem(int iItem, uint dwFlags);

            [PreserveSig]
            HResult SelectAndPositionItems(
                uint cidl,
                nint apidl,
                nint apt,
                uint dwFlags
            );
        }
        #endregion
    }

    internal enum ShellViewMode
    {
        /// <summary>
        /// The view should determine the best view option.
        /// </summary>
        Auto = -1,
        /// <summary>
        /// The view should display medium-size icons.
        /// </summary>
        Icon = 1,
        /// <summary>
        /// The view should display small icons.
        /// </summary>
        SmallIcon = 2,
        /// <summary>
        /// Object names are displayed in a list view.
        /// </summary>
        List = 3,
        /// <summary>
        /// Object names and other selected information, such as the
        /// size or date last updated, are shown.
        /// </summary>
        Details = 4,
        /// <summary>
        /// The view should display thumbnail icons.
        /// </summary>
        Thumbnail = 5,
        /// <summary>
        /// The view should display large icons.
        /// </summary>
        Tile = 6,
        /// <summary>
        /// The view should display large icons.
        /// </summary>
        LargeIcon = Tile,
        /// <summary>
        /// The view should display icons in a filmstrip format.
        /// </summary>
        ThumbStrip = 7,
        /// <summary>
        /// The view should display content mode.
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        Content = 8
    }

    /// <summary>
    /// Provides event parameters for shell view navigation events.
    /// </summary>
    internal class ShellNavigationEventArgs : EventArgs
    {
        private nint _pidl;
        private string _path = string.Empty;

        /// <summary>
        /// Gets a pointer to the item ID list of the item that is being navigated to.
        /// </summary>
        public nint PIDL => _pidl;
        /// <summary>
        /// Gets the path of the item that is being navigated to. Note that if the item is not a standard file system
        /// item this may be an empty string, and you should use the <see cref="PIDL"/> property instead to get a pointer
        /// to the item ID list of the target item.
        /// </summary>
        public string Path => _path;

        internal ShellNavigationEventArgs(nint pidl, string path)
        {
            _pidl = pidl;
            _path = path;
        }
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("361BBDC7-E6EE-4E13-BE58-58E2240C810F")]
    internal interface IExplorerBrowserEvents
    {
        [PreserveSig]
        HResult OnNavigationPending(nint pidlFolder);

        [PreserveSig]
        HResult OnViewCreated(
            [MarshalAs(UnmanagedType.IUnknown)] object psv);

        [PreserveSig]
        HResult OnNavigationComplete(nint pidlFolder);

        [PreserveSig]
        HResult OnNavigationFailed(nint pidlFolder);
    }

    internal partial class ShellViewEventSink : IExplorerBrowserEvents
    {
        internal event EventHandler<nint>? NavigationPending;
        internal event EventHandler<nint>? NavigationComplete;
        internal event EventHandler<nint>? NavigationFailed;
        internal event EventHandler? ViewCreated;

        [LibraryImport("shell32.dll")]
        private static partial nint ILClone(nint pidl);

        [LibraryImport("shell32.dll")]
        private static partial void ILFree(nint pidl);

        public HResult OnNavigationPending(nint pidlFolder)
        {
            nint pidl = ILClone(pidlFolder);

            NavigationPending?.Invoke(this, pidl);


            return HResult.S_OK;
        }

        public HResult OnViewCreated([MarshalAs(UnmanagedType.IUnknown)] object psv)
        {
            ViewCreated?.Invoke(this, EventArgs.Empty);
            return HResult.S_OK;
        }

        public HResult OnNavigationComplete(nint pidlFolder)
        {
            NavigationComplete?.Invoke(this, ILClone(pidlFolder));
            return HResult.S_OK;
        }

        public HResult OnNavigationFailed(nint pidlFolder)
        {
            NavigationFailed?.Invoke(this, ILClone(pidlFolder));
            return HResult.S_OK;
        }
    }

}
