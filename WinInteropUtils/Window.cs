using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FireBlade.WinInteropUtils
{
    /// <summary>
    /// Provides methods for managing Win32 windows.
    /// </summary>
    public partial class Window : IHandle
    {
        private nint _hwnd;

        /// <summary>
        /// Gets the handle (HWND) of the window.
        /// </summary>
        public nint Handle => _hwnd;

        internal static Window FromHandleInternal(nint hwnd) => new Window
        {
            _hwnd = hwnd
        };

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("user32.dll")]
        private static partial bool IsWindow(nint hwnd);

        /// <summary>
        /// Creates a <see cref="Window"/> from an existing handle.
        /// </summary>
        /// <param name="hwnd">The handle of the window.</param>
        /// <returns>A <see cref="Window"/> corresponding to the handle specified by <paramref name="hwnd"/>,
        /// or <see langword="null"/> if the window does not exist.</returns>
        public static Window? FromHandle(nint hwnd)
        {
            if (IsWindow(hwnd))
            {
                return FromHandleInternal(hwnd);
            }

            return null;
        }

        /// <summary>
        /// Determines whether the specified window handle identifies an existing window.
        /// </summary>
        /// <param name="hwnd">A handle to the window to be tested.</param>
        /// <returns><see langword="true"/> if the window handle identifies an existing window; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// A thread should not use <see cref="Exists(nint)"/> for a window that it did not create because the window could be
        /// destroyed after this function was called. Further, because window handles are recycled the handle could
        /// even point to a different window.
        /// </remarks>
        public static bool Exists(nint hwnd) => IsWindow(hwnd);

        /// <summary>
        /// Converts the <see cref="Window"/> to a handle.
        /// </summary>
        /// <param name="wnd">The <see cref="Window"/> to convert.</param>
        public static implicit operator nint(Window wnd) => wnd.Handle;

        /// <summary>
        /// Converts the window handle to a <see cref="Window"/>.
        /// </summary>
        /// <param name="hwnd">The window handle to convert.</param>
        public static explicit operator Window(nint hwnd) => FromHandleInternal(hwnd);

        //[DllImport("user32.dll", CharSet = CharSet.Unicode)]
        //private static extern int GetWindowTextW(nint hwnd, StringBuilder lpString, int nMaxCount);

        //[DllImport("user32.dll", CharSet = CharSet.Unicode)]
        //private static extern int GetWindowTextLengthW(nint hwnd);

        // use this instead of the functions that way we can query text from windows in other apps
        private const int WM_GETTEXT = 0x000D;
        private const int WM_GETTEXTLENGTH = 0x000E;
        private const int WM_SETTEXT = 0x000C;

        [LibraryImport("user32.dll", EntryPoint = "SendMessageW", SetLastError = true)]
        private static partial nint SendMessageW(nint hWnd, uint Msg, nuint wParam, nint lParam);

        /// <summary>
        /// <para>Sends the specified message to the window. The <see cref="SendMessage(uint, nuint, nint)"/> function calls the window procedure for the specified window
        /// and does not return until the window procedure has processed the message.</para>
        ///
        /// To send a message and return immediately, use the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-sendmessagecallbacka">SendMessageCallback</see>
        /// or <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-sendnotifymessagea">SendNotifyMessage</see> function.
        /// To post a message to a thread's message queue and return immediately, use the
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-postmessagea">PostMessage</see>
        /// or <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-postthreadmessagea">PostThreadMessage</see> function.
        /// </summary>
        /// <param name="uMsg">The message to be sent.
        ///
        /// For lists of the system-provided messages, see
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/winmsg/about-messages-and-message-queues">System-Defined Messages</see>.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
        /// <remarks>
        /// <para>Message sending is subject to UIPI. The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.
        /// When a message is blocked by UIPI the last error, retrieved with <see cref="Marshal.GetLastPInvokeError"/>, is set to 5 (access denied).</para>
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
        /// An accessibility application can use <see cref="SendMessage(uint, nuint, nint)"/> to send WM_APPCOMMAND messages to the shell to launch applications.
        /// This functionality is not guaranteed to work for other types of applications.
        /// </remarks>
        [SupportedOSPlatform("windows5.0")] // Windows 2000
        public nint SendMessage(uint uMsg, nuint wParam, nint lParam) => SendMessageW(_hwnd, uMsg, wParam, lParam);

        /// <summary>
        /// <para>Sends the specified message to the window. The <see cref="SendMessage(uint, nuint, nint)"/> function calls the window procedure for the specified window
        /// and does not return until the window procedure has processed the message.</para>
        ///
        /// To send a message and return immediately, use the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-sendmessagecallbacka">SendMessageCallback</see>
        /// or <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-sendnotifymessagea">SendNotifyMessage</see> function.
        /// To post a message to a thread's message queue and return immediately, use the
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-postmessagea">PostMessage</see>
        /// or <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-postthreadmessagea">PostThreadMessage</see> function.
        /// </summary>
        /// <param name="uMsg">The message to be sent.
        ///
        /// For lists of the system-provided messages, see
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/winmsg/about-messages-and-message-queues">System-Defined Messages</see>.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
        /// <remarks>
        /// <para>Message sending is subject to UIPI. The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.
        /// When a message is blocked by UIPI the last error, retrieved with <see cref="Marshal.GetLastPInvokeError"/>, is set to 5 (access denied).</para>
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
        /// An accessibility application can use <see cref="SendMessage(uint, nuint, nint)"/> to send WM_APPCOMMAND messages to the shell to launch applications.
        /// This functionality is not guaranteed to work for other types of applications.
        /// </remarks>
        [SupportedOSPlatform("windows5.0")] // Windows 2000
        public nint SendMessage(uint uMsg, bool wParam, nint lParam) => SendMessage(uMsg, wParam ? 1u : 0u, lParam);

        /// <summary>
        /// <para>Sends the specified message to the window. The <see cref="SendMessage(uint, nuint, nint)"/> function calls the window procedure for the specified window
        /// and does not return until the window procedure has processed the message.</para>
        ///
        /// To send a message and return immediately, use the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-sendmessagecallbacka">SendMessageCallback</see>
        /// or <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-sendnotifymessagea">SendNotifyMessage</see> function.
        /// To post a message to a thread's message queue and return immediately, use the
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-postmessagea">PostMessage</see>
        /// or <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-postthreadmessagea">PostThreadMessage</see> function.
        /// </summary>
        /// <param name="uMsg">The message to be sent.
        ///
        /// For lists of the system-provided messages, see
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/winmsg/about-messages-and-message-queues">System-Defined Messages</see>.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
        /// <remarks>
        /// <para>Message sending is subject to UIPI. The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.
        /// When a message is blocked by UIPI the last error, retrieved with <see cref="Marshal.GetLastPInvokeError"/>, is set to 5 (access denied).</para>
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
        /// An accessibility application can use <see cref="SendMessage(uint, nuint, nint)"/> to send WM_APPCOMMAND messages to the shell to launch applications.
        /// This functionality is not guaranteed to work for other types of applications.
        /// </remarks>
        [SupportedOSPlatform("windows5.0")] // Windows 2000
        public nint SendMessage(uint uMsg, nuint wParam, bool lParam) => SendMessage(uMsg, wParam, lParam ? 1 : 0);

        /// <summary>
        /// <para>Sends the specified message to the window. The <see cref="SendMessage(uint, nuint, nint)"/> function calls the window procedure for the specified window
        /// and does not return until the window procedure has processed the message.</para>
        ///
        /// To send a message and return immediately, use the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-sendmessagecallbacka">SendMessageCallback</see>
        /// or <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-sendnotifymessagea">SendNotifyMessage</see> function.
        /// To post a message to a thread's message queue and return immediately, use the
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-postmessagea">PostMessage</see>
        /// or <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-postthreadmessagea">PostThreadMessage</see> function.
        /// </summary>
        /// <param name="uMsg">The message to be sent.
        ///
        /// For lists of the system-provided messages, see
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/winmsg/about-messages-and-message-queues">System-Defined Messages</see>.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
        /// <remarks>
        /// <para>Message sending is subject to UIPI. The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.
        /// When a message is blocked by UIPI the last error, retrieved with <see cref="Marshal.GetLastPInvokeError"/>, is set to 5 (access denied).</para>
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
        /// An accessibility application can use <see cref="SendMessage(uint, nuint, nint)"/> to send WM_APPCOMMAND messages to the shell to launch applications.
        /// This functionality is not guaranteed to work for other types of applications.
        /// </remarks>
        [SupportedOSPlatform("windows5.0")] // Windows 2000
        public nint SendMessage(uint uMsg, bool wParam, bool lParam) => SendMessage(uMsg, wParam ? 1u : 0u, lParam ? 1 : 0);

        /// <summary>
        /// Gets or sets the text of the window. For top-level windows, this is usually the window title. For controls, this is the text displayed on the control.
        /// </summary>
        public string Text
        {
            get
            {
                int len = (int)SendMessage(WM_GETTEXTLENGTH, 0, 0) + 1; // include null terminator
                char[] buffer = new char[len];

                int copiedChars = 0;

                unsafe
                {
                    fixed (char* p = buffer)
                    {
                        copiedChars = (int)SendMessage(WM_GETTEXT, (nuint)len, (nint)p);
                    }
                }

                if (copiedChars == 0)
                {
                    HRESULT hr = (HRESULT)Marshal.GetHRForLastWin32Error();
                    var ex = Marshal.GetExceptionForHR((int)hr);

                    if (ex != null) throw ex;
                    else return string.Empty;
                }
                else
                {
                    return new string(buffer);
                }
            }
            set
            {
                unsafe
                {
                    fixed (char* p = (value + "\0"))
                    {
                        SendMessage(WM_SETTEXT, 0, (nint)p);
                    }
                }
            }
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
        /// Gets the state of the window.
        /// </summary>
        /// <returns>The state of the window.</returns>
        [SupportedOSPlatform("windows5.0")]
        public WindowState State
        {
            get
            {
                if (!IsWindowVisible(_hwnd))
                    return WindowState.Hidden;
                else if (IsZoomed(_hwnd))
                    return WindowState.Zoomed;
                else if (IsIconic(_hwnd))
                    return WindowState.Iconic;
                else
                    return WindowState.Normal;
            }
        }

        [LibraryImport("user32.dll")]
        private static partial nint WindowFromPoint(POINT Point);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT(int x, int y)
        {
            public int X = x;
            public int Y = y;

            public readonly override string ToString() => $"({X}, {Y})";
        }

        /// <summary>
        /// Retrieves the window that is under the specified point.
        /// </summary>
        /// <param name="pt">The point to be checked.</param>
        /// <returns>The window that is under the point. If no window exists at the given point, the return value is <see langword="null"/>.
        /// If the point is over a static text control, the return value is the window under the static text control.</returns>
        public static Window? FromPoint(Point pt)
        {
            var hwnd = WindowFromPoint(new POINT(pt.X, pt.Y));

            return hwnd != nint.Zero ? FromHandleInternal(hwnd) : null;
        }

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial nint GetWindowLongPtrW(nint hWnd, int nIndex);

        /// <summary>
        /// Retrieves information about the specified window. The function also retrieves the value at a specified offset into the extra window memory.
        /// </summary>
        /// <param name="nIndex">The zero-based offset to the value to be retrieved. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of a LONG_PTR.
        /// To retrieve any other value, specify one of the <see cref="WindowLongPtr"/>.</param>
        /// <returns>If the function succeeds, the return value is the requested value.
        ///
        /// If the function fails, the return value is zero. To get extended error information, call <see cref="Marshal.GetLastPInvokeError"/>.
        ///
        /// If <see cref="SetWindowLongPtr(int, nint)"/> has not been called previously, <see cref="GetWindowLongPtr(int)"/> returns zero for values
        /// in the extra window or class memory.</returns>
        /// <remarks>
        /// Reserve extra window memory by specifying a nonzero value in the cbWndExtra member
        /// of the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/ns-winuser-wndclassexa">WNDCLASSEX</see> structure used
        /// with the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-registerclassexa">RegisterClassEx</see> function.
        /// </remarks>
        [SupportedOSPlatform("windows5.0")] // Windows 2000
        public nint GetWindowLongPtr(int nIndex)
        {
            nint result = GetWindowLongPtrW(_hwnd, nIndex);

            if (result != 0) return result;
            else throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error())!;
        }

        [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static partial nint SetWindowLongPtrW(nint hWnd, int nIndex, nint dwNewLong);

        /// <summary>
        /// Changes an attribute of the specified window. The function also sets a value at the specified offset in the extra window memory.
        /// </summary>
        /// <param name="nIndex">The zero-based offset to the value to be set. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of a LONG_PTR. To set any other value, specify one of the <see cref="WindowLongPtr"/>.</param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns>If the function succeeds, the return value is the previous value of the specified offset.
        ///
        /// If the function fails, the return value is zero. To get extended error information, call <see cref="Marshal.GetLastPInvokeError"/>.
        ///
        /// If the previous value is zero and the function succeeds, the return value is zero, but the function does not clear the last error information.
        /// To determine success or failure, clear the last error information by calling SetLastError with 0, then call <see cref="SetWindowLongPtr(int, nint)"/>. Function failure will be indicated by a return value of zero and a <see cref="Marshal.GetLastPInvokeError"/> result that is nonzero.</returns>
        /// <remarks>
        /// <para>Certain window data is cached, so changes you make using <see cref="SetWindowLongPtr(int, nint)"/> will not take effect until you call the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-setwindowpos">SetWindowPos</see> function.</para>
        ///
        /// <para>If you use <see cref="SetWindowLongPtr(int, nint)"/> with the <see cref="WindowLongPtr.WndProc"/> index to replace the window procedure, the window procedure must conform to the guidelines specified in the description
        /// of the <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-wndproc">WndProc</see> callback function.</para>
        ///
        /// <para>If you use <see cref="SetWindowLongPtr(int, nint)"/> with the <see cref="WindowLongPtr.MessageResult"/> index to set the return value for a message processed by a dialog box procedure,
        /// the dialog box procedure should return <see langword="true"/> directly afterward. Otherwise, if you call any function that results
        /// in your dialog box procedure receiving a window message, the nested window message could overwrite the return value you set by using <see cref="WindowLongPtr.MessageResult"/>.</para>
        ///
        /// <para>Calling <see cref="SetWindowLongPtr(int, nint)"/> with the <see cref="WindowLongPtr.WndProc"/> index creates a subclass of the window class used to create the window. An application
        /// can subclass a system class, but should not subclass a window class created by another process.The <see cref="SetWindowLongPtr(int, nint)"/> function creates
        /// the window subclass by changing the window procedure associated with a particular window class, causing the system to call the new window procedure
        /// instead of the previous one.</para>
        ///
        /// Do not call <see cref="SetWindowLongPtr(int, nint)"/> with the <see cref="WindowLongPtr.HWNDParent"/> index to change the parent of a child window. Instead, use the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-setparent">SetParent</see> function.
        /// <see cref="WindowLongPtr.HWNDParent"/> is used to change the owner of a top-level window, not the parent of a child window.
        /// A window can have either a parent or an owner, or neither, but never both simultaneously.
        /// 
        /// <para>Calling <see cref="SetWindowLongPtr(int, nint)"/> to set the style on a progress bar will reset its position.</para>
        /// </remarks>
        [SupportedOSPlatform("windows5.0")] // Windows 2000
        public nint SetWindowLongPtr(int nIndex, nint dwNewLong)
        {
            nint result = SetWindowLongPtrW(_hwnd, nIndex, dwNewLong);

            if (result != 0) return result;
            else throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error())!;
        }

        [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool PostMessageW(nint hWnd, uint Msg, nuint wParam, nint lParam);

        /// <summary>
        /// Places (posts) a message in the message queue associated with the thread that created the specified window and returns without waiting
        /// for the thread to process the message. <!-- To post a message in the message queue associated with a thread, use the PostThreadMessage function. -->
        /// </summary>
        /// <param name="uMsg"><para>The message to be posted.</para>
        /// For lists of the system-provided messages,
        /// see <see href="https://learn.microsoft.com/en-us/windows/desktop/winmsg/about-messages-and-message-queues">System-Defined Messages</see>.
        /// </param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <remarks>
        /// <para>When a message is blocked by UIPI the last error is set to 5 (access denied).</para>
        /// 
        /// <para>Messages in a message queue are retrieved by calls to the <c>GetMessage</c> or <c>PeekMessage</c> function.</para>
        /// 
        /// <para>Applications that need to communicate using <c>HWND_BROADCAST</c> should use the <see cref="User32.RegisterWindowMessage(string)"/> function to
        /// obtain a unique message for inter-application communication.</para>
        /// <para>The system only does marshalling for system messages (those in the range 0 to (<c>WM_USER</c>-1)). To send other messages (those >= <c>WM_USER</c>) to
        /// another process, you must do custom marshalling.</para>
        /// 
        /// <para>If you send a message in the range below <c>WM_USER</c> to the asynchronous message
        /// functions (<see cref="PostMessage(uint, nuint, nint)"/>, SendNotifyMessage, and SendMessageCallback), its message parameters cannot
        /// include pointers. Otherwise, the operation will fail. The functions will return before the receiving thread has had a chance to process the
        /// message and the sender will free the memory before it is used.</para>
        /// 
        /// <para>Do not post the <c>WM_QUIT</c> message using <see cref="PostMessage(uint, nuint, nint)"/>; use the PostQuitMessage function.</para>
        /// 
        /// <para>An accessibility application can use <see cref="PostMessage(uint, nuint, nint)"/> to post <c>WM_APPCOMMAND</c> messages to the
        /// shell to launch applications. This functionality is not guaranteed to work for other types of applications.</para>
        /// 
        /// <para>There is a limit of 10,000 posted messages per message queue. This limit should be sufficiently large. If your application exceeds the limit, it should be redesigned to avoid consuming so many system resources. To adjust this limit,
        /// modify the following registry key:</para>
        /// 
        /// <code>
        /// HKEY_LOCAL_MACHINE
        ///     SOFTWARE
        ///         Microsoft
        ///             Windows NT
        ///                 CurrentVersion
        ///                     Windows
        ///                         USERPostMessageLimit
        /// </code>
        /// 
        /// 
        /// The minimum acceptable value is 4000.
        /// </remarks>
        /// <example>
        /// The following example shows how to post a private window message using the <see cref="PostMessage(uint, nuint, nint)"/> function. Assume
        /// you defined a private window message called <c>WM_COMPLETE</c>:
        /// <code>
        /// private const int WM_USER = 0x0400;
        /// private const int WM_COMPLETE = WM_USER + 0;
        /// </code>
        /// 
        /// You can post a message to the message queue associated with the thread that created the specified window as shown below:
        /// <code>
        /// var time = DateTime.Now;
        /// var window = Window.FromHandle(hwnd);
        /// 
        /// window.PostMessage(WM_COMPLETE, 0, (nint)time.ToFileTimeUtc());
        /// </code>
        /// </example>
        /// <exception cref="COMException">The 10,000 message quota has been exceeded. There is not enough qouta available to process this command.</exception>
        /// <exception cref="COMException">An unknown error occured.</exception>
        public void PostMessage(uint uMsg, nuint wParam, nint lParam)
        {
            bool result = PostMessageW(_hwnd, uMsg, wParam, lParam);

            if (!result) throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error())!;
        }

        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "GetParent")]
        private static partial nint _GetParent(nint hWnd);

        /// <summary>
        /// Gets the parent window of this <see cref="Window"/>.
        /// </summary>
        /// <remarks>To obtain a window's owner window, use <c>GetWindow</c> with the <c>GW_OWNER</c> flag. To obtain
        /// the parent window and not the owner, use <c>GetAncestor</c> with the <c>GA_PARENT</c> flag.</remarks>
        public Window? Parent
        {
            get
            {
                nint result = _GetParent(_hwnd);

                if (result != 0) return FromHandleInternal(result);
                else return null;
            }
        }

        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        private static partial nint CreateWindowExW(
            uint dwExStyle,
            string lpClassName,
            string lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            nint hWndParent,
            nint hMenu,
            nint hInstance,
            nint lpParam);

        /// <summary>
        /// Specifies the default location for a window.
        /// </summary>
        public static readonly Point WindowDefaultLocation = new Point(unchecked((int)0x80000000), unchecked((int)0x80000000));

        /// <summary>
        /// Specifies the default size for a window.
        /// </summary>
        public static readonly Size WindowDefaultSize = new Size(unchecked((int)0x80000000), unchecked((int)0x80000000));

        [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, EntryPoint = "GetModuleHandleW")]
        private static partial nint GetModuleHandle(string? lpModuleName);

        /// <summary>
        /// Creates an overlapped, pop-up, or child window with an extended window style.
        /// </summary>
        /// <param name="exStyle">The extended window style of the window being created.</param>
        /// <param name="className">A class name string or a class atom.</param>
        /// <param name="windowName">The window name. If the window style specifies a title bar, the window title specified in <paramref name="windowName"/> is displayed in the title bar. When using the function to create controls, such as buttons, check boxes, and static controls,
        /// use <paramref name="windowName"/> to specify the text of the control. When creating a static control with the <c>SS_ICON</c> style, use <paramref name="windowName"/> to specify the icon name or identifier. To specify an identifier, use the syntax "#num".</param>
        /// <param name="style">The style of the window being created. This parameter can be a bitwise OR (<c>|</c>) combination of constants from the normal <see cref="WindowStyles"/>, combined with any additional control style values.</param>
        /// <param name="location">The initial position of the window. For an overlapped or pop-up window, the location is relative to the top-left corner of the screen. For a child window, the location is relative to the top-left corner of the parent window's client area. If this parameter is set to <see cref="WindowDefaultLocation"/>, Windows automatically selects the position.</param>
        /// <param name="size">The initial size of the window, in device units. If this parameter is set to <see cref="WindowDefaultSize"/>, Windows automatically selects a size.</param>
        /// <param name="parent">The parent or owner window of the window being created. To create a child window or an owned window, supply a valid window. This parameter is optional for pop-up windows.</param>
        /// <param name="hMenu">A handle to a menu, or specifies a child-window identifier, depending on the window style. For an overlapped or pop-up window, <paramref name="hMenu"/> identifies the menu to be used with the window; it can be <see langword="null"/> if the class menu is to be used. For a child window, <paramref name="hMenu"/> specifies the child-window identifier, an integer value used by a dialog box control
        /// to notify its parent about events. The application determines the child-window identifier; it must be unique for all child windows with the same parent window.</param>
        /// <param name="param">Pointer to a value to be passed to the window through the <c>CREATESTRUCT</c> structure (lpCreateParams member) pointed to by the <c>lParam</c> parameter of the <c>WM_CREATE</c> message. This message is sent to the created window by this function before it returns. See the Memory Notes section in Remarks for more info.</param>
        /// <remarks>
        /// <para>The function sends <c>WM_NCCREATE</c>, <c>WM_NCCALCSIZE</c>, and <c>WM_CREATE</c> messages to the window being created.</para>
        /// 
        /// <para>If the created window is a child window, its default position is at the bottom of the Z-order. If the created window is a top-level window, its default position
        /// is at the top of the Z-order (but beneath all topmost windows unless the created window is itself topmost).</para>
        /// 
        /// <para>For information on controlling whether the Taskbar displays a button for the created window, see
        /// <see href="https://learn.microsoft.com/windows/desktop/shell/taskbar">Managing Taskbar Buttons</see>.</para>
        /// 
        /// <para>The <see cref="ExWindowStyles.NoActivate"/> value for <paramref name="exStyle"/> prevents foreground activation by the system.
        /// To prevent queue activation when the user clicks on the window, you must process the <c>WM_MOUSEACTIVATE</c> message appropriately.
        /// To bring the window to the foreground or to activate it programmatically, use <see cref="MoveToForeground"/> or <see cref="SetActive"/>.
        /// Returning <c>FALSE</c> (1) to <c>WM_NCACTIVATE</c> prevents the window from losing queue activation. However, the return value is ignored at
        /// activation time.</para>
        /// 
        /// <h1>Memory Notes</h1>
        /// If you set the <paramref name="param"/> parameter to a non-<see langword="null"/> value, the <c>lParam</c> parameter inside the <c>WM_CREATE</c> message
        /// will point to a 
        /// </remarks>
        public Window(ExWindowStyles exStyle, string className, string windowName, uint style, Point location, Size size, Window? parent = null,
            nint? hMenu = null, object? param = null)
        {
            nint p = 0;

            if (param != null)
            {
                GCHandle gch = GCHandle.Alloc(param);
                p = GCHandle.ToIntPtr(gch);
            }

            nint hwnd = CreateWindowExW((uint)exStyle, (className + "\0"), windowName, style, location.X, location.Y, size.Width, size.Height, parent?.Handle ?? 0, hMenu ?? 0,
                GetModuleHandle(null), p);

            if (hwnd != 0)
            {
                _hwnd = hwnd;
            }
            else
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        internal Window()
        {

        }

        private const uint WM_CLOSE = 0x0010;

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <remarks>
        /// <para>There is a difference between closing a window and destroying a window. Closing a window sends the window a <c>WM_CLOSE</c>
        /// message, which the app can then handle to prompt the user to, for example, save their changes. If the app handles <c>WM_CLOSE</c>
        /// the app can call the <see cref="Destroy"/> function, which actually destroys the window. The
        /// <see cref="WindowClass.DefaultWndProc(Window, uint, nuint, object)"/> automatically does this (without a prompt). Destroying the window
        /// is when the actual exiting happens. On destroy, the window receives a <c>WM_DESTROY</c> message.</para>
        /// 
        /// <para>For more details, see <see href="https://learn.microsoft.com/windows/win32/learnwin32/closing-the-window">Closing the Window</see>.</para>
        /// 
        /// To destroy a window, an application must use the <see cref="Destroy"/> function.
        /// </remarks>
        public void Close() => SendMessage(WM_CLOSE, 0, 0);

        /// <summary>
        /// Destroys the window.
        /// </summary>
        /// <returns><see langword="true"/> if the function succeeded; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>The function sends <c>WM_DESTROY</c> and <c>WM_NCDESTROY</c> messages to the window to deactivate it and remove the keyboard focus
        /// from it. The function also destroys the window's menu, destroys timers, removes clipboard ownership, and breaks the clipboard
        /// viewer chain (if the window is at the top of the viewer chain).</para>
        /// 
        /// <para>If the specified window is a parent or owner window, the <see cref="Destroy"/> function automatically destroys the associated child
        /// or owned windows when it destroys the parent or owner window. The function first destroys child or owned windows, and then it destroys
        /// the parent or owner window.</para>
        /// 
        /// <para><see cref="Destroy"/> also destroys modeless dialog boxes.</para>
        ///
        /// <para>A thread cannot use <see cref="Destroy"/> to destroy a window created by a different thread.</para>
        /// 
        /// <para>If the window being destroyed is a child window that does not have the
        /// <see cref="ExWindowStyles.NoParentNotify"/> style, a <c>WM_PARENTNOTIFY</c> message is sent to the parent.</para>
        /// </remarks>
        public bool Destroy() => DestroyWindow(_hwnd);

        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DestroyWindow(nint hWnd);

        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(nint hwnd, int nCmdShow);

        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindowAsync(nint hwnd, int nCmdShow);

        /// <summary>
        /// Gets or sets the visibility of the window. For more options, use the <see cref="Show"/> or <see cref="ShowAsync"/> functions.
        /// </summary>
        public bool Visible
        {
            get => IsWindowVisible(_hwnd);
            set
            {
                //value ? ShowWindow(_hwnd, (int)WindowShowCommand.Show) : ShowWindow(_hwnd, (int)WindowShowCommand.Hide)
                if (value)
                    ShowWindow(_hwnd, (int)WindowShowCommand.Show);
                else
                    ShowWindow(_hwnd, (int)WindowShowCommand.Hide);
            }
        }

        /// <summary>
        /// Sets the window's show state.
        /// </summary>
        /// <param name="cmd">Controls how the window is to be shown.</param>
        /// <returns><see langword="true"/> if the window was previously visible; otherwise, <see langword="false"/>.</returns>
        public bool Show(WindowShowCommand cmd) => ShowWindow(_hwnd, (int)cmd);

        /// <summary>
        /// Sets the window's show state asynchronously.
        /// </summary>
        /// <param name="cmd">Controls how the window is to be shown.</param>
        /// <returns><see langword="true"/> if the operation was successfully started; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Note that this funcion is not "asynchronous" in the C# sense, but that it immediatelly returns, unlike <see cref="Show"/>.</para>
        /// 
        /// This function posts a show-window event to the message queue of the given window. An application can use this function to avoid becoming
        /// nonresponsive while waiting for a nonresponsive application to finish processing a show-window event.
        /// </remarks>
        public bool ShowAsync(WindowShowCommand cmd) => ShowWindowAsync(_hwnd, (int)cmd);

        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool EnableWindow(nint hWnd, [MarshalAs(UnmanagedType.Bool)] bool bEnable);

        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool IsWindowEnabled(nint hWnd);

        /// <summary>
        /// Gets or sets the enabled state of the window.
        /// </summary>
        public bool Enabled
        {
            get => IsWindowEnabled(_hwnd);
            set => EnableWindow(_hwnd, value);
        }

        [LibraryImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetForegroundWindow(nint hWnd);

        /// <summary>
        /// Brings the thread that created the window into the foreground and activates the window. Keyboard input is directed to the window, and various
        /// visual cues are changed for the user. The system assigns a slightly higher priority to the thread that created the foreground window than it does to
        /// other threads.
        /// </summary>
        /// <returns><see langword="true"/> if the window was brought to the foreground; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>The system restricts which processes can set the foreground window. A process can set the foreground window by
        /// calling <see cref="MoveToForeground"/> only if:</para>
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         All of the following conditions are true:
        ///         <list type="bullet">
        ///             <item>The calling process belongs to a desktop application, not a UWP app or a Windows Store app designed for Windows 8 or 8.1.</item>
        ///             <item>The foreground process has not disabled calls to <see cref="MoveToForeground"/> by a previous call
        ///             to the <c>LockSetForegroundWindow</c> function.</item>
        ///             <item>No menus are active.</item>
        ///         </list>
        ///     </item>
        ///     <item>
        ///         Additionally, at least one of the following conditions is true:
        ///         <list type="bullet">
        ///             <item>The foreground lock time-out has expired.</item>
        ///             <item>The calling process is the foreground process.</item>
        ///             <item>The calling process was started by the foreground process.</item>
        ///             <item>There is currently no foreground window, and thus no foreground process.</item>
        ///             <item>The calling process received the last input event.</item>
        ///             <item>Either the foreground process or the calling process is being debugged.</item>
        ///         </list>
        ///     </item>
        /// </list>
        /// 
        /// <para>It is possible for a process to be denied the right to set the foreground window even if it meets these conditions.</para>
        /// 
        /// <para>An application cannot force a window to the foreground while the user is working with another window. Instead, Windows flashes the taskbar button
        /// of the window to notify the user.</para>
        /// </remarks>
        public bool MoveToForeground() => SetForegroundWindow(_hwnd);

        [LibraryImport("User32.dll", SetLastError = true)]
        private static partial nint SetActiveWindow(nint hWnd);

        /// <summary>
        /// Activates the window. The window must be attached to the calling thread's message queue.
        /// </summary>
        /// <returns>The window that was previously active.</returns>
        /// <exception cref="Win32Exception"/>
        /// <remarks>
        /// <para>The <see cref="SetActive"/> function activates a window, but not if the application is in the
        /// background. The window will be brought into the foreground (top of Z-Order) if its application is in the
        /// foreground when the system activates the window.</para>
        /// 
        /// <para>If the window was created by the calling thread, the active window status of the calling thread is set to the handle of the window. Otherwise, the active window
        /// status of the calling thread is set to <see langword="null"/>.</para>
        /// </remarks>
        public Window SetActive()
        {
            var hwnd = SetActiveWindow(_hwnd);

            if (hwnd == nint.Zero)
                throw new Win32Exception(Marshal.GetLastPInvokeError());

            return FromHandleInternal(hwnd);
        }

        [LibraryImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        /// Changes the size, position, and Z order of a child, pop-up, or top-level window. These windows are ordered according to their appearance
        /// on the screen. The topmost window receives the highest rank and is the first window in the Z order.
        /// </summary>
        /// <param name="pos">The new position of the window, in client coordinates.</param>
        /// <param name="size">The new size of the window, in pixels.</param>
        /// <param name="options">The window sizing and positioning flags.</param>
        /// <param name="insertAfter">The window to precede the positioned window in the Z order. This parameter must be a window handle or one of the <see cref="ZOrderWindows"/>.</param>
        /// <exception cref="Win32Exception"/>
        /// <remarks>
        /// 
        /// <para>As part of the Vista re-architecture, all services were moved off the interactive desktop into Session 0. Window and window manager operations are only
        /// effective inside a session and cross-session attempts to manipulate the window will fail. For more information, see
        /// <see href="https://learn.microsoft.com/en-us/previous-versions/aa480152(v=msdn.10)">The Windows Vista Developer Story: Application Compatibility Cookbook.</see></para>
        /// 
        /// <para>If you have changed certain window data using <see cref="SetWindowLongPtr(int, nint)"/>, you must
        /// call <see cref="SetPos(Point, Size, WindowSetPosOptions, Window?)"/> for the changes to take effect. Use the following combination for
        /// <paramref name="options"/>: <see cref="WindowSetPosOptions.NoMove"/> | <see cref="WindowSetPosOptions.NoSize"/>
        /// | <see cref="WindowSetPosOptions.NoZOrder"/> | <see cref="WindowSetPosOptions.FrameChanged"/>.</para>
        /// 
        /// <para>A window can be made a topmost window either by setting the <paramref name="insertAfter"/> parameter
        /// to <see cref="ZOrderWindows.TopMost"/> and ensuring that the <see cref="WindowSetPosOptions.NoZOrder"/> flag is not set, or by setting a
        /// window's position in the Z order so that it is above any existing topmost windows. When a non-topmost window is made topmost, its owned
        /// windows are also made topmost. Its owners, however, are not changed.</para>
        /// 
        /// <para>If neither the <see cref="WindowSetPosOptions.NoActivate"/> nor <see cref="WindowSetPosOptions.NoZOrder"/> flag is specified
        /// (that is, when the application requests that a window be simultaneously activated and its position in the Z order changed), the value specified
        /// in <paramref name="insertAfter"/> is used only in the following circumstances:
        /// <list type="bullet">
        ///     <item>Neither the <see cref="ZOrderWindows.TopMost"/> nor <see cref="ZOrderWindows.NoTopMost"/> flag is specified
        ///     in <paramref name="insertAfter"/>.</item>
        ///     <item>The window is not the active window.</item>
        /// </list>
        /// 
        /// </para>
        /// 
        /// <para>An application cannot activate an inactive window without also bringing it to the top of the Z order. Applications can change an activated
        /// window's position in the Z order without restrictions, or it can activate a window and then move it to the top of the topmost or non-topmost windows.</para>
        /// 
        /// <para>If a topmost window is repositioned to the bottom (<see cref="ZOrderWindows.Bottom"/>) of the Z order or after any non-topmost window, it is no
        /// longer topmost. When a topmost window is made non-topmost, its owners and its owned windows are also made non-topmost windows.</para>
        /// 
        /// <para>A non-topmost window can own a topmost window, but the reverse cannot occur. Any window (for example, a dialog box) owned by a topmost window is itself
        /// made a topmost window, to ensure that all owned windows stay above their owner.</para>
        /// 
        /// <para>If an application is not in the foreground, and should be in the foreground, it must call the <see cref="MoveToForeground"/> function.</para>
        /// 
        /// <para>To use <see cref="SetPos(Point, Size, WindowSetPosOptions, Window?)"/> to bring a window to the top, the process that
        /// owns the window must have <c>SetForegroundWindow</c> permission.</para>
        /// </remarks>
        public void SetPos(Point pos, Size size, WindowSetPosOptions options, Window? insertAfter = null)
        {
            if (!SetWindowPos(_hwnd, insertAfter?._hwnd ?? nint.Zero, pos.X, pos.Y, size.Width, size.Height, (uint)options))
                throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
    }

    /// <summary>
    /// Represents a window class.
    /// </summary>
#pragma warning disable CS8618
    public partial class WindowClass
    {
        /// <summary>
        /// A callback function, which you define in your application, that processes messages sent to a window.
        /// The <c>WndProc</c> name is a placeholder for the name of the function that you define in your application.
        /// </summary>
        /// <param name="window">The window that received the message.</param>
        /// <param name="uMsg">The message. For lists of the system-provided messages, see
        /// <see href="https://learn.microsoft.com/en-us/windows/win32/winmsg/about-messages-and-message-queues#system-defined-messages">System-defined messages</see>.</param>
        /// <param name="wParam">Additional message information. The contents of the <paramref name="wParam"/> parameter
        /// depends on the value of the <paramref name="uMsg"/> parameter.</param>
        /// <param name="lParam">Additional message information. The contents of the <paramref name="lParam"/> parameter
        /// depends on the value of the <paramref name="uMsg"/> parameter. This value is almost always a <see cref="nint"/>;
        ///  see exceptions in the Remarks.</param>
        /// <returns>The result of the message processing, depending on the message sent.</returns>
        /// <remarks>
        /// <para>The lParam is almost always a <see cref="nint"/>, but it can be different depending on the message received if WinInteropUtils
        /// automatically wraps it. The following messages are exceptions to this rule:
        /// 
        /// <list type="table">
        ///    <item>
        ///         <term><b>Message</b></term>
        ///         <description><b>LParam type</b></description>
        ///     </item>
        ///     
        ///     <item>
        ///         <term>WM_CREATE</term>
        ///         <description><see cref="CreateStruct"/></description>
        ///     </item>
        /// </list>
        /// </para>
        /// 
        /// <para>Your window procedure should forward any unprocessed messages
        /// to the <see cref="DefaultWndProc(Window, uint, nuint, object)"/> function.</para>
        /// 
        /// <para>If your application runs on a 32-bit version of Windows operating system, uncaught exceptions
        /// from the callback will be passed onto higher-level exception handlers of your application when available. The system
        /// then calls the unhandled exception filter to handle the exception prior to terminating the process. If the PCA is enabled,
        /// it will offer to fix the problem the next time you run the application.</para>
        /// 
        /// <para>However, if your application runs on a 64-bit version of Windows operating system or WOW64, you should be aware that
        /// a 64-bit operating system handles uncaught exceptions differently based on its 64-bit processor architecture, exception architecture,
        /// and calling convention. The following table summarizes all possible ways that a 64-bit Windows operating system or WOW64 handles uncaught exceptions:</para>
        /// 
        /// <list type="table">
        /// <item>
        /// <term>1</term>
        /// <description>The system suppresses any uncaught exceptions.</description>
        /// </item>
        /// <item>
        /// <term>2</term>
        /// <description>The system first terminates the process, and then the Program Compatibility Assistant (PCA) offers to fix it the next time
        /// you run the application. You can disable the PCA mitigation by adding a Compatibility section to the application manifest.</description>
        /// </item>
        /// <item>
        /// <term>3</term>
        /// <description>The system calls the exception filters but suppresses any uncaught exceptions when it leaves the callback scope,
        /// without invoking the associated handlers.</description>
        /// </item>
        /// </list>
        /// <para>The following table shows how a 64-bit version of the Windows operating system, and WOW64, handles uncaught exceptions.
        /// Notice that behavior type 2 applies only to the 64-bit version of the Windows 7 operating system and later:</para>
        /// <list type="table">
        /// <item>
        /// <term>Windows XP - WOW64</term>
        /// <description>3</description>
        /// </item>
        /// <item>
        /// <term>Windows XP - x64</term>
        /// <description>1</description>
        /// </item>
        /// <item>
        /// <term>Windows Vista - WOW64</term>
        /// <description>3</description>
        /// </item>
        /// <item>
        /// <term>Windows Vista - x64</term>
        /// <description>1</description>
        /// </item>
        /// <item>
        /// <term>Windows Vista SP1 - WOW64</term>
        /// <description>1</description>
        /// </item>
        /// <item>
        /// <term>Windows Vista SP1 - x64</term>
        /// <description>1</description>
        /// </item>
        /// <item>
        /// <term>Windows 7+ - WOW64</term>
        /// <description>1</description>
        /// </item>
        /// <item>
        /// <term>Windows 7+ - x64</term>
        /// <description>2</description>
        /// </item>
        /// </list>
        /// <para>
        /// > [!NOTE]
        /// > On Windows 7 with SP1 (32-bit, 64-bit, or WOW64), the system calls the unhandled exception filter to handle the exception
        /// > prior to terminating the process. If the Program Compatibility Assistant (PCA) is enabled, then it will offer to fix the problem
        /// > the next time you run the application.
        /// </para>
        /// <para>
        /// If you need to handle exceptions in your application, you can use structured exception handling to do so. For more information
        /// on how to use structured exception
        /// handling, see <see href="https://learn.microsoft.com/en-us/windows/win32/debug/structured-exception-handling">Structured exception handling</see>.
        /// </para>
        /// </remarks>
        public delegate nint WndProc(Window window, uint uMsg, nuint wParam, object lParam);

        private delegate nint WndProcInternal(nint hwnd, uint uMsg, nuint wParam, nint lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WNDCLASSEX
        {
            public uint cbSize;
            public uint style;
            public nint lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public nint hInstance;
            public nint hIcon;
            public nint hCursor;
            public nint hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string? lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
            public nint hIconSm;
        }

        /// <summary>
        /// A class atom that uniquely identifies the class registered.
        /// </summary>
        /// <remarks>This value will be 0 before the <see cref="Register"/> function is called.</remarks>
        public ushort Atom { get; private set; }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern ushort RegisterClassExW(ref WNDCLASSEX unnamedParam1);

        [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, EntryPoint = "GetModuleHandleW")]
        private static partial nint GetModuleHandle(string? lpModuleName);

        [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static unsafe partial bool GetClassInfoExW(nint hInstance, string? lpClassName, WNDCLASSEX* lpWndClass);

        [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static partial ushort GlobalFindAtomW(string lpString);

        private const int WM_CREATE = 0x0001;

        [StructLayout(LayoutKind.Sequential)]
        private struct CREATESTRUCTW
        {
            public nint lpCreateParams;
            public nint hInstance;
            public nint hMenu;
            public nint hwndParent;
            public int cy;
            public int cx;
            public int y;
            public int x;
            public nint style;
            public nint lpszName;
            public nint lpszClass;
            public uint dwExStyle;
        }

        /// <summary>
        /// Registers the window class for subsequent use in calls to the <see cref="Window.Window(ExWindowStyles, string, string, uint, Point, Size, Window?, nint?, object?)"/>
        /// function.
        /// </summary>
        public void Register()
        {
            WNDCLASSEX wcex = new WNDCLASSEX();
            wcex.cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>();

            WndProcInternal newProc = nint (nint hwnd, uint uMsg, nuint wParam, nint lParam) =>
            {
                switch (uMsg)
                {
                    case WM_CREATE:
                        var cs = Marshal.PtrToStructure<CREATESTRUCTW>(lParam);
                        var mCs = new CreateStruct
                        {
                            Text = Marshal.PtrToStringUni(cs.lpszName) ?? string.Empty,
                            HMenu = cs.hMenu,
                            CreateParam = cs.lpCreateParams != 0 ? GCHandle.FromIntPtr(cs.lpCreateParams).Target : null,
                            Parent = Window.FromHandleInternal(cs.hwndParent),
                            Style = cs.style,
                            Class = FromExisting(Marshal.PtrToStringUni(cs.lpszClass) ?? string.Empty),
                            ExStyle = (ExWindowStyles)cs.dwExStyle,
                            HInstance = cs.hInstance,
                            Location = new Point(cs.x, cs.y),
                            Size = new Size(cs.cx, cs.cy),
                            _createParams = cs.lpCreateParams
                        };

                        var result = WindowProcedure(Window.FromHandleInternal(hwnd), uMsg, wParam, lParam);

                        if (cs.lpCreateParams != nint.Zero)
                            GCHandle.FromIntPtr(cs.lpCreateParams).Free();

                        return result;
                }
                ;

                return WindowProcedure(Window.FromHandleInternal(hwnd), uMsg, wParam, lParam);
            };

            var wndProc = Marshal.GetFunctionPointerForDelegate(newProc);
            NativeCallbackRoots.Add(newProc);
            NativeCallbackRoots.Add(WindowProcedure);

            wcex.lpfnWndProc = wndProc;
            wcex.lpszMenuName = MenuName;
            wcex.lpszClassName = ClassName;
            wcex.cbClsExtra = ExtraSize;
            wcex.cbWndExtra = ExtraWindowSize;
            wcex.hbrBackground = BackgroundBrush != null ? (int)BackgroundBrush : 0;
            wcex.hIcon = ClassIcon?.Handle ?? nint.Zero;
            wcex.style = (uint)ClassStyle;
            wcex.hInstance = Instance != null ? (nint)Instance : GetModuleHandle(null);
            wcex.hIconSm = SmallClassIcon?.Handle ?? nint.Zero;

            var atom = RegisterClassExW(ref wcex);

            if (atom == 0)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            Atom = atom;
        }

        /// <summary>
        /// The class style(s). This member can be any combination of the <see cref="WinInteropUtils.ClassStyle"/>
        /// enumeration.
        /// </summary>
        public ClassStyle ClassStyle { get; set; }

        /// <summary>
        /// The window procedure. The system calls this function whenever a message is received.
        /// </summary>
        public WndProc WindowProcedure { get; set; }

        /// <summary>
        /// The number of extra bytes to allocate following the window-class structure. The system initializes the bytes to zero.
        /// </summary>
        public int ExtraSize { get; set; }

        /// <summary>
        /// The value that should be assigned to <see cref="ExtraWindowSize"/> if the application uses <see cref="WindowClass"/> to register a dialog box created by using the
        /// <c>CLASS</c> directive in the resource file.
        /// </summary>
        public const int DlgWindowExtra = 30;

        /// <summary>
        /// The number of extra bytes to allocate following the window instance. The system initializes the bytes to zero. If an application
        /// uses <see cref="WindowClass"/> to register a dialog box created by using the <c>CLASS</c> directive in the resource file, it must
        /// set this member to <see cref="DlgWindowExtra"/>.
        /// </summary>
        public int ExtraWindowSize { get; set; }

        /// <summary>
        /// Override the instance handle (HINSTANCE) where the window procedure is defined in.
        /// </summary>
        public nint? Instance { get; set; }

        /// <summary>
        /// The class icon. This member must be an icon resource. If this member is <see langword="null"/>, the
        /// system provides a default icon.
        /// </summary>
        public Icon? ClassIcon { get; set; }

        /// <summary>
        /// The small class icon. This member must be an icon resource. If this member is <see langword="null"/>, the
        /// system provides a default icon.
        /// </summary>
        public Icon? SmallClassIcon { get; set; }

        /// <summary>
        /// The class background brush. If this member is <see langword="null"/>, an application must paint
        /// its own background whenever it is requested to paint in its client area.
        /// </summary>
        public SystemColorId? BackgroundBrush { get; set; }

        /// <summary>
        /// The resource name of the class menu, as the name appears in the resource file. If you use an integer to identify the menu,
        /// use the <see cref="Macros.MakeIntResource(int)"/> macro. If this member is <see langword="null"/>, windows belonging to this class have no default menu.
        /// </summary>
        public string? MenuName { get; set; }

        /// <summary>
        /// A string representing the class name. The maximum length for <see cref="ClassName"/> is 256. If <see cref="ClassName"/> is
        /// greater than the maximum length, the <see cref="Register"/> function will fail.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Gets a <see cref="WindowClass"/> instance for an existing window class.
        /// </summary>
        /// <param name="className">The name of the class to get.</param>
        /// <returns>The <see cref="WindowClass"/> corresponding to the specified <paramref name="className"/>.</returns>
        public static WindowClass FromExisting(string className)
        {
            WNDCLASSEX wcex = new WNDCLASSEX();

            unsafe
            {
                var result = GetClassInfoExW(GetModuleHandle(null), className, &wcex);

                if (!result)
                {
                    result = GetClassInfoExW(0, className, &wcex);

                    if (!result)
                    {
                        throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error())!;
                    }
                }

                var c = new WindowClass();
                c.Instance = wcex.hInstance;
                c.ClassName = wcex.lpszClassName ?? string.Empty;
                c.ExtraWindowSize = wcex.cbWndExtra;
                c.ExtraSize = wcex.cbClsExtra;
                c.ClassStyle = (ClassStyle)wcex.style;
                c.MenuName = wcex.lpszMenuName;
                c.BackgroundBrush = (SystemColorId)wcex.hbrBackground;
                
                if (wcex.lpfnWndProc != nint.Zero)
                    c.WindowProcedure = Marshal.GetDelegateForFunctionPointer<WndProc>(wcex.lpfnWndProc);

                if (wcex.hIcon != nint.Zero)
                    c.ClassIcon = Icon.FromHandle(wcex.hIcon);

                if (wcex.hIcon != nint.Zero)
                    c.SmallClassIcon = Icon.FromHandle(wcex.hIconSm);

                var atom = GlobalFindAtomW(className);
                if (atom != 0)
                {
                    c.Atom = atom;
                }

                return c;
            }
        }

        /// <summary>
        /// Gets a <see cref="WindowClass"/> instance for an existing window class.
        /// </summary>
        /// <param name="className">The name of the class to get.</param>
        /// <param name="hInstance">The handle to the module where the class is registered.</param>
        /// <returns>The <see cref="WindowClass"/> corresponding to the specified <paramref name="className"/>.</returns>
        /// <remarks>
        /// Note that the <see cref="Atom"/> property of the created <see cref="WindowClass"/> may be 0. This is not an error.
        /// Windows assigns an atom only when a class is registered through
        /// <see cref="Register"/>. If the class name already exists — such as a
        /// built-in window class or one registered internally by Windows (e.g.
        /// through <c>InitCommonControls</c>) — the system does not return an atom,
        /// and Windows doesn't provide an API to query it afterward.
        ///
        /// <para>Because Windows does not expose class atoms for pre-existing classes,
        /// the library cannot provide an atom value for them and <see cref="Atom"/>
        /// is set to 0.</para>
        /// </remarks>
        public static WindowClass FromExisting(string className, nint hInstance)
        {
            WNDCLASSEX wcex = new WNDCLASSEX();

            unsafe
            {
                var result = GetClassInfoExW(hInstance, className, &wcex);

                if (!result)
                {
                    throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error())!;
                }

                var c = new WindowClass();
                c.Instance = wcex.hInstance;
                c.ClassName = wcex.lpszClassName ?? string.Empty;
                c.ExtraWindowSize = wcex.cbWndExtra;
                c.ExtraSize = wcex.cbClsExtra;
                c.ClassStyle = (ClassStyle)wcex.style;
                c.MenuName = wcex.lpszMenuName;
                c.BackgroundBrush = (SystemColorId)wcex.hbrBackground;
                c.WindowProcedure = Marshal.GetDelegateForFunctionPointer<WndProc>(wcex.lpfnWndProc);
                c.ClassIcon = Icon.FromHandle(wcex.hIcon);
                c.SmallClassIcon = Icon.FromHandle(wcex.hIconSm);

                var atom = GlobalFindAtomW(className);
                if (atom != 0)
                {
                    c.Atom = atom;
                }

                return c;
            }
        }

        [LibraryImport("user32.dll", EntryPoint = "DefWindowProcW")]
        private static partial nint DefWindowProc(nint hWnd, uint Msg, nuint wParam, nint lParam);

        /// <summary>
        /// Calls the default window procedure to provide default processing for any window messages that an application does not process.
        /// This function ensures that every message is processed. DefWindowProc is called with the same parameters received by
        /// the window procedure.
        /// </summary>
        /// <param name="wnd">The window that received the message.</param>
        /// <param name="uMsg">The message.</param>
        /// <param name="wParam">Additional message information. The content of this parameter depends on the
        /// value of the <paramref name="uMsg"/> parameter.</param>
        /// <param name="lParam">Additional message information. The content of this parameter depends on the
        /// value of the <paramref name="uMsg"/> parameter.</param>
        /// <returns>The return value is the result of the message processing and depends on the message.</returns>
        public static nint DefaultWndProc(Window wnd, uint uMsg, nuint wParam, object lParam)
        {
            nint actualLparam = nint.Zero;
            List<nint> freehGlobal = [];

            if (lParam is nint n) actualLparam = n;
            else
            {
                switch (lParam)
                {
                    case CreateStruct cs:
                        var classNamePtr = Marshal.StringToHGlobalUni(cs.Class.ClassName);
                        var titlePtr = Marshal.StringToHGlobalUni(cs.Text);
                        freehGlobal.Add(classNamePtr);
                        freehGlobal.Add(titlePtr);

                        var c = new CREATESTRUCTW
                        {
                            cx = cs.Size.Width,
                            cy = cs.Size.Height,
                            x = cs.Location.X,
                            y = cs.Location.Y,
                            dwExStyle = (uint)cs.ExStyle,
                            hInstance = cs.HInstance,
                            hMenu = cs.HMenu,
                            hwndParent = cs.Parent?.Handle ?? nint.Zero,
                            lpCreateParams = cs._createParams,
                            lpszClass = classNamePtr,
                            lpszName = titlePtr,
                            style = (nint)cs.Style
                        };

                        nint ptr = Marshal.AllocHGlobal(Marshal.SizeOf<CREATESTRUCTW>());

                        try
                        {
                            Marshal.StructureToPtr(c, ptr, false);

                            actualLparam = ptr;
                            freehGlobal.Add(ptr);
                        }
                        catch { }

                        break;
                }
            }

            var result = DefWindowProc(wnd, uMsg, wParam, actualLparam);

            foreach (var ptr in freehGlobal)
                Marshal.FreeHGlobal(ptr);

            return result;
        }
    }
#pragma warning restore

    /// <summary>
    /// Represents class styles. Class styles define additional elements of the window class.
    /// </summary>
    [Flags]
    public enum ClassStyle
    {
        /// <summary>
        /// Aligns the window's client area on a byte boundary (in the x direction). This style
        /// affects the width of the window and its horizontal placement on the display.
        /// </summary>
        ByteAlignClient = 0x1000,
        /// <summary>
        /// Aligns the window on a byte boundary (in the x direction). This style affects the width
        /// of the window and its horizontal placement on the display.
        /// </summary>
        ByteAlignWindow = 0x2000,
        /// <summary>
        /// Allocates one device context to be shared by all windows in the class. Because window classes are
        /// process specific, it is possible for multiple threads of an application to create a window of the same
        /// class. It is also possible for the threads to attempt to use the device context simultaneously. When
        /// this happens, the system allows only one thread to successfully finish its drawing operation.
        /// </summary>
        ClassDeviceContext = 0x0040,
        /// <summary>
        /// Sends a double-click message to the window procedure
        /// when the user double-clicks the mouse while the cursor is within
        /// a window belonging to the class.
        /// </summary>
        DoubleClicks = 0x0008,
        /// <summary>
        /// Enables the drop shadow effect on a window. Typically, this is enabled for small, short-lived windows such as menus
        /// to emphasize their Z-order relationship to other windows. Windows created from a class with this style must be top-level windows;
        /// they may not be child windows.
        /// </summary>
        DropShadow = 0x00020000,
        /// <summary>
        /// Indicates that the window class is an application global class. For more information, see
        /// <see href="https://learn.microsoft.com/windows/win32/winmsg/about-window-classes#application-global-classes">Application Global Classes</see>.
        /// </summary>
        GlobalClass = 0x4000,
        /// <summary>
        /// Redraws the entire window if a movement or size adjustment changes the width of the client area.
        /// </summary>
        HRedraw = 0x0002,
        /// <summary>
        /// Disables Close on the window menu.
        /// </summary>
        NoClose = 0x0200,
        /// <summary>
        /// Allocates a unique device context for each window in the class.
        /// </summary>
        OwnDeviceContext = 0x0020,
        /// <summary>
        /// Sets the clipping rectangle of the child window to that of the parent window so that the child can
        /// draw on the parent. A window from a class containing the <see cref="ParentDC"/> style receives a regular
        /// device context from the system's cache of device contexts. It does not give the child the parent's device
        /// context or device context settings. Specifying <see cref="ParentDC"/> enhances an application's performance.
        /// </summary>
        ParentDC = 0x0080,
        /// <summary>
        /// Saves, as a bitmap, the portion of the screen image obscured by a window of this class. When the window is removed, the system uses the
        /// saved bitmap to restore the screen image, including other windows that were obscured. Therefore, the system does not send <c>WM_PAINT</c> messages
        /// to windows that were obscured if the memory used by the bitmap has not been discarded and if other screen actions have not invalidated the stored image.
        /// </summary>
        /// <remarks>
        /// This style is useful for small windows (for example, menus or dialog boxes) that are displayed briefly and then removed before other screen activity takes place.
        /// This style increases the time required to display the window, because the system must first allocate memory to store the bitmap.
        /// </remarks>
        SaveBits = 0x0800,
        /// <summary>
        /// Redraws the entire window if a movement or size adjustment changes the height of the client area.
        /// </summary>
        VRedraw = 0x0001
    }

    /// <summary>
    /// Represents a collection whose only purpose is to keep unmanaged callbacks alive for the entire execution of an application's lifetime.
    /// </summary>
    internal static class NativeCallbackRoots
    {
        private static readonly HashSet<Delegate> s_roots = new();

        public static void Add(Delegate d)
        {
            lock (s_roots)
                s_roots.Add(d);
        }
    }

    /// <summary>
    /// Specifies extended window styles.
    /// </summary>
    [Flags]
    public enum ExWindowStyles : long
    {
        /// <summary>
        /// The window accepts drag-drop files.
        /// </summary>
        AcceptFiles = 0x00000010L,
        /// <summary>
        /// Forces a top-level window onto the taskbar when the window is visible.
        /// </summary>
        AppWindow = 0x00040000L,
        /// <summary>
        /// The window has a border with a sunken edge.
        /// </summary>
        ClientEdge = 0x00000200L,
        /// <summary>
        /// Paints all descendants of a window in bottom-to-top painting order using double-buffering.
        /// Bottom-to-top painting order allows a descendent window to have translucency (alpha) and transparency
        /// (color-key) effects, but only if the descendent window also has the <see cref="Transparent"/> extended style
        /// set. Double-buffering allows the window and its descendents to be painted without flicker. This cannot be used
        /// if the window has a class style of <see cref="ClassStyle.OwnDeviceContext"/>, <see cref="ClassStyle.ClassDeviceContext"/>,
        /// or <see cref="ClassStyle.ParentDC"/>.
        /// </summary>
        [SupportedOSPlatform("windows5.1")]
        Composited = 0x02000000L,
        /// <summary>
        /// The title bar of the window includes a question mark. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the
        /// user then clicks a child window, the child receives a WM_HELP message. The child window should pass the message to the parent window procedure, which should
        /// call the <c>WinHelp</c> function using the <c>HELP_WM_HELP</c> command. The Help application displays a pop-up window that typically contains help for the child window.
        /// </summary>
        /// <remarks>
        /// <see cref="ContextHelp"/> cannot be used with
        /// the <see cref="WindowStyles.MaximizeBox"/> or <see cref="WindowStyles.MinimizeBox"/>
        /// styles.
        /// </remarks>
        ContextHelp = 0x00000400L,
        /// <summary>
        /// The window itself contains child windows that should take part in dialog box navigation. If this style is specified, the dialog manager recurses into children of this window
        /// when performing navigation operations such as handling the <c>TAB</c> key, an arrow key, or a keyboard mnemonic.
        /// </summary>
        ControlParent = 0x00010000L,
        /// <summary>
        /// The window has a double border; the window can, optionally, be created with a title bar by specifying the <see cref="WindowStyles.Caption"/> style in the <c>style</c> parameter.
        /// </summary>
        DlgModalFrame = 0x00000001L,
        /// <summary>
        /// The window is a layered window. This style cannot be used if the window has a class style of either <see cref="ClassStyle.OwnDeviceContext"/> or <see cref="ClassStyle.ClassDeviceContext"/>.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 8:</b> The <see cref="Layered"/> style is supported for top-level windows and child windows. Previous Windows versions support <see cref="Layered"/> only for top-level windows.
        /// </remarks>
        Layered = 0x00080000L,
        /// <summary>
        /// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the horizontal origin of the window is on the right edge. Increasing horizontal values advance to the left.
        /// </summary>
        LayoutRTL = 0x00400000L,
        /// <summary>
        /// The window has generic left-aligned properties. This is the default.
        /// </summary>
        Left = LTRReading | RightScrollBar,
        /// <summary>
        /// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present) is to the left of the client area.
        /// For other languages, the style is ignored.
        /// </summary>
        LeftScrollBar = 0x00004000L,
        /// <summary>
        /// The window text is displayed using left-to-right reading-order properties. This is the default.
        /// </summary>
        LTRReading = 0,
        /// <summary>
        /// The window is a MDI (Multi-Document Interface) child window.
        /// </summary>
        MdiChild = 0x00000040L,
        /// <summary>
        /// A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user
        /// minimizes or closes the foreground window. The window should not be activated through programmatic access or via keyboard navigation by accessible technology, such as Narrator.
        /// </summary>
        /// <remarks>
        /// To activate the window, use the <c>SetActiveWindow</c> or <c>SetForegroundWindow</c> function. The window does not appear on the taskbar by default. To force the window to appear
        /// on the taskbar, use the <see cref="AppWindow"/> extended style.
        /// </remarks>
        NoActivate = 0x08000000L,
        /// <summary>
        /// The window does not pass its window layout to its child windows.
        /// </summary>
        NoInheritLayout = 0x00100000L,
        /// <summary>
        /// The child window created with this style does not send the <c>WM_PARENTNOTIFY</c> message to its parent window when it is created or destroyed.
        /// </summary>
        NoParentNotify = 0x00000004L,
        /// <summary>
        /// The window does not render to a redirection surface. This is for windows that do not have visible content or that use mechanisms other than surfaces to provide their visual.
        /// </summary>
        NoRedirectionBitmap = 0x00200000L,
        /// <summary>
        /// The window is an overlapped window.
        /// </summary>
        OverlappedWindow = WindowEdge | ClientEdge,
        /// <summary>
        /// The window is palette window, which is a modeless dialog box that presents an array of commands.
        /// </summary>
        PaletteWindow = WindowEdge | ToolWindow | TopMost,
        /// <summary>
        /// The window has generic "right-aligned" properties. This depends on the window class. This style has an effect only if the shell language is Hebrew, Arabic, or another language
        /// that supports reading-order alignment; otherwise, the style is ignored.
        /// </summary>
        /// <remarks>
        /// Using the <see cref="Right"/> style for static or edit controls has the same effect as using the <c>SS_RIGHT</c> or <c>ES_RIGHT</c> style, respectively. Using this style with button
        /// controls has the same effect as using <c>BS_RIGHT</c> and <c>BS_RIGHTBUTTON</c> styles.
        /// </remarks>
        Right = 0x00001000L,
        /// <summary>
        /// The vertical scroll bar (if present) is to the right of the client area. This is the default.
        /// </summary>
        RightScrollBar = 0,
        /// <summary>
        /// If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties. For other languages, the style is ignored.
        /// </summary>
        RTLReading = 0x00002000L,
        /// <summary>
        /// The window has a three-dimensional border style intended to be used for items that do not accept user input.
        /// </summary>
        StaticEdge = 0x00020000L,
        /// <summary>
        /// The window is intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the dialog
        /// that appears when the user presses <c>ALT+TAB</c>. If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing <c>ALT+SPACE</c>.
        /// </summary>
        ToolWindow = 0x00000080L,
        /// <summary>
        /// The window should be placed above all non-topmost windows and should stay above them, even when the window is deactivated. To add or remove this style, use the <see cref="Window.SetPos"/> function.
        /// </summary>
        TopMost = 0x00000008L,
        /// <summary>
        /// The window should not be painted until siblings beneath the window (that were created by the same thread) have been painted. The window appears transparent because the bits of underlying sibling windows have already been painted.
        /// </summary>
        Transparent = 0x00000020L,
        /// <summary>
        /// The window has a border with a raised edge.
        /// </summary>
        WindowEdge = 0x00000100L
    }

    /// <summary>
    /// Represents the various standard styles that can be applied to a window (after the window has been created, these styles cannot be modified, except as documented).
    /// </summary>
    public static class WindowStyles
    {
        /// <summary>
        /// The window has a thin-line border.
        /// </summary>
        public const uint Border = 0x00800000;
        /// <summary>
        /// The window has a title bar.
        /// </summary>
        public const uint Caption = Border | DlgFrame;
        /// <summary>
        /// The window is a child window. A window with this style cannot have a menu bar. This style cannot
        /// be used with the <see cref="Popup"/> style.
        /// </summary>
        public const uint Child = 0x40000000;
        /// <summary>
        /// The window is a child window. A window with this style cannot have a menu bar. This style cannot
        /// be used with the <see cref="Popup"/> style.
        /// </summary>
        public const uint ChildWindow = Child;
        /// <summary>
        /// Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.
        /// </summary>
        public const uint ClipChildren = 0x02000000;
        /// <summary>
        /// Clips child windows relative to each other; that is, when a particular child window receives a <c>WM_PAINT</c> message, the <see cref="ClipSiblings"/>
        /// style clips all other overlapping child windows out of the region of the child window to be updated. If <see cref="ClipSiblings"/> is not specified
        /// and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
        /// </summary>
        public const uint ClipSiblings = 0x04000000;
        /// <summary>
        /// The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created,
        /// use the <see cref="Window.Enabled"/> property.
        /// </summary>
        public const uint Disabled = 0x08000000;
        /// <summary>
        /// The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.
        /// </summary>
        public const uint DlgFrame = 0x00400000;
        /// <summary>
        /// The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control
        /// with the <see cref="Group"/> style. The first control in each group usually has the <see cref="TabStop"/> style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys.
        /// </summary>
        /// <remarks>
        /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use
        /// the <see cref="Window.SetWindowLongPtr(int, nint)"/> function.
        /// </remarks>
        public const uint Group = 0x00020000;
        /// <summary>
        /// The window has a horizontal scroll bar.
        /// </summary>
        public const uint HScrollBar = 0x00100000;
        /// <summary>
        /// The window is initially minimized. Same as the <see cref="Minimize"/> style.
        /// </summary>
        public const uint Iconic = 0x20000000;
        /// <summary>
        /// The window is initially maximized.
        /// </summary>
        public const uint Maximize = 0x01000000;
        /// <summary>
        /// The window has a maximize button. Cannot be combined with the <see cref="ExWindowStyles.ContextHelp"/> style. The <see cref="SystemMenu"/> style must
        /// also be specified.
        /// </summary>
        public const uint MaximizeBox = 0x00010000;
        /// <summary>
        /// The window is initially minimized. Same as the <see cref="Iconic"/> style.
        /// </summary>
        public const uint Minimize = 0x20000000;
        /// <summary>
        /// The window has a minimize button. Cannot be combined with the <see cref="ExWindowStyles.ContextHelp"/> style. The <see cref="SystemMenu"/> style must
        /// also be specified.
        /// </summary>
        public const uint MinimizeBox = 0x00020000;
        /// <summary>
        /// The window is an overlapped window. An overlapped window has a title bar and a border. Same as the <see cref="Tiled"/> style.
        /// </summary>
        public const uint Overlapped = 0x00000000;
        /// <summary>
        /// The window is an overlapped window. Same as the <see cref="TiledWindow"/> style.
        /// </summary>
        public const uint OverlappedWindow = Overlapped | Caption | SystemMenu | ThickFrame | MinimizeBox | MaximizeBox;
        /// <summary>
        /// The window is a pop-up window. This style cannot be used with the <see cref="Child"/> style.
        /// </summary>
        public const uint Popup = 0x80000000;
        /// <summary>
        /// The window is a pop-up window. The <see cref="Caption"/> and <see cref="PopupWindow"/> styles must be combined to make the window menu visible.
        /// </summary>
        public const uint PopupWindow = Popup | Border | SystemMenu;
        /// <summary>
        /// The window has a sizing border. Same as the <see cref="ThickFrame"/> style.
        /// </summary>
        public const uint SizeBox = 0x00040000;
        /// <summary>
        /// The window has a window menu on its title bar. The <see cref="Caption"/> style must also be specified.
        /// </summary>
        public const uint SystemMenu = 0x00080000;
        /// <summary>
        /// The window is a control that can receive the keyboard focus when the user presses the <c>TAB</c> key. Pressing the <c>TAB</c> key changes the
        /// keyboard focus to the next control with the <see cref="TabStop"/> style.
        /// </summary>
        public const uint TabStop = 0x00010000;
        /// <summary>
        /// The window has a sizing border. Same as the <see cref="SizeBox"/> style.
        /// </summary>
        public const uint ThickFrame = 0x00040000;
        /// <summary>
        /// The window is a tiled window. A tiled window has a title bar and a border. On Windows versions since Windows 2.0, same as the <see cref="Overlapped"/> style.
        /// </summary>
        public const uint Tiled = 0x00000000;
        /// <summary>
        /// The window is a tiled window. A tiled window has a title bar and a border. On Windows versions since Windows 2.0, same as the <see cref="OverlappedWindow"/> style.
        /// </summary>
        public const uint TiledWindow = Tiled | Caption | SystemMenu | ThickFrame | MinimizeBox | MaximizeBox;
        /// <summary>
        /// The window is initially visible.
        /// </summary>
        /// <remarks>
        /// This style can be turned on and off by using the <see cref="Window.Visible"/> property or the <see cref="Window.SetPos"/> function.
        /// </remarks>
        public const uint Visible = 0x10000000;
        /// <summary>
        /// The window has a vertical scroll bar.
        /// </summary>
        public const uint VScrollBar = 0x00200000;
    }

    /// <summary>
    /// The structure passed to the <c>lParam</c> parameter of the <c>WM_CREATE</c> message of a window.
    /// </summary>
    public struct CreateStruct
    {
        /// <summary>
        /// The parameter passed to the <c>param</c> parameter of <see cref="Window"/>.
        /// </summary>
        public object? CreateParam;
        /// <summary>
        /// A handle to the module that owns the new window.
        /// </summary>
        public nint HInstance;
        /// <summary>
        /// A handle to the menu to be used by the new window.
        /// </summary>
        public nint HMenu;
        /// <summary>
        /// The parent window, if the window is a child window. If the window is owned, this member identifies the owner window.
        /// If the window is not a child or owned window, this member is <see langword="null"/>.
        /// </summary>
        public Window? Parent;
        /// <summary>
        /// The size of the new window, in pixels.
        /// </summary>
        public Size Size;
        /// <summary>
        /// The location of the new window. If the new window is a child window, coordinates are relative to the parent window.
        /// Otherwise, the coordinates are relative to the screen origin.
        /// </summary>
        public Point Location;
        /// <summary>
        /// The style for the new window. For a list of the common shared values, see <see cref="WindowStyles"/>.
        /// </summary>
        public long Style;
        /// <summary>
        /// The text of the new window.
        /// </summary>
        public string Text;
        /// <summary>
        /// The class of the new window.
        /// </summary>
        public WindowClass Class;
        /// <summary>
        /// The extended window style for the new window.
        /// </summary>
        public ExWindowStyles ExStyle;

        internal nint _createParams;
    }

    /// <summary>
    /// Represents the show command for <see cref="Window.Show"/> and <see cref="Window.ShowAsync"/>.
    /// </summary>
    public enum WindowShowCommand
    {
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        Hide = 0,
        /// <summary>
        /// Activates and displays a window. If the window is minimized, maximized, or arranged, the system restores it to its original size and position.
        /// An application should specify this flag when displaying the window for the first time.
        /// </summary>
        ShowNormal = 1,
        /// <summary>
        /// Activates and displays a window. If the window is minimized, maximized, or arranged, the system restores it to its original size and position.
        /// An application should specify this flag when displaying the window for the first time.
        /// </summary>
        Normal = ShowNormal,
        /// <summary>
        /// Activates the window and displays it as a minimized window.
        /// </summary>
        ShowMinimized = 2,
        /// <summary>
        /// Activates the window and displays it as a maximized window.
        /// </summary>
        ShowMaximized = 3,
        /// <summary>
        /// Activates the window and displays it as a maximized window.
        /// </summary>
        Maximize = ShowMaximized,
        /// <summary>
        /// Displays a window in its most recent size and position. This value is similar to <see cref="ShowNormal"/>, except that the window is not activated.
        /// </summary>
        ShowNoActivate = 4,
        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        Show = 5,
        /// <summary>
        /// Minimizes the specified window and activates the next top-level window in the Z order.
        /// </summary>
        Minimize = 6,
        /// <summary>
        /// Displays the window as a minimized window. This value is similar to <see cref="ShowMinimized"/>, except the window is not activated.
        /// </summary>
        ShowMinimizedNoActivate = 7,
        /// <summary>
        /// Displays the window in its current size and position. This value is similar to <see cref="Show"/>, except that the window is not activated.
        /// </summary>
        ShowNA = 8,
        /// <summary>
        /// Activates and displays the window. If the window is minimized, maximized, or arranged, the system restores it to its original size and
        /// position. An application should specify this flag when restoring a minimized window.
        /// </summary>
        Restore = 9,
        /// <summary>
        /// Sets the show state based on the command value specified in the <c>STARTUPINFO</c> structure passed to the <c>CreateProcess</c> function by the
        /// program that started the application.
        /// </summary>
        // TODO: Change this once WinProcess gets added
        ShowDefault = 10,
        /// <summary>
        /// Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when minimizing windows
        /// from a different thread.
        /// </summary>
        ForceMinimize = 11
    }

    /// <summary>
    /// Window sizing and positioning flags for <see cref="Window.SetPos(Point, Size, WindowSetPosOptions, Window?)"/>.
    /// </summary>
    [Flags]
    public enum WindowSetPosOptions
    {
        /// <summary>
        /// If the calling thread and the thread that owns the window are attached to different input queues, the system
        /// posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution
        /// while other threads process the request.
        /// </summary>
        AsyncWindowPos = 0x4000,
        /// <summary>
        /// Prevents generation of the <c>WM_SYNCPAINT</c> message.
        /// </summary>
        DeferErase = 0x2000,
        /// <summary>
        /// Draws a frame (defined in the window's class description) around the window.
        /// </summary>
        DrawFrame = 0x0020,
        /// <summary>
        /// Applies new frame styles set using the <see cref="Window.SetWindowLongPtr(int, nint)"/> function. Sends a
        /// <c>WM_NCCALCSIZE</c> message to the window, even if the window's size is not being changed. If this flag is not
        /// specified, <c>WM_NCCALCSIZE</c> is sent only when the window's size is being changed.
        /// </summary>
        FrameChanged = DrawFrame,
        /// <summary>
        /// Hides the window.
        /// </summary>
        HideWindow = 0x0080,
        /// <summary>
        /// Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or
        /// non-topmost group (depending on the setting of the <c>insertAfter</c> parameter).
        /// </summary>
        NoActivate = 0x0010,
        /// <summary>
        /// Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back
        /// into the client area after the window is sized or repositioned.
        /// </summary>
        NoCopyBits = 0x0100,
        /// <summary>
        /// Retains the current position (ignores the <c>pos</c> parameter).
        /// </summary>
        NoMove = 0x0002,
        /// <summary>
        /// Does not change the owner window's position in the Z order.
        /// </summary>
        NoOwnerZOrder = 0x0200,
        /// <summary>
        /// Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and
        /// scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts
        /// of the window and parent window that need redrawing.
        /// </summary>
        NoRedraw = 0x0008,
        /// <summary>
        /// Same as the <see cref="NoOwnerZOrder"/> flag.
        /// </summary>
        NoReposition = NoOwnerZOrder,
        /// <summary>
        /// Prevents the window from receiving the <c>WM_WINDOWPOSCHANGING</c> message.
        /// </summary>
        NoSendChanging = 0x0400,
        /// <summary>
        /// Retains the current size (ignores the <c>size</c> parameter).
        /// </summary>
        NoSize = 0x0001,
        /// <summary>
        /// Retains the current Z order (ignores the <c>insertAfter</c> parameter).
        /// </summary>
        NoZOrder = 0x0004,
        /// <summary>
        /// Displays the window.
        /// </summary>
        ShowWindow = 0x0040
    }

    /// <summary>
    /// Represents windows used for Z position ordering.
    /// </summary>
    /// <remarks>Note that you cannot perform operations on these windows, as they are not valid window handles.</remarks>
    public static class ZOrderWindows
    {
        /// <summary>
        /// Places the window at the bottom of the Z order. If the window identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
        /// </summary>
        public static readonly Window Bottom = Window.FromHandleInternal(1);
        /// <summary>
        /// Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
        /// </summary>
        public static readonly Window NoTopMost = Window.FromHandleInternal(-2);
        /// <summary>
        /// Places the window at the top of the Z order.
        /// </summary>
        public static readonly Window Top = Window.FromHandleInternal(0);
        /// <summary>
        /// Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
        /// </summary>
        public static readonly Window TopMost = Window.FromHandleInternal(-1);
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

    /// <summary>
    /// Defines default index values for <see cref="Window.SetWindowLongPtr(int, nint)"/>
    /// and <see cref="Window.GetWindowLongPtr(int)"/>.
    /// </summary>
    public static class WindowLongPtr
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
        /// Gets or sets a new <see cref="WindowStyles">window style</see>. (GWL_STYLE)
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
}