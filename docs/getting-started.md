# Getting Started
WinInteropUtils provides many wrappers for P/Invoke functions, from simple functions to COM interfaces. This guide will show you how to install and use this library.

> [!WARNING]
> WinInteropUtils is still in beta, so bugs may occur. If you experience any bugs, please report them on the [Issues](https://github.com/FireBlade211/WinInteropUtils/issues/new?template=BUG_REPORT.yaml) page.

## Installation
1. Download the latest DLL from the [Releases](https://github.com/FireBlade211/WinInteropUtils/releases) page.
2. In your Visual Studio project, right-click **References** and select **Add Project Reference**.
3. In the dialog, select the **Browse** tab from the sidebar.
4. Click the **Browse...** button in the bottom-right corner.
5. In the file dialog, browse to the DLL you downloaded and select it.
6. Back in the **Reference Manager** dialog, make sure the DLL name is checked.
7. Finally, press **OK**.

## Usage Examples
### Reading file attributes
This example reads the attributes of a file named `C:\test.txt` and checks if the file is hidden.

```cs
// Get the WindowsFile
var winFile = Shell32.GetFileInfo(@"C:\test.txt");

if (winFile != null)
{
    // Always make sure to dispose WindowsFiles when you're done using them
    // to avoid leaks on GDI handles!
    using WindowsFile file = winFile;

    // Check the attributes
    Console.WriteLine(file.Attributes.HasFlag(WindowsFileAttributes.Hidden)
    ? "File is hidden!"
     : "File is visible...");
}
else
{
    Console.WriteLine("File not found or read error");
}
```

### Showing dialog boxes
The following examples show how to display common dialog boxes.

```cs
// ShellAbout dialog box (for an example run the winver command)
// icon: either a System.Drawing.Icon or nint GDI icon handle

Shell32.ShellAbout(hWnd, "My app name", "Some additional info", myAppIcon);
Shell32.ShellAbout("My app name", "Some additional info"); // without an icon and hwnd

// Icon picker dialog (PickIconDlg)

Shell32.ShowPickIconDialog(@"%SYSTEMROOT%\System32\shell32.dll", 16, out Icon ic);
Shell32.ShowPickIconDialog(@"%SYSTEMROOT%\System32\imageres.dll", 48, out string path, out int idx);

// File properties dialog (doesn't support hwnd because that's how Windows works)

Shell32.ShowFileProperties(@"C:\test.txt");
```

### Using COM interfaces
This example shows how to use COM interfaces.

> [!TIP]
> For more info, see [Using COM](using-com.md).

```cs
using FireBlade.WinInteropUtils.ComponentObjectModel; // make sure to add this for COM!

HRESULT hr = COM.Initialize(CoInit.ApartmentThreaded);

if (Macros.Succeeded(hr))
{
    hr = COM.CreateInstance<IFileOpenDialog>(
        new Guid("84bccd23-5fde-4cdb-aea4-af64b83d78ab"),
        null,
        CreateInstanceContext.InprocServer,
        new Guid("84bccd23-5fde-4cdb-aea4-af64b83d78ab"),
        out IFileOpenDialog dlg);

    if (Macros.Succeeded(hr))
    {
        dlg.SetTitle("My File Dialog - Open a File");

        hr = dlg.Show(hWnd);

        if (Macros.Succeeded(hr))
        {
            Console.WriteLine("Dialog accepted!");
        }
    }

    COM.Uninitialize();
}
```

## API
For more documentation, visit the [API](../api/FireBlade.WinInteropUtils.html) page.