using FireBlade.WinInteropUtils.Dialogs;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FireBlade.WinInteropUtils
{
    /// <summary>
    /// Provides methods for managing and drawing with visual styles.
    /// </summary>
    [SupportedOSPlatform("windows6.0")] // visual styles were introduced in XP but the functions were added in Vista
    public partial class VisualStyle : IDisposable, IHandle
    {
        internal nint _htheme = 0;

        /// <summary>
        /// Gets the handle (<c>HTHEME</c>) of the opened theme data.
        /// </summary>
        public nint Handle => _htheme;

        internal VisualStyle(nint hTheme)
        {
            _htheme = hTheme;
        }

        [LibraryImport("Uxtheme.dll", EntryPoint = "OpenThemeData", StringMarshalling = StringMarshalling.Utf16)]
        private static partial nint _OpenThemeData(nint hwnd, string pszClassList);

        /// <summary>
        /// Opens the theme data for a window and its associated class.
        /// </summary>
        /// <param name="wnd">The window for which theme data is requested.</param>
        /// <param name="classList">A semicolon-seperated list of classes. See the <see href="../docs/visualstyles.md">Visual Styles</see> article for more info.</param>
        /// <returns>The visual style opened.</returns>
        /// <remarks>
        /// <para><see cref="OpenThemeData(Window, string)"/> tries to match each class, one at a time, to a class data section in the active theme. If a match is found,
        /// an associated <see cref="VisualStyle"/> is returned. If no match is found, a <see cref="FileNotFoundException"/> is thrown. You should guard against this in
        /// a <c>try-catch</c> block, and fall back to the static non-visual-style dependent functions in the <c>catch</c> block instead, such as <see cref="DrawText"/>.</para>
        /// 
        /// The <paramref name="classList"/> parameter contains a list, not just a single name, to provide the class an opportunity to get the best match between
        /// the class and the current visual style. For example, a button may pass <c>"OkButton;Button"</c> if its ID is <c>ID_OK</c>. If the current visual style
        /// has an entry for <c>OkButton</c>, that is used; otherwise no visual style is applied.
        /// </remarks>
        /// <exception cref="FileNotFoundException">A matching class data section was not found.</exception>
        public static VisualStyle OpenThemeData(Window wnd, string classList)
        {
            nint hTheme = _OpenThemeData(wnd, classList);

            if (hTheme == 0)
            {
                throw new FileNotFoundException("A matching class data section was not found.");
            }

            return new VisualStyle(hTheme);
        }

        [LibraryImport("Uxtheme.dll")]
        private static partial int CloseThemeData(nint hTheme);

        /// <summary>
        /// Releases the theme data for this <see cref="VisualStyle"/> instance.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            HRESULT hr = (HRESULT)CloseThemeData(_htheme);

            if (hr != HRESULT.S_OK)
            {
                Marshal.ThrowExceptionForHR((int)hr);
            }
        }

        [LibraryImport("Uxtheme.dll", EntryPoint = "DrawThemeText", StringMarshalling = StringMarshalling.Utf16)]
        private static unsafe partial int _DrawThemeText(nint hTheme, nint hdc, int iPartId, int iStateId, string pszText, int cchText, uint dwTextFlags, uint dwTextFlags2, User32.RECT* pRect);

        //[LibraryImport("uxtheme.dll", EntryPoint = "DrawThemeTextEx", StringMarshalling = StringMarshalling.Utf16)]
        //private static unsafe partial int DrawThemeTextEx(
        //    nint hTheme,
        //    nint hdc,
        //    int partId,
        //    int stateId,
        //    string text,
        //    int length,
        //    uint flags,
        //    User32.RECT* pRect,
        //    _DTTOPTS* pOptions);


        /// <summary>
        /// Draws text using the color and font defined by the visual style.
        /// </summary>
        /// <param name="gh">The <see cref="Graphics"/> surface to draw the text on.</param>
        /// <param name="iPartId">The control part that has the desired text appearance.
        /// See <see href="../docs/visualstyles.md#aero-parts-and-states">Aero Parts and States</see> for more info. If this value is 0, the text is drawn
        /// in the default font or a font selected into the device context.</param>
        /// <param name="iStateId">The control state that has the desired text appearance.
        /// See <see href="../docs/visualstyles.md#aero-parts-and-states">Aero Parts and States</see> for more info.</param>
        /// <param name="text">A string containing the text to draw.</param>
        /// <param name="cchText">An integer containing the number of characters to draw. If this parameter is set to -1, all characters are drawn.</param>
        /// <param name="options">A combination of <see cref="ThemeTextOptions"/> values specifying options for the string's formatting. </param>
        /// <param name="rect">A <see cref="Rectangle"/> that contains in logical coordinates, a rectangle in which the text is to be drawn. It is recommended
        /// to use extentRect from GetThemeTextExent to retrieve the correct coordinates.</param>
        /// <remarks>
        /// <!-- This function does not support the <see cref="ThemeTextOptions.CalcRect"/> option. Use
        /// the <see cref="DrawThemeText(Graphics, int, int, string, int, ThemeTextOptions, ref Rectangle, ThemeTextAdvancedOptions)"/> function instead if you need to use it.
        /// -->
        /// </remarks>
        public void DrawThemeText(Graphics gh, int iPartId, int iStateId, string text, int cchText, ThemeTextOptions options, Rectangle rect)
        {
            var rc = new User32.RECT
            {
                Top = rect.Top,
                Left = rect.Left,
                Bottom = rect.Bottom,
                Right = rect.Right
            };

            unsafe
            {
                HRESULT hr = (HRESULT)_DrawThemeText(_htheme, gh.GetHdc(), iPartId, iStateId, text, cchText, (uint)options, 0, &rc);

                if (hr != HRESULT.S_OK)
                {
                    Marshal.ThrowExceptionForHR((int)hr);
                }
            }
        }

        private static uint ToCOLORREF(Color color)
        {
            // COLORREF = 0x00BBGGRR
            return (uint)(color.R | (color.G << 8) | (color.B << 16));
        }

        private static Color FromCOLORREF(uint colorRef)
        {
            byte r = (byte)(colorRef & 0xFF);        // lowest byte = Red
            byte g = (byte)((colorRef >> 8) & 0xFF); // next byte = Green
            byte b = (byte)((colorRef >> 16) & 0xFF);// high byte = Blue

            return Color.FromArgb(r, g, b);
        }

        [LibraryImport("user32.dll", EntryPoint = "DrawText", StringMarshalling = StringMarshalling.Utf16)]
        private static unsafe partial int _DrawText(nint hdc, string lpchText, int cchText, User32.RECT* lprc, uint format);

        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static unsafe partial int DrawTextExW(nint hdc, string lpchText, int cchText, User32.RECT* lprc, uint format, DRAWTEXTPARAMS* lpdtp);

        /// <summary>
        /// Draws formatted text in the specified rectangle without visual styles. Formats the text according to the specified
        /// method (expanding tabs, justifying characters, breaking lines, and so forth).
        /// </summary>
        /// <param name="gh">The <see cref="Graphics"/> surface to draw on.</param>
        /// <param name="text">A string that specifies the text to be drawn.</param>
        /// <param name="rect">A <see cref="Rectangle"/> structure that contains the rectangle (in logical coordinates) in which the text is to be formatted.</param>
        /// <param name="options">The method of formatting the text.</param>
        /// <returns>If the function succeeds, the return value is the height of the text in logical units. If <see cref="ThemeTextOptions.AlignMiddle"/> or
        /// <see cref="ThemeTextOptions.AlignBottom"/> is specified, the return value is the offset from lpRect->top to the bottom of the drawn text.
        /// If the function fails, the return value is zero.</returns>
        /// <remarks>
        /// <para>Unless the <see cref="ThemeTextOptions.NoClip"/> option is used, DrawText clips the text so that it does not appear outside the specified rectangle.
        /// Note that text with significant overhang may be clipped, for example, an initial "W" in the text string or text that is in italics.
        /// All formatting is assumed to have multiple lines unless the <see cref="ThemeTextOptions.SingleLine"/> option is specified.</para>
        /// 
        /// <para>If the selected font is too large for the specified rectangle, the function does not attempt to substitute a smaller font.</para>
        /// </remarks>
        public static int DrawText(Graphics gh, string text, ref Rectangle rect, ThemeTextOptions options)
        {
            User32.RECT rc = new User32.RECT
            {
                Bottom = rect.Bottom,
                Left = rect.Left,
                Top = rect.Top,
                Right = rect.Right
            };

            unsafe
            {
                var result = _DrawText(gh.GetHdc(), text + "\0", -1, &rc, (uint)options);

                rect = Rectangle.FromLTRB(rc.Left, rect.Top, rc.Right, rc.Bottom);

                return result;
            }
        }

        /// <summary>
        /// Draws formatted text in the specified rectangle without visual styles. Formats the text according to the specified
        /// method (expanding tabs, justifying characters, breaking lines, and so forth).
        /// </summary>
        /// <param name="gh">The <see cref="Graphics"/> surface to draw on.</param>
        /// <param name="text">A string that specifies the text to be drawn.</param>
        /// <param name="rect">A <see cref="Rectangle"/> structure that contains the rectangle (in logical coordinates) in which the text is to be formatted.</param>
        /// <param name="options">The method of formatting the text.</param>
        /// <param name="adv">A <see cref="DrawTextAdvancedOptions"/> structure that specifies additional formatting options.</param>
        /// <returns>If the function succeeds, the return value is the height of the text in logical units. If <see cref="ThemeTextOptions.AlignMiddle"/> or
        /// <see cref="ThemeTextOptions.AlignBottom"/> is specified, the return value is the offset from lpRect->top to the bottom of the drawn text.
        /// If the function fails, the return value is zero.</returns>
        /// <remarks>
        /// <para>Unless the <see cref="ThemeTextOptions.NoClip"/> option is used, DrawText clips the text so that it does not appear outside the specified rectangle.
        /// Note that text with significant overhang may be clipped, for example, an initial "W" in the text string or text that is in italics.
        /// All formatting is assumed to have multiple lines unless the DT_SINGLELINE format is specified.</para>
        /// 
        /// <para>If the selected font is too large for the specified rectangle, the function does not attempt to substitute a smaller font.</para>
        /// </remarks>
        public static int DrawText(Graphics gh, string text, ref Rectangle rect, ThemeTextOptions options, ref DrawTextAdvancedOptions adv)
        {
            User32.RECT rc = new User32.RECT
            {
                Bottom = rect.Bottom,
                Left = rect.Left,
                Top = rect.Top,
                Right = rect.Right
            };

            unsafe
            {
                DRAWTEXTPARAMS dtp = new DRAWTEXTPARAMS();
                dtp.iTabLength = adv.TabLength;
                dtp.iLeftMargin = adv.LeftMargin;
                dtp.iRightMargin = adv.RightMargin;
                dtp.cbSize = (uint)Marshal.SizeOf<DRAWTEXTPARAMS>();
                
                uint uiLengthDrawn = 0;
                dtp.uiLengthDrawn = &uiLengthDrawn;

                var result = DrawTextExW(gh.GetHdc(), text + "\0", -1, &rc, (uint)options, &dtp);

                rect = Rectangle.FromLTRB(rc.Left, rect.Top, rc.Right, rc.Bottom);

                adv.LengthDrawn = uiLengthDrawn;

                return result;
            }
        }

        // this doesn't work
        ///// <summary>
        ///// Draws text using the color and font defined by the visual style.
        ///// </summary>
        ///// <param name="gh">The <see cref="Graphics"/> surface to draw the text on.</param>
        ///// <param name="iPartId">The control part that has the desired text appearance.
        ///// See <see href="../docs/visualstyles.md#aero-parts-and-states">Aero Parts and States</see> for more info. If this value is 0, the text is drawn
        ///// in the default font or a font selected into the device context.</param>
        ///// <param name="iStateId">The control state that has the desired text appearance.
        ///// See <see href="../docs/visualstyles.md#aero-parts-and-states">Aero Parts and States</see> for more info.</param>
        ///// <param name="text">A string containing the text to draw.</param>
        ///// <param name="cchText">An integer containing the number of characters to draw. If this parameter is set to -1, all characters are drawn.</param>
        ///// <param name="options">A combination of <see cref="ThemeTextOptions"/> values specifying options for the string's formatting. </param>
        ///// <param name="rect">A <see cref="Rectangle"/> that contains in logical coordinates, a rectangle in which the text is to be drawn. It is recommended
        ///// to use extentRect from GetThemeTextExent to retrieve the correct coordinates.</param>
        ///// <param name="adv">Advanced options for the drawing process.</param>
        //public void DrawThemeText(Graphics gh, int iPartId, int iStateId, string text, int cchText, ThemeTextOptions options, ref Rectangle rect, ThemeTextAdvancedOptions adv)
        //{
        //    var rc = new User32.RECT
        //    {
        //        Top = rect.Top,
        //        Left = rect.Left,
        //        Bottom = rect.Bottom,
        //        Right = rect.Right
        //    };

        //    unsafe
        //    {
        //        _DTTOPTS dto = new();
        //        dto.iBorderSize = adv.BorderSize;
        //        dto.iGlowSize = adv.GlowSize;
        //        dto.dwSize = (uint)Marshal.SizeOf<_DTTOPTS>();
        //        dto.ptShadowOffset = new POINT
        //        {
        //            x = adv.ShadowOffset.X,
        //            y = adv.ShadowOffset.Y
        //        };
        //        dto.crShadow = ToCOLORREF(adv.ShadowColor);
        //        dto.crBorder = ToCOLORREF(adv.BorderColor);
        //        dto.crText = ToCOLORREF(adv.TextColor);
        //        dto.dwFlags = (uint)adv.Flags;
        //        dto.fApplyOverlay = adv.ApplyOverlay;
        //        dto.iColorPropId = (int)adv.ColorProp;
        //        dto.iFontPropId = (int)adv.FontProp;
        //        dto.iTextShadowType = (int)adv.ShadowType;

        //        var internalStruct = new DTTOPTSLPARAMINTERNAL(); // class (not struct)
        //        internalStruct.ActualCallback = adv.Callback;
        //        internalStruct.LParam = adv.CallbackParam;
        //        GCHandle lParam = GCHandle.Alloc(internalStruct);

        //        dto.lParam = GCHandle.ToIntPtr(lParam);

        //        var pCb = Marshal.GetFunctionPointerForDelegate<DTT_CALLBACK_PROC>(DttCallback);
        //        dto.pfnDrawTextCallback = pCb;

        //        HRESULT hr = (HRESULT)DrawThemeTextEx(_htheme, gh.GetHdc(), iPartId, iStateId, text, cchText, (uint)options, &rc, &dto);

        //        lParam.Free();
        //        GC.KeepAlive(pCb);

        //        if (hr != HRESULT.S_OK)
        //        {
        //            Marshal.ThrowExceptionForHR((int)hr);
        //        }
        //    }
        //}

        //internal int DttCallback(
        //    nint hdc,
        //    [MarshalAs(UnmanagedType.LPWStr)] string pszText,
        //    int cchText,
        //    ref User32.RECT prc,
        //    uint dwFlags,
        //    nint lParam)
        //{
        //    var handle = GCHandle.FromIntPtr(lParam);
        //    var payload = (DTTOPTSLPARAMINTERNAL)handle.Target!;

        //    var rect = Rectangle.FromLTRB(prc.Left, prc.Top, prc.Right, prc.Bottom);

        //    using var g = Graphics.FromHdc(hdc);

        //    if (payload.ActualCallback != null)
        //    {
        //        int val = payload.ActualCallback(
        //            g,
        //            pszText,
        //            cchText,
        //            ref rect,
        //            (ThemeTextAdvancedOptionsFlags)dwFlags,
        //            payload.LParam);

        //        // Write back rect in case callback modified it
        //        prc.Left = rect.Left;
        //        prc.Top = rect.Top;
        //        prc.Right = rect.Right;
        //        prc.Bottom = rect.Bottom;

        //        return val;
        //    }

        //    return 0;
        //}

    }

    ///// <summary>
    ///// A custom class used as the LPARAM member of <see cref="_DTTOPTS"/> that provides the actual LPARAM and more info about the callback.
    ///// </summary>
    //internal class DTTOPTSLPARAMINTERNAL
    //{
    //    public object? LParam;
    //    public ThemeTextCallbackProc? ActualCallback;
    //}

    /// <summary>
    /// Represents the options for <see cref="VisualStyle.DrawThemeText(Graphics, int, int, string, int, ThemeTextOptions, Rectangle)"/>.
    /// </summary>
    [Flags]
    public enum ThemeTextOptions
    {
        /// <summary>
        /// Renders the text string at the bottom of the display rectangle. This value is used only with the <see cref="SingleLine"/> value.
        /// </summary>
        AlignBottom = 0x00000008,
        ///// <summary>
        ///// Determines the width and height of the display rectangle.
        ///// </summary>
        //CalcRect = 0x00000400,
        /// <summary>
        /// Centers text horizontally in the display rectangle.
        /// </summary>
        AlignCenter = 0x00000001,
        /// <summary>
        /// Duplicates the text-displaying characteristics of a multi-line edit control. Specifically, the average character width is calculated
        /// in the same manner as for an edit control, adn the function does not display a partially visible last line.
        /// </summary>
        EditControl = 0x00002000,
        /// <summary>
        /// Truncates a text string that is wider than the display rectangle and adds an ellipsis (...) to indicate the truncation. The string is
        /// not modified unless the <see cref="ModifyString"/> value is also specified.
        /// </summary>
        EndEllipsis = 0x00008000,
        /// <summary>
        /// Expands tab characters. The default number of characters per tab is 8. Cannot be used with the <see cref="EndEllipsis"/>, <see cref="WordEllipsis"/>,
        /// and <see cref="PathEllipsis"/> values.
        /// </summary>
        ExpandTabs = 0x00000040,
        /// <summary>
        /// Include the external leading of a font in the line height. Normally, external leading is not included in the height of a line of text.
        /// </summary>
        ExternalLeading = 0x00000200,
        /// <summary>
        /// Ignore the prefix character &amp; in the text. The letter that follows is not underlined, but other prefix characters are still processed.
        /// For example:
        /// <para>Input string: <c>A&amp;bc&amp;&amp;d</c></para>
        /// <para>Normal: <c>A<u>b</u>c&amp;d</c></para>
        /// <para><see cref="HidePrefix"/>: <c>Abc&amp;d</c></para>
        /// </summary>
        HidePrefix = 0x00100000,
        /// <summary>
        /// Aligns text to the left.
        /// </summary>
        AlignLeft = 0x00000000,
        /// <summary>
        /// Modifies a string to match the displayed text. This value has no effect unless <see cref="EndEllipsis"/> or <see cref="PathEllipsis"/> is specified.
        /// </summary>
        ModifyString = 0x00010000,
        /// <summary>
        /// Draws the text string without clipping the display rectangle.
        /// </summary>
        NoClip = 0x00000100,
        /// <summary>
        /// Prevents a line break at a double-byte character set (DBCS), so that the line-breaking rule is equivalent to single-byte character set (SBCS). This
        /// can be used, for example, to make icon labels written in Korean text more readable. This value has no effect unless <see cref="WordBreak"/> is specified.
        /// </summary>
        NoFullWidthCharBreak = 0x00080000,
        /// <summary>
        /// Turns off processing of prefix characters. Normally, the function interprets the prefix character &amp; as a directive to underscore the character that follows,
        /// and the prefix characters &amp;&amp; as a directive to print a single &amp;. By specifying <see cref="NoPrefix"/>, this processing is turned off. For example:
        /// <para>Input string: <c>A&amp;bc&amp;&amp;d</c></para>
        /// <para>Normal: <c>A<u>b</u>c&amp;d</c></para>
        /// <para><see cref="NoPrefix"/>: <c>A&amp;bc&amp;&amp;d</c></para>
        /// </summary>
        NoPrefix = 0x00000800,
        /// <summary>
        /// Replaces characters in the middle of text with an ellipsis so that the result fits in the display rectangle. If the string contains backslash (\) characters,
        /// <see cref="PathEllipsis"/> preserves as much as possible of the text after the last backslash. The string is not modified unless the <see cref="ModifyString"/>
        /// is specified.
        /// </summary>
        PathEllipsis = 0x00004000,
        /// <summary>
        /// Draws only an underline at the position of the character following the prefix character &amp;. Normally the function interprets the &amp; as a directive to underline
        /// the character that follows and the prefix characters &amp;&amp; as a directive to print a single &amp;. By specifying <see cref="PrefixOnly"/>, no characters are drawn,
        /// only an underline. White spaces are placed in the positions where characters would normally appear. For example:
        /// <para>Input string: <c>A&amp;bc&amp;&amp;d</c></para>
        /// <para>Normal: <c>A<u>b</u>c&amp;d</c></para>
        /// <para><see cref="PrefixOnly"/>: " "</para>
        /// </summary>
        PrefixOnly = 0x00200000,
        /// <summary>
        /// Aligns text to the right.
        /// </summary>
        AlignRight = 0x00000002,
        /// <summary>
        /// Lays out text in right-to-left (RTL) order for bidirectional text, for example, text in a Hebrew or Arabic font. The default direction for text is left-to-right.
        /// </summary>
        RtlReading = 0x00020000,
        /// <summary>
        /// Displays text on a single line. Carriage returns (<c>\r</c>) and line feeds (<c>\n</c>) do not break the line.
        /// </summary>
        SingleLine = 0x00000020,
        /// <summary>
        /// Sets tab stops.
        /// </summary>
        TabStop = 0x00000080,
        /// <summary>
        /// Renders the text at the top of the display rectangle.
        /// </summary>
        AlignTop = 0x00000000,
        /// <summary>
        /// Centers text vertically. This value is used only with the <see cref="SingleLine"/> value.
        /// </summary>
        AlignMiddle = 0x00000004,
        /// <summary>
        /// Breaks lines between words if a word would extend past the edge of the display rectangle. A carriage return/line feed (CR/LF, <c>\r\n</c>) sequence also breaks the line.
        /// </summary>
        WordBreak = 0x00000010,
        /// <summary>
        /// Truncates any word that does not fit in the display rectangle and adds an ellipsis.
        /// </summary>
        WordEllipsis = 0x00040000
    }

    /// <summary>
    /// Represents a structure that contains extended formatting options 
    /// </summary>
    public struct DrawTextAdvancedOptions
    {
        /// <summary>
        /// The size of each tab stop, in units equal to the average character width.
        /// </summary>
        public int TabLength { get; set; }

        /// <summary>
        /// The left margin, in units equal to the average character width.
        /// </summary>
        public int LeftMargin { get; set; }

        /// <summary>
        /// The right margin, in units equal to the average character width.
        /// </summary>
        public int RightMargin { get; set; }

        /// <summary>
        /// Receives the number of characters processed by <see cref="VisualStyle.DrawText(Graphics, string, ref Rectangle, ThemeTextOptions, ref DrawTextAdvancedOptions)"/>,
        /// including white-space characters. The number can be the length of the string or the index of the first line that falls below the drawing area.
        /// Note that <see cref="VisualStyle.DrawText(Graphics, string, ref Rectangle, ThemeTextOptions, ref DrawTextAdvancedOptions)"/> always
        /// processes the entire string if the <see cref="ThemeTextOptions.NoClip"/> formatting flag is specified.
        /// </summary>
        public uint LengthDrawn { get; internal set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct DRAWTEXTPARAMS
    {
        public uint cbSize;
        public int iTabLength;
        public int iLeftMargin;
        public int iRightMargin;
        public uint* uiLengthDrawn;
    }

    //[StructLayout(LayoutKind.Sequential)]
    //internal struct _DTTOPTS
    //{
    //    public uint dwSize;
    //    public uint dwFlags;
    //    public uint crText;
    //    public uint crBorder;
    //    public uint crShadow;
    //    public int iTextShadowType;
    //    public POINT ptShadowOffset;
    //    public int iBorderSize;
    //    public int iFontPropId;
    //    public int iColorPropId;
    //    public int iStateId;
    //    [MarshalAs(UnmanagedType.Bool)]
    //    public bool fApplyOverlay;
    //    public int iGlowSize;
    //    public nint pfnDrawTextCallback;
    //    public nint lParam;
    //}

    //internal delegate int DTT_CALLBACK_PROC(nint hdc, [MarshalAs(UnmanagedType.LPWStr)] string pszText, int cchText, ref User32.RECT prc, uint dwFlags, nint lParam);

    ///// <summary>
    ///// A callback procedure for <see cref="VisualStyle.DrawThemeText(Graphics, int, int, string, int, ThemeTextOptions, ref Rectangle, ThemeTextAdvancedOptions)"/>.
    ///// </summary>
    ///// <param name="gh">The <see cref="Graphics"/> surface on which the text is being drawn.</param>
    ///// <param name="text">The text to draw.</param>
    ///// <param name="cchText">The amount of characters to draw.</param>
    ///// <param name="rc">The rectangle on which to draw.</param>
    ///// <param name="flags">The value passed to the <see cref="ThemeTextAdvancedOptions.Flags"/> member.</param>
    ///// <param name="param">The value passed to the <see cref="ThemeTextAdvancedOptions.CallbackParam"/> member.</param>
    ///// <returns>The amount of characters that have been drawn, or 0 to indicate that default processing should be performed.</returns>
    //public delegate int ThemeTextCallbackProc(Graphics gh, string text, int cchText, ref Rectangle rc, ThemeTextAdvancedOptionsFlags flags, object? param);

    ///// <summary>
    ///// Specifies advanced options for <see cref="VisualStyle.DrawThemeText(Graphics, int, int, string, int, ThemeTextOptions, ref Rectangle, ThemeTextAdvancedOptions)"/>.
    ///// </summary>
    //public struct ThemeTextAdvancedOptions
    //{
    //    /// <summary>
    //    /// A combination of flags that specify whether certain values of the structure have been specified, how to interpret them, and how to render the final text.
    //    /// </summary>
    //    public ThemeTextAdvancedOptionsFlags Flags { get; set; }

    //    /// <summary>
    //    /// Specifies the color of the text that will be drawn.
    //    /// </summary>
    //    public Color TextColor { get; set; }

    //    /// <summary>
    //    /// Specifies the color of the outline that will be drawn around the text.
    //    /// </summary>
    //    public Color BorderColor { get; set; }

    //    /// <summary>
    //    /// Specifies the color of the shadow that will be drawn behind the text.
    //    /// </summary>
    //    public Color ShadowColor { get; set; }

    //    /// <summary>
    //    /// Specifies the type of the shadow that will be drawn behind the text. This member can be one of the following values.
    //    /// </summary>
    //    public TextShadowType ShadowType { get; set; }

    //    /// <summary>
    //    /// Specifies the amount of offset, in logical coordinates, between the shadow and the text.
    //    /// </summary>
    //    public Point ShadowOffset { get; set; }

    //    /// <summary>
    //    /// Specifies the radius of the outline that wil be drawn around the text.
    //    /// </summary>
    //    public int BorderSize { get; set; }

    //    /// <summary>
    //    /// Specifies an alternate font property to use when drawing text.
    //    /// </summary>
    //    /// <remarks>
    //    /// If this value is valid and the corresponding flag is set in <see cref="Flags"/>, this value will override the value of <see cref="TextColor"/>.
    //    /// </remarks>
    //    public SystemFontId FontProp { get; set; }

    //    /// <summary>
    //    /// Specifies an alternate color property to use when drawing text.
    //    /// </summary>
    //    /// <remarks>
    //    /// If this value is valid and the corresponding flag is set in <see cref="Flags"/>, this value will override the value of <see cref="TextColor"/>.
    //    /// </remarks>
    //    public SystemColorId ColorProp { get; set; }

    //    /// <summary>
    //    /// If <see langword="true"/>, text will be drawn on top of the shadow and outline effects. If <see langword="false"/>, just the shadow and outline
    //    /// effects will be drawn.
    //    /// </summary>
    //    public bool ApplyOverlay { get; set; }

    //    /// <summary>
    //    /// Specifies the size of a glow that will be drawn on the background prior to any text being drawn.
    //    /// </summary>
    //    public int GlowSize { get; set; }

    //    /// <summary>
    //    /// A callback function for <see cref="VisualStyle.DrawThemeText(Graphics, int, int, string, int, ThemeTextOptions, ref Rectangle, ThemeTextAdvancedOptions)"/>.
    //    /// </summary>
    //    public ThemeTextCallbackProc Callback { get; set; }

    //    /// <summary>
    //    /// Parameter for callback function specified in <see cref="Callback"/>.
    //    /// </summary>
    //    public object? CallbackParam { get; set; }
    //}

    ///// <summary>
    ///// The mask of <see cref="ThemeTextAdvancedOptions"/>.
    ///// </summary>
    //public enum ThemeTextAdvancedOptionsFlags : ulong
    //{
    //    /// <summary>
    //    /// The <see cref="ThemeTextAdvancedOptions.TextColor"/> member value is valid.
    //    /// </summary>
    //    TextColor = 1,
    //    /// <summary>
    //    /// The <see cref="ThemeTextAdvancedOptions.BorderColor"/> member value is valid.
    //    /// </summary>
    //    BorderColor = 1UL << 1,
    //    /// <summary>
    //    /// The <see cref="ThemeTextAdvancedOptions.ShadowColor"/> member value is valid.
    //    /// </summary>
    //    ShadowColor = 1UL << 2,
    //    /// <summary>
    //    /// The <see cref="ThemeTextAdvancedOptions.ShadowType"/> member value is valid.
    //    /// </summary>
    //    ShadowType = 1UL << 3,
    //    /// <summary>
    //    /// The <see cref="ThemeTextAdvancedOptions.ShadowOffset"/> member value is valid.
    //    /// </summary>
    //    ShadowOffset = 1UL << 4,
    //    /// <summary>
    //    /// The <see cref="ThemeTextAdvancedOptions.BorderSize"/> member value is valid.
    //    /// </summary>
    //    BorderSize = 1UL << 5,
    //    /// <summary>
    //    /// The FontProp member value is valid.
    //    /// </summary>
    //    FontProp = 1UL << 6,
    //    /// <summary>
    //    /// The ColorProp member value is valid.
    //    /// </summary>
    //    ColorProp = 1UL << 7,
    //    /// <summary>
    //    /// The <c>rect</c> parameter of the <see cref="VisualStyle.DrawThemeText(Graphics, int, int, string, int, ThemeTextOptions, ref Rectangle, ThemeTextAdvancedOptions)"/> function that uses the <see cref="ThemeTextAdvancedOptions"/> structure
    //    /// will be used as both an in and out parameter. After the function returns, the <c>rect</c> parameter will contain the rectangle that corresponds to the region calculated to be drawn.
    //    /// </summary>
    //    CalcRect = 1UL << 9,
    //    /// <summary>
    //    /// The <see cref="ThemeTextAdvancedOptions.ApplyOverlay"/> member value is valid.
    //    /// </summary>
    //    ApplyOverlay = 1UL << 10,
    //    /// <summary>
    //    /// The <see cref="ThemeTextAdvancedOptions.GlowSize"/> member value is valid.
    //    /// </summary>
    //    GlowSize = 1UL << 11,
    //    /// <summary>
    //    /// The <see cref="ThemeTextAdvancedOptions.Callback"/> member value is valid.
    //    /// </summary>
    //    Callback = 1UL << 12,
    //    /// <summary>
    //    /// Draws text with antialiased alpha. Use of this option requires a top-down DIB (Device-Independent Bitmap) section. This flag works only if the <see cref="Graphics"/> instance pass to the function has a top-down DIB section currently selected in it. For more information, see
    //    /// <see href="https://learn.microsoft.com/windows/desktop/gdi/device-independent-bitmaps">Device-Independent Bitmaps</see>
    //    /// </summary>
    //    Composited = 1UL << 13,
    //    /// <summary>
    //    /// A combination of all valid values.
    //    /// </summary>
    //    All = TextColor | BorderColor | ShadowColor | ShadowType | ShadowOffset | BorderSize | FontProp | ColorProp | CalcRect | ApplyOverlay | GlowSize | Composited
    //}

    ///// <summary>
    ///// Specifies the shadow type for <see cref="ThemeTextAdvancedOptions"/>.
    ///// </summary>
    //public enum TextShadowType
    //{
    //    /// <summary>
    //    /// No shadow will be drawn.
    //    /// </summary>
    //    None = 0,
    //    /// <summary>
    //    /// The shadow will be drawn to appear detailed underneath text.
    //    /// </summary>
    //    Single = 1,
    //    /// <summary>
    //    /// The shadow will be drawn to appear blurred underneath text.
    //    /// </summary>
    //    Continuous = 2
    //}

    /// <summary>
    /// Defines the identifiers for system fonts.
    /// </summary>
    public enum SystemFontId
    {
        /// <summary>
        /// The font used by window captions.
        /// </summary>
        CaptionFont = 801,
        /// <summary>
        /// The font used by window small captions.
        /// </summary>
        SmallCaptionFont = 802,
        /// <summary>
        /// The font used by menus.
        /// </summary>
        MenuFont = 803,
        /// <summary>
        /// The font used in status messages.
        /// </summary>
        StatusFont = 804,
        /// <summary>
        /// The font used to display messages in a message box.
        /// </summary>
        MessageBoxFont = 805,
        /// <summary>
        /// The font used for icons.
        /// </summary>
        IconTitleFont = 806
    }

    /// <summary>
    /// Defines the identifiers for system colors.
    /// </summary>
    public enum SystemColorId
    {
        /// <summary>
        /// Dark shadow for 3D display elements.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        Dark3DShadow = 21,
        /// <summary>
        /// Face color for 3D display elements and dialog box backgrounds.
        /// </summary>
        Face3D = 15,
        /// <summary>
        /// Highlight color for 3D display elements (for edges facing the light source.)
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        Highlight3D = 20,
        /// <summary>
        /// Highlight color for 3D display elements (for edges facing the light source.)
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        HiLight3D = Highlight3D,
        /// <summary>
        /// Light color for 3D display elements (for edges facing the light source.)
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        Light3D = 22,
        /// <summary>
        /// Shadow color for 3D display elements (for edges facing away from light source.)
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        Shadow3D = 16,
        /// <summary>
        /// Active window border color.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        ActiveBorder = 10,
        /// <summary>
        /// Active window title bar color. The associated foreground color is <see cref="CaptionText"/>.
        /// </summary>
        /// <remarks>
        /// <para>Specifies the left-side color in the color gradient of an active window's title bar if the gradient effect is enabled.</para>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        ActiveCaption = 2,
        /// <summary>
        /// Background color of Multi Document Interface (MDI) applications.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        AppWorkSpace = 12,
        /// <summary>
        /// Desktop background color.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        Background = 1,
        /// <summary>
        /// Face color for 3D display elements and for dialog box backgrounds. The associated foreground color is <see cref="ButtonText"/>.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        ButtonFace = 15,
        /// <summary>
        /// Highlight color for 3D display elements (for edges facing the light source.)
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        ButtonHighlight = 20,
        /// <summary>
        /// Highlight color for 3D display elements (for edges facing the light source.)
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        ButtonHiLight = 20,
        /// <summary>
        /// Text on push buttons. The associated background color is <see cref="ButtonFace"/>. 
        /// </summary>
        ButtonText = 18,
        /// <summary>
        /// Text in caption, size box, and scroll bar arrow box. The associated background color is <see cref="ActiveCaption"/>.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        CaptionText = 9,
        /// <summary>
        /// Desktop background color.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        Desktop = Background,
        /// <summary>
        /// Right side color in the color gradient of an active window's title bar. <see cref="ActiveCaption"/> specifies the left side color.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        GradientActiveCaption = 27,
        /// <summary>
        /// Right side color in the color gradient of an inactive window's title bar. <see cref="InactiveCaption"/> specifies the left side color.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        GradientInactiveCaption = 28,
        /// <summary>
        /// Grayed (disabled) text. This color is set to 0 if the current display driver does not support a solid gray color.
        /// </summary>
        GrayText = 17,
        /// <summary>
        /// Item(s) selected in a control. The associated foreground color is <see cref="HighlightText"/>.
        /// </summary>
        Highlight = 13,
        /// <summary>
        /// Text of item(s) selected in a control. The associated background color is <see cref="Highlight"/>.
        /// </summary>
        HighlightText = 14,
        /// <summary>
        /// Color for a hyperlink or hot-tracked item. The associated background clor is <see cref="SystemColorId.Window"/>.
        /// </summary>
        HotLight = 26,
        /// <summary>
        /// Inactive window border.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        InactiveBorder = 11,
        /// <summary>
        /// Inactive window caption. The associated foreground color is <see cref="InactiveCaptionText"/>. Specifies the left-side color in the color gradient of
        /// an inactive window's title bar if the gradient effect is enabled.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        InactiveCaption = 3,
        /// <summary>
        /// Color of text in an inactive caption. The associated background color is <see cref="InactiveCaption"/>.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        InactiveCaptionText = 19,
        /// <summary>
        /// Background color for tooltip controls. The associated foreground color is <see cref="InfoText"/>.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        InfoBackground = 24,
        /// <summary>
        /// Text color for tooltip controls. The associated background color is <see cref="InfoBackground"/>.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        InfoText = 23,
        /// <summary>
        /// Menu background. The associated foreground color is <see cref="MenuText"/>.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        Menu = 4,
        /// <summary>
        /// The color used to highlight menu items when the menu appears as a flat menu. The highlighted menu item is outlined with <see cref="Highlight"/>.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        [UnsupportedOSPlatform("windows5.0")]
        [SupportedOSPlatform("windows5.1")]
        MenuHiLight = 29,
        /// <summary>
        /// The background color for the menu bar when menus appear as flat menus. However, <see cref="Menu"/> continues to specify the background color of the
        /// menu popup.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        [UnsupportedOSPlatform("windows5.0")]
        [SupportedOSPlatform("windows5.1")]
        MenuBar = 30,
        /// <summary>
        /// Text in menus. The associated background color is <see cref="Menu"/>.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        MenuText = 7,
        /// <summary>
        /// Scroll bar gray area.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        ScrollBar = 0,
        /// <summary>
        /// Window background. The associated foreground colors are <see cref="WindowText"/> and <see cref="HotLight"/>.
        /// </summary>
        Window = 5,
        /// <summary>
        /// Window frame.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > <b>Windows 10 or greater:</b> This value is not supported.
        /// </remarks>
        [UnsupportedOSPlatform("windows10.0")]
        WindowFrame = 6,
        /// <summary>
        /// Text in windows. The associated background color is <see cref="Window"/>.
        /// </summary>
        WindowText = 8
    }
}