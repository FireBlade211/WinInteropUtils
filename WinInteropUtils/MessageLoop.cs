using System.Drawing;
using System.Runtime.InteropServices;
using System;
using System.ComponentModel;

namespace FireBlade.WinInteropUtils
{
    /// <summary>
    /// Provides methods for managing message loops.
    /// </summary>
    /// <example>
    /// To create a message loop, use the following pattern:
    /// <code>
    /// WinMessage msg = new WinMessage();
    /// while (MessageLoop.GetMessage(ref msg, null, 0, 0))
    /// {
    ///     MessageLoop.TranslateMessage(ref msg);
    ///     MessageLoop.DispatchMessage(ref msg);
    /// }
    /// </code>
    /// </example>
    public static partial class MessageLoop
    {
        [LibraryImport("User32.dll", EntryPoint = "GetMessageW", SetLastError = true)]
        private static unsafe partial int PInvokeGetMessage(MSG* lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        /// <summary>
        /// Retrieves a message from the calling thread's message queue. The function dispatches incoming sent messages until a posted message is available for retrieval.
        /// </summary>
        /// <param name="msg">A <see cref="WinMessage"/> structure that receives message information from the thread's message queue.</param>
        /// <param name="wnd">The window whose messages are to be retrieved. The window must belong to the current thread.</param>
        /// <param name="minMessageFilter">The integer value of the lowest message value to be retrieved. Use <c>WM_KEYFIRST</c> (0x0100) to specify the
        /// first keyboard message or <c>WM_MOUSEFIRST</c> (0x0200) to specify the first mouse message.</param>
        /// <param name="maxMessageFilter">The integer value of the highest message value to be retrieved. Use <c>WM_KEYLAST</c> to specify the last keyboard
        /// message or <c>WM_MOUSELAST</c> to specify the last mouse message.</param>
        /// <returns><see langword="true"/> if the function retrieves a message other than <c>WM_QUIT</c>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="Win32Exception">The function has failed. For example, the function fails if <paramref name="wnd"/> is an invalid window.</exception>
        /// <remarks>
        /// <para>If <paramref name="wnd"/> is <see langword="null"/>, <see cref="GetMessage(ref WinMessage, Window?, uint, uint)"/> retrieves messages
        /// for any window that belongs to the current thread, and any messages on the current thread's message queue whose <c>hwnd</c> value
        /// is <see cref="nint.Zero"/>. Therefore if <paramref name="wnd"/> is <see langword="null"/>, both window messages and thread messages are processed.</para>
        /// 
        /// <para>Use <c>WM_INPUT</c> in <paramref name="minMessageFilter"/> and <paramref name="maxMessageFilter"/> to specify only the
        /// <c>WM_INPUT</c> messages.</para>
        /// 
        /// <para>If <paramref name="minMessageFilter"/> and <paramref name="maxMessageFilter"/> are both zero,
        /// <see cref="GetMessage(ref WinMessage, Window?, uint, uint)"/> returns all available messages (that is, no range filtering is performed).</para>
        /// 
        /// <para>An application typically uses the return value to determine whether to end the main message loop and exit the program.</para>
        /// 
        /// <para>The <see cref="GetMessage(ref WinMessage, Window?, uint, uint)"/> function retrieves messages associated with the
        /// window in <paramref name="wnd"/> parameter or any of its children, as specified by the <c>IsChild</c> function, and within
        /// the range of message values given by the <paramref name="minMessageFilter"/> and <paramref name="maxMessageFilter"/> parameters.
        /// Note that an application can only use the low word in the <paramref name="maxMessageFilter"/> and <paramref name="maxMessageFilter"/>
        /// parameters; the high word is reserved for the system.</para>
        /// 
        /// <para>Note that <see cref="GetMessage(ref WinMessage, Window?, uint, uint)"/> always retrieves <c>WM_QUIT</c> messages,
        /// no matter which values you specify for <paramref name="minMessageFilter"/> and <paramref name="maxMessageFilter"/>.</para>
        /// 
        /// <para>During this call, the system delivers pending, nonqueued messages, that is, messages sent to windows owned by the calling thread using
        /// the <see cref="Window.SendMessage(uint, nuint, nint)"/>, <c>SendMessageCallback</c>, <c>SendMessageTimeout</c>, or <c>SendNotifyMessage</c> function.
        /// Then the first queued message that matches the specified filter is retrieved. The system may also process internal events. If no filter is specified,
        /// messages are processed in the following order:
        /// <list type="number">
        ///     <item>Sent messages</item>
        ///     <item>Posted messages</item>
        ///     <item>Input (hardware) messages and system internal events</item>
        ///     <item>Sent messages (again)</item>
        ///     <item><c>WM_PAINT</c> messages</item>
        ///     <item><c>WM_TIMER</c> messages</item>
        /// </list>
        /// </para>
        /// 
        /// <para>To retrieve input messages before posted messages, use the <paramref name="minMessageFilter"/> and <paramref name="maxMessageFilter"/> parameters.</para>
        /// 
        /// <para><see cref="GetMessage(ref WinMessage, Window?, uint, uint)"/> does not remove <c>WM_PAINT</c> messages from the queue. The messages remain in the
        /// queue until processed.</para>
        /// 
        /// <para>If a top-level window stops responding to messages for more than several seconds, the system considers the window to be not responding and replaces
        /// it with a ghost window that has the same z-order, location, size, and visual attributes. This allows the user to move it, resize it, or even close the
        /// application. However, these are the only actions available because the application is actually not responding. When in the debugger mode, the system
        /// does not generate a ghost window.</para>
        /// 
        /// <!-- this renders as plain text in vs but will render as an actual header in the docs -->
        /// <h1>DPI Virtualization</h1>
        /// 
        /// <para>This API does not participate in DPI virtualization. The output is in the mode of the window that the message is targeting. The calling
        /// thread is not taken into consideration.</para>
        /// </remarks>
        public static bool GetMessage(ref WinMessage msg, Window? wnd, uint minMessageFilter, uint maxMessageFilter)
        {
            var m = new MSG
            {
                pt = new POINT
                {
                    x = msg.CursorPos.X,
                    y = msg.CursorPos.Y
                },
                hwnd = msg.Window?.Handle ?? nint.Zero,
                lParam = msg.LParam,
                message = msg.MessageId,
                time = msg._time,
                wParam = msg.WParam
            };

            unsafe
            {
                var result = PInvokeGetMessage(&m,
                    wnd?.Handle ?? nint.Zero,
                    minMessageFilter,
                    maxMessageFilter);

                if (result == -1)
                    throw new Win32Exception(Marshal.GetLastPInvokeError());

                msg._pt = m.pt;
                msg._message = m.message;
                msg._lParam = m.lParam;
                msg._time = m.time;
                msg._wParam = m.wParam;
                msg._wnd = m.hwnd != nint.Zero ? Window.FromHandleInternal(m.hwnd) : null;
                
                return result != 0;
            }
        }

        [LibraryImport("user32.dll", EntryPoint = "PeekMessageW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static unsafe partial bool PInvokePeekMessage(MSG* lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        /// <summary>
        /// Dispatches incoming nonqueued messages, checks the thread message queue for a posted message, and retrieves the message (if any exist).
        /// </summary>
        /// <param name="msg">A <see cref="WinMessage"/> structure that receives message information.</param>
        /// <param name="wnd">The window whose messages are to be retrieved. The window must belong to the current thread.</param>
        /// <param name="minMessageFilter">The value of the first message in the range of messages to be examined. Use <c>WM_KEYFIRST</c> (0x0100) to specify
        /// the first keyboard message or <c>WM_MOUSEFIRST</c> (0x0200) to specify the first mouse message.</param>
        /// <param name="maxMessageFilter">The value of the last message in the range of messages to be examined. Use <c>WM_KEYLAST</c> to specify the last
        /// keyboard message or <c>WM_MOUSELAST</c> to specify the last mouse message.</param>
        /// <param name="removeMsg">Specifies how messages are to be handled.</param>
        /// <param name="limit">Specifies what messages should be processed.</param>
        /// <returns><see langword="true"/> if a message is available; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <para><see cref="PeekMessage(ref WinMessage, Window?, uint, uint, MessageRemoval, MessageProcessingLimit)"/> retrieves messages associated
        /// with the window in the <paramref name="wnd"/> parameter or any of its children as specified by the <c>IsChild</c> function, and within
        /// the range of message values given by the <paramref name="minMessageFilter"/> and <paramref name="maxMessageFilter"/> parameters.
        /// Note that an application can only use the low word in the <paramref name="minMessageFilter"/> and <paramref name="maxMessageFilter"/> parameters;
        /// the high word is reserved for the system.</para>
        /// 
        /// <para>Note that <see cref="PeekMessage(ref WinMessage, Window?, uint, uint, MessageRemoval, MessageProcessingLimit)"/> always
        /// retrieves <c>WM_QUIT</c> messages, no matter which values you specify for <paramref name="minMessageFilter"/> and <paramref name="maxMessageFilter"/>.</para>
        /// 
        /// <para>During this call, the system dispatches (DispatchMessage) pending, nonqueued messages, that is, messages sent to windows owned by the calling
        /// thread using the <see cref="Window.SendMessage(uint, nuint, nint)"/>, <c>SendMessageCallback</c>, <c>SendMessageTimeout</c>,
        /// or <c>SendNotifyMessage</c> function. Then the first queued message that matches the specified filter is retrieved. The system may also
        /// process internal events. If no filter is specified, messages are processed in the following order:
        /// 
        /// <list type="number">
        ///     <item>Sent messages</item>
        ///     <item>Posted messages</item>
        ///     <item>Input (hardware) messages and system internal events</item>
        ///     <item>Sent messages (again)</item>
        ///     <item><c>WM_PAINT</c> messages</item>
        ///     <item><c>WM_TIMER</c> messages</item>
        /// </list>
        /// 
        /// </para>
        /// 
        /// <para>To retrieve input messages before posted messages, use the <paramref name="minMessageFilter"/> and <paramref name="maxMessageFilter"/> parameters.</para>
        /// 
        /// <para>The <see cref="PeekMessage(ref WinMessage, Window?, uint, uint, MessageRemoval, MessageProcessingLimit)"/> function normally does not
        /// remove <c>WM_PAINT</c> messages from the queue. <c>WM_PAINT</c> messages remain in the queue until they are processed. However, if a
        /// <c>WM_PAINT</c> message has a <see langword="null"/> update region,
        /// <see cref="PeekMessage(ref WinMessage, Window?, uint, uint, MessageRemoval, MessageProcessingLimit)"/> does remove it from the queue.</para>
        /// 
        /// <para>If a top-level window stops responding to messages for more than several seconds, the system considers the window to be not responding
        /// and replaces it with a ghost window that has the same z-order, location, size, and visual attributes. This allows the user to move it, resize it,
        /// or even close the application. However, these are the only actions available because the application is actually not responding. When an application
        /// is being debugged, the system does not generate a ghost window.</para>
        /// 
        /// <h1>DPI Virtualization</h1>
        /// 
        /// <para>This API does not participate in DPI virtualization. The output is in the mode of the window that the message is
        /// targeting. The calling thread is not taken into consideration.</para>
        /// </remarks>
        public static bool PeekMessage(ref WinMessage msg, Window? wnd, uint minMessageFilter, uint maxMessageFilter, MessageRemoval removeMsg,
            MessageProcessingLimit limit)
        {
            var m = new MSG
            {
                pt = new POINT { x = msg.CursorPos.X, y = msg.CursorPos.Y },
                hwnd = msg.Window?.Handle ?? nint.Zero,
                lParam = msg.LParam,
                message = msg.MessageId,
                time = msg._time,
                wParam = msg.WParam
            };

            unsafe
            {
                bool result = PInvokePeekMessage(&m, wnd?.Handle ?? nint.Zero, minMessageFilter, maxMessageFilter, (uint)removeMsg | (uint)limit);

                msg._pt = m.pt;
                msg._message = m.message;
                msg._lParam = m.lParam;
                msg._time = m.time;
                msg._wParam = m.wParam;
                msg._wnd = m.hwnd != nint.Zero ? Window.FromHandleInternal(m.hwnd) : null;

                return result;
            }
        }

        [LibraryImport("user32.dll", EntryPoint = "TranslateMessage", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static unsafe partial bool PInvokeTranslateMessage(MSG* lpMsg);

        /// <summary>
        /// Translates virtual-key messages into character messages. The character messages are posted to the calling thread's message queue, to be read
        /// the next time the thread calls the <see cref="GetMessage(ref WinMessage, Window?, uint, uint)"/> or
        /// <see cref="PeekMessage(ref WinMessage, Window?, uint, uint, MessageRemoval, MessageProcessingLimit)"/> function.
        /// </summary>
        /// <param name="msg">A <see cref="WinMessage"/> structure that contains message information retrieved from the calling thread's message queue
        /// by using the <see cref="GetMessage(ref WinMessage, Window?, uint, uint)"/>
        /// or <see cref="PeekMessage(ref WinMessage, Window?, uint, uint, MessageRemoval, MessageProcessingLimit)"/> function.</param>
        /// <returns><see langword="true"/> if the message is translated (that is, a character message is posted to the thread's message queue) or the message is
        /// <c>WM_KEYDOWN</c>, <c>WM_KEYUP</c>, <c>WM_SYSKEYDOWN</c>, or <c>WM_SYSKEYUP</c>; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>The <see cref="TranslateMessage(ref WinMessage)"/> function does not modify the message pointed to by the <paramref name="msg"/> parameter.</para>
        /// 
        /// <para><c>WM_KEYDOWN</c> and <c>WM_KEYUP</c> combinations produce a <c>WM_CHAR</c> or <c>WM_DEADCHAR</c> message. <c>WM_SYSKEYDOWN</c>
        /// and <c>WM_SYSKEYUP</c> combinations produce a <c>WM_SYSCHAR</c> or <c>WM_SYSDEADCHAR</c> message.</para>
        /// 
        /// <para><see cref="TranslateMessage(ref WinMessage)"/> produces <c>WM_CHAR</c> messages only for keys that are mapped to ASCII characters by the keyboard driver.</para>
        /// 
        /// <para>If applications process virtual-key messages for some other purpose, they should not call <see cref="TranslateMessage(ref WinMessage)"/>. For instance,
        /// an application should not call <see cref="TranslateMessage(ref WinMessage)"/> if the <c>TranslateAccelerator</c> function returns a nonzero value. Note that
        /// the application is responsible for retrieving and dispatching input messages to the dialog box. Most applications use the main message loop for this.
        /// However, to permit the user to move to and to select controls by using the keyboard, the application must call <c>IsDialogMessage</c>. For more information,
        /// see <see href="https://learn.microsoft.com/en-us/windows/desktop/dlgbox/dlgbox-programming-considerations">Dialog Box Keyboard Interface.</see></para>
        /// </remarks>
        public static bool TranslateMessage(ref WinMessage msg)
        {
            var m = new MSG
            {
                pt = new POINT { x = msg.CursorPos.X, y = msg.CursorPos.Y },
                hwnd = msg.Window?.Handle ?? nint.Zero,
                lParam = msg.LParam,
                message = msg.MessageId,
                time = msg._time,
                wParam = msg.WParam
            };

            unsafe
            {
                bool result = PInvokeTranslateMessage(&m);
                return result;
            }
        }

        [LibraryImport("user32.dll", EntryPoint = "DispatchMessageW", SetLastError = false)]
        private static unsafe partial nint PInvokeDispatchMessage(MSG* lpMsg);

        /// <summary>
        /// Dispatches a message to a window procedure. It is typically used to dispatch a message retrieved by
        /// the <see cref="GetMessage(ref WinMessage, Window?, uint, uint)"/> function.
        /// </summary>
        /// <param name="msg">A structure that contains the message.</param>
        /// <returns>The return value specifies the value returned by the window procedure. Although its meaning depends
        /// on the message being dispatched, the return value generally is ignored.</returns>
        /// <remarks>
        /// <para>The <see cref="WinMessage"/> structure must contain valid message values. If the <paramref name="msg"/> parameter points to
        /// a <c>WM_TIMER</c> message and the <c>lParam</c> parameter of the <c>WM_TIMER</c> message is not <see langword="null"/>, 
        /// <c>lParam</c> points to a function that is called instead of the window procedure.</para>
        /// 
        /// <para>Note that the application is responsible for retrieving and dispatching input messages to the dialog box. Most applications use the main 
        /// message loop for this. However, to permit the user to move to and to select controls by using the keyboard, the application must call
        /// <c>IsDialogMessage</c>. For more information, see
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/dlgbox/dlgbox-programming-considerations">Dialog Box Keyboard Interface</see>.</para>
        /// </remarks>
        public static nint DispatchMessage(ref WinMessage msg)
        {
            var m = new MSG
            {
                pt = new POINT { x = msg.CursorPos.X, y = msg.CursorPos.Y },
                hwnd = msg.Window?.Handle ?? nint.Zero,
                lParam = msg.LParam,
                message = msg.MessageId,
                time = msg._time,
                wParam = msg.WParam
            };

            unsafe
            {
                nint result = PInvokeDispatchMessage(&m);
                return result;
            }
        }

        [LibraryImport("user32.dll", EntryPoint = "PostQuitMessage", SetLastError = false)]
        private static partial void PInvokePostQuitMessage(int nExitCode);

        /// <summary>
        /// Indicates to the system that a thread has made a request to terminate (quit). It is typically used in response to a <c>WM_DESTROY</c> message.
        /// </summary>
        /// <param name="exitCode">The application exit code. This value is used as the <c>wParam</c> parameter of the <c>WM_QUIT</c> message.</param>
        /// <remarks>
        /// <para>The <see cref="PostQuitMessage(int)"/> function posts a <c>WM_QUIT</c> message to the thread's message queue and returns immediately;
        /// the function simply indicates to the system that the thread is requesting to quit at some time in the future.</para>
        /// 
        /// <para>When the thread retrieves the <c>WM_QUIT</c> message from its message queue, it should exit its message loop and return control to the system.
        /// The exit value returned to the system must be the wParam parameter of the <c>WM_QUIT</c> message.</para>
        /// </remarks>
        public static void PostQuitMessage(int exitCode = 0) => PInvokePostQuitMessage(exitCode);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MSG
    {
        public nint hwnd;
        public uint message;
        public nuint wParam;
        public nint lParam;
        public uint time;
        public POINT pt;
        public uint lPrivate;
    }

    /// <summary>
    /// Provides information about a Windows message.
    /// </summary>
    public struct WinMessage
    {
        internal Window? _wnd;

        /// <summary>
        /// The window whose window procedure receives the message. This member
        /// is <see langword="null"/> when the message is a thread message.
        /// </summary>
        public Window? Window => _wnd;

        internal uint _message;

        /// <summary>
        /// The message identifier. Applications can only use the low word; the high word is reserved by the system.
        /// </summary>
        public uint MessageId => _message;

        internal nuint _wParam;

        /// <summary>
        /// Additional information about the message. The exact meaning depends on the value
        /// of the <see cref="MessageId"/> member.
        /// </summary>
        public nuint WParam => _wParam;

        internal nint _lParam;

        /// <summary>
        /// Additional information about the message. The exact meaning depends on the value
        /// of the <see cref="MessageId"/> member.
        /// </summary>
        /// <remarks>
        /// This value represents the LPARAM before the framework processes it as a managed object
        /// when received in your window procedure.
        /// </remarks>
        public nint LParam => _lParam;

        internal uint _time;

        /// <summary>
        /// The time at which the message was posted.
        /// </summary>
        public DateTime Time => (DateTime.Now - TimeSpan.FromMilliseconds(Environment.TickCount64)) + TimeSpan.FromMilliseconds(_time);

        internal POINT _pt;

        /// <summary>
        /// The cursor position, in screen coordinates, when the message was posted.
        /// </summary>
        public Point CursorPos => new Point(_pt.x, _pt.y);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int x;
        public int y;
    }

    /// <summary>
    /// Specifies message removal options for <see cref="MessageLoop.PeekMessage(ref WinMessage, Window?, uint, uint, MessageRemoval, MessageProcessingLimit)"/>.
    /// </summary>
    [Flags]
    public enum MessageRemoval : uint
    {
        /// <summary>
        /// Messages are not removed from the queue after processing by
        /// <see cref="MessageLoop.PeekMessage(ref WinMessage, Window?, uint, uint, MessageRemoval, MessageProcessingLimit)"/>.
        /// </summary>
        NoRemove = 0x0000u,
        /// <summary>
        /// Messages are removed from the queue after processing by
        /// <see cref="MessageLoop.PeekMessage(ref WinMessage, Window?, uint, uint, MessageRemoval, MessageProcessingLimit)"/>.
        /// </summary>
        Remove = 0x0001,
        /// <summary>
        /// Prevents the system from releasing any thread that is waiting for the caller to go idle. Combine this value with
        /// either <see cref="NoRemove"/> or <see cref="Remove"/>.
        /// </summary>
        NoYield = 0x0002
    }

    /// <summary>
    /// Specifies what message types should be processed by <see cref="MessageLoop.PeekMessage(ref WinMessage, Window?, uint, uint, MessageRemoval, MessageProcessingLimit)"/>.
    /// </summary>
    [Flags]
    public enum MessageProcessingLimit
    {
        /// <summary>
        /// Process all messages,
        /// </summary>
        None = 0,
        /// <summary>
        /// Process mouse and keyboard messages.
        /// </summary>
        Input = (((0x0002 | 0x0004) | 0x0001 | 0x0400 | 0x0800 | 0x1000) << 16),
        /// <summary>
        /// Process all posted messages, including timers and hotkeys.
        /// </summary>
        PostMessage = ((0x0008 | 0x0080 | 0x0010) << 16),
        /// <summary>
        /// Process paint messages.
        /// </summary>
        Paint = (0x0020 << 16),
        /// <summary>
        /// Process all sent messages.
        /// </summary>
        SendMessage = (0x0040 << 16)
    }
}
