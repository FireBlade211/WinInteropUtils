# Using COM
**COM** (Component-Object Model) is a Windows framework that allows for reusable components in your code. This guide will go over how to use COM in WinInteropUtils.

> [!WARNING]
> **COM** is for advanced users, so there's manual memory management and other advanced things. Wrappers for COM interfaces will come in future releases, but for now this is the only option.

## Basic Setup
To begin using COM, you need to import the appropirate COM namespaces into your script:
```cs
using FireBlade.WinInteropUtils.ComponentObjectModel; // for base COM class and additional future COM non-interface wrappers
using FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces; // for COM interfaces

using FireBlade.WinInteropUtils; // base WinInteropUtils namespace, required for Macros (useful for COM because we need to check a lot of HRESULTs)
```

Now we're ready to use COM. Over the course of this guide we will build up a sample of a file dialog. Before using COM interfaces, we need to initialize COM:

```cs
// if you're using v0.11 or earlier the function is named COM.CoInitialize instead
HRESULT hr = COM.Initialize(CoInit.ApartmentThreaded); // or MultiThreaded if inside a non-GUI/non-STA app
```

Now, after initializng COM, we need to check if the initialization succeeded:
```cs
if (Macros.Succeeded(hr))
{
    // init succeeded: proceeded with COM

    // our COM code...
}
```

## Creating and Interacting with a COM instance
Now that COM is initialized, we need to actually create the COM instance, and once again check for success:

```cs

hr = COM.CreateInstance<IFileOpenDialog>(
    new Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7"),
    null,
    CreateInstanceContext.InprocServer,
    new Guid("d57c7288-d4ad-4768-be02-9d969532d960"),
    out IFileOpenDialog dlg);

if (Macros.Succeeded(hr))
{
    // creation succeeded: proceed with interaction
}
```

And we can now proceed with interacting with the interface, which is a lot easier. In the following example, we configure the file dialog with a custom title and 2 filters:

> [!NOTE]
> This code defines the `COMDLG_FILTERSPEC` struct itself, because WinInteropUtils doesn't support it yet.

```cs
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct COMDLG_FILTERSPEC
{
    public string pszName;
    public string pszSpec;
}


COMDLG_FILTERSPEC[] filters = new COMDLG_FILTERSPEC[]
{
    new COMDLG_FILTERSPEC { pszName = "Text Files", pszSpec = "*.txt" },
    new COMDLG_FILTERSPEC { pszName = "All Files", pszSpec = "*.*" }
};

// Allocate unmanaged memory (SetFileTypes requires pointer)
IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<COMDLG_FILTERSPEC>() * filters.Length);

try
{
    // Copy each struct into unmanaged memory
    for (int i = 0; i < filters.Length; i++)
    {
        IntPtr structPtr = IntPtr.Add(ptr, i * Marshal.SizeOf<COMDLG_FILTERSPEC>());
        Marshal.StructureToPtr(filters[i], structPtr, false);
    }

    // ptr now points to the unmanaged array
    dlg.SetFileTypes((uint)filters.Length, ptr);
}
finally
{
    Marshal.FreeHGlobal(ptr);
}

dlg.SetTitle("Cool File Dialog");
```

And now we can actually SHOW the file dialog and read its results:
```cs
hr = dlg.Show(hWnd);

if (Macros.Succeeded(hr))
{
    Console.WriteLine("Dialog accepted!");

    hr = dlg.GetResult(out nint iptr);

    if (Macros.Succeeded(hr))
    {
        IShellItem item = (IShellItem)Marshal.GetTypedObjectForIUnknown(iptr, typeof(IShellItem));

        hr = item.GetDisplayName(
            SIGDN.SIGDN_FILESYSPATH,
            out nint pptr);

        if (Macros.Succeeded(hr))
        {
            string path = Marshal.PtrToStringUni(pptr);

            Console.WriteLine("Chosen path: " + path);
            
            Marshal.FreeCoTaskMem(pptr);
        }
    }
}
```

## Cleanup
Once you're done using COM, you need to deinitialize the COM library (if the initialization succeeded) and release any COM interfaces, using the [Release](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IUnknown.html#FireBlade_WinInteropUtils_ComponentObjectModel_Interfaces_IUnknown_Release) method:
```cs
dlg.Release();
COM.Uninitialize();
``` 

## Final Example
So now, our final example code looks like this:
```cs
using FireBlade.WinInteropUtils.ComponentObjectModel;
using FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces;
using FireBlade.WinInteropUtils;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct COMDLG_FILTERSPEC
{
    public string pszName;
    public string pszSpec;
}

HRESULT hr = COM.Initialize(CoInit.ApartmentThreaded);

if (Macros.Succeeded(hr))
{
    hr = COM.CreateInstance<IFileOpenDialog>(
        new Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7"),
        null,
        CreateInstanceContext.InprocServer,
        new Guid("d57c7288-d4ad-4768-be02-9d969532d960"),
        out IFileOpenDialog dlg);

    if (Macros.Succeeded(hr))
    {
        COMDLG_FILTERSPEC[] filters = new COMDLG_FILTERSPEC[]
        {
            new COMDLG_FILTERSPEC { pszName = "Text Files", pszSpec = "*.txt" },
            new COMDLG_FILTERSPEC { pszName = "All Files", pszSpec = "*.*" }
        };

        // Allocate unmanaged memory (SetFileTypes requires pointer)
        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<COMDLG_FILTERSPEC>() * filters.Length);

        try
        {
            // Copy each struct into unmanaged memory
            for (int i = 0; i < filters.Length; i++)
            {
                IntPtr structPtr = IntPtr.Add(ptr, i * Marshal.SizeOf<COMDLG_FILTERSPEC>());
                Marshal.StructureToPtr(filters[i], structPtr, false);
            }

            // ptr now points to the unmanaged array
            dlg.SetFileTypes((uint)filters.Length, ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        dlg.SetTitle("Cool File Dialog");

        hr = dlg.Show(hWnd);

        if (Macros.Succeeded(hr))
        {
            Console.WriteLine("Dialog accepted!");

            hr = dlg.GetResult(out nint iptr);

            if (Macros.Succeeded(hr))
            {
                IShellItem item = (IShellItem)Marshal.GetTypedObjectForIUnknown(iptr, typeof(IShellItem));

                hr = item.GetDisplayName(
                    SIGDN.SIGDN_FILESYSPATH,
                    out nint pptr);

                if (Macros.Succeeded(hr))
                {
                    string path = Marshal.PtrToStringUni(pptr);

                    Console.WriteLine("Chosen path: " + path);
                    
                    Marshal.FreeCoTaskMem(pptr);
                }

                item.Release();
            }
        }

        dlg.Release();
    }

    COM.Uninitialize();
}
```

If we run this sample code, a file dialog should appear and, after picking a file, its path should be outputted.