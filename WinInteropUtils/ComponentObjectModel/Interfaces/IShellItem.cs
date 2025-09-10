using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;

namespace FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces
{
    /// <summary>
    /// Exposes methods that retrieve information about a Shell item. <see cref="IShellItem"/> and
    /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/shobjidl_core/nn-shobjidl_core-ishellitem2">IShellItem2</see>
    /// are the preferred representations of items in any new code.
    /// </summary>
    /// <remarks>
    /// Third parties do not implement this interface; only use the implementation provided with the system.
    /// </remarks>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    [SupportedOSPlatform("windows6.0")]
    public partial interface IShellItem : IUnknown
    {
        // HRESULT BindToHandler(IBindCtx *pbc, REFGUID bhid, REFIID riid, void **ppv);
        /// <summary>
        /// Binds to a handler for an item as specified by the handler ID value (BHID).
        /// </summary>
        /// <param name="pbc">A pointer to an IBindCtx interface on a bind context object.
        /// Used to pass optional parameters to the handler. The contents of the bind context are handler-specific.
        /// For example, when binding to BHID_Stream, the STGM flags in the bind context indicate the mode of access desired (read or read/write).</param>
        /// <param name="bhid">Reference to a GUID that specifies which handler will be created. The GUIDs are defined in <c>shlguid.h</c>.</param>
        /// <param name="riid">IID of the object type to retrieve.</param>
        /// <param name="ppv">When this method returns, contains a pointer of type <paramref name="riid"/> that is returned by the
        /// handler specified by <paramref name="bhid"/>.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an
        /// <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out nint ppv);

        // HRESULT GetParent(IShellItem **ppsi);
        /// <summary>
        /// Gets the parent of an <see cref="IShellItem"/> object.
        /// </summary>
        /// <param name="ppsi">The address of a pointer to the parent of an <see cref="IShellItem"/> interface.</param>
        /// <returns>Returns <see cref="HRESULT.S_OK"/> if successful, or an error value otherwise.</returns>
        [PreserveSig]
        public HRESULT GetParent(out nint ppsi);

        // HRESULT GetDisplayName(SIGDN sigdnName, LPWSTR *ppszName);
        /// <summary>
        /// Gets the display name of the <see cref="IShellItem"/> object.
        /// </summary>
        /// <param name="sigdnName">One of the <see cref="SIGDN"/> values that indicates how the name should look.</param>
        /// <param name="ppszName">A value that, when this function returns successfully, receives the address of a pointer to the retrieved display name.</param>
        /// <returns><see cref="HRESULT.S_OK"/> if successful; otherwise, an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT GetDisplayName(SIGDN sigdnName, out nint ppszName);

        // HRESULT GetAttributes(SFGAOF sfgaoMask, SFGAOF *psfgaoAttribs);
        /// <summary>
        /// Gets a requested set of attributes of the <see cref="IShellItem"/> object.
        /// </summary>
        /// <param name="sfgaoMask">Specifies the attributes to retrieve. One or more of the SFGAO values. Use a bitwise OR operator
        /// to determine the attributes to retrieve.</param>
        /// <param name="psfgaoAttribs">A pointer to a value that, when this method returns successfully, contains the requested attributes.
        /// One or more of the SFGAO values. Only those attributes specified by <paramref name="sfgaoMask"/> are returned; other attribute values are undefined.</param>
        /// <returns><see cref="HRESULT.S_OK"/> if the attributes returned exactly match those requested
        /// in <paramref name="sfgaoMask"/>, <see cref="HRESULT.S_FALSE"/> if the attributes do not exactly match, or a
        /// standard <see cref="COM"/> error value otherwise.</returns>
        [PreserveSig]
        public HRESULT GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

        // HRESULT Compare(IShellItem *psi, SICHINTF hint, int *piOrder);
        /// <summary>
        /// Compares two <see cref="IShellItem"/> objects.
        /// </summary>
        /// <param name="psi">A pointer to an <see cref="IShellItem"/> object to compare with the existing <see cref="IShellItem"/> object.</param>
        /// <param name="hint">One of the <see cref="SICHINTF"/> values that determines how to perform the comparison.
        /// See <see cref="SICHINTF"/> for the list of possible values for this parameter.</param>
        /// <param name="piOrder">This parameter receives the result of the comparison. If the two items are the same this parameter equals zero;
        /// if they are different the parameter is nonzero.</param>
        /// <returns><see cref="HRESULT.S_OK"/> if the items are the same, <see cref="HRESULT.S_FALSE"/> if they are different, or an error value otherwise.</returns>
        [PreserveSig]
        public HRESULT Compare(IntPtr psi, SICHINTF hint, out int piOrder);
    }

    // SIGDN enum for display name type
    /// <summary>
    /// Requests the form of an item's display name to retrieve through <see cref="IShellItem.GetDisplayName(SIGDN, out nint)"/>.
    /// </summary>
    [Flags]
    [SupportedOSPlatform("windows5.1")] // sp1 required
    public enum SIGDN : uint
    {
        /// <summary>
        /// Returns the display name relative to the parent folder. In UI this name is generally ideal for display to the user.
        /// </summary>
        SIGDN_NORMALDISPLAY = 0,
        /// <summary>
        ///  Returns the parsing name relative to the parent folder. This name is not suitable for use in UI.
        /// </summary>
        SIGDN_PARENTRELATIVEPARSING = 0x80018001,
        /// <summary>
        /// Returns the parsing name relative to the desktop. This name is not suitable for use in UI.
        /// </summary>
        SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
        /// <summary>
        /// Returns the editing name relative to the parent folder. In UI this name is suitable for display to the user.
        /// </summary>
        SIGDN_PARENTRELATIVEEDITING = 0x80031001,
        /// <summary>
        /// Returns the editing name relative to the desktop. In UI this name is suitable for display to the user.
        /// </summary>
        SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
        /// <summary>
        /// Returns the item's file system path, if it has one. Only items that report SFGAO_FILESYSTEM have a file system path.
        /// When an item does not have a file system path, a call to <see cref="IShellItem.GetDisplayName(SIGDN, out nint)"/> on that item will fail.
        /// In UI this name is suitable for display to the user in some cases, but note that it might not be specified for all items.
        /// </summary>
        SIGDN_FILESYSPATH = 0x80058000,
        /// <summary>
        /// Returns the item's URL, if it has one. Some items do not have a URL, and in those cases a
        /// call to <see cref="IShellItem.GetDisplayName(SIGDN, out nint)"/> will fail. This name is suitable for display to the user in some cases, but note
        /// that it might not be specified for all items.
        /// </summary>
        SIGDN_URL = 0x80068000,
        /// <summary>
        /// Returns the path relative to the parent folder in a friendly format as displayed in an address bar. This name is suitable for display to the user.
        /// </summary>
        SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
        /// <summary>
        /// Returns the path relative to the parent folder.
        /// </summary>
        SIGDN_PARENTRELATIVE = 0x80080001,
        /// <summary>
        /// Returns the path relative to the parent folder for UI. Introduced in Windows 8.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        SIGDN_PARENTRELATIVEFORUI = 0x80094001
    }

    // SICHINTF enum for Compare hints
    /// <summary>
    /// Used to determine how to compare two Shell items. <see cref="IShellItem.Compare(nint, SICHINTF, out int)"/> uses this enumerated type.
    /// </summary>
    [Flags]
    [SupportedOSPlatform("windows5.1")]
    [SupportedOSPlatform("windows6.1")]
    public enum SICHINTF : uint
    {
        /// <summary>
        /// This relates to the iOrder parameter of the <see cref="IShellItem.Compare(nint, SICHINTF, out int)"/> method
        /// and indicates that the comparison is based on the display in a folder view.
        /// </summary>
        SICHINT_DISPLAY = 0x00000000,
        /// <summary>
        /// Exact comparison of two instances of a Shell item.
        /// </summary>
        SICHINT_ALLFIELDS = 0x80000000,
        /// <summary>
        /// This relates to the iOrder parameter of the <see cref="IShellItem.Compare(nint, SICHINTF, out int)"/> interface and indicates
        /// that the comparison is based on a canonical name.
        /// </summary>
        SICHINT_CANONICAL = 0x10000000,
        /// <summary>
        /// Windows 7 and later. If the Shell items are not the same, test the file system paths.
        /// </summary>
        [SupportedOSPlatform("windows6.1")] // win7
        SICHINT_TEST_FILESYSPATH_IF_NOT_EQUAL = 0x20000000
    }
}
