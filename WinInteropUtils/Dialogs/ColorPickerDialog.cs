using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace FireBlade.WinInteropUtils.Dialogs
{
    /// <summary>
    /// Represents a dialog that allows the user to pick a color.
    /// </summary>
    public class ColorPickerDialog : DialogWindow<Color?, ColorPickerDialog>
    {
        private const int CC_ENABLEHOOK = 0x00000010;
        private const int CC_ANYCOLOR = 0x00000100;
        private const int CC_SOLIDCOLOR = 0x00000080;
        private const int CC_RGBINIT = 0x00000001;
        private const int CC_SHOWHELP = 0x00000008;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CHOOSECOLORW
        {
            public uint lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public uint rgbResult;
            public IntPtr lpCustColors;
            public uint Flags;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string? lpTemplateName;
        }

        [DllImport("comdlg32.dll", EntryPoint = "ChooseColorW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ChooseColor(ref CHOOSECOLORW lpcc);

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

        /// <summary>
        /// Represents the collection of custom colors.
        /// </summary>
        /// <remarks>
        /// When the <see cref="Show()"/> or <see cref="Show(nint)"/> functions return, this property will contain the custom colors
        /// set by the user. You can store this array to persist the custom colors between sessions. The array has a capacity of <c>16</c>.
        /// </remarks>
        public Color[] CustomColors { get; set; } = new Color[16];

        /// <summary>
        /// Specifies the default color chosen in the dialog.
        /// </summary>
        public Color? DefaultColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the hook procedure defined in <see cref="HookProcedure"/>.
        /// </summary>
        public bool UseHookProc { get; set; } = false;

        /// <summary>
        /// Specifies the dialog hook procedure when <see cref="UseHookProc"/> is equal to <see langword="true"/>.
        /// </summary>
        public ColorDialogHookProc? HookProcedure { get; set; }

        /// <summary>
        /// If <see langword="true"/>, causes the dialog box to display all available colors in the set of basic colors.
        /// </summary>
        public bool AnyColor { get; set; } = false;

        /// <summary>
        /// If <see langword="true"/>, causes the dialog box to display only solid colors in the set of basic colors.
        /// </summary>
        public bool SolidColorsOnly { get; set; } = false;

        /// <summary>
        /// Specifies how the dialog handles opening the full custom color picker.
        /// </summary>
        public ColorPickerDialogFullOpenMode FullOpenMode { get; set; } = ColorPickerDialogFullOpenMode.Enabled;

        /// <summary>
        /// Indicates whether to show the <b>Help</b> button.
        /// </summary>
        /// <remarks>
        /// To implement help, set the <see cref="ShowHelp"/> property to <see langword="true"/>, and call
        /// <see cref="User32.RegisterWindowMessage(string)"/>, passing in the string <c>commdlg_help</c>. Then, store the value returned by the function and check
        /// for it in your window procedure.
        /// </remarks>
        public bool ShowHelp { get; set; } = false;

        /// <summary>
        /// Shows the color dialog modelessly.
        /// </summary>
        /// <returns>The chosen <see cref="Color"/>, or <see langword="null"/> if the user cancelled the dialog.</returns>
        public override Color? Show() => Show(nint.Zero);

        /// <summary>
        /// Shows the color dialog modally.
        /// </summary>
        /// <returns>The chosen <see cref="Color"/>, or <see langword="null"/> if the user cancelled the dialog.</returns>
        public override Color? Show(nint hwnd)
        {
            CHOOSECOLORW ccw = new CHOOSECOLORW();
            ccw.lStructSize = (uint)Marshal.SizeOf<CHOOSECOLORW>();
            ccw.hwndOwner = hwnd;
            ccw.Flags = (uint)PackFlags(new() { { CC_ANYCOLOR, AnyColor  }, { CC_SOLIDCOLOR, SolidColorsOnly }, { CC_RGBINIT, DefaultColor != null },
                { CC_SHOWHELP, ShowHelp } }) | (uint)FullOpenMode;
            ccw.rgbResult = DefaultColor != null ? ToCOLORREF((Color)DefaultColor) : 0;

            int size = Marshal.SizeOf<uint>() * CustomColors.Length;

            // Allocate unmanaged memory
            IntPtr pCustColors = Marshal.AllocHGlobal(size);

            var managedArray = Array.ConvertAll(CustomColors.Select(ToCOLORREF).ToArray(), x => (int)x);

            // Copy the managed array into unmanaged memory
            Marshal.Copy(managedArray, 0, pCustColors, managedArray.Length);

            ccw.lpCustColors = pCustColors;

            ColorDialogHookProc? del = null;
            GCHandle? lastHandle = null;

            if (UseHookProc && HookProcedure != null)
            {
                ccw.Flags |= CC_ENABLEHOOK;

                del = nuint (nint hDlg, uint uMsg, nuint wParam, nint lParam) =>
                {
                    switch (uMsg)
                    {
                        case WM_INITDIALOG:
                            lastHandle?.Free();

                            lastHandle = GCHandle.Alloc(this);

                            return HookProcedure(hDlg, uMsg, wParam, GCHandle.ToIntPtr((GCHandle)lastHandle));
                    }

                    return HookProcedure(hDlg, uMsg, wParam, lParam);
                };

                ccw.lpfnHook = Marshal.GetFunctionPointerForDelegate(del);
            }

            var result = ChooseColor(ref ccw);

            Marshal.Copy(pCustColors, managedArray, 0, managedArray.Length);

            for (int i = 0; i < managedArray.Length; i++)
            {
                CustomColors[i] = FromCOLORREF((uint)managedArray[i]);
            }

            // Free memory
            Marshal.FreeHGlobal(pCustColors);

            GC.KeepAlive(del);
            lastHandle?.Free();

            return result ? FromCOLORREF(ccw.rgbResult) : null;
        }

        /// <summary>
        /// Shows the color dialog modelessly.
        /// </summary>
        /// <param name="defaultColor">The default color.</param>
        /// <returns>The chosen color.</returns>
        /// <remarks>For more customization, create an instance of the <see cref="ColorPickerDialog"/> class.</remarks>
        public static Color? Show(Color defaultColor) => new ColorPickerDialog
        {
            DefaultColor = defaultColor
        }.Show();

        /// <summary>
        /// Shows the color dialog modally.
        /// </summary>
        /// <param name="hwnd">The owner window handle.</param>
        /// <param name="defaultColor">The default color.</param>
        /// <returns>The chosen color.</returns>
        /// <remarks>For more customization, create an instance of the <see cref="ColorPickerDialog"/> class.</remarks>
        public static Color? Show(nint hwnd, Color defaultColor) => new ColorPickerDialog
        {
            DefaultColor = defaultColor
        }.Show(hwnd);

        /// <summary>
        /// Shows the color dialog modally.
        /// </summary>
        /// <param name="hwnd">The owner window handle.</param>
        /// <param name="defaultColor">The default color.</param>
        /// <param name="customColors">The array of custom colors in the dialog.</param>
        /// <returns>The chosen color.</returns>
        /// <remarks>For more customization, create an instance of the <see cref="ColorPickerDialog"/> class.</remarks>
        public static Color? Show(nint hwnd, Color defaultColor, Color[] customColors) => new ColorPickerDialog
        {
            DefaultColor = defaultColor,
            CustomColors = customColors
        }.Show(hwnd);

        /// <summary>
        /// Shows the color dialog modelessly.
        /// </summary>
        /// <param name="defaultColor">The default color.</param>
        /// <param name="customColors">The array of custom colors in the dialog.</param>
        /// <returns>The chosen color.</returns>
        /// <remarks>For more customization, create an instance of the <see cref="ColorPickerDialog"/> class.</remarks>
        public static Color? Show(Color defaultColor, Color[] customColors) => new ColorPickerDialog
        {
            DefaultColor = defaultColor,
            CustomColors = customColors
        }.Show();

        /// <summary>
        /// Shows the <see cref="ColorPickerDialog"/>.
        /// </summary>
        /// <param name="dlg">The <see cref="ColorPickerDialog"/> to show.</param>
        /// <returns>The result of the dialog.</returns>
        public static new Color? Show(ColorPickerDialog dlg) => DialogWindow<Color?, ColorPickerDialog>.Show(dlg);

        /// <summary>
        /// Shows the <see cref="ColorPickerDialog"/> modally.
        /// </summary>
        /// <param name="dlg">The <see cref="ColorPickerDialog"/> to show.</param>
        /// <param name="hwnd">The owner window handle.</param>
        /// <returns>The result of the dialog.</returns>
        public static new Color? Show(ColorPickerDialog dlg, nint hwnd) => DialogWindow<Color?, ColorPickerDialog>.Show(dlg, hwnd);
    }

    /// <summary>
    /// Defines the hook procedure for a <see cref="ColorPickerDialog"/>.
    /// </summary>
    /// <param name="hDlg">A handle to the Color dialog box for which the message is intended.</param>
    /// <param name="uMsg">The identifier of the message being received.</param>
    /// <param name="wParam">Additional information about the message. The exact meaning depends on the value of <paramref name="uMsg"/>.</param>
    /// <param name="lParam">Additional information about the message. The exact meaning depends on the value of <paramref name="uMsg"/>.</param>
    /// <returns>If the hook procedure returns zero, the default dialog box procedure processes the message.
    ///
    /// If the hook procedure returns a nonzero value, the default dialog box procedure ignores the message.</returns>
    /// <remarks>
    /// If the <paramref name="uMsg"/> parameter indicates the WM_INITDIALOG message, then <paramref name="lParam"/> is a garbage collector handle pointing to
    /// the instance of the <see cref="ColorPickerDialog"/> class. To get the instance, call the <see cref="GCHandle.FromIntPtr(nint)"/> function and pass in the given
    /// <paramref name="lParam"/>.
    /// </remarks>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate nuint ColorDialogHookProc(nint hDlg, uint uMsg, nuint wParam, nint lParam);

    /// <summary>
    /// Defines how a <see cref="ColorPickerDialog"/> handles opening the full custom color picker.
    /// </summary>
    public enum ColorPickerDialogFullOpenMode
    {
        /// <summary>
        /// The <c>Define Custom Colors</c> button is disabled - the user can only pick basic colors.
        /// </summary>
        Disabled = 0x00000004,
        /// <summary>
        /// The <c>Define Custom Colors</c> button is enabled, but the full color picker is not opened.
        /// </summary>
        Enabled = 0,
        /// <summary>
        /// The full custom color picker is opened.
        /// </summary>
        Opened = 0x00000002
    }
}