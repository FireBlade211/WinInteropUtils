using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;

namespace FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces
{
    /// <summary>
    /// Exposes a method that represents a modal window.
    /// </summary>
    [GeneratedComInterface]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("b4db1657-70d7-485e-8e3e-6fcb5a5c1802")]
    [SupportedOSPlatform("windows5.1")] // XP
    public partial interface IModalWindow : IUnknown
    {
        // HRESULT Show(HWND hwndOwner);
        /// <summary>
        /// Launches the modal window.
        /// </summary>
        /// <param name="hwndOwner">The handle of the owner window. This value can be <see langword="null"/>.</param>
        /// <returns>If the method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code,
        /// including Macros.HResultFromWin32(<see cref="Win32ErrorCode.ERROR_CANCELLED"/>), indicating the user closed the
        /// window by cancelling the operation.</returns>
        [PreserveSig]
        public HRESULT Show(nint hwndOwner);
    }
}
