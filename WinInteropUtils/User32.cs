using FireBlade.WinInteropUtils.ComponentObjectModel;
using FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static FireBlade.WinInteropUtils.ComponentObjectModel.COM;

namespace FireBlade.WinInteropUtils
{
    /// <summary>
    /// Declares P/Invoke functions from <c>User32.dll</c>.
    /// </summary>
    public static partial class User32
    {
        [LibraryImport("user32.dll", EntryPoint = "IsGUIThread")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool IsGuiThread([MarshalAs(UnmanagedType.Bool)] bool bConvert);

        /// <summary>
        /// Determines whether the calling thread is already a GUI thread. It can also optionally convert the thread to a GUI thread (<paramref name="bConvert"/>).
        /// </summary>
        /// <param name="bConvert">If <see langword="true"/> and the thread is not a GUI thread, convert the thread to a GUI thread.</param>
        /// <returns><para>The function returns a nonzero value in the following situations:</para>
        ///
        /// <para>- If the calling thread is already a GUI thread.</para>
        /// <para>- If <paramref name="bConvert"/> is <see langword="true"/> and the function successfully converts the thread to a GUI thread.</para>
        /// Otherwise, the function returns zero.
        /// If <paramref name="bConvert"/> is <see langword="true"/> and the function cannot successfully convert the thread to a GUI thread, IsGUIThread
        /// returns ERROR_NOT_ENOUGH_MEMORY.</returns>
        [SupportedOSPlatform("windows5.1")] // Windows XP
        public static bool IsGUIThread(bool bConvert = false) => IsGuiThread(bConvert);

        [StructLayout(LayoutKind.Sequential)]
        internal struct TITLEBARINFO
        {
            public int cbSize;
            public RECT rcTitleBar;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public uint[] rgstate;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left, Top, Right, Bottom;

            public readonly Rectangle ToRectangle() =>
                new(Left, Top, Right - Left, Bottom - Top);
        }

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetTitleBarInfo")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetTitleBarInfoW(nint hwnd, ref TITLEBARINFO pti);

        /// <summary>
        /// Retrieves information about the specified title bar.
        /// </summary>
        /// <param name="hWnd">A handle to the title bar whose information is to be retrieved.</param>
        /// <returns><para>If the function succeeds, the return value is non-<see langword="null"/>.</para>
        ///
        /// If the function fails, the return value is <see langword="null"/>. To get extended error information, call <see cref="Marshal.GetLastPInvokeError"/>.</returns>
        [SupportedOSPlatform("windows5.0")]
        public static TitleBarInfo? GetTitleBarInfo(nint hWnd)
        {
            var tbi = new TITLEBARINFO
            {
                cbSize = Marshal.SizeOf<TITLEBARINFO>(),
                rgstate = new uint[6]
            };

            if (!GetTitleBarInfoW(hWnd, ref tbi))
                return null;

            var info = new TitleBarInfo
            {
                Position = tbi.rcTitleBar.ToRectangle(),
                ElementStates = []
            };

            for (int i = 0; i < 6; i++)
            {
                info.ElementStates[(TitleBarElement)i] = (TitleBarElementState)tbi.rgstate[i];
            }

            return info;
        }

        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        private static partial uint RegisterWindowMessageW(string lpString);

        /// <summary>
        /// Defines a new window message that is guaranteed to be unique throughout the system. The message value can be used when sending or posting messages.
        /// </summary>
        /// <param name="msg">The message to be registered.</param>
        /// <returns><para>If the message is successfully registered, the return value is a message identifier in the range <c>0xC000</c> through <c>0xFFFF</c>.</para>
        /// <para>If the function fails, the return value is zero. To get extended error information, call <see cref="Marshal.GetLastPInvokeError"/>.</para>
        /// </returns>
        public static uint RegisterWindowMessage(string msg) => RegisterWindowMessageW(msg);
    }

    /// <summary>
    /// Stores data about a window's title bar.
    /// </summary>
    /// <remarks>
    /// This data does not update live; it stays at the values it was at when <see cref="User32.GetTitleBarInfo(nint)"/> was called.
    /// </remarks>
    public class TitleBarInfo
    {
        /// <summary>
        /// Gets the position of the title bar (at the time <see cref="User32.GetTitleBarInfo(nint)"/> was called).
        /// </summary>
        public Rectangle Position { get; internal init; }
        /// <summary>
        /// Gets the states of title bar elements (at the time <see cref="User32.GetTitleBarInfo(nint)"/> was called).
        /// </summary>
        public Dictionary<TitleBarElement, TitleBarElementState> ElementStates { get; internal init; }

#pragma warning disable CS8618
        internal TitleBarInfo() { }
#pragma warning restore
    }

    /// <summary>
    /// Defines elements on a title bar.
    /// </summary>
    public enum TitleBarElement
    {
        /// <summary>
        /// The title bar itself.
        /// </summary>
        TitleBar = 0,
        /// <summary>
        /// A reserved value. Do not use.
        /// </summary>
        [Obsolete("TitleBarElement.Reserved is a reserved value. Do not use.")]
        Reserved = 1,
        /// <summary>
        /// The minimize (-) button.
        /// </summary>
        MinimizeButton = 2,
        /// <summary>
        /// The maximize/restore button.
        /// </summary>
        MaximizeButton = 3,
        /// <summary>
        /// The help (?) button.
        /// </summary>
        HelpButton = 4,
        /// <summary>
        /// The close (X) button.
        /// </summary>
        CloseButton = 5
    }

    /// <summary>
    /// Defines the states of a title bar element.
    /// </summary>
    [Flags]
    public enum TitleBarElementState
    {
        /// <summary>
        /// The element can accept the focus.
        /// </summary>
        Focusable = 0x00100000,
        /// <summary>
        /// The element is invisible.
        /// </summary>
        Invisible = 0x00008000,
        /// <summary>
        /// The element has no visible representation.
        /// </summary>
        Offscreen = 0x00010000,
        /// <summary>
        /// The element is unavailable.
        /// </summary>
        Unavailable = 0x00000001,
        /// <summary>
        /// The element is in the pressed state.
        /// </summary>
        Pressed = 0x00000008
    }
}
