using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Versioning;
using System.Text;

namespace FireBlade.WinInteropUtils
{
    /// <summary>
    /// Declares P/Invoke functions from <c>Shell32.dll</c>.
    /// </summary>
    public static partial class Shell32
    {
        [DllImport("shell32.dll", EntryPoint = "PickIconDlg", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PickIconDlg(nint hWnd, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszIconPath, uint cchIconPath, ref int piIconIndex);

        // The Windows API's PickIconDlg allows passing null for the hWnd

        /// <summary>
        /// Declares the <c>MAX_PATH</c> environment variable from the Windows API.
        /// </summary>
        public const int MAX_PATH = 260;

        /// <summary>
        /// Displays a dialog box that allows the user to choose an icon from the selection available embedded in a resource such as an executable or DLL file.
        /// </summary>
        /// <param name="hWnd">The handle of the parent window.</param>
        /// <param name="defIconPath">A string that contains the fully qualified path of the default resource that contains the icons.
        /// You should verify that the path is valid before using it.</param>
        /// <param name="defIconIndex">An integer that specifies the index of the initial selection.</param>
        /// <param name="iconPath">If the user chooses a different resource in the dialog, contains the path of that file when the function returns; otherwise, it contains
        /// the path stored in <paramref name="defIconPath"/>.</param>
        /// <param name="iconIndex">When this function returns successfully, receives the index of the icon that was selected.</param>
        /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
        [SupportedOSPlatform("windows5.1")] // Windows XP
        public static bool ShowPickIconDialog(nint hWnd, string defIconPath, int defIconIndex, out string iconPath, out int iconIndex)
        {
            var sb = new StringBuilder(defIconPath, MAX_PATH);
            iconIndex = defIconIndex;

            var result = PickIconDlg(hWnd, sb, (uint)sb.Capacity, ref iconIndex);
            iconPath = sb.ToString();
            return result;
        }

        /// <summary>
        /// Displays a dialog box that allows the user to choose an icon from the selection available embedded in a resource such as an executable or DLL file.
        /// </summary>
        /// <param name="defIconPath">A string that contains the fully qualified path of the default resource that contains the icons.
        /// You should verify that the path is valid before using it.</param>
        /// <param name="defIconIndex">An integer that specifies the index of the initial selection.</param>
        /// <param name="iconPath">If the user chooses a different resource in the dialog, contains the path of that file when the function returns; otherwise, it contains
        /// the path stored in <paramref name="defIconPath"/>.</param>
        /// <param name="iconIndex">When this function returns successfully, receives the index of the icon that was selected.</param>
        /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
        [SupportedOSPlatform("windows5.1")] // Windows XP
        public static bool ShowPickIconDialog(string defIconPath, int defIconIndex, out string iconPath, out int iconIndex)
        {
            var sb = new StringBuilder(defIconPath, MAX_PATH);
            iconIndex = defIconIndex;

            var result = PickIconDlg(nint.Zero, sb, (uint)sb.Capacity, ref iconIndex);
            iconPath = sb.ToString();
            return result;
        }


        /// <summary>
        /// Displays a dialog box that allows the user to choose an icon from the selection available embedded in a resource such as an executable or DLL file.
        /// </summary>
        /// <param name="hWnd">The handle of the parent window.</param>
        /// <param name="defIconPath">A string that contains the fully qualified path of the default resource that contains the icons.
        /// You should verify that the path is valid before using it.</param>
        /// <param name="defIconIndex">An integer that specifies the index of the initial selection.</param>
        /// <param name="icon">When this function returns, contains the <see cref="Icon"/> that was selected. If loading the icon fails, contains <see cref="SystemIcons.Error"/>.</param>
        /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
        [SupportedOSPlatform("windows5.1")] // Windows XP
        public static bool ShowPickIconDialog(nint hWnd, string defIconPath, int defIconIndex, out Icon icon)
        {
            var sb = new StringBuilder(defIconPath, MAX_PATH);

            int iconIndex = defIconIndex;

            var result = PickIconDlg(hWnd, sb, (uint)sb.Capacity, ref iconIndex);
            icon = Icon.ExtractIcon(sb.ToString(), iconIndex) ?? SystemIcons.Error; // ExtractIconW

            return result;
        }

        /// <summary>
        /// Displays a dialog box that allows the user to choose an icon from the selection available embedded in a resource such as an executable or DLL file.
        /// </summary>
        /// <param name="defIconPath">A string that contains the fully qualified path of the default resource that contains the icons.
        /// You should verify that the path is valid before using it.</param>
        /// <param name="defIconIndex">An integer that specifies the index of the initial selection.</param>
        /// <param name="icon">When this function returns, contains the <see cref="Icon"/> that was selected. If loading the icon fails, contains <see cref="SystemIcons.Error"/>.</param>
        /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
        [SupportedOSPlatform("windows5.1")] // Windows XP
        public static bool ShowPickIconDialog(string defIconPath, int defIconIndex, out Icon icon)
        {
            var sb = new StringBuilder(defIconPath, MAX_PATH);

            int iconIndex = defIconIndex;

            var result = PickIconDlg(nint.Zero, sb, (uint)sb.Capacity, ref iconIndex);
            icon = Icon.ExtractIcon(sb.ToString(), iconIndex) ?? SystemIcons.Error;

            return result;
        }

        [DllImport("shell32.dll", EntryPoint = "ShellAbout", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShellAboutW(nint hWnd, StringBuilder szApp, string? szOtherStuff, nint hIcon);

        /// <summary>
        /// Displays a <c>ShellAbout</c> dialog box. For an example of this dialog box, run the <c>winver</c> command.
        /// </summary>
        /// <example>
        /// For an example of this dialog box, run the <c>winver</c> command.
        /// </example>
        /// <param name="hWnd">A window handle to a parent window.</param>
        /// <param name="app"><para>A string that contains text to be displayed in the title bar of the <c>ShellAbout</c> dialog box and on the first line of the dialog box after the text
        /// "Microsoft". If the text contains a separator <c>(#)</c> that divides it into two parts, the function displays the first part in the title bar and the second part on
        /// the first line after the text "Microsoft".</para>
        ///
        /// <b><i>Windows Vista, Windows Server 2008</i></b>: This string cannot exceed 200 characters in length. The contents of <paramref name="app"/> will no longer
        /// show after "Microsoft", unless there is a <c>#</c> separator, in which case the part after the <c>#</c> will completely replace the first line.</param>
        /// <param name="moreText">A string that contains text to be displayed in the dialog box after the version and copyright information.
        /// This parameter can be <see langword="null"/>.</param>
        /// <param name="icon">An icon that is displayed in the dialog box. This parameter can be <see langword="null"/>,
        /// in which case the dialog displays the Windows icon.</param>
        /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
        [SupportedOSPlatform("windows5.1")] // Windows XP
        [SupportedOSPlatform("windows5.0")] // Windows 2000 (server)
        public static bool ShellAbout(nint hWnd, string app, string? moreText, Icon? icon = null)
        {
            var sb = new StringBuilder(app);

            return ShellAboutW(hWnd, sb, moreText, icon?.Handle ?? nint.Zero);
        }

        /// <summary>
        /// Displays a <c>ShellAbout</c> dialog box. For an example of this dialog box, run the <c>winver</c> command.
        /// </summary>
        /// <example>
        /// For an example of this dialog box, run the <c>winver</c> command.
        /// </example>
        /// <param name="app"><para>A string that contains text to be displayed in the title bar of the <c>ShellAbout</c> dialog box and on the first line of the dialog box after the text
        /// "Microsoft". If the text contains a separator <c>(#)</c> that divides it into two parts, the function displays the first part in the title bar and the second part on
        /// the first line after the text "Microsoft".</para>
        ///
        /// <b><i>Windows Vista, Windows Server 2008</i></b>: This string cannot exceed 200 characters in length. The contents of <paramref name="app"/> will no longer
        /// show after "Microsoft", unless there is a <c>#</c> separator, in which case the part after the <c>#</c> will completely replace the first line.</param>
        /// <param name="moreText">A string that contains text to be displayed in the dialog box after the version and copyright information.
        /// This parameter can be <see langword="null"/>.</param>
        /// <param name="icon">An icon that is displayed in the dialog box. This parameter can be <see langword="null"/>,
        /// in which case the dialog displays the Windows icon.</param>
        /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
        [SupportedOSPlatform("windows5.1")] // Windows XP
        [SupportedOSPlatform("windows5.0")] // Windows 2000 (server)
        public static bool ShellAbout(string app, string moreText, Icon? icon = null)
        {
            var sb = new StringBuilder(app);

            return ShellAboutW(nint.Zero, sb, moreText, icon?.Handle ?? nint.Zero);
        }

        /// <summary>
        /// The internal Windows API struct for <see cref="SHGetFileInfoW(string, uint, ref SHFILEINFO, uint, uint)"/>. Use with
        /// <see cref="GetFileInfoEx(string, WindowsFileAttributes, SHGetFileInfoFlags, ref SHFILEINFO)"/>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHFILEINFO
        {
#pragma warning disable CS1591
            public nint hIcon;
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
#pragma warning restore
        }

        [LibraryImport("user32.dll", EntryPoint = "DestroyIcon")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DestroyIconW(nint handle);

        /// <summary>
        /// Destroys an icon and frees any memory the icon occupied.
        /// </summary>
        /// <param name="hIcon">A handle to the icon to be destroyed. The icon must not be in use.</param>
        /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>. To get extended error
        /// information, call <see cref="Marshal.GetLastPInvokeError"/>.</returns>
        /// <remarks>
        /// <para>It is only necessary to call <see cref="DestroyIcon(nint)"/> for icons and cursors created with the following functions:
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-createiconfromresourceex">CreateIconFromResourceEx</see>
        /// (if called without the LR_SHARED flag),
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-createiconindirect">CreateIconIndirect</see>, and
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-copyicon">CopyIcon</see>.
        /// Do not use this function to destroy a shared icon. A shared icon is valid as long as the module from which it was loaded remains in memory.
        /// The following functions obtain a shared icon:</para>
        ///<list type="bullet">
        /// <item>
        ///     <description><see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-loadicona">LoadIcon</see></description>
        /// </item>
        /// <item>
        ///     <description><see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-loadimagea">LoadImage</see>
        ///     (if you use the LR_SHARED flag)</description>
        /// </item>
        /// <item>
        ///     <description><see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-copyimage">CopyImage</see>
        ///     (if you use the LR_COPYRETURNORG flag and the hImage parameter is a shared icon)</description>
        /// </item>
        /// <item>
        ///     <description><see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-createiconfromresource">CreateIconFromResource</see></description>
        /// </item>
        /// <item>
        ///     <description><see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-createiconfromresourceex">CreateIconFromResourceEx</see>
        ///     (if you use the LR_SHARED flag)</description>
        /// </item>
        ///</list>
        /// </remarks>
        [SupportedOSPlatform("windows5.0")] // Windows 2000
        public static bool DestroyIcon(nint hIcon)
        {
            return DestroyIconW(hIcon);
        }

        [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern nuint SHGetFileInfoW([MarshalAs(UnmanagedType.LPTStr)] string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        /// <summary>
        /// Provides flags for the Windows API function <see cref="SHGetFileInfoW(string, uint, ref SHFILEINFO, uint, uint)"/>.
        /// To use this function, call the <see cref="GetFileInfoEx(string, WindowsFileAttributes, SHGetFileInfoFlags, ref SHFILEINFO)"/> wrapper instead.
        /// </summary>
        [Flags]
        public enum SHGetFileInfoFlags
        {
            /// <summary>
            /// Version 5.0. Apply the appropriate overlays to the file's icon. The SHGFI_ICON flag must also be set.
            /// </summary>
            SHGFI_ADDOVERLAYS = 0x000000020,
            /// <summary>
            /// Modify SHGFI_ATTRIBUTES to indicate that the dwAttributes member of the SHFILEINFO structure at psfi contains the specific attributes that are desired. These attributes are passed to IShellFolder::GetAttributesOf. If this flag is not specified, 0xFFFFFFFF is passed to IShellFolder::GetAttributesOf, requesting all attributes. This flag cannot be specified with the SHGFI_ICON flag.
            /// </summary>
            SHGFI_ATTR_SPECIFIED = 0x000020000,
            /// <summary>
            /// Retrieve the item attributes. The attributes are copied to the dwAttributes member of the structure specified in the psfi parameter. These are the same attributes that are obtained from IShellFolder::GetAttributesOf.
            /// </summary>
            SHGFI_ATTRIBUTES = 0x000000800,
            /// <summary>
            /// Retrieve the display name for the file, which is the name as it appears in Windows Explorer. The name is copied to the szDisplayName member of the structure specified in psfi. The returned display name uses the long file name, if there is one, rather than the 8.3 form of the file name. Note that the display name can be affected by settings such as whether extensions are shown.
            /// </summary>
            SHGFI_DISPLAYNAME = 0x000000200,
            /// <summary>
            /// Retrieve the type of the executable file if pszPath identifies an executable file. The information is packed into the return value. This flag cannot be specified with any other flags.
            /// </summary>
            SHGFI_EXETYPE = 0x000002000,
            /// <summary>
            /// Retrieve the handle to the icon that represents the file and the index of the icon within the system image list. The handle is copied to the hIcon member of the structure specified by psfi, and the index is copied to the iIcon member.
            /// </summary>
            SHGFI_ICON = 0x000000100,
            /// <summary>
            /// Retrieve the name of the file that contains the icon representing the file specified by pszPath, as returned by the IExtractIcon::GetIconLocation method of the file's icon handler. Also retrieve the icon index within that file. The name of the file containing the icon is copied to the szDisplayName member of the structure specified by psfi. The icon's index is copied to that structure's iIcon member.
            /// </summary>
            SHGFI_ICONLOCATION = 0x000001000,
            /// <summary>
            /// Modify SHGFI_ICON, causing the function to retrieve the file's large icon. The SHGFI_ICON flag must also be set.
            /// </summary>
            SHGFI_LARGEICON = 0x000000000,
            /// <summary>
            /// Modify SHGFI_ICON, causing the function to add the link overlay to the file's icon. The SHGFI_ICON flag must also be set.
            /// </summary>
            SHGFI_LINKOVERLAY = 0x000008000,
            /// <summary>
            /// Modify SHGFI_ICON, causing the function to retrieve the file's open icon. Also used to modify SHGFI_SYSICONINDEX, causing the function to return the handle to the system image list that contains the file's small open icon. A container object displays an open icon to indicate that the container is open. The SHGFI_ICON and/or SHGFI_SYSICONINDEX flag must also be set.
            /// </summary>
            SHGFI_OPENICON = 0x000000002,
            /// <summary>
            /// Version 5.0. Return the index of the overlay icon. The value of the overlay index is returned in the upper eight bits of the iIcon member of the structure specified by psfi. This flag requires that the SHGFI_ICON be set as well.
            /// </summary>
            SHGFI_OVERLAYINDEX = 0x000000040,
            /// <summary>
            /// Indicate that pszPath is the address of an ITEMIDLIST structure rather than a path name.
            /// </summary>
            SHGFI_PIDL = 0x000000008,
            /// <summary>
            /// Modify SHGFI_ICON, causing the function to blend the file's icon with the system highlight color. The SHGFI_ICON flag must also be set.
            /// </summary>
            SHGFI_SELECTED = 0x000010000,
            /// <summary>
            /// Modify SHGFI_ICON, causing the function to retrieve a Shell-sized icon. If this flag is not specified the function sizes the icon according to the system metric values. The SHGFI_ICON flag must also be set.
            /// </summary>
            SHGFI_SHELLICONSIZE = 0x000000004,
            /// <summary>
            /// Modify SHGFI_ICON, causing the function to retrieve the file's small icon. Also used to modify SHGFI_SYSICONINDEX, causing the function to return the handle to the system image list that contains small icon images. The SHGFI_ICON and/or SHGFI_SYSICONINDEX flag must also be set.
            /// </summary>
            SHGFI_SMALLICON = 0x000000001,
            /// <summary>
            /// Retrieve the index of a system image list icon. If successful, the index is copied to the iIcon member of psfi. The return value is a handle to the system image list. Only those images whose indices are successfully copied to iIcon are valid. Attempting to access other images in the system image list will result in undefined behavior.
            /// </summary>
            SHGFI_SYSICONINDEX = 0x000004000,
            /// <summary>
            /// Retrieve the string that describes the file's type. The string is copied to the szTypeName member of the structure specified in psfi.
            /// </summary>
            SHGFI_TYPENAME = 0x000000400,
            /// <summary>
            /// Indicates that the function should not attempt to access the file specified by pszPath. Rather, it should act as if the file specified by pszPath exists with the file attributes passed in dwFileAttributes. This flag cannot be combined with the SHGFI_ATTRIBUTES, SHGFI_EXETYPE, or SHGFI_PIDL flags.
            /// </summary>
            SHGFI_USEFILEATTRIBUTES = 0x000000010
        }

        /// <summary>
        /// Retrieves information about an object in the file system, such as a file, folder, directory, or drive root.
        /// </summary>
        /// <param name="path"><para>A string of maximum length <see cref="MAX_PATH"/> that contains the path and file name.
        /// Both absolute and relative paths are valid.</para>
        ///
        /// This string can use either short (the 8.3 form) or long file names.</param>
        /// <returns>The <see cref="WindowsFile"/> instance containing the data about the file if successful; otherwise, <see langword="null"/>.</returns>
        [SupportedOSPlatform("windows5.1")] // Windows XP
        [SupportedOSPlatform("windows5.0")] // Windows 2000 Server
        public static WindowsFile? GetFileInfo(string path)
        {
            if (path.Length >= MAX_PATH)
                throw new ArgumentOutOfRangeException(nameof(path), "path must be max MAX_PATH characters long. Use the MAX_PATH constant defined in the Shell32 class to validate.");

            COM.CoInitializeEx(COM.CoInit.ApartmentThreaded);
            var shFileInfo = new SHFILEINFO();

            if (SHGetFileInfoW(path, 0, ref shFileInfo, (uint)Marshal.SizeOf(shFileInfo),
                (uint)(SHGetFileInfoFlags.SHGFI_ATTRIBUTES |
                SHGetFileInfoFlags.SHGFI_DISPLAYNAME |
                SHGetFileInfoFlags.SHGFI_ICON |
                SHGetFileInfoFlags.SHGFI_TYPENAME)) != 0)
            {
                return new WindowsFile(shFileInfo, path);
            }

            COM.CoUninitialize();

            return null;
        }
        /// <summary>
        /// Retrieves information about an object in the file system, such as a file, folder, directory, or drive root. This function provides all the customization
        /// of the Windows API function <see cref="SHGetFileInfoW(string, uint, ref SHFILEINFO, uint, uint)"/>, unlike <see cref="GetFileInfo(string)"/>.
        /// </summary>
        /// <param name="pszPath"><para>A string of maximum length <see cref="MAX_PATH"/> that contains the path and file name. Both absolute and relative paths are valid.</para>
        ///
        /// <para>If the <paramref name="uFlags"/> parameter includes the <see cref="SHGetFileInfoFlags.SHGFI_PIDL"/> flag, this parameter
        /// must be the address of an ITEMIDLIST(PIDL) structure that contains the list of item identifiers that uniquely identifies the file
        /// within the Shell's namespace. The PIDL must be a fully qualified PIDL. Relative PIDLs are not allowed.</para>
        ///
        /// <para>If the <paramref name="uFlags"/> parameter includes the <see cref="SHGetFileInfoFlags.SHGFI_USEFILEATTRIBUTES"/> flag, this parameter does not have to be a valid file name.
        /// The function will proceed as if the file exists with the specified name and with the file attributes passed in the dwFileAttributes
        /// parameter. This allows you to obtain information about a file type by passing just the extension for pszPath and passing
        /// FILE_ATTRIBUTE_NORMAL in dwFileAttributes.</para>
        ///
        /// This string can use either short (the 8.3 form) or long file names.</param>
        /// <param name="dwFileAttributes">A combination of one or more file attribute flags (<see cref="WindowsFileAttributes"/> values). If <paramref name="uFlags"/> does not
        /// include the <see cref="SHGetFileInfoFlags.SHGFI_USEFILEATTRIBUTES"/> flag, this parameter is ignored, so in that case you should set it
        /// to <see cref="WindowsFileAttributes.None"/> instead.</param>
        /// <param name="uFlags">The flags that specify the file information to retrieve. This should be a combination of <see cref="SHGetFileInfoFlags"/> values.</param>
        /// <param name="psfi">Pointer to a <see cref="SHFILEINFO"/> structure to receive the file information.</param>
        /// <returns>A value whose meaning depends on the <paramref name="uFlags"/> parameter.
        ///
        /// If <paramref name="uFlags"/> does not contain <see cref="SHGetFileInfoFlags.SHGFI_EXETYPE"/> or <see cref="SHGetFileInfoFlags.SHGFI_SYSICONINDEX"/>,
        /// the return value is non-zero if successful, or zero otherwise.
        ///
        /// <para>If <paramref name="uFlags"/> contains the <see cref="SHGetFileInfoFlags.SHGFI_EXETYPE"/> flag, the return value specifies the type of the executable file.
        /// It will be one of the following values:</para>
        /// <list type="table">
        ///     <item>
        ///         <term>0</term>
        ///         <description>Nonexecutable file or an error condition.</description>
        ///     </item>
        ///     <item>
        ///         <term>LOWORD = NE or PE and HIWORD = Windows version</term>
        ///         <description>Windows application.</description>
        ///     </item>
        ///     <item>
        ///         <term>LOWORD = PE and HIWORD = 0</term>
        ///         <description>Console application or .bat file</description>
        ///     </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>You should call this function from a background thread. Failure to do so could cause the UI to stop responding.</para>
        ///
        /// <para>If <see cref="GetFileInfoEx(string, WindowsFileAttributes, SHGetFileInfoFlags, ref SHFILEINFO)"/> returns an icon handle in the <see cref="SHFILEINFO.hIcon"/>
        /// member of the <see cref="SHFILEINFO"/> structure pointed to by <paramref name="psfi"/>, you are responsible for freeing it with
        /// <see cref="DestroyIcon(nint)"/> when you no longer need it. The <see cref="WindowsFile"/> wrapper automatically cleans up these resources on dispose, so you
        /// should ideally use <see cref="WindowsFile"/> and convert <paramref name="psfi"/> to a <see cref="WindowsFile"/> using the <see cref="WindowsFile(SHFILEINFO, string)"/>
        /// constructor. Additionaly, <see cref="WindowsFile"/> provides managed types for almost all the <see cref="SHFILEINFO"/> properties, so using it is preffered.</para>
        /// 
        /// <para><i>Note: Once you have a handle to a system image list, you can
        /// use the <see href="https://learn.microsoft.com/en-us/windows/desktop/Controls/image-lists">Image List API</see> to manipulate it like any other image list.
        /// Because system image lists are created on a per-process basis, you should treat them as read-only objects. Writing to a system image list
        /// may overwrite or delete one of the system images, making it unavailable or incorrect for the remainder of the process.</i></para>
        /// 
        /// <para>You must initialize Component Object Model (COM) with <see cref="COM.CoInitializeEx(COM.CoInit)"/> prior to calling <see cref="SHGetFileInfoW(string, uint, ref SHFILEINFO, uint, uint)"/>.
        /// <see cref="GetFileInfoEx(string, WindowsFileAttributes, SHGetFileInfoFlags, ref SHFILEINFO)"/> wraps this automatically.</para>
        /// 
        /// When you use the <see cref="SHGetFileInfoFlags.SHGFI_EXETYPE"/> flag with a Windows application, the Windows version of the executable is given in
        /// the <c>HIWORD</c> of the return value. This version is returned as a hexadecimal value. For details on equating this value with
        /// a specific Windows version, see <see href="https://learn.microsoft.com/en-us/windows/desktop/WinProg/using-the-windows-headers">Using the Windows Headers</see>.
        /// </remarks>
        [SupportedOSPlatform("windows5.1")] // Windows XP
        [SupportedOSPlatform("windows5.0")] // Windows 2000 Server
        public static nuint GetFileInfoEx(string pszPath, WindowsFileAttributes dwFileAttributes, SHGetFileInfoFlags uFlags, ref SHFILEINFO psfi)
        {
            COM.CoInitializeEx(COM.CoInit.ApartmentThreaded);

            var result = SHGetFileInfoW(pszPath, (uint)dwFileAttributes, ref psfi, (uint)Marshal.SizeOf(psfi), (uint)uFlags);

            COM.CoUninitialize();
            return result;
        }

        [LibraryImport("Shell32.dll", SetLastError = true)]
        private static partial HRESULT SHILCreateFromPath([MarshalAs(UnmanagedType.LPWStr)] string pszPath, out nint ppIdl, ref uint rgflnOut);

        [LibraryImport("Shell32.dll", SetLastError = true)]
        private static partial nint ILFindLastID(nint pidl);

        [LibraryImport("Shell32.dll", SetLastError = true)]
        private static partial nint ILClone(nint pidl);

        [DllImport("Shell32.dll", SetLastError = true)]
        private static extern bool ILRemoveLastID(nint pidl);

        [LibraryImport("Shell32.dll", SetLastError = true)]
        private static partial void ILFree(nint pidl);

        [DllImport("Shell32.dll", SetLastError = true)]
        private static extern HRESULT CIDLData_CreateFromIDArray(nint pidlFolder, uint cidl, nint[] apidl, out IDataObject ppdtobj);

        [DllImport("Shell32.dll", SetLastError = true)] // Can't use LibraryImport with IDataObject
        private static extern HRESULT SHMultiFileProperties(IDataObject pdtobj, uint dwFlags);

        /// <summary>
        /// Displays a merged property sheet for a set of files. Property values common to all the files are shown, while those
        /// that differ display the string (multiple values).
        /// </summary>
        /// <param name="filePaths">The file paths of the files to show.</param>
        /// <returns>If this function succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns a <see cref="HRESULT"/> error code.</returns>
        public static HRESULT ShowFileProperties(params string[] filePaths)
        {
            nint pidlParent = nint.Zero, pidlFull = nint.Zero, pidlItem = nint.Zero;
            uint rgflnOut = 0;
            var aPidl = new nint[255];
            uint nIndex = 0;

            HRESULT hr;
            foreach (string filePath in filePaths)
            {
                hr = SHILCreateFromPath(filePath, out pidlFull, ref rgflnOut);
                if (hr == HRESULT.S_OK)
                {
                    pidlItem = ILFindLastID(pidlFull);
                    aPidl[nIndex++] = ILClone(pidlItem);
                    ILRemoveLastID(pidlFull);
                    pidlParent = ILClone(pidlFull);
                    ILFree(pidlFull);
                }
            }

            hr = CIDLData_CreateFromIDArray(pidlParent, nIndex, aPidl, out IDataObject pDO);
            if (hr == HRESULT.S_OK)
            {
                hr = SHMultiFileProperties(pDO, 0);
            }

            if (pidlParent != nint.Zero)
                ILFree(pidlParent);
            for (int i = 0; i < nIndex; i++)
            {
                if (aPidl[i] != nint.Zero)
                    ILFree(aPidl[i]);
            }

            return hr;
        }
    }

    /// <summary>
    /// Stores data about a Windows file.
    /// </summary>
    /// <param name="info">The <see cref="Shell32.SHFILEINFO"/> to create the <see cref="WindowsFile"/> from.</param>
    /// <param name="path">The file path of the file.</param>
    [SupportedOSPlatform("windows")]
    public class WindowsFile(Shell32.SHFILEINFO info, string path) : IDisposable
    {
        /// <summary>
        /// Gets the icon of the file.
        /// </summary>
        public Icon Icon => (Icon)Icon.FromHandle(shFileInfo.hIcon).Clone();

        /// <summary>
        /// Gets the display name of the file.
        /// </summary>
        public string DisplayName => shFileInfo.szDisplayName;

        /// <summary>
        /// Gets the display name of the file type of the file that this <see cref="WindowsFile"/> stores, or a localized standard Windows value (such as "File folder")
        /// for folders.
        /// </summary>
        public string TypeName => shFileInfo.szTypeName;

        /// <summary>
        /// Gets the file or directory attributes.
        /// </summary>
        public WindowsFileAttributes Attributes
        {
            get
            {
                return (WindowsFileAttributes)shFileInfo.dwAttributes;
            }
        }

        /// <summary>
        /// Gets the icon index of the file's icon in Windows's internal image list.
        /// </summary>
        public int IconIndex
        {
            get
            {
                return shFileInfo.iIcon;
            }
        }

        /// <summary>
        /// Gets the text content of the file.
        /// </summary>
        /// <remarks>
        /// Unlike the rest of the <see cref="WindowsFile"/> class, this value is not cached.
        /// </remarks>
        public string Content => File.ReadAllText(_path);

        /// <summary>
        /// Gets the bytes of the file.
        /// </summary>
        /// <remarks>
        /// Unlike the rest of the <see cref="WindowsFile"/> class, this value is not cached.
        /// </remarks>
        public byte[] Bytes => File.ReadAllBytes(_path);

        private Shell32.SHFILEINFO shFileInfo = info;
        private string _path = path;

        /// <summary>
        /// Gets the file path this <see cref="WindowsFile"/> represents.
        /// </summary>
        public string FilePath => _path;

        /// <summary>
        /// Set to <see langword="true"/> when the <see cref="WindowsFile"/> instance is disposed.
        /// </summary>
        public bool Disposed { get; private set; } = false;

        /// <summary>
        /// Frees any resources associated with this <see cref="WindowsFile"/>.
        /// </summary>
        public void Dispose()
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(5) && !Disposed)
            {
                Shell32.DestroyIcon(shFileInfo.hIcon);
                Disposed = true;
            }
        }

        /// <summary>
        /// Converts this <see cref="WindowsFile"/> to a string.
        /// </summary>
        /// <returns>This <see cref="WindowsFile"/>, converted to a string.</returns>
        public override string ToString()
        {
            return $"{DisplayName} ({TypeName}; attributes: {string.Join(", ", Attributes.GetValues())})";
        }
    }

    /// <summary>
    /// Specifies attributes for Windows files. 
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/fileio/file-attribute-constants">File Attribute Constants</seealso>
    [Flags]
    [SupportedOSPlatform("windows")]
    public enum WindowsFileAttributes
    {
        /// <summary>
        /// A file that does not have other attributes set. This attribute is valid only when used alone. (FILE_ATTRIBUTE_NORMAL)
        /// </summary>
        None = 0x00000080,
        /// <summary>
        /// A file that is read-only. Applications can read the file,
        /// but cannot write to it or delete it. This attribute is not honored on directories.
        /// For more information, see <see href="https://support.microsoft.com/topic/you-cannot-view-or-change-the-read-only-or-the-system-attributes-of-folders-in-windows-server-2003-in-windows-xp-in-windows-vista-or-in-windows-7-55bd5ec5-d19e-6173-0df1-8f5b49247165">
        /// You cannot view or change the Read-only or the System attributes
        /// of folders in Windows Server 2003, in Windows XP, in Windows Vista or in Windows 7</see>. (FILE_ATTRIBUTE_READONLY)
        /// </summary>
        ReadOnly = 0x00000001,
        /// <summary>
        /// The file or directory is hidden. It is not included in an ordinary directory listing. (FILE_ATTRIBUTE_HIDDEN)
        /// </summary>
        Hidden = 0x00000002,
        /// <summary>
        /// A file or directory that the operating system uses as part of, or uses exclusively. (FILE_ATTRIBUTE_SYSTEM)
        /// </summary>
        System = 0x00000004,
        /// <summary>
        /// The handle that identifies a directory. (FILE_ATTRIBUTE_DIRECTORY)
        /// </summary>
        Directory = 0x00000010,
        /// <summary>
        /// A file or directory that is an archive file or directory. Applications typically use this attribute
        /// to mark files for backup or removal. (FILE_ATTRIBUTE_ARCHIVE)
        /// </summary>
        Archive = 0x00000020,
        /// <summary>
        /// This value is reserved for system use. Do not use. (FILE_ATTRIBUTE_DEVICE)
        /// </summary>
        [Obsolete("This value is reserved for system use. Do not use.")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Device = 0x00000040,
        /// <summary>
        /// A file that is being used for temporary storage. File systems avoid writing data back to mass storage
        /// if sufficient cache memory is available, because typically, an application deletes a temporary file after the handle is closed.
        /// In that scenario, the system can entirely avoid writing the data. Otherwise, the data is written after the handle is closed. (FILE_ATTRIBUTE_TEMPORARY)
        /// </summary>
        Temporary = 0x00000100,
        /// <summary>
        /// A file that is a sparse file. (FILE_ATTRIBUTE_SPARSE_FILE)
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
        /// The data of a file is not available immediately. This attribute indicates that the file data is physically moved to offline storage. This attribute is used
        /// by Remote Storage, which is the hierarchical storage management software. Applications should not arbitrarily change this attribute. (FILE_ATTRIBUTE_OFFLINE)
        /// </summary>
        Offline = 0x00001000,
        /// <summary>
        /// The file or directory is not to be indexed by the content indexing service. (FILE_ATTRIBUTE_NOT_CONTENT_INDEXED)
        /// </summary>
        NoContentIndex = 0x00002000,
        /// <summary>
        /// A file or directory that is encrypted. For a file, all data streams in the file are encrypted. For a directory,
        /// encryption is the default for newly created files and subdirectories. (FILE_ATTRIBUTE_ENCRYPTED)
        /// </summary>
        Encrypted = 0x00004000,
        /// <summary>
        /// The directory or user data stream is configured with integrity (only supported on ReFS volumes). It is not included in an ordinary directory
        /// listing. The integrity setting persists with the file if it's renamed. If a file is copied the destination file will have integrity set if either the source file
        /// or destination directory have integrity set. <b>Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003 and
        /// Windows XP</b>: This flag is not supported until <i>Windows Server 2012</i>. (FILE_ATTRIBUTE_INTEGRITY_STREAM)
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        IntegrityStream = 0x00008000,
        /// <summary>
        /// This value is reserved for system use. Do not use. (FILE_ATTRIBUTE_VIRTUAL)
        /// </summary>
        [Obsolete("This value is reserved for system use. Do not use.")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Virtual = 0x00010000,
        /// <summary>
        /// The user data stream not to be read by the background data integrity scanner (AKA scrubber). When set on a directory it only provides inheritance.
        /// This flag is only supported on Storage Spaces and ReFS volumes. It is not included in an ordinary directory listing.  Windows Server 2008 R2, Windows
        /// 7, Windows Server 2008, Windows Vista, Windows Server 2003 and Windows XP: This flag is not supported until Windows 8 and Windows Server 2012.
        /// (FILE_ATTRIBUTE_NO_SCRUB_DATA)
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        NoScrubData = 0x00020000,
        /// <summary>
        /// A file or directory with extended attributes. This value is for internal use only. Do not use. (FILE_ATTRIBUTE_EA)
        /// </summary>
        [Obsolete("This value is reserved for system use. Do not use.")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ExAttrib = 0x00040000,
        /// <summary>
        /// This attribute indicates user intent that the file or directory should be kept fully present locally even when not being actively accessed.
        /// This attribute is for use with hierarchical storage management software. (FILE_ATTRIBUTE_PINNED)
        /// </summary>
        Pinned = 0x00080000,
        /// <summary>
        /// This attribute indicates that the file or directory should not be kept fully present locally except when being actively accessed.
        /// This attribute is for use with hierarchical storage management software. (FILE_ATTRIBUTE_UNPINNED)
        /// </summary>
        Unpinned = 0x00100000,
        /// <summary>
        /// This attribute only appears in directory enumeration classes (FILE_DIRECTORY_INFORMATION, FILE_BOTH_DIR_INFORMATION, etc.).
        /// When this attribute is set, it means that the file or directory has no physical representation on the local system; the item is virtual.
        /// Opening the item will be more expensive than normal, e.g. it will cause at least some of it to be fetched from a remote store. (FILE_ATTRIBUTE_RECALL_ON_OPEN)
        /// </summary>
        RecallOnOpen = 0x00040000,
        /// <summary>
        /// When this attribute is set, it means that the file or directory is not fully present locally. For a file that means that not all of its data is on local storage
        /// (e.g. it may be sparse with some data still in remote storage). For a directory it means that some of the directory contents are being virtualized from another
        /// location. Reading the file / enumerating the directory will be more expensive than normal, e.g. it will cause at least some of the file/directory content to be
        /// fetched from a remote store. Only kernel-mode callers can set this bit. File system mini filters below the 180000 – 189999 altitude range
        /// (FSFilter HSM Load Order Group) must not issue targeted cached reads or writes to files that have this attribute set. This could lead to cache pollution and
        /// potential file corruption. For more information, see
        /// <see href="https://learn.microsoft.com/en-us/windows-hardware/drivers/ifs/placeholders_guidance">Handling placeholders</see>.
        /// (FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS)
        /// </summary>
        RecallOnDataAccess = 0x00400000,
        /// <summary>
        /// A system file. A system file has the <see cref="Hidden"/> and <see cref="System"/> attributes. This is a composite value.
        /// </summary>
        SystemFile = Hidden | System,
        /// <summary>
        /// An archive directory. Applications typically use this attribute
        /// to mark directories for backup or removal. This is a composite value.
        /// </summary>
        ArchiveDir = Archive | Directory
    }
}
