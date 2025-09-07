# Basic COM Interfaces
This article lists the basic COM interfaces.

> [!NOTE]
> This article lists only COM interfaces defined by WinInteropUtils.<br>For interfaces for the Common Item Dialog, see [File Dialog COM Interfaces](com-interfaces_filedlg.md).

## IUnknown
[API](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IUnknown.html)<br>
The base interface that all COM interfaces inherit from. This interface defines the base [Release](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IUnknown.html#FireBlade_WinInteropUtils_ComponentObjectModel_Interfaces_IUnknown_Release) and [AddRef](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IUnknown.html#FireBlade_WinInteropUtils_ComponentObjectModel_Interfaces_IUnknown_AddRef) methods.

## IShellItem
[API](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IShellItem.html)<br>
Exposes methods that retrieve information about a Shell item, such as a file or directory. Third parties do not implement this interface; only use the implementation provided with the system.

## IModalWindow
[API](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IModalWindow.html)<br>
This interface represents a modal window, such as a file dialog. This interface only exposes the [Show](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IModalWindow#FireBlade_WinInteropUtils_ComponentObjectModel_Interfaces_IModalWindow_Show) method.