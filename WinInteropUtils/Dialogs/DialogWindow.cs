namespace FireBlade.WinInteropUtils.Dialogs
{
    /// <summary>
    /// Represents a dialog window that can be shown modally or modelessly. This class is abstract.
    /// </summary>
    /// <typeparam name="TShowReturn">The return type of the <see cref="Show()"/> and <see cref="Show(nint)"/> methods.</typeparam>
    /// <typeparam name="TSelf">The class that is inheriting <see cref="DialogWindow{TShowReturn, TSelf}"/>.</typeparam>
    public abstract class DialogWindow<TShowReturn, TSelf> where TSelf : DialogWindow<TShowReturn, TSelf>
    {
        /// <summary>
        /// The message sent to a dialog's hook procedure when it initializes.
        /// </summary>
        protected const int WM_INITDIALOG = 0x0110;

        /// <summary>
        /// Packs the specified flags for use in a dialog's struct's <c>dwFlags</c> or <c>Flags</c> member.
        /// </summary>
        /// <param name="flags">The dictionary of flags to pack. The key indicates whether to pack the flag, and the value indicates the value of the flag.</param>
        /// <returns>The packed flags, or <c>0</c> if no flags were marked as packable.</returns>
        protected int PackFlags(Dictionary<int, bool> flags)
        {
            var flag = 0;

            foreach (var kvp in flags)
            {
                if (kvp.Value)
                    flag |= kvp.Key;
            }

            return flag;
        }

        /// <summary>
        /// When overriden in a dervied class, shows the dialog window.
        /// </summary>
        /// <returns>The result of the dialog.</returns>
        public abstract TShowReturn Show();

        /// <summary>
        /// When overriden in a dervied class, shows the dialog window modally.
        /// </summary>
        /// <returns>The result of the dialog.</returns>
        /// <param name="hWnd">The handle of the owner window.</param>
        public abstract TShowReturn Show(nint hWnd);

        /// <summary>
        /// Shows the <typeparamref name="TSelf"/>.
        /// </summary>
        /// <param name="dlg">The <typeparamref name="TSelf"/> to show.</param>
        /// <returns>The result of the dialog.</returns>
        public static TShowReturn Show(TSelf dlg) => dlg.Show();

        /// <summary>
        /// Shows the <typeparamref name="TSelf"/> modally.
        /// </summary>
        /// <returns>The result of the dialog.</returns>
        /// <param name="dlg">The <typeparamref name="TSelf"/> to show.</param>
        /// <param name="hWnd">The handle of the owner window.</param>
        public static TShowReturn Show(TSelf dlg, nint hWnd) => dlg.Show(hWnd);
    }
}
