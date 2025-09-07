# WinInteropUtils
**WinInteropUtils** is a C# library written in .NET 8 that provides managed P/Invoke wrappers for tons of functions you can use, from reading file attributes to interacting with COM interfaces.

The base namespace  is `FireBlade.WinInteropUtils`.

> [!WARNING]
> WinInteropUtils is still in beta, so bugs may occur. If you experience any bugs, please report them on the [Issues](https://github.com/FireBlade211/WinInteropUtils/issues/new?template=BUG_REPORT.yaml) page.

## How to install (for now)
Because **WinInteropUtils** isn't a NuGet package yet, to use it, simply download the DLL file, and go into the add references dialog, select Browse, and open the DLL. For a more detailed guide visit the [Installation section of the Getting Started page.](getting-started.md#installation)
> [!WARNING]
> **WinInteropUtils** is built on .NET 8, so it won't work for .NET Framework projects.

## Features
<!-- For some reason DocFX uses this weird link format -->
- [WindowsFile](../api/FireBlade.WinInteropUtils.WindowsFile.html) file info wrapper
- Full enums (1600+ values) for [HRESULT](../api/FireBlade.WinInteropUtils.HRESULT.html)s and [Win32 error codes](../api/FireBlade.WinInteropUtils.Win32ErrorCode.html)
- Dialog boxes
- Useful utility [User32](../api/FireBlade.WinInteropUtils.User32.html) functions ([SendMessage](../api/FireBlade.WinInteropUtils.User32.html#FireBlade_WinInteropUtils_User32_SendMessage_System_IntPtr_System_UInt32_System_UIntPtr_System_IntPtr_), [SetWindowLongPtr](../api/FireBlade.WinInteropUtils.User32.html#FireBlade_WinInteropUtils_User32_SetWindowLongPtr_System_IntPtr_System_Int32_System_IntPtr_)...)
- [Constants](../api/FireBlade.WinInteropUtils.Win32Constants.html) and [Macros](../api/FireBlade.WinInteropUtils.Macros.html)
- [COM interfaces](using-com.md)
- Icon management ([DestroyIcon](../api/FireBlade.WinInteropUtils.Shell32.html#FireBlade_WinInteropUtils_Shell32_DestroyIcon_System_IntPtr_), ...)