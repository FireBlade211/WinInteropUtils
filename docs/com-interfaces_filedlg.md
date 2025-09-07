# File Dialog COM Interfaces
This article lists the COM interfaces for the Common Item Dialog, the modernized common file dialog introduced in Windows Vista.

> [!NOTE]
> This article lists only COM interfaces defined by WinInteropUtils.<br>For basic interfaces that are not associated with the Common Item Dialog, see [Basic COM Interfaces](com-interfaces_basic.md).

## IFileDialog
[API](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IFileDialog.html)<br>
The base interface for file dialogs. This interface exposes methods that initialize, show, and get results from the common file dialog, and inherits from [IModalWindow](com-interfaces_basic.md#imodalwindow).

## IFileOpenDialog
[API](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IFileOpenDialog.html)<br>
This interface exposes methods for the Open file dialog. The only methods exposed in this interface are [GetResults](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IFileOpenDialog.html#FireBlade_WinInteropUtils_ComponentObjectModel_Interfaces_IFileOpenDialog_GetResults) and [GetSelectedItems](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IFileOpenDialog.html#FireBlade_WinInteropUtils_ComponentObjectModel_Interfaces_IFileOpenDialog_GetSelectedItems). It inherits from [IFileDialog](#ifiledialog).

## IFileSaveDialog
[API](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IFileSaveDialog.html)<br>
This interface exposes methods for the Save file dialog. It extends the [IFileDialog](#ifiledialog) interface by adding methods specific to the save dialog, which include those that provide support for the collection of metadata to be persisted with the file.
## IFileDialogCustomize
[API](../api/FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces.IFileDialogCustomize.html)<br>
This interface exposes methods that allow adding controls and customizing a file dialog.
> [!WARNING]
> Controls are added to the dialog before the dialog is shown. Their layout is implied by the order in which they are added. Once the dialog is shown, controls cannot be added or removed, but the existing controls can be hidden or disabled at any time. Their labels can also
be changed at any time.