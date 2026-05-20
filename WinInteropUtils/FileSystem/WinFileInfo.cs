using FireBlade.WinInteropUtils.Memory;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static FireBlade.WinInteropUtils.Macros;

namespace FireBlade.WinInteropUtils.FileSystem
{
    /// <summary>
    /// Represents information about a <see cref="WinFile"/>.
    /// </summary>
    public sealed partial class WinFileInfo : IDisposable
    {
        [LibraryImport("shell32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static unsafe partial nuint SHGetFileInfoW(nint pszPath, uint dwFileAttributes, SHFILEINFO* psfi,
            uint cbFileInfo, uint uFlags);

        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint SHGFI_ATTRIBUTES = 0x000000800;
        private const uint SHGFI_DISPLAYNAME = 0x000000200;
        private const uint SHGFI_EXETYPE = 0x000002000;
        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_ICONLOCATION = 0x000001000;
        private const uint SHGFI_LARGEICON = 0x000000000;
        private const uint SHGFI_PIDL = 0x000000008;
        private const uint SHGFI_SHELLICONSIZE = 0x000000004;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_SYSICONINDEX = 0x000004000;
        private const uint SHGFI_TYPENAME = 0x000000400;

        private const int MZ = 0x5A4D;
        private const int NE = 0x454E;
        private const int PE = 0x4550;

        private WinFileIcon LoadIcon(nint hico, int sysIdx)
        {
            Icon icon = (Icon)Icon.FromHandle(hico).Clone();
            Shell32.DestroyIcon(hico); // why is this in shell32 its a user32 function why are these my life choices

            return new WinFileIcon(icon, sysIdx);
        }

        /// <summary>
        /// Retrieves the info of the file specified by <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path of the file to retrieve info about.</param>
        /// <param name="options">Specifies additional options for the file info to retrieve.</param>
        /// <param name="attribs">File attributes. See <b>Remarks</b> for more info.</param>
        /// <remarks>
        /// <para>If the <paramref name="attribs"/> parameter is set, it indicates that the function should not
        /// attempt to access the file specified by <paramref name="path"/>. Rather, it should act as if the
        /// file specified by <paramref name="path"/> exists with the file attributes passed in
        /// <paramref name="attribs"/>.</para>
        /// 
        /// You should call this function from a background thread. Failure to do so could cause the UI to stop responding.
        /// </remarks>
        public WinFileInfo(string path, WinFileInfoOptions options = WinFileInfoOptions.None, WinFileAttributes? attribs = null)
        {
            using (MemoryString str = new MemoryString(path))
            {
                unsafe
                {
                    SHFILEINFO sfi = new();

                    uint uShared = SHGFI_ICON | SHGFI_SYSICONINDEX;
                    uShared |= (uint)options;

                    if (attribs != null)
                        uShared |= SHGFI_USEFILEATTRIBUTES;

                    #region CALL 1 - Small Icon and Info

                    // we could write this inline but we seperate it to make it nicer
                    uint uFlags = uShared;
                    uFlags |= SHGFI_ATTRIBUTES;
                    uFlags |= SHGFI_DISPLAYNAME;
                    uFlags |= SHGFI_SMALLICON;
                    uFlags |= SHGFI_TYPENAME;

                    SHGetFileInfoW(str, (uint)attribs.GetValueOrDefault(), &sfi, (uint)sizeof(SHFILEINFO),
                        uFlags);

                    _attribs    = (WinFileAttributes)sfi.dwAttributes;
                    _smico      = LoadIcon(sfi.hIcon, sfi.iIcon);
                    _disp       = LoadFixedBuffer(sfi.szDisplayName, WinConstants.MAX_PATH);
                    _typeName   = LoadFixedBuffer(sfi.szTypeName, 80);

                    #endregion
                    #region CALL 2 - Large Icon and Icon Location

                    uFlags = uShared;
                    uFlags |= SHGFI_LARGEICON;
                    uFlags |= SHGFI_ICONLOCATION;

                    SHGetFileInfoW(str, (uint)attribs.GetValueOrDefault(), &sfi, (uint)sizeof(SHFILEINFO), uFlags);

                    _lgico      = LoadIcon(sfi.hIcon, sfi.iIcon);
                    _iconLoc    = LoadFixedBuffer(sfi.szDisplayName, WinConstants.MAX_PATH);

                    #endregion
                    #region CALL 3 - Shell Icon

                    uFlags  = uShared;
                    uFlags |= SHGFI_SHELLICONSIZE;

                    SHGetFileInfoW(str, (uint)attribs.GetValueOrDefault(), &sfi, (uint)sizeof(SHFILEINFO), uFlags);

                    _shico = LoadIcon(sfi.hIcon, sfi.iIcon);
                    #endregion
                    #region CALL 4 - EXE Type
                    
                    if (attribs == null)
                    {
                        uFlags = SHGFI_EXETYPE;

                        nuint exe = SHGetFileInfoW(str, (uint)attribs.GetValueOrDefault(), &sfi, (uint)sizeof(SHFILEINFO),
                            uFlags);

                        ushort xl = LowWord(exe);
                        ushort xh = HighWord(exe);

                        if (xl == NE && xh != 0)
                            _exeType = ExecutableType.NE;
                        else if (xl == PE && xh != 0)
                            _exeType = ExecutableType.PE;
                        else if (xl == MZ && xh == 0)
                            _exeType = ExecutableType.MSDos;
                        else if (xl == PE && xh == 0)
                            _exeType = ExecutableType.Console;
                    }

                    #endregion
                }
            }
        }

        private ExecutableType _exeType = ExecutableType.None;
        private WinFileAttributes _attribs;

        /// <summary>
        /// Gets the type of the executable file.
        /// </summary>
        /// <returns>A value from the <see cref="FileSystem.ExecutableType"/> enumeration if the file
        /// is an executable file; otherwise, <see cref="ExecutableType.None"/>.</returns>
        public ExecutableType ExecutableType => _exeType;

        /// <summary>
        /// Gets the attributes of the file.
        /// </summary>
        /// <example>
        /// To check if the file is a directory:
        /// <code>
        /// if (file.Attributes.HasFlag(WinFileAttributes.Directory))
        /// {
        ///     // TODO: Do stuff
        /// }
        /// 
        /// file.Dispose();
        /// </code>
        /// </example>
        public WinFileAttributes Attributes => _attribs;

        private WinFileIcon _smico = null!;

        /// <summary>
        /// Gets the small icon of the file.
        /// </summary>
        public WinFileIcon SmallIcon => _smico;

        private WinFileIcon _lgico = null!;

        /// <summary>
        /// Gets the large icon of the file.
        /// </summary>
        public WinFileIcon LargeIcon => _lgico;

        private WinFileIcon _shico = null!;

        /// <summary>
        /// Gets the Shell-sized icon of the file.
        /// </summary>
        public WinFileIcon ShellIcon => _shico;

        private string _disp;

        /// <summary>
        /// Gets the name of the file as it appears in File Explorer.
        /// </summary>
        /// <remarks>Note that the display name can be affected by settings such as whether extensions are shown.</remarks>
        public string DisplayName => _disp;

        private string _typeName;

        /// <summary>
        /// Gets the string that describes the file's type.
        /// </summary>
        /// <remarks>For example, if the file is a .TXT file, the type name may be "Text file".</remarks>
        public string TypeName => _typeName;

        private string _iconLoc;

        /// <summary>
        /// Gets the location of the icon.
        /// </summary>
        public string IconLocation => _iconLoc;

        private unsafe static string LoadFixedBuffer(char* buffer, int maxLength)
        {
            int len = 0;
            while (len < maxLength && buffer[len] != '\0')
                len++;

            return new string(buffer, 0, len);
        }

        private bool _disposed;

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="WinFileInfo"/> was disposed.
        /// </summary>
        public bool IsDisposed => _disposed;

        /// <summary>
        /// Releases all resources associated with this <see cref="WinFileInfo"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            SmallIcon.Dispose();
            LargeIcon.Dispose();
            ShellIcon.Dispose();
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal unsafe struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;

            public fixed char szDisplayName[260];
            public fixed char szTypeName[80];
        }
    }

    /// <summary>
    /// Represents the options for <see cref="WinFileInfo(string, WinFileInfoOptions, WinFileAttributes?)"/>.
    /// </summary>
    [Flags]
    public enum WinFileInfoOptions
    {
        /// <summary>
        /// Fetch the file info normally.
        /// </summary>
        None = 0,
        /// <summary>
        /// Apply the appropriate overlays to the file's icon.
        /// </summary>
        IconOverlays = 0x000000020,
        /// <summary>
        /// Add the link overlay to the file's icon.
        /// </summary>
        LinkOverlay = 0x000008000,
        /// <summary>
        /// Retrieve the file's open icon.
        /// </summary>
        /// <remarks>A container object displays an open icon to indicate that the container is open.</remarks>
        OpenIcon = 0x000000002,
        /// <summary>
        /// Blend the file's icon with the system highlight color.
        /// </summary>
        Selected = 0x000010000
    }

    /// <summary>
    /// Specifies attributes for Windows files. 
    /// </summary>
    [Flags]
    public enum WinFileAttributes
    {
        /// <summary>
        /// A file that does not have other attributes set. This attribute is valid only when used alone.
        /// </summary>
        None = 0x00000080,
        /// <summary>
        /// A file that is read-only. Applications can read the file,
        /// but cannot write to it or delete it. This attribute is not honored on directories.
        /// You cannot view or change the Read-only or the System attributes
        /// of folders in Windows Server 2003, in Windows XP, in Windows Vista or in Windows 7.
        /// </summary>
        ReadOnly = 0x00000001,
        /// <summary>
        /// The file or directory is hidden. It is not included in an ordinary directory listing.
        /// </summary>
        Hidden = 0x00000002,
        /// <summary>
        /// A file or directory that the operating system uses as part of, or uses exclusively.
        /// </summary>
        System = 0x00000004,
        /// <summary>
        /// The information queried identifies a directory.
        /// </summary>
        Directory = 0x00000010,
        /// <summary>
        /// A file or directory that is an archive file or directory. Applications typically use this attribute
        /// to mark files for backup or removal.
        /// </summary>
        Archive = 0x00000020,
        /// <summary>
        /// This value is reserved for system use. Do not use.
        /// </summary>
        [Obsolete("This value is reserved for system use. Do not use.", DiagnosticId = ErrorDiagIDs.ReservedEnumValue)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Device = 0x00000040,
        /// <summary>
        /// A file that is being used for temporary storage. File systems avoid writing data back to mass storage
        /// if sufficient cache memory is available, because typically, an application deletes a temporary file after
        /// the handle is closed. In that scenario, the system can entirely avoid writing the data. Otherwise, the
        /// data is written after the handle is closed.
        /// </summary>
        Temporary = 0x00000100,
        /// <summary>
        /// A file that is a sparse file.
        /// </summary>
        Sparse = 0x00000200,
        /// <summary>
        /// A file or directory that has an associated reparse point, or a file that is a symbolic link.
        /// </summary>
        ReparsePoint = 0x00000400,
        /// <summary>
        /// A file or directory that is compressed. For a file, all of the data in the file is compressed. For a directory,
        /// compression is the default for newly created files and subdirectories.
        /// </summary>
        Compressed = 0x00000800,
        /// <summary>
        /// The data of a file is not available immediately. This attribute indicates that the file data is physically moved to offline
        /// storage. This attribute is used by Remote Storage, which is the hierarchical storage management software. Applications
        /// should not arbitrarily change this attribute.
        /// </summary>
        Offline = 0x00001000,
        /// <summary>
        /// The file or directory is not to be indexed by the content indexing service.
        /// </summary>
        NoContentIndex = 0x00002000,
        /// <summary>
        /// A file or directory that is encrypted. For a file, all data streams in the file are encrypted. For a directory,
        /// encryption is the default for newly created files and subdirectories.
        /// </summary>
        Encrypted = 0x00004000,
        /// <summary>
        /// The directory or user data stream is configured with integrity (only supported on ReFS volumes). It is not
        /// included in an ordinary directory listing. The integrity setting persists with the file if it's renamed.
        /// If a file is copied the destination file will have integrity set if either the source file
        /// or destination directory have integrity set. <b>Windows Server 2008 R2, Windows 7, Windows Server 2008,
        /// Windows Vista, Windows Server 2003 and Windows XP</b>: This flag is not supported
        /// until <i>Windows Server 2012</i>.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        IntegrityStream = 0x00008000,
        /// <summary>
        /// This value is reserved for system use. Do not use.
        /// </summary>
        [Obsolete("This value is reserved for system use. Do not use.", DiagnosticId = ErrorDiagIDs.ReservedEnumValue)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Virtual = 0x00010000,
        /// <summary>
        /// The user data stream not to be read by the background data integrity scanner (AKA scrubber). When set on
        /// a directory it only provides inheritance. This flag is only supported on Storage Spaces and ReFS volumes.
        /// It is not included in an ordinary directory listing. Windows Server 2008 R2, Windows 7, Windows Server 2008,
        /// Windows Vista, Windows Server 2003 and Windows XP: This flag is not supported until Windows 8 and Windows Server 2012.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        NoScrubData = 0x00020000,
        /// <summary>
        /// A file or directory with extended attributes. This value is for internal use only. Do not use.
        /// </summary>
        [Obsolete("This value is reserved for system use. Do not use.", DiagnosticId = ErrorDiagIDs.ReservedEnumValue)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ExAttrib = 0x00040000,
        /// <summary>
        /// This attribute indicates user intent that the file or directory should be kept fully present locally even
        /// when not being actively accessed. This attribute is for use with hierarchical storage management software.
        /// </summary>
        Pinned = 0x00080000,
        /// <summary>
        /// This attribute indicates that the file or directory should not be kept fully present locally except when
        /// being actively accessed. This attribute is for use with hierarchical storage management software.
        /// </summary>
        Unpinned = 0x00100000,
        /// <summary>
        /// This attribute only appears in directory enumeration classes. When this attribute is set, it means that the
        /// file or directory has no physical representation on the local system; the item is virtual. Opening the item
        /// will be more expensive than normal, e.g. it will cause at least some of it to be fetched from a remote store.
        /// </summary>
        RecallOnOpen = 0x00040000,
        /// <summary>
        /// When this attribute is set, it means that the file or directory is not fully present locally. For a file that
        /// means that not all of its data is on local storage (e.g. it may be sparse with some data still in remote storage).
        /// For a directory it means that some of the directory contents are being virtualized from another location. Reading
        /// the file/enumerating the directory will be more expensive than normal, e.g. it will cause at least some of the
        /// file/directory content to be fetched from a remote store. Only kernel-mode callers can set this bit. File system
        /// mini filters below the 180000–189999 altitude range (FSFilter HSM Load Order Group) must not issue targeted cached
        /// reads or writes to files that have this attribute set. This could lead to cache pollution and potential file corruption.
        /// For more information, see
        /// <see href="https://learn.microsoft.com/en-us/windows-hardware/drivers/ifs/placeholders_guidance">
        /// Handling placeholders</see>.
        /// </summary>
        RecallOnDataAccess = 0x00400000,
        /// <summary>
        /// A system file. A system file has the <see cref="Hidden"/> and <see cref="System"/> attributes.
        /// This is a composite value.
        /// </summary>
        SystemFile = Hidden | System,
        /// <summary>
        /// An archive directory. Applications typically use this attribute
        /// to mark directories for backup or removal. This is a composite value.
        /// </summary>
        ArchiveDir = Archive | Directory
    }

    /// <summary>
    /// Represents the type of an EXE file.
    /// </summary>
    public enum ExecutableType
    {
        /// <summary>
        /// Invalid type.
        /// </summary>
        /// <remarks>
        /// This value is used if the file is not an executable file or another error occurs.
        /// </remarks>
        None = 0,
        /// <summary>
        /// 32- or 64-bit Windows Portable Executable file.
        /// </summary>
        PE,
        /// <summary>
        /// 16-bit New Executable file.
        /// </summary>
        NE,
        /// <summary>
        /// MS-DOS application (.COM).
        /// </summary>
        MSDos,
        /// <summary>
        /// Console application.
        /// </summary>
        Console
    }

    /// <summary>
    /// Represents the icon of a <see cref="WinFile"/>.
    /// </summary>
    public sealed class WinFileIcon : IDisposable
    {
        private int _sys;

        /// <summary>
        /// Gets the index of the icon in the system image list.
        /// </summary>
        public int SysIconIndex => _sys;

        private Icon _ico;
        
        /// <summary>
        /// Gets the icon.
        /// </summary>
        public Icon Icon => _ico;

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFileIcon"/> class with the specified icon.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <param name="sysIndex">The index of the icon within the system image list.</param>
        public WinFileIcon(Icon icon, int sysIndex)
        {
            _sys = sysIndex;
            _ico = icon;
        }

        /// <summary>
        /// Releases the resources associated with the icon.
        /// </summary>
        // system.drawing.icon guards against double dispose
        public void Dispose() => _ico.Dispose();
    }
}
