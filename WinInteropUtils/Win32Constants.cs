namespace FireBlade.WinInteropUtils
{
    /// <summary>
    /// Defines common Win32 constants.
    /// </summary>
    public static class Win32Constants
    {
        /// <summary>
        /// Defines the <c>FACILITY_WIN32</c> macro from <c>winerror.h</c>.
        /// </summary>
        public const int FACILITY_WIN32 = 7;

        /// <summary>
        /// Defines the <c>MAX_PATH</c> constant, being the maximum length of a file path for applications that don't have long file path support
        /// (unless overriden with the <c>\\?\</c> UNC path prefix).
        /// </summary>
        public const int MAX_PATH = 260;
    }
}
