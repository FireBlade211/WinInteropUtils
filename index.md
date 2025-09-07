# WinInteropUtils
**WinInteropUtils** is a C# library written in .NET 8 that provides managed P/Invoke wrappers for tons of functions you can use, from reading file attributes to interacting with COM interfaces.

The base namespace  is `FireBlade.WinInteropUtils`.

> [!WARNING]
> WinInteropUtils is still in beta, so bugs may occur. If you experience any bugs, please report them on the [Issues](https://github.com/FireBlade211/WinInteropUtils/issues/new?template=BUG_REPORT.yaml) page.

## How to install (for now)
Because **WinInteropUtils** isn't a NuGet package yet, to use it, simply download the DLL file, and go into the add references dialog, select Browse, and open the DLL. For a more detailed guide visit the [Installation section of the Getting Started page.](getting-started.md#installation)
> [!WARNING]
> **WinInteropUtils** is built on .NET 8, so it won't work for .NET Framework projects.