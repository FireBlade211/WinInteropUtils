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
        /// <summary>
        /// Changes an attribute of the specified window. The function also sets a value at the specified offset in the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.
        /// The <see cref="SetWindowLongPtr(nint, int, nint)"/> function fails if the process that owns the window specified by the <paramref name="hWnd"/> parameter is at a higher process privilege in the UIPI
        /// hierarchy than the process the calling thread resides in. Windows XP/2000: The <see cref="SetWindowLongPtr(nint, int, nint)"/> function fails if the window specified by the hWnd parameter
        /// does not belong to the same process as the calling thread.</param>
        /// <param name="nIndex">The zero-based offset to the value to be set. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of a LONG_PTR. To set any other value, specify one of the <see cref="DefaultWindowLongPtrValues"/>.</param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns>If the function succeeds, the return value is the previous value of the specified offset.
        ///
        /// If the function fails, the return value is zero. To get extended error information, call <see cref="Marshal.GetLastPInvokeError"/>.
        ///
        /// If the previous value is zero and the function succeeds, the return value is zero, but the function does not clear the last error information.
        /// To determine success or failure, clear the last error information by calling SetLastError with 0, then call <see cref="SetWindowLongPtr(nint, int, nint)"/>. Function failure will be indicated by a return value of zero and a <see cref="Marshal.GetLastPInvokeError"/> result that is nonzero.</returns>
        /// <remarks>
        /// <para>Certain window data is cached, so changes you make using <see cref="SetWindowLongPtr(nint, int, nint)"/> will not take effect until you call the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-setwindowpos">SetWindowPos</see> function.</para>
        ///
        /// <para>If you use <see cref="SetWindowLongPtr(nint, int, nint)"/> with the <see cref="DefaultWindowLongPtrValues.WndProc"/> index to replace the window procedure, the window procedure must conform to the guidelines specified in the description
        /// of the <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-wndproc">WndProc</see> callback function.</para>
        ///
        /// <para>If you use <see cref="SetWindowLongPtr(nint, int, nint)"/> with the <see cref="DefaultWindowLongPtrValues.MessageResult"/> index to set the return value for a message processed by a dialog box procedure,
        /// the dialog box procedure should return <see langword="true"/> directly afterward. Otherwise, if you call any function that results
        /// in your dialog box procedure receiving a window message, the nested window message could overwrite the return value you set by using <see cref="DefaultWindowLongPtrValues.MessageResult"/>.</para>
        ///
        /// <para>Calling <see cref="SetWindowLongPtr(nint, int, nint)"/> with the <see cref="DefaultWindowLongPtrValues.WndProc"/> index creates a subclass of the window class used to create the window. An application
        /// can subclass a system class, but should not subclass a window class created by another process.The <see cref="SetWindowLongPtr(nint, int, nint)"/> function creates
        /// the window subclass by changing the window procedure associated with a particular window class, causing the system to call the new window procedure
        /// instead of the previous one.</para>
        ///
        /// Do not call <see cref="SetWindowLongPtr(nint, int, nint)"/> with the <see cref="DefaultWindowLongPtrValues.HWNDParent"/> index to change the parent of a child window. Instead, use the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-setparent">SetParent</see> function.
        /// <see cref="DefaultWindowLongPtrValues.HWNDParent"/> is used to change the owner of a top-level window, not the parent of a child window.
        /// A window can have either a parent or an owner, or neither, but never both simultaneously.
        /// 
        /// <para>Calling <see cref="SetWindowLongPtr(nint, int, nint)"/> to set the style on a progress bar will reset its position.</para>
        /// </remarks>
        [SupportedOSPlatform("windows5.0")] // Windows 2000
        public static nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong)
        {
            if (Environment.Is64BitProcess)
            {
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            }
            else
            {
                return new nint(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
            }
        }

        [LibraryImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static partial int SetWindowLong32(nint hWnd, int nIndex, int dwNewLong);

        [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static partial nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

        /// <summary>
        /// Retrieves information about the specified window. The function also retrieves the value at a specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be retrieved. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of a LONG_PTR.
        /// To retrieve any other value, specify one of the <see cref="DefaultWindowLongPtrValues"/>.</param>
        /// <returns>If the function succeeds, the return value is the requested value.
        ///
        /// If the function fails, the return value is zero.To get extended error information, call <see cref="Marshal.GetLastPInvokeError"/>.
        ///
        /// If <see cref="SetWindowLongPtr(nint, int, nint)"/> has not been called previously, <see cref="GetWindowLongPtr(nint, int)"/> returns zero for values in the extra window or class memory.</returns>
        /// <remarks>
        /// Reserve extra window memory by specifying a nonzero value in the cbWndExtra member of the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/ns-winuser-wndclassexa">WNDCLASSEX</see> structure used with the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-registerclassexa">RegisterClassEx</see> function.
        /// Note:
        /// The winuser.h header defines <see cref="GetWindowLongPtr(nint, int)"/> as an alias that automatically selects the ANSI or Unicode version of this function
        /// based on the definition of the UNICODE preprocessor constant. Mixing usage of the encoding-neutral alias with code that is not
        /// encoding-neutral can lead to mismatches that result in compilation or runtime errors. For more information, see <see href="https://learn.microsoft.com/en-us/windows/win32/intl/conventions-for-function-prototypes">Conventions for Function Prototypes</see>.
        /// </remarks>
        [SupportedOSPlatform("windows5.0")] // Windows 2000
        public static nint GetWindowLongPtr(nint hWnd, int nIndex)
        {
            if (nint.Size == 8)
            {
                return GetWindowLongPtr64(hWnd, nIndex);
            }
            else
            {
                return GetWindowLong32(hWnd, nIndex);
            }
        }

        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowLongPtrW")]
        private static partial nint GetWindowLongPtr64(nint hWnd, int nIndex);

        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowLongW")]
        private static partial nint GetWindowLong32(nint hWnd, int nIndex);

        [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
        private static partial nint SendMessageW(nint hWnd, uint Msg, nuint wParam, nint lParam);

        /// <summary>
        /// <para>Sends the specified message to a window or windows. The <see cref="SendMessage(nint, uint, nuint, nint)"/> function calls the window procedure for the specified window
        /// and does not return until the window procedure has processed the message.</para>
        ///
        /// To send a message and return immediately, use the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-sendmessagecallbacka">SendMessageCallback</see>
        /// or <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-sendnotifymessagea">SendNotifyMessage</see> function.
        /// To post a message to a thread's message queue and return immediately, use the
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-postmessagea">PostMessage</see>
        /// or <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-postthreadmessagea">PostThreadMessage</see> function.
        /// </summary>
        /// <param name="hWnd"><para>A handle to the window whose window procedure will receive the message. If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent
        /// to all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows;
        /// but the message is not sent to child windows.</para>
        ///
        /// Message sending is subject to UIPI. The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.</param>
        /// <param name="Msg">The message to be sent.
        ///
        /// For lists of the system-provided messages, see
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/winmsg/about-messages-and-message-queues">System-Defined Messages</see>.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
        /// <remarks>
        /// <para>When a message is blocked by UIPI the last error, retrieved with <see cref="Marshal.GetLastPInvokeError"/>, is set to 5 (access denied).</para>
        ///
        /// <para>Applications that need to communicate using HWND_BROADCAST should use the
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-registerwindowmessagea">RegisterWindowMessage</see> function to obtain a
        /// unique message for inter-application communication.</para>
        ///
        /// <para>The system only does marshalling for system messages (those in the range 0 to (WM_USER-1)). To send other messages (those >= WM_USER) to another process,
        /// you must do custom marshalling.</para>
        ///
        /// <para>If the specified window was created by the calling thread, the window procedure is called immediately as a subroutine.
        /// If the specified window was created by a different thread, the system switches to that thread and calls the appropriate window procedure.
        /// Messages sent between threads are processed only when the receiving thread executes message retrieval code.The sending thread is blocked until
        /// the receiving thread processes the message. However, the sending thread will process incoming nonqueued messages while waiting for its message to be processed.
        /// To prevent this, use <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-sendmessagetimeouta">SendMessageTimeout</see>
        /// with SMTO_BLOCK set. For more information on non-queued messages, see
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/winmsg/about-messages-and-message-queues">Messages and Message Queues</see>.</para>
        ///
        /// An accessibility application can use <see cref="SendMessage(nint, uint, nuint, nint)"/> to send WM_APPCOMMAND messages to the shell to launch applications.
        /// This functionality is not guaranteed to work for other types of applications.
        /// </remarks>
        [SupportedOSPlatform("windows5.0")] // Windows 2000
        public static nint SendMessage(nint hWnd, uint Msg, nuint wParam, nint lParam)
        {
            return SendMessageW(hWnd, Msg, wParam, lParam);
        }

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
        public static bool IsGUIThread(bool bConvert = false)
        {
            return IsGuiThread(bConvert);
        }

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


        [StructLayout(LayoutKind.Sequential)]
        private struct POINT(int x, int y)
        {
            public int X = x;
            public int Y = y;

            public readonly override string ToString() => $"({X}, {Y})";
        }

        [LibraryImport("user32.dll")]
        private static partial nint WindowFromPoint(POINT Point);

        /// <summary>
        /// Retrieves a handle to the window that is under the specified point.
        /// </summary>
        /// <param name="point">The point to be checked.</param>
        /// <returns>A handle to the window that is under the point. If no window exists at the given point, the return value is <see langword="null"/>.
        /// If the point is over a static text control, the return value is a handle to the window under the static text control.</returns>
        [SupportedOSPlatform("windows5.0")]
        public static nint? GetWindowAtPoint(Point point)
        {
            var hwnd = WindowFromPoint(new POINT(point.X, point.Y));

            return hwnd != nint.Zero ? hwnd : null;
        }

        /// <summary>
        /// Retrieves a handle to the window that is under the specified point.
        /// </summary>
        /// <param name="x">The X position to be checked.</param>
        /// <param name="y">The Y position to be checked.</param>
        /// <returns>A handle to the window that is under the point. If no window exists at the given point, the return value is <see langword="null"/>.
        /// If the point is over a static text control, the return value is a handle to the window under the static text control.</returns>
        [SupportedOSPlatform("windows5.0")]
        public static nint? GetWindowAtPoint(int x, int y)
        {
            var hwnd = WindowFromPoint(new POINT(x, y));

            return hwnd != nint.Zero ? hwnd : null;
        }

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool IsZoomed(nint hWnd);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool IsIconic(nint hWnd);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool IsWindowVisible(nint hWnd);

        /// <summary>
        /// Determines the state of the window represented by the handle <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd">The window handle to check.</param>
        /// <returns>The state of the window.</returns>
        [SupportedOSPlatform("windows5.0")]
        public static WindowState GetWindowState(nint hWnd)
        {
            if (!IsWindowVisible(hWnd))
                return WindowState.Hidden;
            else if (IsZoomed(hWnd))
                return WindowState.Zoomed;
            else if (IsIconic(hWnd))
                return WindowState.Iconic;
            else
                return WindowState.Normal;
        }
    }

    /// <summary>
    /// Defines default index values for <see cref="User32.SetWindowLongPtr(nint, int, nint)"/> and <see cref="User32.GetWindowLongPtr(nint, int)"/>.
    /// </summary>
    public static class DefaultWindowLongPtrValues
    {
        /// <summary>
        /// Gets or sets a new extended window style. (GWL_EXSTYLE)
        /// </summary>
        public const int ExStyle = -20;

        /// <summary>
        /// Gets or sets a new application instance handle. (GWLP_HINSTANCE)
        /// </summary>
        public const int AppHInstance = -6;

        /// <summary>
        /// Gets or sets a new owner for a top-level window. (GWLP_HWNDPARENT)
        /// </summary>
        public const int HWNDParent = -8;

        /// <summary>
        /// Gets or sets a new identifier of the child window. The window cannot be a top-level window. (GWLP_ID)
        /// </summary>
        public const int ID = -12;

        /// <summary>
        /// Gets or sets a new <see href="https://learn.microsoft.com/en-us/windows/desktop/winmsg/window-styles">window style</see>. (GWL_STYLE)
        /// </summary>
        public const int Style = -16;

        /// <summary>
        /// Gets or sets the user data associated with the window. This data is intended for use by the application that created the window. Its value is initially zero. (GWLP_USERDATA)
        /// </summary>
        public const int UserData = -21;

        /// <summary>
        /// Gets or sets a new address for the window procedure. (GWLP_WNDPROC)
        /// </summary>
        public const int WndProc = -4;

        /// <summary>
        /// Gets or sets the return value of a message processed in the dialog box procedure. (DWLP_MSGRESULT)
        /// </summary>
        public const int MessageResult = 0;
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

    /// <summary>
    /// Defines states a window can be in.
    /// </summary>
    public enum WindowState
    {
        /// <summary>
        /// The window is in the normal state.
        /// </summary>
        Normal,
        /// <summary>
        /// The window is zoomed (maximized).
        /// </summary>
        Zoomed,
        /// <summary>
        /// The window is iconic (minimized).
        /// </summary>
        Iconic,
        /// <summary>
        /// The window is hidden.
        /// </summary>
        Hidden,
        /// <summary>
        /// The window is zoomed (maximized).
        /// </summary>
        Maximized = Zoomed,
        /// <summary>
        /// The window is iconic (minimized).
        /// </summary>
        Minimized = Iconic,
        /// <summary>
        /// The window is hidden.
        /// </summary>
        Invisible = Hidden,
        /// <summary>
        /// The window is in the normal state.
        /// </summary>
        Visible = Normal
    }
}
