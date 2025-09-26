using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace FireBlade.WinInteropUtils
{
    /// <summary>
    /// Allows you to get a shell stock icon. Unlike the built-in .NET <see cref="SystemIcons.GetStockIcon(StockIconId, int)"/> or
    /// <see cref="SystemIcons.GetStockIcon(StockIconId, StockIconOptions)"/> functions, this wrapper allows you to enter any icon ID you want.
    /// </summary>
    public class StockIconHelper
    {
        private const uint SHGSI_ICON = 0x000000100;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHSTOCKICONINFO
        {
            public uint cbSize;
            public IntPtr hIcon;
            public int iSysImageIndex;
            public int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }

        [DllImport("shell32.dll", SetLastError = false)]
        private static extern int SHGetStockIconInfo(
            int siid,
            uint uFlags,
            ref SHSTOCKICONINFO psii);

        /// <summary>
        /// Gets the icon from the specified custom <paramref name="stockIconId"/>.
        /// </summary>
        /// <param name="stockIconId">The ID of the stock icon.</param>
        /// <param name="options">Additional options for the icon.</param>
        /// <returns>The created icon.</returns>
        /// <exception cref="Exception">An error occured in the <c>SHGetStockIconInfo</c> WinAPI function.</exception>
        public static Icon GetIcon(int stockIconId, StockIconOptions options = StockIconOptions.Default)
        {
            var info = new SHSTOCKICONINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);

            uint flags = SHGSI_ICON | (uint)options;

            int hr = SHGetStockIconInfo(stockIconId, flags, ref info);
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);

            Icon icon = (Icon)Icon.FromHandle(info.hIcon).Clone();
            Shell32.DestroyIcon(info.hIcon);
            return icon;
        }

        /// <summary>
        /// Gets the icon for the specified value from the <see cref="StockIcon"/> enumeration.
        /// </summary>
        /// <param name="icon">The icon to get.</param>
        /// <param name="options">Additional options for the icon.</param>
        /// <returns>The created icon.</returns>
        /// <exception cref="Exception">An error occured in the <c>SHGetStockIconInfo</c> WinAPI function.</exception>
        public static Icon GetIcon(StockIcon icon, StockIconOptions options = StockIconOptions.Default) => GetIcon((int)icon, options);
    }

    /// <summary>
    /// Icon identifiers for the <see cref="StockIconHelper.GetIcon(StockIcon, StockIconOptions)"/> function.
    /// </summary>
    public enum StockIcon
    {
        /// <summary>
        /// Document of a type with no associated application.
        /// </summary>
        /// <remarks>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_docnoassoc.png)]
        /// </remarks>
        DocumentNoAssociation = 0,
        /// <summary>
        /// Document of a type with an associated application.
        /// </summary>
        /// <remarks>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_docassoc.png)
        /// </remarks>
        DocumentWithAssociaton = 1,
        /// <summary>
        /// Generic application with no custom icon.
        /// </summary>
        /// <remarks>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_application.png)
        /// </remarks>
        Application = 2,
        /// <summary>
        /// Folder (generic, unspecified state).
        /// </summary>
        /// <remarks>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_folder.png)
        /// </remarks>
        Folder = 3,
        /// <summary>
        /// Folder (open).
        /// </summary>
        /// <remarks>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_folderopen.png)
        /// </remarks>
        FolderOpen = 4,
        /// <summary>
        /// 5.25-inch disk drive.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_drive525.png)</remarks>
        Drive525 = 5,
        /// <summary>
        /// 3.5-inch disk drive.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_drive35.png)</remarks>
        Drive35 = 6,
        /// <summary>
        /// Removable drive.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_driveremove.png)</remarks>
        DriveRemove = 7,
        /// <summary>
        /// Fixed drive (hard disk).
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_drivefixed.png)</remarks>
        DriveFixed = 8,
        /// <summary>
        /// Network drive (connected).
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_drivenet.png)</remarks>
        DriveNetwork = 9,
        /// <summary>
        /// Network drive (disconnected).
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_drivenetdisabled.png)</remarks>
        DriveNetworkDisabled = 10,
        /// <summary>
        /// CD drive.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_drivecd.png)</remarks>
        DriveCD = 11,
        /// <summary>
        /// RAM disk drive.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_driveram.png)</remarks>
        DriveRAM = 12,
        /// <summary>
        /// The entire network.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_world.png)</remarks>
        World = 13,
        /// <summary>
        /// A computer on the network.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_server.png)</remarks>
        Server = 15,
        /// <summary>
        /// A local printer or print destination.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_printer.png)</remarks>
        Printer = 16,
        /// <summary>
        /// The <b>Network</b> virtual folder.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mynetwork.png)</remarks>
        MyNetwork = 17,
        /// <summary>
        /// The <b>Network</b> virtual folder.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mynetwork.png)</remarks>
        Network = MyNetwork,
        /// <summary>
        /// The <b>Search</b> feature.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_find.png)</remarks>
        Find = 22,
        /// <summary>
        /// The <b>Help and Support</b> feature.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_help.png)</remarks>
        Help = 23,
        /// <summary>
        /// Overlay for a shared item.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_share.png)</remarks>
        Share = 28,
        /// <summary>
        /// Overlay for a shortcut.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_link.png)</remarks>
        Link = 29,
        /// <summary>
        /// Overlay for a shortcut.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_link.png)</remarks>
        Shortcut = Link,
        /// <summary>
        /// Overlay for items that are expected to be slow to access.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_slowfile.png)</remarks>
        SlowFile = 30,
        /// <summary>
        /// The Recycle Bin (empty).
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_recycler.png)</remarks>
        Recycler = 31,
        /// <summary>
        /// The Recycle Bin (empty).
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_recycler.png)</remarks>
        RecycleBinEmpty = Recycler,
        /// <summary>
        /// The Recycle Bin (not empty).
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_recyclerfull.png)</remarks>
        RecyclerFull = 32,
        /// <summary>
        /// The Recycle Bin (not empty).
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_recyclerfull.png)</remarks>
        RecycleBinFull = RecyclerFull,
        /// <summary>
        /// Audio CD media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediacdaudio.png)</remarks>
        MediaCDAudio = 40,
        /// <summary>
        /// Security lock.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_lock.png)</remarks>
        Lock = 47,
        /// <summary>
        /// Security lock.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_lock.png)</remarks>
        SecurityLock = Lock,
        /// <summary>
        /// A virtual folder that contains the results of a search.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_autolist.png)</remarks>
        AutoList = 49,
        /// <summary>
        /// A network printer.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_printernet.png)</remarks>
        PrinterNet = 50,
        /// <summary>
        /// A server shared on a network.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_servershare.png)</remarks>
        ServerShare = 51,
        /// <summary>
        /// A local fax printer.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_printerfax.png)</remarks>
        PrinterFax = 52,
        /// <summary>
        /// A network fax printer.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_printerfaxnet.png)</remarks>
        PrinterFaxNet = 53,
        /// <summary>
        /// A file that receives the output of
        /// a <b>Print to file</b> operation.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_printerfile.png)</remarks>
        PrinterFile = 54,
        /// <summary>
        /// A file that receives the output of
        /// a <b>Print to file</b> operation.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_printerfile.png)</remarks>
        PrintToFile = PrinterFile,
        /// <summary>
        /// A category that results from a <b>Stack by</b> command
        /// to organize the contents of a folder.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_stack.png)</remarks>
        Stack = 55,
        /// <summary>
        /// Super Video CD (SVCD) media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediasvcd.png)</remarks>
        MediaSVCD = 56,
        /// <summary>
        /// A folder that contains only subfolders as child items.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_stuffedfolder.png)</remarks>
        StuffedFolder = 57,
        /// <summary>
        /// Unknown drive type.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_driveunknown.png)</remarks>
        DriveUnknown = 58,
        /// <summary>
        /// DVD drive.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_drivedvd.png)</remarks>
        DriveDVD = 59,
        /// <summary>
        /// DVD media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediadvd.png)</remarks>
        MediaDVD = 60,
        /// <summary>
        /// DVD-RAM media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediadvdram.png)</remarks>
        MediaDVDRAM = 61,
        /// <summary>
        /// DVD-RW media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediadvdrw.png)</remarks>
        MediaDVDRW = 62,
        /// <summary>
        /// DVD-R media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediadvdr.png)</remarks>
        MediaDVDR = 63,
        /// <summary>
        /// DVD-ROM media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediadvdrom.png)</remarks>
        MediaDVDROM = 64,
        /// <summary>
        /// CD+ (enhanced audio CD) media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediacdaudioplus.png)</remarks>
        MediaCDAudioPlus = 65,
        /// <summary>
        /// CD-RW media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediacdrw.png)</remarks>
        MediaCDRW = 66,
        /// <summary>
        /// CD-R media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediacdr.png)</remarks>
        MediaCDR = 67,
        /// <summary>
        /// A writable CD in the process of being burned.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediacdburn.png)</remarks>
        MediaCDBurn = 68,
        /// <summary>
        /// Blank writable CD media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediablankcd.png)</remarks>
        MediaBlankCD = 69,
        /// <summary>
        /// CD-ROM media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediacdrom.png)</remarks>
        MediaCDROM = 70,
        /// <summary>
        /// An audio file.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_audiofiles.png)</remarks>
        AudioFiles = 71,
        /// <summary>
        /// An image file.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_imagefiles.png)</remarks>
        ImageFiles = 72,
        /// <summary>
        /// A video file.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_videofiles.png)</remarks>
        VideoFiles = 73,
        /// <summary>
        /// A mixed file.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mixedfiles.png)</remarks>
        MixedFiles = 74,
        /// <summary>
        /// Folder back.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_folderback.png)</remarks>
        FolderBack = 75,
        /// <summary>
        /// Folder front.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_folderfront.png)</remarks>
        FolderFront = 76,
        /// <summary>
        /// Security shield. Use for UAC prompts only.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_shield.png)</remarks>
        Shield = 77,
        /// <summary>
        /// Security shield. Use for UAC prompts only.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_shield.png)</remarks>
        UAC = Shield,
        /// <summary>
        /// Security shield. Use for UAC prompts only.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_shield.png)</remarks>
        UserAccountControl = Shield,
        /// <summary>
        /// Security shield. Use for UAC prompts only.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_shield.png)</remarks>
        UACShield = Shield,
        /// <summary>
        /// Warning.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_warning.png)</remarks>
        Warning = 78,
        /// <summary>
        /// Informational.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_info.png)</remarks>
        Info = 79,
        /// <summary>
        /// Informational.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_info.png)</remarks>
        Information = Info,
        /// <summary>
        /// Error.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_error.png)</remarks>
        Error = 80,
        /// <summary>
        /// Error.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_error.png)</remarks>
        Critical = Error,
        /// <summary>
        /// Key.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_key.png)</remarks>
        Key = 81,
        /// <summary>
        /// Software.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_software.png)</remarks>
        Software = 82,
        /// <summary>
        /// A UI item, such as a button, that issues a rename command.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_rename.png)</remarks>
        Rename = 83,
        /// <summary>
        /// A UI item, such as a button, that issues a delete command.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_delete.png)</remarks>
        Delete = 84,
        /// <summary>
        /// A UI item, such as a button, that issues a delete command.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_delete.png)</remarks>
        Remove = Delete,
        /// <summary>
        /// Audio DVD media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediaaudiodvd.png)</remarks>
        MediaAudioDVD = 85,
        /// <summary>
        /// Movie DVD media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediamoviedvd.png)</remarks>
        MediaMovieDVD = 86,
        /// <summary>
        /// Enhanced CD media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediaenhancedcd.png)</remarks>
        MediaEnhancedCD = 87,
        /// <summary>
        /// High definition DVD media in the HD DVD format.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediahddvd.png)</remarks>
        MediaHDDVD = 89,
        /// <summary>
        /// High definition DVD media in the Blu-ray Disc™ format.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediabluray.png)</remarks>
        MediaBluRay = 90,
        /// <summary>
        /// Video CD (VCD) media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediavcd.png)</remarks>
        MediaVCD = 91,
        /// <summary>
        /// DVD+R media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediadvdplusr.png)</remarks>
        MediaDVDPlusR = 92,
        /// <summary>
        /// DVD+RW media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediadvdplusrw.png)</remarks>
        MediaDVDPlusRW = 93,
        /// <summary>
        /// A desktop computer.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_desktoppc.png)</remarks>
        DesktopPC = 94,
        /// <summary>
        /// A mobile computer (laptop).
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mobilepc.png)</remarks>
        MobilePC = 95,
        /// <summary>
        /// The <b>User Accounts</b> Control Panel item.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_users.png)</remarks>
        Users = 96,
        /// <summary>
        /// Smart media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediasmartmedia.png)</remarks>
        SmartMedia = 97,
        /// <summary>
        /// CompactFlash media.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediacompactflash.png)</remarks>
        MediaCompactFlash = 98,
        /// <summary>
        /// A cell phone.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_devicecellphone.png)</remarks>
        DeviceCellPhone = 99,
        /// <summary>
        /// A digital camera.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_devicecamera.png)</remarks>
        DeviceCamera = 100,
        /// <summary>
        /// A digital video camera.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_devicevideocamera.png)</remarks>
        DeviceVideoCamera = 101,
        /// <summary>
        /// An audio player.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_deviceaudioplayer.png)</remarks>
        DeviceAudioPlayer = 102,
        /// <summary>
        /// Connect to network.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_networkconnect.png)</remarks>
        NetworkConnect = 103,
        /// <summary>
        /// The <b>Network and Internet</b> Control Panel item.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_internet.png)</remarks>
        Internet = 104,
        /// <summary>
        /// A compressed file with a <b>.zip</b> file name extension.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_zipfile.png)</remarks>
        ZipFile = 105,
        /// <summary>
        /// The <b>Additional Options</b> Control Panel item.
        /// </summary>
        /// <remarks>![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_settings.png)</remarks>
        Settings = 106,
        /// <summary>
        /// High definition DVD drive (any type - HD DVD-ROM, HD DVD-R, HD-DVD-RAM) that uses the HD DVD format.
        /// </summary>
        /// <remarks>
        /// <para>Requires Windows Vista SP1 or later.</para>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_drivehddvd.png)
        /// </remarks>
        [SupportedOSPlatform("windows6.0")]
        DriveHDDVD = 132,
        /// <summary>
        /// High definition DVD drive (any type - BD-ROM, BD-R, BD-RE) that uses the Blu-ray Disc format.
        /// </summary>
        /// <remarks>
        /// <para>Requires Windows Vista SP1 or later.</para>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_drivebd.png)
        /// </remarks>
        [SupportedOSPlatform("windows6.0")]
        DriveBD = 133,
        /// <summary>
        /// High definition DVD-ROM media in the HD DVD-ROM format.
        /// </summary>
        /// <remarks>
        /// <para>Requires Windows Vista SP1 or later.</para>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediahddvdrom.png)
        /// </remarks>
        [SupportedOSPlatform("windows6.0")]
        MediaHDDVDROM = 134,
        /// <summary>
        /// High definition DVD-R media in the HD DVD-R format.
        /// </summary>
        /// <remarks>
        /// <para>Requires Windows Vista SP1 or later.</para>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediahddvdr.png)
        /// </remarks>
        [SupportedOSPlatform("windows6.0")]
        MediaHDDVDR = 135,
        /// <summary>
        /// High definition DVD-RAM media in the HD DVD-RAM format.
        /// </summary>
        /// <remarks>
        /// <para>Requires Windows Vista SP1 or later.</para>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediahddvdram.png)
        /// </remarks>
        [SupportedOSPlatform("windows6.0")]
        MediaHDDVDRAM = 136,
        /// <summary>
        /// High definition DVD-ROM media in the Blu-ray Disc BD-ROM format.
        /// </summary>
        /// <remarks>
        /// <para>Requires Windows Vista SP1 or later.</para>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediabdrom.png)
        /// </remarks>
        [SupportedOSPlatform("windows6.0")]
        MediaBDROM = 137,
        /// <summary>
        /// High definition write-once media in the Blu-ray Disc BD-R format.
        /// </summary>
        /// <remarks>
        /// <para>Requires Windows Vista SP1 or later.</para>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediabdr.png)
        /// </remarks>
        [SupportedOSPlatform("windows6.0")]
        MediaBDR = 138,
        /// <summary>
        /// High definition read/write media in the Blu-ray Disc BD-RE format.
        /// </summary>
        /// <remarks>
        /// <para>Requires Windows Vista SP1 or later.</para>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_mediabdre.png)
        /// </remarks>
        [SupportedOSPlatform("windows6.0")]
        MediaBDRE = 139,
        /// <summary>
        /// A cluster disk array.
        /// </summary>
        /// <remarks>
        /// <para>Requires Windows Vista SP1 or later.</para>
        /// ![Icon preview](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/images/siid_clustereddrive.png)
        /// </remarks>
        [SupportedOSPlatform("windows6.0")]
        ClusteredDrive = 140
    }
}
