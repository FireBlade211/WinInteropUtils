using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;

namespace FireBlade.WinInteropUtils.Dialogs
{
    /// <summary>
    /// Provides methods for showing a Win32 message box.
    /// </summary>
    public sealed class Win32MessageBox : DialogWindow<Win32MessageBoxResult, Win32MessageBox>
    {
        /// <summary>
        /// Fires when help is requested from the message box.
        /// </summary>
        public event EventHandler<HelpEventArgs>? OnHelp;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MSGBOXPARAMSW
        {
            public uint cbSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszText;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszCaption;
            public uint dwStyle;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszIcon;  // should be LPCWSTR, not HICON
            public UIntPtr dwContextHelpId;
            public IntPtr lpfnMsgBoxCallback;
            public uint dwLanguageId;
        }

        // libraryimport doesn't support ref parameters
        [DllImport("user32.dll", EntryPoint = "MessageBoxIndirectW", CharSet = CharSet.Unicode)]
        private static extern int MessageBoxIndirect(ref MSGBOXPARAMSW msgboxParams);

        /// <summary>
        /// Specifies the buttons on the message box.
        /// </summary>
        public Win32MessageBoxButtons Buttons { get; set; } = Win32MessageBoxButtons.Ok;

        /// <summary>
        /// Gets or sets the icon displayed on the message box.
        /// </summary>
        public Win32MessageBoxIcon Icon { get; set; } = Win32MessageBoxIcon.None;

        ///// <summary>
        ///// The path to the custom icon to use when <see cref="Icon"/> is <see cref="Win32MessageBoxIcon.User"/>, or a resource ID passed to
        ///// the <see cref="Macros.MakeIntResource(int)"/> macro (InstanceHandle needs to be set to work with <see cref="Macros.MakeIntResource(int)"/>).
        ///// </summary>
        //public string? CustomIcon { get; set; }

        private int _defBtn = 1;

        /// <summary>
        /// Specifies the default button on the message box. This value must be in a range of 1-4.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is out of the range. The value must be in a range of 1-4.</exception>
        public int DefaultButton
        {
            get => _defBtn;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 4);

                _defBtn = value;
            }
        }
        
        /// <summary>
        /// Specifies the caption of the message box. If set to <see langword="null"/>, defaults to Error.
        /// </summary>
        public string? Caption { get; set; }

        /// <summary>
        /// Specifies the text of the message box.
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Adds a Help button to the message box. When the user clicks the Help button or presses F1,
        /// the message box fires the <see cref="OnHelp"/> event.
        /// </summary>
        public bool ShowHelp { get; set; } = false;

        /// <summary>
        /// Specifies the culture this message box uses. If <see langword="null"/>, uses the default system culture.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > Each localized release of Windows typically contains resources only for a limited set of languages.
        /// Thus, for example, the U.S. version offers <b>English</b>, the French version offers <b>French</b>, the German version
        /// offers <b>German</b>, and the Japanese version offers <b>Japanese</b>. Each version offers <b>Neutral</b>. This limits
        /// the set of values that can be used with the <see cref="Culture"/> parameter. Before specifying a language identifier, you should
        /// enumerate the locales that are installed on a system.
        /// </remarks>
        public CultureInfo? Culture { get; set; }

        /// <summary>
        /// Specifies the help context. When <see cref="OnHelp"/> fires, this specifies the value that will be
        /// in the <see cref="HelpEventArgs.ContextId"/> property of the <see cref="HelpEventArgs"/>.
        /// </summary>
        public nuint? ContextHelpId { get; set; }

        /// <summary>
        /// Specifies the modality of the message box. The default is <see cref="Win32MessageBoxModality.ApplicationModal"/>.
        /// </summary>
        public Win32MessageBoxModality Modality { get; set; } = Win32MessageBoxModality.ApplicationModal;

        /// <summary>
        /// Same as desktop of the interactive window station. For more
        /// information, see <see href="https://learn.microsoft.com/en-us/windows/desktop/winstation/window-stations">Window Stations</see>.
        /// </summary>
        /// <remarks>
        /// If the current input desktop is not the default desktop, <see cref="Show()"/> and <see cref="Show(nint)"/> does not
        /// return until the user switches to the default desktop.
        /// </remarks>
        public bool DefaultDesktopOnly { get; set; } = false;

        /// <summary>
        /// If <see langword="true"/>, the message box is right aligned. Otherwise, it is left-aligned. The default is <see langword="false"/>.
        /// </summary>
        public bool RightAlign { get; set; } = false;

        /// <summary>
        /// If <see langword="true"/>, displays message and caption text using right-to-left
        /// reading order on Hebrew and Arabic systems. (called RtlReading in Win32)
        /// </summary>
        public bool RightToLeft { get; set; } = false;

        /// <summary>
        /// If <see langword="true"/>, the message box becomes the foreground window. Internally, the system calls
        /// the <c>SetForegroundWindow</c> function for the message box.
        /// </summary>
        public bool SetForeground { get; set; } = false;

        /// <summary>
        /// If <see langword="true"/>, the message box is created as a top-most window (stays on top).
        /// </summary>
        public bool TopMost { get; set; } = false;

        /// <summary>
        /// The caller is a service notifying the user of an event. The function displays a message box
        /// on the current active desktop, even if there is no user logged on to the computer.
        /// </summary>
        /// <remarks>
        /// <para>> [!NOTE]
        /// > <b>Terminal Services:</b> If the calling thread has an impersonation token, the function directs
        /// the message box to the session specified in the impersonation token.</para>
        /// 
        /// <para>If this flag is set, the <see cref="Show()"/> function should be called instead of <see cref="Show(nint)"/>.
        /// This is so that the message box can appear on a desktop other than the desktop corresponding to the window corresponding
        /// to the specified <c>hWnd</c> in <see cref="Show(nint)"/>.</para>
        /// 
        /// For information on security considerations in regard to using this
        /// flag, see <see href="https://learn.microsoft.com/en-us/windows/desktop/Services/interactive-services">Interactive Services</see>. In particular, be aware
        /// that this flag can produce interactive content on a locked desktop and should therefore be used for only a very limited set of scenarios,
        /// such as resource exhaustion.
        /// </remarks>
        public bool ServiceNotification { get; set; } = false;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void MsgBoxCallback(nint lpHelpInfo);

        private uint MakeMessageBoxFlags()
        {
            uint f = 0;

            if (Icon != Win32MessageBoxIcon.None)
                f |= (uint)Icon;

            f |= (uint)Buttons;

            f |= DefaultButton switch
            {
                1 => 0x00000000,
                2 => 0x00000100,
                3 => 0x00000200,
                4 => 0x00000300,
                _ => (uint)0x00000000 // shouldn't happen because defaultbutton setter throws out of range exceptions but just in case
            };

            if (ShowHelp)
                f |= 0x00004000;

            f |= (uint)Modality;

            if (DefaultDesktopOnly)
                f |= 0x00020000;
            if (RightAlign)
                f |= 0x00080000;
            if (RightToLeft)
                f |= 0x00100000;
            if (SetForeground)
                f |= 0x00010000;
            if (TopMost)
                f |= 0x00040000;
            if (ServiceNotification)
                f |= 0x00200000;

            return f;
        }

        private void MessageBoxHelpCallbackFunc(nint lpHelpInfo)
        {
            HELPINFO help = Marshal.PtrToStructure<HELPINFO>(lpHelpInfo);

            OnHelp?.Invoke(this, new HelpEventArgs(help));
        }

        /// <summary>
        /// Shows the message box modelessly.
        /// </summary>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the strings in the <see cref="Text"/> and <see cref="Caption"/> members should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// </remarks>
        public override Win32MessageBoxResult Show() => Show(nint.Zero);

        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <param name="hWnd">The handle of the parent window to attach the message box to.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the strings in the <see cref="Text"/> and <see cref="Caption"/> members should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// </remarks>
        public override Win32MessageBoxResult Show(nint hWnd)
        {
            MSGBOXPARAMSW m_Params = new();

            m_Params.cbSize = (uint)Marshal.SizeOf<MSGBOXPARAMSW>();
            m_Params.lpszCaption = Caption!;
            m_Params.lpszText = Text;
            m_Params.dwStyle = MakeMessageBoxFlags();
            m_Params.hwndOwner = hWnd;

            if (Culture != null)
                m_Params.dwLanguageId = (ushort)Culture.LCID;

            if (ContextHelpId != null)
                m_Params.dwContextHelpId = (nuint)ContextHelpId;

            MsgBoxCallback cb = MessageBoxHelpCallbackFunc;
            m_Params.lpfnMsgBoxCallback = Marshal.GetFunctionPointerForDelegate(cb);

            //if (Icon == Win32MessageBoxIcon.User)
            //{
            //    if (CustomIcon == null)
            //        throw new InvalidOperationException("CustomIcon must be set when using Win32MessageBoxIcon.User.");

            //    m_Params.lpszIcon = CustomIcon;
            //}

            int button = MessageBoxIndirect(ref m_Params);

            GC.KeepAlive(cb);

            if (button == 0) // From MSDN: MessageBoxIndirect returns 0 if there is not enough memory to create the message box,
                             // returns the button clicked otherwise, or IDCANCEL if the message box was cancelled
            {
                throw new OutOfMemoryException("There was not enough memory to create the message box.");
            }

            return (Win32MessageBoxResult)button;
        }

        /// <summary>
        /// Shows a message box with the specified text and the default caption (Error) as a child of the window specified by <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd">The handle of the parent window to attach the message box to.</param>
        /// <param name="text">The text on the message box.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the string in the <paramref name="text"/> parameter should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show(nint)"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(nint hWnd, string text) => Show(hWnd, text, null);

        /// <summary>
        /// Shows a message box with the specified text and caption as a child of the window specified by <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd">The handle of the parent window to attach the message box to.</param>
        /// <param name="text">The text on the message box.</param>
        /// <param name="caption">The caption (title) of the message box.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the strings in the <paramref name="text"/> and <paramref name="caption"/> parameters should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show(nint)"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(nint hWnd, string text, string? caption) => new Win32MessageBox
        {
            Text = text,
            Caption = caption
        }.Show(hWnd);

        /// <summary>
        /// Shows a modeless message box with the specified text and the default caption (Error).
        /// </summary>
        /// <param name="text">The text on the message box.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the string in the <paramref name="text"/> parameter should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show()"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(string text) => Show(text, null);

        /// <summary>
        /// Shows a modeless message box with the specified text and caption.
        /// </summary>
        /// <param name="text">The text on the message box.</param>
        /// <param name="caption">The caption (title) of the message box.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the strings in the <paramref name="text"/> and <paramref name="caption"/> parameters should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show()"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(string text, string? caption) => new Win32MessageBox
        {
            Text = text,
            Caption = caption
        }.Show();

        /// <summary>
        /// Shows a modeless message box with the specified text, icon, and buttons.
        /// </summary>
        /// <param name="text">The text on the message box.</param>
        /// <param name="icon">The icon displayed on the message box. One of the <see cref="Win32MessageBoxIcon"/> values.</param>
        /// <param name="buttons">The buttons on the message box. One of the <see cref="Win32MessageBoxButtons"/> values.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the string in the <paramref name="text"/> parameter should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show()"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(string text, Win32MessageBoxIcon icon, Win32MessageBoxButtons buttons) => Show(text, null, icon, buttons);

        /// <summary>
        /// Shows a modeless message box with the specified text, caption, icon, and buttons.
        /// </summary>
        /// <param name="text">The text on the message box.</param>
        /// <param name="caption">The caption (title) of the message box.</param>
        /// <param name="icon">The icon displayed on the message box. One of the <see cref="Win32MessageBoxIcon"/> values.</param>
        /// <param name="buttons">The buttons on the message box. One of the <see cref="Win32MessageBoxButtons"/> values.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the strings in the <paramref name="text"/> and <paramref name="caption"/> parameters should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show()"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(string text, string? caption, Win32MessageBoxIcon icon, Win32MessageBoxButtons buttons) => new Win32MessageBox
        {
            Text = text,
            Caption = caption,
            Icon = icon,
            Buttons = buttons
        }.Show();

        /// <summary>
        /// Shows a message box with the specified text, icon, and buttons as a child of the window specified by <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd">The handle of the parent window to attach the message box to.</param>
        /// <param name="text">The text on the message box.</param>
        /// <param name="icon">The icon displayed on the message box. One of the <see cref="Win32MessageBoxIcon"/> values.</param>
        /// <param name="buttons">The buttons on the message box. One of the <see cref="Win32MessageBoxButtons"/> values.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the string in the <paramref name="text"/> parameter should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show(nint)"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(nint hWnd,
            string text,
            Win32MessageBoxIcon icon,
            Win32MessageBoxButtons buttons) => Show(hWnd, text, null, icon, buttons);

        /// <summary>
        /// Shows a message box with the specified text, caption, icon, and buttons as a child of the window specified by <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd">The handle of the parent window to attach the message box to.</param>
        /// <param name="text">The text on the message box.</param>
        /// <param name="caption">The caption (title) of the message box.</param>
        /// <param name="icon">The icon displayed on the message box. One of the <see cref="Win32MessageBoxIcon"/> values.</param>
        /// <param name="buttons">The buttons on the message box. One of the <see cref="Win32MessageBoxButtons"/> values.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the strings in the <paramref name="text"/> and <paramref name="caption"/> parameters should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show(nint)"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(nint hWnd, string text, string? caption, Win32MessageBoxIcon icon, Win32MessageBoxButtons buttons) => new Win32MessageBox
        {
            Text = text,
            Caption = caption,
            Icon = icon,
            Buttons = buttons
        }.Show(hWnd);

        /// <summary>
        /// Shows a modeless message box with the specified text and buttons.
        /// </summary>
        /// <param name="text">The text on the message box.</param>
        /// <param name="buttons">The buttons on the message box. One of the <see cref="Win32MessageBoxButtons"/> values.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the string in the <paramref name="text"/> parameter should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show()"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(string text, Win32MessageBoxButtons buttons) => Show(text, null, buttons);

        /// <summary>
        /// Shows a modeless message box with the specified text, caption, and buttons.
        /// </summary>
        /// <param name="text">The text on the message box.</param>
        /// <param name="caption">The caption (title) of the message box.</param>
        /// <param name="buttons">The buttons on the message box. One of the <see cref="Win32MessageBoxButtons"/> values.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the strings in the <paramref name="text"/> and <paramref name="caption"/> parameters should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show()"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(string text, string? caption, Win32MessageBoxButtons buttons) => new Win32MessageBox
        {
            Text = text,
            Caption = caption,
            Buttons = buttons
        }.Show();

        /// <summary>
        /// Shows a message box with the specified text and buttons as a child of the window specified by <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd">The handle of the parent window to attach the message box to.</param>
        /// <param name="text">The text on the message box.</param>
        /// <param name="buttons">The buttons on the message box. One of the <see cref="Win32MessageBoxButtons"/> values.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the string in the <paramref name="text"/> parameter should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show(nint)"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(nint hWnd, string text, Win32MessageBoxButtons buttons) => Show(hWnd, text, null, buttons);

        /// <summary>
        /// Shows a message box with the specified text, caption, and buttons as a child of the window specified by <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd">The handle of the parent window to attach the message box to.</param>
        /// <param name="text">The text on the message box.</param>
        /// <param name="caption">The caption (title) of the message box.</param>
        /// <param name="buttons">The buttons on the message box. One of the <see cref="Win32MessageBoxButtons"/> values.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the strings in the <paramref name="text"/> and <paramref name="caption"/> parameters should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show(nint)"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(nint hWnd, string text, string? caption, Win32MessageBoxButtons buttons) => new Win32MessageBox
        {
            Text = text,
            Caption = caption,
            Buttons = buttons
        }.Show(hWnd);

        /// <summary>
        /// Shows a modeless message box with the specified text and icon.
        /// </summary>
        /// <param name="text">The text on the message box.</param>
        /// <param name="icon">The icon displayed on the message box. One of the <see cref="Win32MessageBoxIcon"/> values.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the string in the <paramref name="text"/> parameter should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show()"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(string text, Win32MessageBoxIcon icon) => Show(text, null, icon);

        /// <summary>
        /// Shows a modeless message box with the specified text, caption, and icon.
        /// </summary>
        /// <param name="text">The text on the message box.</param>
        /// <param name="caption">The caption (title) of the message box.</param>
        /// <param name="icon">The icon displayed on the message box. One of the <see cref="Win32MessageBoxIcon"/> values.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the strings in the <paramref name="text"/> and <paramref name="caption"/> parameters should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show()"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(string text, string? caption, Win32MessageBoxIcon icon) => new Win32MessageBox
        {
            Text = text,
            Caption = caption,
            Icon = icon
        }.Show();

        /// <summary>
        /// Shows a message box with the specified text and icon as a child of the window specified by <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd">The handle of the parent window to attach the message box to.</param>
        /// <param name="text">The text on the message box.</param>
        /// <param name="icon">The icon displayed on the message box. One of the <see cref="Win32MessageBoxIcon"/> values.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the string in the <paramref name="text"/> parameter should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show(nint)"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(nint hWnd, string text, Win32MessageBoxIcon icon) => Show(hWnd, text, null, icon);

        /// <summary>
        /// Shows a message box with the specified text, caption, and icon as a child of the window specified by <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd">The handle of the parent window to attach the message box to.</param>
        /// <param name="text">The text on the message box.</param>
        /// <param name="caption">The caption (title) of the message box.</param>
        /// <param name="icon">The icon displayed on the message box. One of the <see cref="Win32MessageBoxIcon"/> values.</param>
        /// <returns>
        /// <para>The button that was clicked.</para>
        /// 
        /// If a message box has a Cancel button, the function returns the <see cref="Win32MessageBoxResult.Cancel"/> value
        /// if either the ESC key is pressed or the Cancel button is selected. If the message box has no Cancel button, pressing ESC has no effect.
        /// </returns>
        /// <exception cref="OutOfMemoryException">There was not enough memory to create the message box.</exception>
        /// <remarks>
        /// <para>When you use a system-modal message box to indicate that the system is low on memory,
        /// the strings in the <paramref name="text"/> and <paramref name="caption"/> parameters should not be
        /// taken from a resource file, because an attempt to load the resource may fail.</para>
        /// 
        /// For extended customization, create an instance of the <see cref="Win32MessageBox"/> class with the <see cref="Win32MessageBox.Win32MessageBox"/> constructor
        /// and show it with the <see cref="Show(nint)"/> method.
        /// </remarks>
        public static Win32MessageBoxResult Show(nint hWnd, string text, string? caption, Win32MessageBoxIcon icon) => new Win32MessageBox
        {
            Text = text,
            Caption = caption,
            Icon = icon
        }.Show(hWnd);
    }

    /// <summary>
    /// Defines the buttons on a message box.
    /// </summary>
    public enum Win32MessageBoxButtons : long
    {
        /// <summary>
        /// The message box contains one push button: OK. This is the default.
        /// </summary>
        Ok = 0x00000000L,
        /// <summary>
        /// The message box contains two push buttons: OK and Cancel.
        /// </summary>
        OkCancel = 0x00000001L,
        /// <summary>
        /// The message box contains two push buttons: Retry and Cancel.
        /// </summary>
        RetryCancel = 0x00000005L,
        /// <summary>
        /// The message box contains two push buttons: Yes and No.
        /// </summary>
        YesNo = 0x00000004L,
        /// <summary>
        /// The message box contains three push buttons: Yes, No, and Cancel.
        /// </summary>
        YesNoCancel = 0x00000003L,
        /// <summary>
        /// The message box contains three push buttons: Cancel, Try Again, Continue.
        /// Use this instead of <see cref="AbortRetryIgnore"/>.
        /// </summary>
        CancelRetryContinue = 0x00000006L,
        /// <summary>
        /// The message box contains three push buttons: Abort, Retry, and Ignore.
        /// </summary>
        AbortRetryIgnore = 0x00000002L
    }

    /// <summary>
    /// Represents the result of the message box.
    /// </summary>
    public enum Win32MessageBoxResult
    {
        /// <summary>
        /// The Abort button was selected.
        /// </summary>
        Abort = 3,
        /// <summary>
        /// The Cancel button was selected.
        /// </summary>
        Cancel = 2,
        /// <summary>
        /// The Continue button was selected.
        /// </summary>
        Continue = 11,
        /// <summary>
        /// The Ignore button was selected.
        /// </summary>
        Ignore = 5,
        /// <summary>
        /// The No button was selected.
        /// </summary>
        No = 7,
        /// <summary>
        /// The OK button was selected.
        /// </summary>
        Ok = 1,
        /// <summary>
        /// The Retry button was selected.
        /// </summary>
        Retry = 4,
        /// <summary>
        /// The Try Again button was selected.
        /// </summary>
        TryAgain = 10,
        /// <summary>
        /// The Yes button was selected.
        /// </summary>
        Yes = 6
    }

    /// <summary>
    /// Specifies the icon in a message box.
    /// </summary>
    public enum Win32MessageBoxIcon : long
    {
        /// <summary>
        /// No icon is displayed in the message box.
        /// </summary>
        None,
        /// <summary>
        /// An exclamation-point icon appears in the message box.
        /// </summary>
        Warning = 0x00000030L,
        /// <summary>
        /// An exclamation-point icon appears in the message box.
        /// </summary>
        Exclamation = Warning,
        /// <summary>
        /// An icon consisting of a lowercase letter <i>i</i> in a circle appears in the message box.
        /// </summary>
        Information = 0x00000040L,
        /// <summary>
        /// An icon consisting of a lowercase letter <i>i</i> in a circle appears in the message box.
        /// </summary>
        Asterisk = Information,
        /// <summary>
        /// An icon consisting of a lowercase letter <i>i</i> in a circle appears in the message box.
        /// </summary>
        Info = Information,
        /// <summary>
        /// A question-mark icon appears in the message box.
        /// </summary>
        /// <remarks>
        /// The question-mark message icon is no longer recommended because it does not clearly represent a specific type of message and 
        /// because the phrasing of a message as a question could apply to any message type. In addition, users can confuse the message symbol
        /// question mark with Help information. Therefore, do not use this question mark message symbol in your message boxes.
        /// The system continues to support its inclusion only for backward compatibility.
        /// </remarks>
        [Obsolete("The question-mark message icon is not recommended; see documentation for details.")]
        Question = 0x00000020L,
        /// <summary>
        /// A stop-sign icon appears in the message box.
        /// </summary>
        Error = 0x00000010L,
        /// <summary>
        /// A stop-sign icon appears in the message box.
        /// </summary>
        Stop = Error,
        /// <summary>
        /// A stop-sign icon appears in the message box.
        /// </summary>
        Hand = Error,
        ///// <summary>
        ///// A custom icon appears in the message box. The icon is pointed to by the <see cref="Win32MessageBox.CustomIcon"/> property.
        ///// </summary>
        //User = 0x00000080L
    }

    /// <summary>
    /// Provides event arguments for help events.
    /// </summary>
    public class HelpEventArgs : EventArgs
    {
        internal HELPINFO helpStructInternal;

        /// <summary>
        /// The type of context for which help is requested.
        /// </summary>
        public HelpContext ContextType => (HelpContext)helpStructInternal.iContextType;

        /// <summary>
        /// The identifier of the window or control if <see cref="ContextType"/>  is <see cref="HelpContext.Window"/>, or identifier of
        /// the menu item if <see cref="ContextType"/> is <see cref="HelpContext.MenuItem"/>.
        /// </summary>
        public int ControlId => helpStructInternal.iCtrlId;

        /// <summary>
        /// The handle of the child window, control, or menu item.
        /// </summary>
        public nint ControlHandle => helpStructInternal.hItemHandle;

        /// <summary>
        /// The help context identifier of the window or control.
        /// </summary>
        public nint ContextId => helpStructInternal.dwContextId;

        /// <summary>
        /// The <see cref="Point"/> that contains the screen coordinates of the mouse cursor. This is useful
        /// for providing help based on the position of the mouse cursor.
        /// </summary>
        public Point MousePos => new(helpStructInternal.MousePos.x, helpStructInternal.MousePos.y);

        internal HelpEventArgs(HELPINFO helpStruct)
        {
            helpStructInternal = helpStruct;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HELPINFO
    {
        public uint cbSize;      // Size of this structure, in bytes
        public int iContextType; // Either HELPINFO_WINDOW (1) or HELPINFO_MENUITEM (2)
        public int iCtrlId;      // Control ID or menu item ID
        public IntPtr hItemHandle; // Handle to control (HWND) or menu handle (HMENU)
        public IntPtr dwContextId; // Application-defined help context ID
        public POINT MousePos;   // Mouse position in screen coordinates
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int x;
        public int y;
    }
    /// <summary>
    /// The type of context for which help is requested.
    /// </summary>
    public enum HelpContext
    {
        /// <summary>
        /// Help requested for a menu item.
        /// </summary>
        MenuItem = 0x0002,
        /// <summary>
        /// Help requested for a control or window.
        /// </summary>
        Window = 0x0001
    }

    /// <summary>
    /// Specifies the modality of a <see cref="Win32MessageBox"/>.
    /// </summary>
    public enum Win32MessageBoxModality : long
    {
        /// <summary>
        /// The user must respond to the message box before continuing work in the window identified by the <c>hWnd</c> parameter. However,
        /// the user can move to the windows of other threads and work in those windows.
        /// </summary>
        /// <remarks>
        /// Depending on the hierarchy of windows in the application, the user may be able to move to other windows within the thread.
        /// All child windows of the parent of the message box are automatically disabled, but pop-up windows are not.
        /// </remarks>
        ApplicationModal = 0x00000000L,
        /// <summary>
        /// Same as <see cref="ApplicationModal"/> except that the message box is a top-most window (stays on top).
        /// Use system-modal message boxes to notify the user of serious, potentially damaging
        /// errors that require immediate attention (for example, running out of memory). This flag has no effect on the user's
        /// ability to interact with windows other than those associated with <c>hWnd</c>.
        /// </summary>
        SystemModal = 0x00001000L,
        /// <summary>
        /// Same as <see cref="ApplicationModal"/> except that all the top-level windows belonging to the current thread are disabled
        /// if the <c>hWnd</c> parameter is <see langword="null"/>. Use this flag when the calling application or library does not
        /// have a window handle available but still needs to prevent input to other windows in the calling
        /// thread without suspending other threads.
        /// </summary>
        TaskModal = 0x00002000L
    }
}
