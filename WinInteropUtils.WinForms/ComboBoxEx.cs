using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Runtime.InteropServices;

// WARNING: WinInteropUtils.WinForms is still unfinished, so until it actually gets finished it won't be included in releases
// Right now the ComboBoxEx crashes the designer, and has ever since I added the Items collection, the DropDownStyle, and the image list

namespace FireBlade.WinInteropUtils.WinForms
{
    /// <summary>
    /// A variant of a <see cref="ComboBox"/> with native support for item images.
    /// </summary>
    [Description("A variant of a ComboBox with native support for item images.")]
    internal partial class ComboBoxEx : Control
    {
        internal const int WM_USER = 0x0400;
        private const int CBEM_SETIMAGELIST = (WM_USER + 2);
        private const int CBS_SIMPLE = 0x0001;
        private const int CBS_DROPDOWN = 0x0002;
        private const int CBS_DROPDOWNLIST = 0x0003;
        private const int CBS_TYPEMASK = 0x0003; // mask for all three

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassName = "ComboBoxEx32";

                // Clear out the existing style bits in the low 2 bits
                cp.Style &= ~CBS_TYPEMASK;

                cp.Style |= DropDownStyle switch
                {
                    ComboBoxStyle.Simple => CBS_SIMPLE,
                    ComboBoxStyle.DropDown => CBS_DROPDOWN,
                    ComboBoxStyle.DropDownList or _ => CBS_DROPDOWNLIST
                };

                return cp;
            }
        }

        private ImageList? _itemImgList;

        /// <summary>
        /// Specifies the image list that supplies images for ComboBoxEx items.
        /// </summary>
        [Description("Specifies the image list that supplies images for ComboBoxEx items.")]
        public ImageList? ItemImageList
        {
            get => _itemImgList;
            set
            {
                _itemImgList = value;
                User32.SendMessage(Handle, CBEM_SETIMAGELIST, 0, value?.Handle ?? 0);
            }
        }

        private ComboBoxStyle _dropDownStyle = ComboBoxStyle.DropDownList;

        public ComboBoxStyle DropDownStyle
        {
            get => _dropDownStyle;
            set
            {
                if (DropDownStyle == value) return;

                _dropDownStyle = value;

                if (IsHandleCreated)
                {
                    RecreateHandle();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        //[MergableProperty(false)]
        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        public ComboBoxExItemCollection Items { get; }

        public ComboBoxEx()
        {
            InitializeComponent();

            Items = new(this);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct COMBOBOXEXITEMW
    {
        public int cbSize;
        public int mask;
        public int iItem;
        public IntPtr pszText;      // LPWSTR
        public int cchTextMax;
        public int iImage;
        public int iSelectedImage;
        public int iOverlay;
        public int iIndent;
        public IntPtr lParam;
    }

    /// <summary>
    /// Represents a collection of items in a <see cref="ComboBoxEx"/> control.
    /// </summary>
    [ListBindable(false)]
    public class ComboBoxExItemCollection : ICollection<ComboBoxExItem>, IList<ComboBoxExItem>
    {
        internal List<ComboBoxExItem> _items = [];
        internal ComboBoxEx _cb;
        private const int CBEM_INSERTITEM = ComboBoxEx.WM_USER + 11;
        private const int CBEM_DELETEITEM = 0x0144;

        internal List<GCHandle> gcHandles = [];
        internal List<nint> handles = [];

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public ComboBoxExItem this[int index] { get => _items[index]; set => _items[index] = value; }

        public void Add(ComboBoxExItem item)
        {
            _items.Add(item);

            var cbexi = new COMBOBOXEXITEMW();
            cbexi.cbSize = Marshal.SizeOf<COMBOBOXEXITEMW>();
            cbexi.pszText = Marshal.StringToHGlobalUni(item.Text);
            cbexi.cchTextMax = item.Text.Length + 1; // null terminator
            cbexi.iImage = item.ImageIndex;
            cbexi.iIndent = item.Indentation;
            cbexi.iItem = item.Index;
            cbexi.iOverlay = item.OverlayImageIndex;
            cbexi.iSelectedImage = item.ImageIndexSelected;

            var handle = GCHandle.Alloc(item.Tag);

            cbexi.lParam = GCHandle.ToIntPtr(handle);

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<COMBOBOXEXITEMW>());
            Marshal.StructureToPtr(cbexi, ptr, false);

            User32.SendMessage(_cb.Handle, CBEM_INSERTITEM, 0, ptr);

            // Store handles for freeing later with FreeHGlobal
            handles.Add(ptr);
            handles.Add(cbexi.pszText);
            gcHandles.Add(handle);
        }

        public void Clear()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                User32.SendMessage(_cb.Handle, CBEM_DELETEITEM, (nuint)i, 0);
            }

            _items.Clear();

            foreach (var h in handles)
                Marshal.FreeHGlobal(h);

            handles.Clear();

            foreach (var gch in gcHandles)
                gch.Free();
            gcHandles.Clear();
        }

        public bool Contains(ComboBoxExItem item) => _items.Contains(item);

        public void CopyTo(ComboBoxExItem[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public IEnumerator<ComboBoxExItem> GetEnumerator() => _items.GetEnumerator();

        public bool Remove(ComboBoxExItem item)
        {
            var i = _items.IndexOf(item);

            _items.Remove(item);
            
            var h = handles.ElementAt(i);
            var gch = gcHandles.ElementAt(i);
            gch.Free();
            Marshal.FreeHGlobal(h);

            User32.SendMessage(_cb.Handle, CBEM_DELETEITEM, (nuint)i, 0);

            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(ComboBoxExItem item) => _items.IndexOf(item);

        public void Insert(int index, ComboBoxExItem item) => _items.Insert(index, item);

        public void RemoveAt(int index) => _items.RemoveAt(index);

        internal ComboBoxExItemCollection(ComboBoxEx combo)
        {
            _cb = combo;
        }
    }
    public class ComboBoxExItem
    {
        /// <summary>
        /// The index of the item.
        /// </summary>
        public int Index { get; internal set; }
        /// <summary>
        /// The text that is shown on the item.
        /// </summary>
        public string Text { get; set; } = string.Empty;
        /// <summary>
        /// The index of the image from the image list to use.
        /// </summary>
        public int ImageIndex { get; set; } = -1;
        /// <summary>
        /// The index of the image from the image list to use when selected.
        /// </summary>
        public int ImageIndexSelected { get; set; } = -1;
        /// <summary>
        /// The index of the overlay image from the image list.
        /// </summary>
        public int OverlayImageIndex { get; set; } = -1;
        /// <summary>
        /// The item indentation. Each indent is about 10 px.
        /// </summary>
        public int Indentation { get; set; } = 0;
        /// <summary>
        /// Additional data about the item.
        /// </summary>
        public object? Tag { get; set; }
        /// <summary>
        /// <see langword="true"/> if the item was disposed and cannot be used in another <see cref="ComboBoxExItemCollection"/>. This only gets set
        /// when the item gets removed from the <see cref="ComboBoxExItemCollection"/>, because <see cref="ComboBoxExItem"/> doesn't
        /// implement <see cref="IDisposable"/>, because the unmanaged item objects only get created when the items are added to the collection.
        /// </summary>
        public bool Disposed { get; internal set; } = false;

        public ComboBoxExItem()
        {

        }
    }
}