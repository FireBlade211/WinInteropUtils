# File System
**WinInteropUtils** v0.31 introduces the new `WinFile` and `WinFileInfo` APIs to replace the old `WindowsFile` API. These APIs allow you to easily manage file system items, query shell info about them, and manipulate their data.

The new APIs live in the namespace `FireBlade.WinInteropUtils.FileSystem`, as part of our goal to separate APIs into proper namespaces instead of cluttering the base namespace.

## WinFile
The `WinFile` API allows you to open and close Win32 file handles and utilize them in an object-oriented manner. You can query the size, content, and attributes of the file, as well as write to it, create a new file, or use special Windows-exclusive options.

## WinFileInfo
The `WinFileInfo` API allows you to query information about a file, such as the small, large, and shell-sized icons, as well as the Shell display name, the type of an EXE or DLL file, and the display name of the file type.

The file doesn't have to exist; by specifying the `attribs` parameter in the `WinFileInfo` constructor, you can use the API in a simulated mode - the API will query information as if the file existed at the specified location with the specified attributes, without needing to actually create the file.

> [!TIP]
> If you already have a `WinFile`, you can get the `WinFileInfo` for it by using the `FileInfo` property.

## More info
For more information about the **Windows filesystem**, see the following subtopics:

- [Caching](caching.md)
- [Reparse Points](reparsepoints.md)