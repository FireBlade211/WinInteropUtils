using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace FireBlade.WinInteropUtils.FileSystem
{
    /// <summary>
    /// Represents a Windows file.
    /// </summary>
    public sealed partial class WinFile : IDisposable, IHandle
    {
        private nint _hfile;

        /// <summary>
        /// Gets the handle (<c>HFILE</c>) of this file.
        /// </summary>
        public nint Handle => _hfile;

        private string _path = string.Empty;

        /// <summary>
        /// Gets the path to this file.
        /// </summary>
        public string Path => _path;

        /// <summary>
        /// Gets the information about this file.
        /// </summary>
        public WinFileInfo FileInfo => new WinFileInfo(_path);

        [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static partial nint CreateFileW(
    string lpFileName,
    uint dwDesiredAccess,
    uint dwShareMode,
    nint lpSecurityAttributes,
    uint dwCreationDisposition,
    uint dwFlagsAndAttributes,
    nint hTemplateFile);

        private const nint INVALID_HANDLE_VALUE = -1;

        [StructLayout(LayoutKind.Explicit)]
        private struct LARGE_INTEGER
        {
            [FieldOffset(0)]
            public uint LowPart;

            [FieldOffset(4)]
            public int HighPart;

            [FieldOffset(0)]
            public long QuadPart;
        }

        [LibraryImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private unsafe static partial bool GetFileSizeEx(
          nint hFile,
          LARGE_INTEGER* lpFileSize
        );

        /// <summary>
        /// Gets the current size of the file, in bytes.
        /// </summary>
        public long Size
        {
            get
            {
                unsafe
                {
                    LARGE_INTEGER li = new LARGE_INTEGER();

                    if (GetFileSizeEx(_hfile, &li))
                        return li.QuadPart;

                    else return 0;
                }
            }
        }

        [LibraryImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private unsafe static partial bool ReadFile(
          nint hFile,
          nint lpBuffer,
          uint nNumberOfBytesToRead,
          uint* lpNumberOfBytesRead,
          nint lpOverlapped
        );

        [LibraryImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private unsafe static partial bool WriteFile(
          nint hFile,
          nint lpBuffer,
          uint nNumberOfBytesToWrite,
          uint* lpNumberOfBytesWritten,
          nint lpOverlapped
        );

        private const uint ReadChunkSize = 64 * 1024;

        /// <summary>
        /// Gets or sets the bytes of the file.
        /// </summary>
        /// <remarks>
        /// Do not repeatedly get this property; instead, cache it, as reading files can be expensive.
        /// </remarks>
        public byte[] Bytes
        {
            get
            {
                long size = this.Size;
                List<byte> result = new(int.CreateTruncating(size));

                unsafe
                {
                    byte* buffer = (byte*)NativeMemory.Alloc(ReadChunkSize);

                    try
                    {
                        long bytesReadTotal = 0;

                        while (bytesReadTotal < size)
                        {
                            uint toRead = (uint)Math.Min(ReadChunkSize, size - bytesReadTotal);

                            uint read = 0;

                            bool success = ReadFile(
                                _hfile,
                                (nint)buffer,
                                toRead,
                                &read,
                                nint.Zero);

                            if (!success || read == 0)
                                break;

                            result.AddRange(new ReadOnlySpan<byte>(buffer, (int)read));

                            bytesReadTotal += read;
                        }
                    }
                    finally
                    {
                        NativeMemory.Free(buffer);
                    }
                }

                return result.ToArray();
            }
            set
            {
                long size = value.LongLength;

                unsafe
                {
                    byte* buffer = (byte*)NativeMemory.Alloc(ReadChunkSize);

                    try
                    {
                        long bytesWrittenTotal = 0;

                        while (bytesWrittenTotal < size)
                        {
                            uint toWrite = (uint)Math.Min(ReadChunkSize, size - bytesWrittenTotal);

                            fixed (byte* src = &value[(int)bytesWrittenTotal])
                            {
                                Buffer.MemoryCopy(
                                    src,
                                    buffer,
                                    ReadChunkSize,
                                    toWrite);
                            }

                            uint written = 0;

                            bool success = WriteFile(
                                _hfile,
                                (nint)buffer,
                                toWrite,
                                &written,
                                nint.Zero);

                            if (!success || written == 0)
                                break;

                            bytesWrittenTotal += written;
                        }
                    }
                    finally
                    {
                        NativeMemory.Free(buffer);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the string content of the file.
        /// </summary>
        /// <remarks>This property assumes the file is encoded with UTF-8. If the file is a different encoding,
        /// you may have to use <see cref="Encoding.GetString(byte[])"/> or <see cref="Encoding.GetBytes(string)"/> instead.</remarks>
        public string Content
        {
            get => Encoding.UTF8.GetString(Bytes);
            set => Bytes = Encoding.UTF8.GetBytes(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFile"/> class with the specified path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="access">The requested access to the file. A bitwise combination of <see cref="WinFileAccess"/> values.</param>
        /// <param name="shareMode">The requested sharing mode of the file.</param>
        /// <param name="mode">The action to take on a file that exists or does not exist.</param>
        /// <param name="attribs">The file attributes and flags.</param>
        /// <param name="options">Additional options for opening the file.</param>
        /// <param name="templateFile">A valid template file with the <see cref="WinFileAccess.Read"/> access right.
        /// The template file supplies file attributes and extended attributes for the file that is being created.
        /// If opening an existing file, this parameter is ignored.</param>
        public WinFile(string path, WinFileAccess access = WinFileAccess.Read, WinFileShareMode shareMode = WinFileShareMode.None,
            WinFileOpenMode mode = WinFileOpenMode.OpenExisting, WinFileAttributes attribs = WinFileAttributes.None,
            WinFileOptions options = WinFileOptions.None, WinFile? templateFile = null)
        {
            _path = path;

            nint handle = CreateFileW(
                path,
                (uint)access,
                (uint)shareMode,
                nint.Zero,
                (uint)mode,
                (uint)attribs | (uint)options,
                templateFile?._hfile ?? nint.Zero);

            if (handle != INVALID_HANDLE_VALUE)
                _hfile = handle;
            else
            {
                int error = Marshal.GetLastPInvokeError();
                HResult hr = Macros.HResultFromWin32((Win32ErrorCode)error);

                throw hr;
            }
        }

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool CloseHandle(nint hObject);

        private bool _disposed;

        /// <summary>
        /// Releases this <see cref="WinFile"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            CloseHandle(_hfile);
        }
    }

    /// <summary>
    /// Represents the file access mode of a <see cref="WinFile"/>.
    /// </summary>
    [Flags]
    public enum WinFileAccess : uint
    {
        /// <summary>
        /// Default access (no access).
        /// </summary>
        /// <remarks>When using this access mode, you can query certain information and metadata about the file,
        /// even if <see cref="Read"/> access would be denied.</remarks> 
        None = 0,
        /// <summary>
        /// Read access.
        /// </summary>
        Read = 0x80000000,
        /// <summary>
        /// Write access.
        /// </summary>
        Write = 0x40000000
    }

    /// <summary>
    /// Represents the sharing mode of a <see cref="WinFile"/>.
    /// </summary>
    [Flags]
    public enum WinFileShareMode
    {
        /// <summary>
        /// Prevents subsequent open operations on a file or device if they request delete, read, or write access.
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// Enables subsequent open operations on a file or device to request delete access.
        /// Otherwise, no process can open the file or device if it requests delete access.
        /// </summary>
        /// <remarks>
        /// <para>If this flag is not specified, but the file or device has been opened for delete access, the function fails.</para>
        /// > [!NOTE]
        /// > Delete access allows both delete and rename operations.
        /// </remarks>
        Delete = 0x00000004,
        /// <summary>
        /// Enables subsequent open operations on a file or device to request read access.
        /// Otherwise, no process can open the file or device if it requests read access.
        /// </summary>
        /// <remarks>If this flag is not specified, but the file or device has been opened for read access, the
        /// function fails.</remarks>
        Read = 0x00000001,
        /// <summary>
        /// Enables subsequent open operations on a file or device to request write access.
        /// Otherwise, no process can open the file or device if it requests write access.
        /// </summary>
        /// <remarks>If this flag is not specified, but the file or device has been opened for write access or
        /// has a file mapping with write access, the function fails.</remarks>
        Write = 0x00000002,
        /// <summary>
        /// Enables subsequent open operations on a file or device to request read or write access.
        /// Otherwise, no process can open the file or device if it requests read or write access.
        /// This is a composite value.
        /// </summary>
        ReadWrite = Read | Write
    }

    /// <summary>
    /// Specifies how to open a <see cref="WinFile"/>.
    /// </summary>
    public enum WinFileOpenMode
    {
        /// <summary>
        /// Creates a new file, always. If the specified file exists and is writable, the file is truncated.
        /// </summary>
        CreateAlways = 2,
        /// <summary>
        /// Creates a new file, only if it does not already exist. If the specified file exists, <see cref="WinFile"/> throws
        /// an <see cref="IOException"/>.
        /// </summary>
        CreateNew = 1,
        /// <summary>
        /// Opens a file, always. If the specified file does not exist and is a valid path to a writable location, the file is created.
        /// </summary>
        OpenAlways = 4,
        /// <summary>
        /// Opens a file or device, only if it exists. If the specified file does not exist, <see cref="WinFile"/> throws
        /// a <see cref="FileNotFoundException"/>.
        /// </summary>
        OpenExisting = 3,
        /// <summary>
        /// Opens a file and truncates it so that its size is zero bytes, only if it exists. If the specified
        /// file does not exist, <see cref="WinFile"/> throws a <see cref="FileNotFoundException"/>.
        /// </summary>
        /// <remarks>The calling process must open the file with <see cref="WinFileShareMode.Write"/>
        /// bit set as part of the <c>shareMode</c> parameter.</remarks>
        TruncateExisting = 5
    }

    /// <summary>
    /// Represents options for a <see cref="WinFile"/>.
    /// </summary>
    [Flags]
    public enum WinFileOptions
    {
        /// <summary>
        /// Open or create the file with default options.
        /// </summary>
        None = 0,
        /// <summary>
        /// The file is being opened or created for a backup or restore operation. The system ensures that the
        /// calling process overrides file security checks when the process has Backup and Restore
        /// privileges.
        /// </summary>
        BackupSemantics = 0x02000000,
        /// <summary>
        /// The file is to be deleted immediately after all of its handles are closed,
        /// which includes the specified handle and any other open or duplicated handles.
        /// </summary>
        /// <remarks>If there are existing open handles to a file, the call fails unless they were
        /// all opened with the <see cref="WinFileShareMode.Delete"/> share mode. Subsequent open requests
        /// for the file fail, unless the <see cref="WinFileShareMode.Delete"/> share mode is specified.</remarks>
        DeleteOnClose = 0x04000000,
        /// <summary>
        /// The file or device is being opened with no system caching for data reads and writes.
        /// This option does not affect hard disk caching or memory mapped files.
        /// </summary>
        /// <remarks>There are strict requirements for successfully working with <see cref="WinFile"/>s
        /// using the <see cref="NoBuffering"/> flag, for details
        /// see <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/file-buffering">File Buffering.</see></remarks>
        NoBuffering = 0x20000000,
        /// <summary>
        /// The file data is requested, but it should continue to be located in remote storage. It should not
        /// be transported back to local storage. This option is for use by remote storage systems.
        /// </summary>
        NoRecall = 0x00100000,
        /// <summary>
        /// Normal reparse point processing will not occur; <see cref="WinFile"/> will attempt to open the reparse
        /// point. When a file is opened, a file handle is returned, whether or not the filter that
        /// controls the reparse point is operational.
        /// </summary>
        /// <remarks>This option cannot be used with <see cref="WinFileOpenMode.CreateAlways"/>.
        /// If the file is not a reparse point, then this flag is ignored. For more information, see the
        /// <see href="../docs/filesystem/reparsepoints.html">Reparse Points</see> page.</remarks>
        ReparsePoint = 0x00200000,
        ///// <summary>
        ///// The file or device is being opened or created for asynchronous I/O. 
        ///// </summary>
        //Overlapped = 0x40000000
        /// <summary>
        /// Access will occur according to POSIX rules. This includes allowing multiple files with names, differing
        /// only in case, for file systems that support that naming.
        /// </summary>
        /// <remarks>Use care when using this option, because files created with this flag may not be accessible
        /// by applications that are written for MS-DOS or 16-bit Windows.</remarks>
        PosixSemantics = 0x01000000,
        /// <summary>
        /// Access is intended to be random. The system can use this as a hint to optimize file caching.
        /// </summary>
        /// <remarks>This option has no effect if the file system does not support cached I/O and the <see cref="NoBuffering"/>
        /// option. For more information, see the <see href="../docs/filesystem/caching.html">
        /// Filesystem Caching</see> page.</remarks>
        RandomAccess = 0x10000000,
        /// <summary>
        /// The file or device is being opened with session awareness. If this flag is not specified, then per-session
        /// devices (such as a device using RemoteFX USB Redirection) cannot be opened by processes running in session 0. 
        /// </summary>
        /// <remarks>This flag has no effect for callers not in session 0.
        /// This flag is supported only on server editions of Windows.</remarks>
        [SupportedOSPlatform("windows6.2")]
        SessionAware = 0x00800000,
        /// <summary>
        /// Access is intended to be sequential from beginning to end. The system can use this as a hint to optimize file caching.
        /// This option should not be used if read-behind (that is, reverse scans) will be used.
        /// </summary>
        /// <remarks>This flag has no effect if the file system does not support cached I/O and
        /// the <see cref="NoBuffering"/> option. For more information, see the <see href="../docs/filesystem/caching.html">
        /// Filesystem Caching</see> page.</remarks>
        SequentialScan = 0x08000000,
        /// <summary>
        /// Write operations will not go through any intermediate cache, they will go directly to disk.
        /// </summary>
        /// <remarks>For more information, see the <see href="../docs/filesystem/caching.html">
        /// Filesystem Caching</see> page.</remarks>
        WriteThrough = unchecked((int)0x80000000)
    }
}
