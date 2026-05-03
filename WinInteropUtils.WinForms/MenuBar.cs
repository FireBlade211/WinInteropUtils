using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static FireBlade.WinInteropUtils.Macros;

namespace FireBlade.WinInteropUtils.WinForms
{
    /// <summary>
    /// Represents a Win32 menu bar.
    /// </summary>
    internal partial class MenuBar : MenuStrip, IHandle
    {
        [DefaultValue(DockStyle.Top)]
        public override DockStyle Dock => base.Dock;

        public MenuBar()
        {
            InitializeComponent();
            Dock = DockStyle.Top;
        }

        private static readonly ConditionalWeakTable<IContainer, MenuBar> _instances =
new();

        private nint _hMenu;

        /// <summary>
        /// Gets the handle (<c>HMENU</c>) of the Win32 menu.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new nint Handle => _hMenu;

        [LibraryImport("User32.dll")]
        private static partial nint CreateMenu();

        [LibraryImport("User32.dll", SetLastError = true)]
        private static partial nint CreatePopupMenu();

        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DestroyMenu(nint hMenu);

        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetMenu(nint hWnd, nint hMenu);

        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AppendMenuW(
          nint hMenu,
          uint uFlags,
          nuint uIDNewItem,
          [MarshalAs(UnmanagedType.LPWStr)] string lpNewItem
        );

        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AppendMenuW(
          nint hMenu,
          uint uFlags,
          nuint uIDNewItem,
          nint lpNewItem
        );

        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool CheckMenuRadioItem(nint hmenu, uint first, uint last, uint check, uint flags);

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent != null)
            {
                var form = FindForm();

                if (form != null)
                {
                    int count = form.Controls.OfType<MenuBar>().Count();
                    if (count > 1 && !DesignMode)
                        throw new InvalidOperationException("Only one MenuBar per Form is allowed.");
                }
            }
        }

        private const uint _startId = 1000u;
        private uint _id = 1u;
        private MenuHook? _hook;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            _id = 1;

            if (!DesignMode)
            {
                BeginInvoke(() =>
                {
                    _hMenu = CreateMenu();

                    AddMenus(_hMenu, Items.Cast<ToolStripItem>());

                    var form = FindForm();
                    if (form != null)
                    {
                        SetMenu(form.Handle, _hMenu);
                        _hook = new MenuHook();
                        _hook.AssignHandle(form.Handle);
                        _hook.MessageReceived = HandleParentMessage;
                    }
                });
            }
        }

        private Dictionary<uint, ToolStripItem> _idItemMap = [];
        private Dictionary<ToolStripMenuItem, nint> _hMenuItemMap = [];

        private void AddMenus(nint hMenu, IEnumerable<ToolStripItem> items)
        {
            foreach (var item in items.Where(x => x.Available))
            {
                if (item is ToolStripMenuItem tsmi)
                {
                    if (tsmi.HasDropDownItems)
                    {
                        nint hM = CreatePopupMenu();

                        AddMenus(hM, tsmi.DropDownItems.OfType<ToolStripItem>());

                        AppendMenuW(hMenu, 0x00000010u, (nuint)hM, item.Text ?? string.Empty);

                        _hMenuItemMap[tsmi] = hM;
                    }
                    else
                    {
                        var flags = 0u;

                        if (tsmi.Checked)
                            flags |= 0x00000008;

                        if (tsmi is ToolStripRadioMenuItem)
                            flags |= 0x00000200;

                        if (tsmi.Alignment == ToolStripItemAlignment.Right)
                            flags |= 0x00004000;

                        if (!tsmi.Enabled)
                            flags |= 0x00000002 | 0x00000001;

                        if (tsmi.DisplayStyle == ToolStripItemDisplayStyle.Image && tsmi.Image != null)
                        {
                            flags |= 0x00000004;

                            AppendMenuW(hMenu, flags, _startId + _id, new Bitmap(tsmi.Image).GetHbitmap());
                        }
                        else if (tsmi.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText && tsmi.Image != null)
                            throw new NotSupportedException("The ImageAndText display style is not supported on Win32 MenuBars.");
                        else
                            AppendMenuW(hMenu, flags, _startId + _id, item.Text ?? string.Empty);

                        _idItemMap[_startId + _id] = tsmi;
                        _id++;
                    }
                }
                else if (item is ToolStripSeparator tss)
                {
                    AppendMenuW(hMenu, 0x00000800u, 0, string.Empty);
                }
                else if (item is ToolStripControlHost)
                    continue;
                else
                {
                    uint flags = 0;

                    if (item.Alignment == ToolStripItemAlignment.Right)
                        flags |= 0x00004000;

                    if (!item.Enabled)
                        flags |= 0x00000002 | 0x00000001;

                    if (item.DisplayStyle == ToolStripItemDisplayStyle.Image && item.Image != null)
                    {
                        flags |= 0x00000004;

                        AppendMenuW(hMenu, flags, _startId + _id, new Bitmap(item.Image).GetHbitmap());
                    }
                    else if (item.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText && item.Image != null)
                        throw new NotSupportedException("The ImageAndText display style is not supported on Win32 MenuBars.");
                    else
                        AppendMenuW(hMenu, flags, _startId + _id, item.Text ?? string.Empty);

                    _idItemMap[_startId + _id] = item;
                    _id++;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MENUITEMINFO
        {
            public uint cbSize;
            public uint fMask;
            public uint fType;
            public uint fState;
            public uint wID;
            public nint hSubMenu;
            public nint hbmpChecked;
            public nint hbmpUnchecked;
            public nuint dwItemData;
            public nint dwTypeData;
            public uint cch;
            public nint hbmpItem;
        }

#pragma warning disable CS8500
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static unsafe partial bool SetMenuItemInfoW(
            nint hMenu,
            uint uItem,
            [MarshalAs(UnmanagedType.Bool)] bool fByPositon,
            MENUITEMINFO* lpmii
        );

        [LibraryImport("user32.dll")]
        private static partial uint CheckMenuItem(nint hMenu, uint uIDCheckItem, uint uCheck);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DrawMenuBar(nint hWnd);

        protected override void OnItemAdded(ToolStripItemEventArgs e)
        {
            base.OnItemAdded(e);

            if (e.Item != null)
            {
                e.Item.TextChanged += (s, e) => OnItemChanged((ToolStripItem)s!);
                e.Item.AvailableChanged += (s, e) => OnItemChanged((ToolStripItem)s!);
                e.Item.DisplayStyleChanged += (s, e) => OnItemChanged((ToolStripItem)s!);
                e.Item.EnabledChanged += (s, e) => OnItemChanged((ToolStripItem)s!);
            }
        }

        private unsafe void OnItemChanged(ToolStripItem item)
        {
            if (!(item is ToolStripMenuItem mi)) return;
            if (mi.OwnerItem == null) return;

            if (!_hMenuItemMap.TryGetValue((ToolStripMenuItem)mi.OwnerItem, out nint hMenu))
                hMenu = _hMenu;

            if (hMenu == nint.Zero) return;

            uint id = 0;
            foreach (var kvp in _idItemMap)
                if (kvp.Value == item)
                    id = kvp.Key;

            if (id == 0) return;

            MENUITEMINFO mii = new MENUITEMINFO();
            mii.cbSize = (uint)Marshal.SizeOf<MENUITEMINFO>();
            mii.fMask = 0x00000001 | 0x00000040; // MIIM_STATE | MIIM_STRING

            mii.fState = (mi.Checked ? 0x00000008u : 0x00000000u) | (mi.Enabled ? 0u : 0x00000003u);

            IntPtr textPtr = Marshal.StringToHGlobalUni(item.Text ?? string.Empty);
            try
            {
                mii.dwTypeData = textPtr;
                mii.cch = (uint)((item.Text?.Length ?? 0) + 1); // +1 for null terminator

                if (!SetMenuItemInfoW(hMenu, id, false, &mii))
                    throw new Win32Exception();
                else
                    Debug.WriteLine("SUCCESS");
            }
            catch
            {
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(textPtr);
            }

            var form = FindForm();
            if (form != null)
                DrawMenuBar(form.Handle);
        }
#pragma warning restore

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (!DesignMode)
                DestroyMenu(_hMenu);

            base.OnHandleDestroyed(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
                base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!DesignMode)
            {
                if (Parent != null)
                {
                    Rectangle rect = new Rectangle(Point.Empty, Size);
                    e.Graphics.TranslateTransform(-Left, -Top);
                    PaintEventArgs pea = new PaintEventArgs(e.Graphics, rect);
                    InvokePaintBackground(Parent, pea);
                    InvokePaint(Parent, pea);
                    e.Graphics.TranslateTransform(Left, Top);
                }
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }

        //private const int WM_USER = 0x0400;
        private const int WM_COMMAND = 0x0111;
        //private const uint WM_REFLECT = WM_USER + 0x1C00;

        private void HandleParentMessage(Message m)
        {
            switch (m.Msg)
            {
                case WM_COMMAND:
                    Debug.WriteLine($"WM_COMMAND triggered! WPARAM: {m.WParam}, LPARAM: {m.LParam}");
                    if (HighWord(m.WParam) == 0)
                    {
                        var id = LowWord(m.WParam);

                        if (_idItemMap.TryGetValue(id, out var item))
                        {
                            item.PerformClick();

                            if (item is ToolStripMenuItem mi && mi.CheckOnClick && _hMenuItemMap.TryGetValue(mi, out nint hm))
                            {
                                //mi.Checked = !mi.Checked;
                                OnItemChanged(item);
                                //_ = CheckMenuItem(hm, id, 0x00000000u | (mi.Checked ? 0x00000008u : 0x00000000u));
                            }

                            if (item is ToolStripRadioMenuItem radio)
                            {
                                uint? idStart = null;
                                uint? idEnd = null;

                                #region Get the items
                                if (radio.StartItem != null)
                                    foreach (var kvp in _idItemMap)
                                        if (kvp.Value.Equals(radio.StartItem))
                                        {
                                            idStart = kvp.Key;
                                            break;
                                        }

                                if (radio.EndItem != null)
                                    foreach (var kvp in _idItemMap)
                                        if (kvp.Value.Equals(radio.EndItem))
                                        {
                                            idEnd = kvp.Key;
                                            break;
                                        }


                                if (radio.OwnerItem is ToolStripMenuItem owner)
                                {
                                    var separated = ToolStripSeperatedItem.GroupBySeparators(owner.DropDownItems.Cast<ToolStripItem>());

                                    if (idStart == null)
                                    {
                                        foreach (var g in separated)
                                        {
                                            if (g.Items.Contains(radio))
                                            {
                                                foreach (var i in g.Items)
                                                {
                                                    foreach (var kvp in _idItemMap)
                                                        if (kvp.Value.Equals(i))
                                                        {
                                                            idStart = kvp.Key;
                                                            break;
                                                        }

                                                    if (idStart != null) break;
                                                }

                                                if (idStart != null) break;
                                            }

                                            if (idStart != null) break;
                                        }
                                    }

                                    if (idEnd == null)
                                    {
                                        foreach (var g in separated)
                                        {
                                            if (g.Items.Contains(radio))
                                            {
                                                // have to call generic version because it's not mutable
                                                foreach (var i in g.Items.Reverse<ToolStripItem>())
                                                {
                                                    foreach (var kvp in _idItemMap)
                                                        if (kvp.Value.Equals(i))
                                                        {
                                                            idEnd = kvp.Key;
                                                            break;
                                                        }

                                                    if (idEnd != null) break;
                                                }

                                                if (idEnd != null) break;
                                            }

                                            if (idEnd != null) break;
                                        }
                                    }
                                }
                                #endregion

                                if (idStart != null && idEnd != null && item.OwnerItem is ToolStripMenuItem tsmi
                                    && _hMenuItemMap.TryGetValue(tsmi, out nint hMenu))
                                {
                                    //MessageBox.Show($"Start: {idStart}\nEnd: {idEnd}\nCheck: {id}", "DEBUG");

                                    CheckMenuRadioItem(hMenu, (uint)idStart, (uint)idEnd, id, 0x00000000); // MF_BYCOMMAND
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private class ToolStripSeperatedItem
        {
            public List<ToolStripItem> Items { get; set; } = [];

            public static IEnumerable<ToolStripSeperatedItem> GroupBySeparators(IEnumerable<ToolStripItem> items)
            {
                var result = new List<ToolStripSeperatedItem>();
                ToolStripSeperatedItem current = new();

                foreach (var item in items)
                {
                    if (item is ToolStripSeparator)
                    {
                        if (current.Items.Count > 0)
                        {
                            result.Add(current);
                            current = new();
                        }
                    }
                    else
                    {
                        current.Items.Add(item);
                    }
                }

                if (current.Items.Count > 0)
                    result.Add(current);

                return result;
            }
        }

        private class MenuHook : NativeWindow
        {
            public Action<Message>? MessageReceived;

            protected override void WndProc(ref Message m)
            {
                MessageReceived?.Invoke(m);
                base.WndProc(ref m);
            }
        }
    }

    /// <summary>
    /// Represents a radio button menu item. Note that this class can only be used inside a <see cref="MenuBar"/>; it will not automatically
    /// act as a radio button inside a regular <see cref="ToolStrip"/>.
    /// </summary>
    internal class ToolStripRadioMenuItem : ToolStripMenuItem
    {
        /// <summary>
        /// Gets or sets the first start item in the mutually exclusive radio button group.
        /// </summary>
        /// <remarks>If this property is <see langword="null"/>, the item is auto-detected by separators.</remarks>
        [Description("Gets or sets the first start item in the mutually exclusive radio button group. " +
            "If this property is null, the item is auto-detected by separators.")]
        public ToolStripRadioMenuItem? StartItem { get; set; }
        /// <summary>
        /// Gets or sets the last end item in the mutually exclusive radio button group.
        /// </summary>
        /// <remarks>If this property is <see langword="null"/>, the item is auto-detected by separators.</remarks>
        [Description("Gets or sets the last end item in the mutually exclusive radio button group. " +
            "If this property is null, the item is auto-detected by separators.")]
        public ToolStripRadioMenuItem? EndItem { get; set; }

        public ToolStripRadioMenuItem()
        {

        }
    }
}
