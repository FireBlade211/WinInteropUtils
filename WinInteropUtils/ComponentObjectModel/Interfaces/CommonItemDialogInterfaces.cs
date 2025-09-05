using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;

namespace FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces
{
    /// <summary>
    /// Exposes methods that initialize, show, and get results from the common file dialog.
    /// </summary>
    [GeneratedComInterface]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
    [SupportedOSPlatform("windows6.0")]
    public partial interface IFileDialog : IModalWindow
    {
        // HRESULT SetFileTypes(UINT cFileTypes, const COMDLG_FILTERSPEC *rgFilterSpec);
        /// <summary>
        /// Sets the file types that the dialog can open or save.
        /// </summary>
        /// <param name="cFileTypes">The number of elements in the array specified by <paramref name="rgFilterSpec"/>.</param>
        /// <param name="rgFilterSpec">A pointer to an array of COMDLG_FILTERSPEC structures, each representing a file type.</param>
        /// <returns>If the method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns
        /// an <see cref="HRESULT"/> error code, including the following:
        /// <list type="table">
        /// <item>
        /// <term><see cref="HRESULT.E_UNEXPECTED"/></term>
        /// <description><see cref="SetFileTypes(uint, nint)"/> has already been called.</description>
        /// </item>
        /// <item>
        /// <term><see cref="HRESULT.E_UNEXPECTED"/></term>
        /// <description>The FOS_PICKFOLDERS flag was set in the <see cref="SetOptions(uint)"/> method.</description>
        /// </item>
        /// <item>
        /// <term><see cref="HRESULT.E_INVALIDARG"/></term>
        /// <description>The <paramref name="rgFilterSpec"/> parameter is <see langword="null"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>When using the Open dialog, the file types declared there are used to filter the view. When using the Save
        /// dialog, these values determine which file name extension is appended to the file name.</para>
        /// 
        /// This method must be called before the dialog is shown and can only be called once for each
        /// dialog instance. File types cannot be modified once the Common Item dialog box is displayed.
        /// </remarks>
        /// <example>
        /// The following code example demonstrates the use of the array of COMDLG_FILTERSPEC structures in the context
        /// of this method. The example array consists of three COMDLG_FILTERSPEC structures. The first declares two
        /// patterns for the dialog filter, the second declares a single pattern, and the last shows
        /// files of all types. The variables szJPG, szBMP, and szAll are assumed to be previously declared
        /// strings that provide a friendly name for each filter. <b>Warning: This is C++ code, as the COMDLG_FILTERSPEC struct hasn't yet been
        /// ported to WinInteropUtils.</b>
        /// COMDLG_FILTERSPEC rgSpec[] =
        /// { 
        ///    { szJPG, L"*.jpg;*.jpeg" },
        ///    { szBMP, L"*.bmp" },
        ///    { szAll, L"*.*" },
        /// };
        /// </example>
        [PreserveSig]
        public HRESULT SetFileTypes(uint cFileTypes, IntPtr rgFilterSpec);

        // HRESULT SetFileTypeIndex(UINT iFileType);
        /// <summary>
        /// Sets the file type that appears as selected in the dialog.
        /// </summary>
        /// <param name="iFileType">The index of the file type in the file type array
        /// passed to <see cref="SetFileTypes(uint, nint)"/> in its cFileTypes parameter. Note that this is a one-based index, not zero-based.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT SetFileTypeIndex(uint iFileType);

        // HRESULT GetFileTypeIndex(UINT *piFileType);
        /// <summary>
        /// Gets the currently selected file type.
        /// </summary>
        /// <param name="piFileType">A pointer to a UINT value that receives the index of the selected file
        /// type in the file type array passed to <see cref="SetFileTypes(uint, nint)"/> in its cFileTypes parameter.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para><see cref="GetFileTypeIndex(out uint)"/> can be called either while the dialog is open or after it has closed.</para>
        /// 
        /// <i>Note: The index returned by this function is a one-based index rather than zero-based.</i>
        /// </remarks>
        [PreserveSig]
        public HRESULT GetFileTypeIndex(out uint piFileType);

        // HRESULT Advise(IFileDialogEvents *pfde, DWORD *pdwCookie);
        /// <summary>
        /// Assigns an event handler that listens for events coming from the dialog.
        /// </summary>
        /// <param name="pfde">A pointer to an <see href="https://learn.microsoft.com/en-us/windows/desktop/api/shobjidl_core/nn-shobjidl_core-ifiledialogevents">IFileDialogEvents</see>
        /// implementation that will receive events from the dialog.</param>
        /// <param name="pdwCookie">A pointer to a <c>DWORD</c> that receives a value identifying this event handler. When the client
        /// is finished with the dialog, that client must call the <see cref="Unadvise(uint)"/> method with this value.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT Advise(IntPtr pfde, out uint pdwCookie);

        // HRESULT Unadvise(DWORD dwCookie);
        /// <summary>
        /// Removes an event handler that was attached through the <see cref="Advise(nint, out uint)"/> method.
        /// </summary>
        /// <param name="dwCookie">The DWORD value that represents the event handler. This value is obtained through the
        /// <c>pdwCookie</c> parameter of the <see cref="Advise(nint, out uint)"/> method.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT Unadvise(uint dwCookie);

        // HRESULT SetOptions(FILEOPENDIALOGOPTIONS fos);
        /// <summary>
        /// Sets flags to control the behavior of the dialog.
        /// </summary>
        /// <param name="fos">One or more of the FILEOPENDIALOGOPTIONS values.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an
        /// <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// Generally, this method should take the value that was retrieved by <see cref="GetOptions(out uint)"/>
        /// and modify it to include or exclude options by setting the appropriate flags.
        /// </remarks>
        [PreserveSig]
        public HRESULT SetOptions(uint fos);

        // HRESULT GetOptions(FILEOPENDIALOGOPTIONS *pfos);
        /// <summary>
        /// Gets the current flags that are set to control dialog behavior.
        /// </summary>
        /// <param name="pfos">When this method returns successfully, points to a value made up of one or more of
        /// the FILEOPENDIALOGOPTIONS values.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT GetOptions(out uint pfos);

        // HRESULT SetDefaultFolder(IShellItem *psi);
        /// <summary>
        /// Sets the folder used as a default if there is not a recently used folder value available.
        /// </summary>
        /// <param name="psi">A pointer to the <see cref="IShellItem"/> interface that represents the folder.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns
        /// an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT SetDefaultFolder(IntPtr psi);

        // HRESULT SetFolder(IShellItem *psi);
        /// <summary>
        /// Sets a folder that is always selected when the dialog is opened, regardless of previous user action.
        /// </summary>
        /// <param name="psi">A pointer to the <see cref="IShellItem"/> interface that represents the folder.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>This folder overrides any "most recently used" folder. If this method is called while the dialog is displayed,
        /// it causes the dialog to navigate to the specified folder.</para>
        /// 
        /// <para>In general, we do not recommended the use of this method. If you call <see cref="SetFolder(nint)"/> before you display the dialog box,
        /// the most recent location that the user saved to or opened from is not shown. Unless there is a very specific reason for
        /// this behavior, it is not a good or expected user experience and should therefore be avoided. In almost all instances,
        /// <see cref="SetDefaultFolder(nint)"/> is the better method.</para>
        /// 
        /// <para>As of Windows 7, if the path of the folder specified through <paramref name="psi"/> is the default path of a known folder,
        /// the known folder's current path is used in the dialog. That path might not be the same as the path specified in <paramref name="psi"/>;
        /// for instance, if the known folder has been redirected. If the known folder is a library (virtual folders Documents, Music, Pictures, and Videos),
        /// the library's path is used in the dialog. If the specified library is hidden (as they are by default as of Windows 8.1),
        /// the library's default save location is used in the dialog, such as the Microsoft OneDrive Documents folder for the Documents library.
        /// Because of these mappings, the folder location used in the dialog might not be exactly as you specified when you called this method.</para>
        /// </remarks>
        [PreserveSig]
        public HRESULT SetFolder(IntPtr psi);

        // HRESULT GetFolder(IShellItem **ppsi);
        /// <summary>
        /// Gets either the folder currently selected in the dialog, or, if the dialog is not currently displayed,
        /// the folder that is to be selected when the dialog is opened.
        /// </summary>
        /// <param name="ppsi">The address of a pointer to the <see cref="IShellItem"/> interface that represents the folder.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// The calling application is responsible for releasing the retrieved <see cref="IShellItem"/> when it is no longer needed.
        /// </remarks>
        [PreserveSig]
        public HRESULT GetFolder(out IntPtr ppsi);

        // HRESULT GetCurrentSelection(IShellItem **ppsi);
        /// <summary>
        /// Gets the user's current selection in the dialog.
        /// </summary>
        /// <param name="ppsi">The address of a pointer to the interface that represents the item currently selected in the dialog.
        /// This item can be a file or folder selected in the view window, or something that the user has entered
        /// into the dialog's edit box. The latter case may require a parsing operation (cancelable by the user) that blocks
        /// the current thread.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT GetCurrentSelection(out IntPtr ppsi);

        // HRESULT SetFileName(LPCWSTR pszName);
        /// <summary>
        /// Sets the file name that appears in the <b>File name</b> edit box when that dialog box is opened.
        /// </summary>
        /// <param name="pszName">A pointer to the name of the file.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);

        // HRESULT GetFileName(LPWSTR *pszName);
        /// <summary>
        /// Retrieves the text currently entered in the dialog's <b>File name</b> edit box.
        /// </summary>
        /// <param name="pszName">The address of a pointer to a string that, when this method returns successfully, receives the text.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>The text in the <b>File name</b> edit box does not necessarily reflect the item the user chose.
        /// To get the item the user chose, use <see cref="GetResult"/>.</para>
        /// 
        /// The calling application is responsible for releasing the retrieved buffer by using the CoTaskMemFree function.
        /// </remarks>
        [PreserveSig]
        public HRESULT GetFileName(out IntPtr pszName);

        // HRESULT SetTitle(LPCWSTR pszTitle);
        /// <summary>
        /// Sets the title of the dialog.
        /// </summary>
        /// <param name="pszTitle">A pointer to a string that contains the title text.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

        /// <summary>
        /// Closes the dialog.
        /// </summary>
        /// <param name="hr">The code that will be returned by <see cref="Show(nint)"/> to indicate that the dialog was closed before a selection was made.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>An application can call this method from a callback method or function while the dialog is open.
        /// The dialog will close and the <see cref="Show(nint)"/> method will return with the <see cref="HRESULT"/> specified in <paramref name="hr"/>.</para>
        /// 
        /// If this method is called, there is no result available for the <see cref="GetResult"/> or <see cref="IFileOpenDialog.GetResults(out nint)"/> methods,
        /// and they will fail if called.
        /// </remarks>
        [PreserveSig]
        public HRESULT Close(HRESULT hr);

        /// <summary>
        /// Gets the choice that the user made in the dialog.
        /// </summary>
        /// <param name="ppsi">The address of a pointer to an <see cref="IShellItem"/> that represents the user's choice.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para><see cref="GetResult(out nint)"/> can be called after the dialog has closed or during the handling of an <c>OnFileOk</c> event.
        /// Calling this method at any other time will fail. If multiple items were chosen, this method will fail. In the case of multiple
        /// items, call <see cref="IFileOpenDialog.GetResults(out nint)"/>.</para>
        ///
        /// <see cref="Show(nint)"/> must return a success code for a result to be available to <see cref="GetResult(out nint)"/>.
        /// </remarks>
        [PreserveSig]
        public HRESULT GetResult(out nint ppsi);

        /// <summary>
        /// Sets the filter. Deprecated since Windows 7.
        /// </summary>
        /// <param name="pFilter">A pointer to the
        /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/shobjidl_core/nn-shobjidl_core-ishellitemfilter">IShellItemFilter</see> that is to be set.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>This method can be used if the application needs to perform special filtering to remove some items from the dialog box's view.
        /// IncludeItem will be called for each item that would normally be included in the view. GetEnumFlagsForItem
        /// is not used. To filter by file type, <see cref="SetFileTypes(uint, nint)"/> should be used,
        /// because in folders with a large number of items it may offer better performance than applying an IShellItemFilter.</para>
        /// This method is obsolete since Windows 7. Do not use it in new projects.
        /// </remarks>
        [ObsoletedOSPlatform("windows6.1")]
        [PreserveSig]
        public HRESULT SetFilter(nint pFilter);
    }

    /// <summary>
    /// Extends the <see cref="IFileDialog"/> interface by adding methods specific to the open dialog.
    /// </summary>
    [GeneratedComInterface]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("d57c7288-d4ad-4768-be02-9d969532d960")]
    [SupportedOSPlatform("windows6.0")]
    public partial interface IFileOpenDialog : IFileDialog
    {
        // Additional methods specific to file open dialogs
        /// <summary>
        /// Gets the user's choices in a dialog that allows multiple selection.
        /// </summary>
        /// <param name="ppenum">The address of a pointer to an IShellItemArray through which the items selected in the dialog can be accessed.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>This method can be used whether the selection consists of a single item or multiple items.</para>
        /// 
        /// <para><see cref="GetResults(out nint)"/> can be called after the dialog has closed or during the handling
        /// of an IFileDialogEvents.OnFileOk event. Calling this method at any other time will fail.</para>
        /// 
        /// <see cref="Show(nint)"/> must return a success code for a result to be available to <see cref="GetResults(out nint)"/>.
        /// </remarks>
        [PreserveSig]
        public HRESULT GetResults(out IntPtr ppenum); // Returns IShellItemArray
        /// <summary>
        /// Gets the currently selected items in the dialog. These items may be items selected in the view,
        /// or text selected in the file name edit box.
        /// </summary>
        /// <param name="ppsai">The address of a pointer to an IShellItemArray through which the selected items can be accessed.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>This method can be used for single item or multiple item selections. If the user has entered new text in the file name
        /// field, this can be a time-consuming operation. When the application calls this method, the application parses the text in the filename
        /// field. For example, if this is a network share, the operation could take some time. However, this operation will not block the UI, since
        /// the user should able to stop the operation, which will result in <see cref="GetSelectedItems(out nint)"/> returning a failure code).</remarks>
        [PreserveSig]
        public HRESULT GetSelectedItems(out IntPtr ppsai); // Returns IShellItemArray
    }

    /// <summary>
    /// Extends the <see cref="IFileDialog"/> interface by adding methods specific to the save dialog, which include those that provide support for the collection
    /// of metadata to be persisted with the file.
    /// </summary>
    [GeneratedComInterface]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("84bccd23-5fde-4cdb-aea4-af64b83d78ab")]
    [SupportedOSPlatform("windows6.0")]
    public partial interface IFileSaveDialog : IFileDialog
    {
        // Additional methods specific to file save dialogs
        /// <summary>
        /// Sets an item to be used as the initial entry in a Save As dialog.
        /// </summary>
        /// <param name="psi">Pointer to an <see cref="IShellItem"/> that represents the item.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// The name of the item is displayed in the file name edit box, and the containing folder is opened in the view.
        /// This would generally be used when the application is saving an item that already exists. For new items, use <see cref="SetFileName(string)"/>.
        /// </remarks>
        [PreserveSig]
        public HRESULT SetSaveAsItem(IntPtr psi); // IShellItem
        /// <summary>
        /// Provides a property store that defines the default values to be used for the item being saved.
        /// </summary>
        /// <param name="pStore">Pointer to the IPropertyStore interface that represents the property store that contains the associated metadata.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>This method can be called at any time before the dialog is opened or while the dialog is showing. If an item has inherent properties,
        /// this method should be called with those properties before showing the dialog.</para>
        /// 
        /// <para>When using Save As, the application should provide the properties of the item being saved to the Save dialog. Those properties should be retreived
        /// from the original item by calling <c>GetPropertyStore</c> with the <c>GPS_HANDLERPROPERTIESONLY</c> flag.</para>
        /// 
        /// <para>To retrieve the properties of the saved item (which may have been modified by the user) after the dialog
        /// closes, call <see cref="GetProperties(out nint)"/>.</para>
        /// 
        /// To turn on property collection and indicate which properties should be displayed in the Save dialog, use <see cref="SetCollectedProperties(nint, bool)"/>.
        /// </remarks>
        [PreserveSig]
        public HRESULT SetProperties(IntPtr pStore); // IPropertyStore
        /// <summary>
        /// Specifies which properties will be collected in the save dialog.
        /// </summary>
        /// <param name="pStore">Pointer to the interface that represents the list of properties to collect. This parameter can be <see langword="null"/>.</param>
        /// <param name="fAppendDefault"><see langword="true"/> to show default properties for the currently selected filetype in addition to the properties
        /// specified by <paramref name="pStore"/>. <see langword="false"/> to show only properties specified by <paramref name="pStore"/>.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>The calling application can use the PSGetPropertyDescriptionListFromString function to construct an IPropertyDescriptionList
        /// from a string such as "prop:Comments;Subject;".</para>
        /// 
        /// <para>For more information about property schemas,
        /// see <see href="https://learn.microsoft.com/en-us/windows/desktop/properties/building-property-handlers-property-schemas">Property Schemas</see>.</para>
        /// 
        /// <para><see cref="SetCollectedProperties(nint, bool)"/> can be called at any time before the dialog is displayed or while it is visible.
        /// If different properties are to be collected depending on the chosen filetype, then <see cref="SetCollectedProperties(nint, bool)"/>
        /// can be called in response to OnTypeChange.</para>
        /// 
        /// <i>Note: By default, no properties are collected in the save dialog.</i>
        /// </remarks>
        [PreserveSig]
        public HRESULT SetCollectedProperties(IntPtr pStore, [MarshalAs(UnmanagedType.Bool)] bool fAppendDefault);
        /// <summary>
        /// Retrieves the set of property values for a saved item or an item in the process of being saved.
        /// </summary>
        /// <param name="ppStore">Address of a pointer to an IPropertyStore that receives the property values.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>This method can be called while the dialog is showing to retrieve the current set of values in the metadata collection pane.
        /// It can also be called after the dialog has closed, to retrieve the final set of values.</para>
        /// 
        /// The call to this method will fail unless property collection has been turned on with a call to <see cref="SetCollectedProperties(nint, bool)"/>.
        /// </remarks>
        [PreserveSig]
        public HRESULT GetProperties(out IntPtr ppStore); // IPropertyStore
        /// <summary>
        /// Applies a set of properties to an item using the Shell's copy engine.
        /// </summary>
        /// <param name="psi">Pointer to the <see cref="IShellItem"/> that represents the file being saved.
        /// This is usually the item retrieved by <see cref="IFileDialog.GetResult(out nint)"/>.</param>
        /// <param name="pStore">Pointer to the IPropertyStore that represents the property values to be applied to the file.
        /// This can be the property store returned by <see cref="GetProperties(out nint)"/>.</param>
        /// <param name="hwnd">The handle of the application window.</param>
        /// <param name="pSink">Pointer to an optional IFileOperationProgressSink that the calling application can use if they want to be notified
        /// of the progress of the property stamping. This value may be <see langword="null"/>.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks><para>This method should be used when the application has turned on property collection (<see cref="SetCollectedProperties(nint, bool)"/>),
        /// but does not persist the properties themselves into the saved file.</para>
        /// 
        /// <i>Note: The file represented by the item specified in <paramref name="psi"/> must exist in physical storage before making the call
        /// to <see cref="ApplyProperties(nint, nint, nint, nint)"/>, so it must have been previously saved at some point.</i>
        /// </remarks>
        [PreserveSig]
        public HRESULT ApplyProperties(IntPtr psi, IntPtr pStore, IntPtr hwnd, IntPtr pSink); // IShellItem, IPropertyStore, HWND, IFileOperationProgressSink
    }

    /// <summary>
    /// Exposes methods that allow an application to add controls to a common item dialog.
    /// </summary>
    /// <remarks>
    /// <para>Controls are added to the dialog before the dialog is shown. Their layout is implied by the order in which they are added.
    /// Once the dialog is shown, controls cannot be added or removed, but the existing controls can be hidden or disabled at any time. Their labels can also
    /// be changed at any time.</para>
    /// 
    /// <para>Container controls are controls that can have items added to them. Container controls include combo boxes, menus, the drop-down list
    /// attached to the Open button, and any option button groups. The order that items appear in a container is the order in which they were added. There is no
    /// facility for reordering them. IDs are scoped to the parent control. Container controls, with the exception of menus, have a selected item.</para>
    /// 
    /// Items with a container control cannot be changed after they have been created, except for their enabled and visible states. However, they can be added
    /// and removed at any time. For example, if you needed to change the text of a menu, you would have to
    /// remove the current menu and add another with the correct text.
    /// </remarks>
    [GeneratedComInterface]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("973510db-7d7f-452b-8975-74a85828d354")]
    [SupportedOSPlatform("windows6.0")]
    public partial interface IFileDialogCustomize : IUnknown
    {
        /// <summary>
        /// Enables a drop-down list on the Open or Save button in the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the drop-down list.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>The Open or Save button label takes on the text of the first item in the drop-down.</para>
        /// 
        /// Use <see cref="AddControlItem(uint, uint, string)"/> to add items to the drop-down.
        /// </remarks>
        [PreserveSig]
        public HRESULT EnableOpenDropDown(uint dwIDCtl);
        /// <summary>
        /// Adds a menu to the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the menu to add.</param>
        /// <param name="pszLabel">A <see langword="null"/>-terminated Unicode string that contains the menu name.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>The default state for this control is enabled and visible.</para>
        /// 
        /// To add items to this control, use <see cref="AddControlItem(uint, uint, string)"/>.
        /// </remarks>
        [PreserveSig]
        public HRESULT AddMenu(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        /// <summary>
        /// Adds a button to the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the button to add.</param>
        /// <param name="pszLabel">A <see langword="null"/>-terminated Unicode string that contains the button text.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>The default state for this control is enabled and visible.</remarks>
        [PreserveSig]
        public HRESULT AddPushButton(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        /// <summary>
        /// Adds a combo box to the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the combo box to add.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>The default state for this control is enabled and visible.</remarks>
        [PreserveSig]
        public HRESULT AddComboBox(uint dwIDCtl);
        /// <summary>
        /// Adds a check button (check box) to the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the check button to add.</param>
        /// <param name="pszLabel">A <see langword="null"/>-terminated Unicode string that contains the button text.</param>
        /// <param name="bChecked">A <see cref="bool"/> indicating the current state of the check
        /// button. <see langword="true"/> if checked; <see langword="false"/> otherwise.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>The default state for this control is enabled and visible.</remarks>
        [PreserveSig]
        public HRESULT AddCheckButton(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel, [MarshalAs(UnmanagedType.Bool)] bool bChecked);
        /// <summary>
        /// Adds an option button (also known as radio button) group to the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the option button group to add.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>The default state for this control is enabled and visible.</remarks>
        [PreserveSig]
        public HRESULT AddRadioButtonList(uint dwIDCtl);
        /// <summary>
        /// Adds an edit box control to the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the edit box to add.</param>
        /// <param name="pszText">A <see langword="null"/>-terminated Unicode string that provides the default text displayed in the edit box.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>The default state for this control is enabled and visible.</para>
        /// 
        /// To add a label next to the edit box, place it in a visual group with <see cref="StartVisualGroup"/>.
        /// </remarks>
        [PreserveSig]
        public HRESULT AddEditBox(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszText);
        /// <summary>
        /// Adds a separator to the dialog, allowing a visual separation of controls.
        /// </summary>
        /// <param name="dwIDCtl">The control ID of the separator.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>The default state for this control is enabled and visible.</remarks>
        [PreserveSig]
        public HRESULT AddSeparator(uint dwIDCtl);
        /// <summary>
        /// Adds text content to the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the text to add.</param>
        /// <param name="pszText">A <see langword="null"/>-terminated Unicode string that contains the text.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>The default state for this control is enabled and visible.</remarks>
        [PreserveSig]
        public HRESULT AddText(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszText);
        /// <summary>
        /// Sets the text associated with a control, such as button text or an edit box label.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the control whose text is to be changed.</param>
        /// <param name="pszLabel">A null-terminated Unicode string that contains the text.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>Control labels can be changed at any time, including when the dialog is visible.</remarks>
        [PreserveSig]
        public HRESULT SetControlLabel(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        /// <summary>
        /// Gets the current visibility and enabled states of a given control.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the control in question.</param>
        /// <param name="pdwState">A pointer to a variable that receives one or more values
        /// from the CDCONTROLSTATE enumeration that indicate the current state of the control.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT GetControlState(uint dwIDCtl, out uint pdwState);
        /// <summary>
        /// Sets the current visibility and enabled states of a given control.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the control in question.</param>
        /// <param name="dwState">One or more values from the CDCONTROLSTATE enumeration that indicate the current state of the control.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>When the dialog is shown, controls cannot be added or removed, but the existing controls can be hidden or disabled at any time.</remarks>
        [PreserveSig]
        public HRESULT SetControlState(uint dwIDCtl, uint dwState);
        /// <summary>
        /// Gets the current text in an edit box control.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the edit box.</param>
        /// <param name="ppszText">The address of a pointer to a <see langword="null"/>-terminated Unicode string that receives the text.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>It is the responsibility of the calling application to free the buffer referenced by <paramref name="ppszText"/>
        /// when it is no longer needed. Use <c>CoTaskMemFree</c> to free the buffer.</remarks>
        [PreserveSig]
        public HRESULT GetEditBoxText(uint dwIDCtl, out IntPtr ppszText);
        /// <summary>
        /// Sets the text in an edit box control found in the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the edit box.</param>
        /// <param name="pszText">A <see langword="null"/>-terminated Unicode string that contains the text.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT SetEditBoxText(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszText);
        /// <summary>
        /// Gets the current state of a check button (check box) in the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the check box.</param>
        /// <param name="pbChecked">The address of a <see cref="bool"/> value that indicates whether the box is checked.
        /// <see langword="true"/> means checked; <see langword="false"/>, unchecked.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT GetCheckButtonState(uint dwIDCtl, [MarshalAs(UnmanagedType.Bool)] out bool pbChecked);
        /// <summary>
        /// Sets the state of a check button (check box) in the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the check box.</param>
        /// <param name="bChecked">A <see cref="bool"/> value that indicates whether the box
        /// is checked. <see langword="true"/> means checked; <see langword="false"/>, unchecked.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT SetCheckButtonState(uint dwIDCtl, [MarshalAs(UnmanagedType.Bool)] bool bChecked);
        /// <summary>
        /// Adds an item to a container control in the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the container control to which the item is to be added.</param>
        /// <param name="dwIDItem">The ID of the item.</param>
        /// <param name="pszLabel">A <see langword="null"/>-terminated Unicode string that contains the item's text, which can be either a label or, in the case
        /// of a drop-down list, the item itself.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>The default state for this item is enabled and visible. Items in control groups cannot be changed after they have been created,
        /// with the exception of their enabled and visible states.</para>
        /// 
        /// Container controls include option button groups, combo boxes, drop-down lists on the Open or Save button, and menus.
        /// </remarks>
        [PreserveSig]
        public HRESULT AddControlItem(uint dwIDCtl, uint dwIDItem, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        /// <summary>
        /// Removes an item from a container control in the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the container control from which the item is to be removed.</param>
        /// <param name="dwIDItem">The ID of the item.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>Container controls include option button groups, combo boxes, drop-down lists on the Open or Save button, and menus.</remarks>
        [PreserveSig]
        public HRESULT RemoveControlItem(uint dwIDCtl, uint dwIDItem);
        /// <summary>
        /// Gets the current state of an item in a container control found in the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the container control.</param>
        /// <param name="dwIDItem">The ID of the item.</param>
        /// <param name="pdwState">A pointer to a variable that receives one of more values from the CDCONTROLSTATE
        /// enumeration that indicate the current state of the control.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>The default state of a control item is enabled and visible. Items in control groups cannot be changed after 
        /// they have been created, with the exception of their enabled and visible states.</para>
        /// 
        /// Container controls include option button groups, combo boxes, drop-down lists on the Open or Save button, and menus.
        /// </remarks>
        [PreserveSig]
        public HRESULT GetControlItemState(uint dwIDCtl, uint dwIDItem, out uint pdwState);
        /// <summary>
        /// Sets the current state of an item in a container control found in the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the container control.</param>
        /// <param name="dwIDItem">The ID of the item.</param>
        /// <param name="dwState">One or more values from the CDCONTROLSTATE enumeration that indicate the new state of the control.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>The default state of a control item is enabled and visible. Items in control groups cannot be changed after they have
        /// been created, with the exception of their enabled and visible states.</para>
        /// 
        /// Container controls include option button groups, combo boxes, drop-down lists on the Open or Save button, and menus.
        /// </remarks>
        [PreserveSig]
        public HRESULT SetControlItemState(uint dwIDCtl, uint dwIDItem, uint dwState);
        /// <summary>
        /// Gets a particular item from specified container controls in the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the container control.</param>
        /// <param name="pdwIDItem">The ID of the item that the user selected in the control.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>To determine the user's final choice, this method can be called on option button groups, combo boxes, and drop-down lists on
        /// the Open or Save button after the dialog has closed. This method cannot be called on menus.</para>
        /// 
        /// For option button groups and combo boxes, this method can also be called while the dialog is showing, to determine the current choice.
        /// </remarks>
        [PreserveSig]
        public HRESULT GetSelectedControlItem(uint dwIDCtl, out uint pdwIDItem);
        /// <summary>
        /// Sets the selected state of a particular item in an option button group or a combo box found in the dialog.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the container control.</param>
        /// <param name="dwIDItem">The ID of the item to display as selected in the control.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT SetSelectedControlItem(uint dwIDCtl, uint dwIDItem);

        /// <summary>
        /// Stops the addition of elements to a visual group in the dialog.
        /// </summary>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        [PreserveSig]
        public HRESULT EndVisualGroup();

        /// <summary>
        /// Places a control in the dialog so that it stands out compared to other added controls.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the control.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>This method causes the control to be placed near the Open or Save button instead of being grouped with the rest of the custom controls.</para>
        /// 
        /// <para>Only check buttons (check boxes), push buttons, combo boxes, and menus—or a visual group that contains
        /// only a single item of one of those types—can be made prominent.</para>
        /// 
        /// Only one control can be marked in this way. If a dialog has only one added control, that control is marked as prominent by default.
        /// </remarks>
        [PreserveSig]
        public HRESULT MakeProminent(uint dwIDCtl);

        /// <summary>
        /// Sets the text of a control item. For example, the text that accompanies a radio button or an item in a menu.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the container control.</param>
        /// <param name="dwIDItem">The ID of the item.</param>
        /// <param name="pszLabel">A <see langword="null"/>-terminated Unicode string that contains the text.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>The default state of a control item is enabled and visible. Items in control groups cannot be changed
        /// after they have been created, with the exception of their enabled and visible states.</para>
        /// 
        /// Container controls include option button groups, combo boxes, drop-down lists on the Open or Save button, and menus.
        /// </remarks>
        [PreserveSig]
        public HRESULT SetControlItemText(uint dwIDCtl, uint dwIDItem, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

        /// <summary>
        /// Declares a visual group in the dialog. Subsequent calls to any "add" method add those elements to this group.
        /// </summary>
        /// <param name="dwIDCtl">The ID of the visual group.</param>
        /// <param name="pszLabel">A <see langword="null"/>-terminated Unicode string that contains text that appears next to the visual group.</param>
        /// <returns>If this method succeeds, it returns <see cref="HRESULT.S_OK"/>. Otherwise, it returns an <see cref="HRESULT"/> error code.</returns>
        /// <remarks>
        /// <para>Controls will continue to be added to this visual group until you call <see cref="EndVisualGroup"/>.</para>
        /// 
        /// A visual group can be hidden and disabled like any other control, except that doing so affects all of the
        /// controls within it. Individual members of the visual group can also be hidden and disabled singly.
        /// </remarks>
        [PreserveSig]
        public HRESULT StartVisualGroup(uint dwIDCtl, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
    }
}
