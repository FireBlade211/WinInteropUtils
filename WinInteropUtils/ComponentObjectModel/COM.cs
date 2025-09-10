using FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

// Moved to a seperate namespace to allow expansions for COM APIs in the future
namespace FireBlade.WinInteropUtils.ComponentObjectModel
{
    /// <summary>
    /// Provides basic COM (Component-Object Model) methods from <c>Ole32.dll</c> required to use COM.
    /// </summary>
    public static partial class COM
    {
        [LibraryImport("ole32.dll", EntryPoint = "CoInitializeEx")]
        private static partial int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

        [LibraryImport("ole32.dll", EntryPoint = "CoInitialize")]
        private static partial int CoInitialize(IntPtr pvReserved);

        /// <summary>
        /// Initializes the COM library on the current thread and identifies the concurrency model as single-thread apartment (STA).
        /// </summary>
        /// <remarks>
        /// <para>New applications should call <see cref="Initialize(COMInitOptions)"/> instead of <see cref="Initialize()"/>.</para>
        /// 
        /// If you want to use the Windows Runtime, you must call
        /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/roapi/nf-roapi-roinitialize">RoInitialize</see>
        /// or <see href="https://learn.microsoft.com/en-us/windows/win32/api/roapi/nf-roapi-initialize">Windows::Foundation::Initialize</see> instead.
        /// </remarks>
        /// <returns>
        /// This function can return the standard
        /// return values <see cref="HRESULT.E_INVALIDARG"/>, <see cref="HRESULT.E_OUTOFMEMORY"/>,
        /// and <see cref="HRESULT.E_UNEXPECTED"/>, as well as the following values:
        /// 
        /// <list type="table">
        /// <item>
        /// <term><see cref="HRESULT.S_OK"/></term>
        /// <description>The COM library was initialized successfully on this thread.</description>
        /// </item>
        /// 
        /// <item>
        /// <term><see cref="HRESULT.S_FALSE"/></term>
        /// <description>The COM library is already initialized on this thread.</description>
        /// </item>
        /// 
        /// <item>
        /// <term><see cref="HRESULT.RPC_E_CHANGED_MODE"/></term>
        /// <description>A previous call to <see cref="Initialize(COMInitOptions)"/> specified the concurrency model for this thread
        /// as multithread apartment (MTA). This could also indicate that a change from neutral-threaded apartment to single-threaded
        /// apartment has occurred.</description>
        /// </item>
        /// </list>
        /// </returns>
        public static HRESULT Initialize() => (HRESULT)CoInitialize(nint.Zero);

        /// <summary>
        /// Initializes the COM library for use by the calling thread, sets the thread's concurrency model, and creates a new apartment for the thread if one is required.
        /// </summary>
        /// <param name="options">The concurrency model and initialization options for the thread. Any combination of values from <see cref="COMInitOptions"/> can be used, except
        /// that the <see cref="COMInitOptions.ApartmentThreaded"/> and <see cref="COMInitOptions.MultiThreaded"/> flags cannot both be set.
        /// The default is <see cref="COMInitOptions.MultiThreaded"/>.</param>
        /// <returns>This function can return either <see cref="HRESULT.S_OK"/> or <see cref="HRESULT.S_FALSE"/>.</returns>
        /// <remarks>
        /// <para><see cref="Initialize(COMInitOptions)"/> must be called at least once, and is usually called only once, for each thread that uses the COM library. Multiple calls
        /// to <see cref="Initialize(COMInitOptions)"/> by the same thread are allowed as long as they pass the same concurrency flag, but subsequent valid calls return
        /// <see cref="HRESULT.S_FALSE"/>. If the concurrency flag does not match, then the call fails and returns <see cref="HRESULT.RPC_E_CHANGED_MODE"/>. 
        /// (For the purpose of this rule, a call to CoInitialize is equivalent to calling <see cref="Initialize(COMInitOptions)"/> with the <see cref="COMInitOptions.ApartmentThreaded"/> flag.) To
        /// uninitialize the COM library gracefully on a thread, each successful call to CoInitialize or <see cref="Initialize(COMInitOptions)"/>, including any call that
        /// returns <see cref="HRESULT.S_FALSE"/>, must be balanced by a corresponding call to <see cref="Uninitialize"/>. Once COM has been uninitialized on a thread,
        /// you can reinitialize it in any mode, subject to the constraints above.</para>
        ///
        /// <para>You need to initialize the COM library on a thread before you call any of the library functions except CoGetMalloc, to get
        /// a pointer to the standard allocator, and the memory allocation functions. Otherwise, the COM function will return <see cref="HRESULT.CO_E_NOTINITIALIZED"/>.</para>
        ///
        /// <para>Objects created in a single-threaded apartment (STA) receive method calls only from their apartment's thread, so calls are serialized and arrive
        /// only at message-queue boundaries (when the PeekMessage or SendMessage function is called).</para>
        ///
        /// <para>Objects created on a COM thread in a multithread apartment (MTA) must be able to receive method calls from other threads at any time. You
        /// would typically implement some form of concurrency control in a multithreaded object's code using synchronization primitives such as critical sections,
        /// semaphores, or mutexes to help protect the object's data.</para>
        ///
        /// <para>When an object that is configured to run in the neutral threaded apartment (NTA) is called by a thread that is in either
        /// an STA or the MTA, that thread transfers to the NTA. If this thread subsequently calls <see cref="Initialize(COMInitOptions)"/>, the call fails and returns
        /// <see cref="HRESULT.RPC_E_CHANGED_MODE"/>.</para>
        ///
        /// <para>Because OLE technologies are not thread-safe, the OleInitialize function calls <see cref="Initialize(COMInitOptions)"/> with the <see cref="COMInitOptions.ApartmentThreaded"/> flag. As a result,
        /// an apartment that is initialized for multithreaded object concurrency cannot use the features enabled by OleInitialize.</para>
        ///
        /// Because there is no way to control the order in which in-process servers are loaded or unloaded, do not call CoInitialize, <see cref="Initialize(COMInitOptions)"/>,
        /// or <see cref="Uninitialize"/> from the <c>DllMain</c> function.
        /// </remarks>
        public static HRESULT Initialize(COMInitOptions options) => (HRESULT)CoInitializeEx(nint.Zero, (uint)options);

        /// <summary>
        /// Closes the COM library on the current thread, unloads all DLLs loaded by the thread,
        /// frees any other resources that the thread maintains, and forces all RPC connections on the thread to close.
        /// </summary>
        /// <remarks>
        /// <para>A thread must call <see cref="Uninitialize"/> once for each successful call it has made to the <see cref="Initialize(COMInitOptions)"/> function,
        /// including any call that returns S_FALSE. Only the <see cref="Uninitialize"/> call corresponding to the <see cref="Initialize(COMInitOptions)"/> call that initialized the library
        /// can close it.</para>
        ///
        /// <para>Calls to OleInitialize must be balanced by calls to OleUninitialize. The OleUninitialize function calls <see cref="Uninitialize"/> internally, so applications
        /// that call OleUninitialize do not also need to call <see cref="Uninitialize"/>.</para>
        ///
        ///
        /// <para><see cref="Uninitialize"/> should be called on application shutdown, as the last call made to the COM library after the application hides
        /// its main windows and falls through its main message loop. If there are open conversations remaining, <see cref="Uninitialize"/>
        /// starts a modal message loop and dispatches any pending messages from the containers or server for this COM application.
        /// By dispatching the messages, <see cref="Uninitialize"/> ensures that the application does not quit before receiving all of its pending messages.
        /// Non-COM messages are discarded.</para>
        ///
        ///
        /// Because there is no way to control the order in which in-process servers are loaded or unloaded, do not call CoInitialize, <see cref="Initialize(COMInitOptions)"/>,
        /// or <see cref="Uninitialize"/> from the <c>DllMain</c> function.
        /// </remarks>
        [LibraryImport("ole32.dll", EntryPoint = "CoUninitialize")]
        public static partial void Uninitialize();

        /// <summary>
        /// Specifies flags for <see cref="Initialize(COMInitOptions)"/>.
        /// </summary>
        /// <remarks>
        /// <para>When a thread is initialized through a call to <see cref="Initialize(COMInitOptions)"/>, you choose whether to initialize it as apartment-threaded or
        /// multithreaded by designating one of the members of <see cref="COMInitOptions"/> as its parameter. This designates how incoming calls to any object created by that
        /// thread are handled, that is, the object's concurrency.</para>
        ///
        /// <para>Apartment-threading, while allowing for multiple threads of execution, serializes all incoming calls by requiring that calls to methods
        /// of objects created by this thread always run on the same thread, i.e. the apartment/thread that created them. In addition, calls can arrive only at message-queue
        /// boundaries. Because of this serialization, it is not typically necessary to write concurrency control into the code for the object, other than to avoid calls
        /// to <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-peekmessagea">PeekMessage</see> and
        /// <see cref="User32.SendMessage(nint, uint, nuint, nint)"/> during processing that must not be interrupted by other method invocations or calls to other 
        /// objects in the same apartment/thread.</para>
        ///
        /// <para>Multi-threading (also called free-threading) allows calls to methods of objects created by this thread to be run on any thread. There is no
        /// serialization of calls, i.e. many calls may occur to the same method or to the same object or simultaneously. Multi-threaded object concurrency offers the
        /// highest performance and takes the best advantage of multiprocessor hardware for cross-thread, cross-process, and cross-machine calling,
        /// since calls to objects are not serialized in any way.This means, however, that the code for objects must enforce its own concurrency model, typically through
        /// the use of synchronization primitives, such as critical sections, semaphores, or mutexes. In addition, because the object doesn't control the lifetime
        /// of the threads that are accessing it, no thread-specific state may be stored in the object
        /// (in <see href="https://learn.microsoft.com/en-us/windows/desktop/ProcThread/thread-local-storage">Thread Local Storage</see>).</para>
        ///
        ///
        /// <i>Note: The multi-threaded apartment is intended for use by non-GUI threads. Threads in multi-threaded apartments should not perform UI actions.
        /// This is because UI threads require a message pump, and COM does not pump messages for threads in a multi-threaded apartment.</i>
        /// </remarks>
        [Flags]
        public enum COMInitOptions
        {
            /// <summary>
            /// Initializes the thread for STA (Single-Threaded Apartment).
            /// </summary>
            ApartmentThreaded = 0x2,
            /// <summary>
            /// Initializes the thread for MTA (Multi-Threaded Apartment). This is the default.
            /// </summary>
            MultiThreaded = 0x0,
            /// <summary>
            /// Disables DDE for OLE1 support.
            /// </summary>
            DisableOle1DDE = 0x4,
            /// <summary>
            /// Increase memory usage in an attempt to increase performance.
            /// </summary>
            SpeedOverMemory = 0x8
        }

        [DllImport("ole32.dll", EntryPoint = "CoCreateInstance")]
        private static extern int CoCreateInstance(
            ref Guid rclsid,
            [MarshalAs(UnmanagedType.IUnknown)] object? pUnkOuter,
            uint dwClsContext,
            ref Guid riid,
            out IntPtr ppv);

        /// <summary>
        /// <para>Creates and default-initializes a single object of the class associated with a specified CLSID.</para>
        /// 
        /// Call CoCreateInstance when you want to create only one object on the local system. To create a single object on a remote system, call the CoCreateInstanceEx
        /// function. To create multiple objects based on a single CLSID, call the CoGetClassObject function.
        /// </summary>
        /// <typeparam name="TCoInterface"></typeparam>
        /// <param name="rclsid">The CLSID associated with the data and code that will be used to create the object.</param>
        /// <param name="pUnkOuter">If <see langword="null"/>, indicates that the object is not being created as part of an aggregate. If non-<see langword="null"/>, pointer
        /// to the aggregate object's <see cref="IUnknown"/> interface (the controlling <see cref="IUnknown"/>).</param>
        /// <param name="dwClsContext">Context in which the code that manages the newly created object will run.
        /// A bitwise combination of <see cref="CreateInstanceContext"/> values.</param>
        /// <param name="riid">A reference to the identifier of the interface to be used to communicate with the object.</param>
        /// <param name="ppv">Upon successful return, <paramref name="ppv"/> contains the requested interface. Upon failure, <paramref name="ppv"/> contains
        /// <see langword="null"/>.</param>
        /// <returns>A <see cref="HRESULT"/>. It can be one of the following values:
        /// <list type="table">
        /// <item>
        /// <term><see cref="HRESULT.S_OK"/></term>
        /// <description>An instance of the specified object class was successfully created.</description>
        /// </item>
        /// <item>
        /// <term><see cref="HRESULT.REGDB_E_CLASSNOTREG"/></term>
        /// <description>A specified class is not registered in the registration database. Also can indicate that the type of server you requested
        /// in the <see cref="CreateInstanceContext"/> enumeration is not registered or the values for the server types in the registry are corrupt.</description>
        /// </item>
        /// <item>
        /// <term><see cref="HRESULT.CLASS_E_NOAGGREGATION"/></term>
        /// <description>This class cannot be created as part of an aggregate.</description>
        /// </item>
        /// <item>
        /// <term><see cref="HRESULT.E_NOINTERFACE"/></term>
        /// <description>The specified class does not implement the requested interface,
        /// or the controlling <see cref="IUnknown"/> does not expose the requested interface.</description>
        /// </item>
        /// <item>
        /// <term><see cref="HRESULT.E_POINTER"/></term>
        /// <description>The <paramref name="ppv"/> parameter is <see langword="null"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>The <see cref="CreateInstance"/> function provides
        /// a convenient shortcut by connecting to the class object associated with the specified CLSID, creating a default-initialized instance,
        /// and releasing the class object.</para>
        /// 
        /// <para>It is convenient to use <see cref="CreateInstance"/> when
        /// you need to create only a single instance of an object on the local machine. If you are creating an instance on remote
        /// computer, call CoCreateInstanceEx.</para>
        /// 
        /// <para>In the <see cref="CreateInstanceContext"/> enumeration, you can specify the type of server used to manage
        /// the object. The constants can be <see cref="CreateInstanceContext.InprocServer"/>,
        /// <see cref="CreateInstanceContext.InprocHandler"/>, <see cref="CreateInstanceContext.LocalServer"/>, <see cref="CreateInstanceContext.RemoteServer"/> or any
        /// combination of these values.</para>
        /// 
        /// <para><i>UWP applications</i></para>
        /// 
        /// Although there are no restrictions on which CLSIDs a UWP application can pass
        /// to <see cref="CreateInstance"/>, many objects will fail
        /// with <see cref="HRESULT.E_ACCESSDENIED"/> for security reasons, especially if they do not run in-process. Additionally, even if you can successfully
        /// create an object, it might fail at a later time due to UWP security constraints, app-model differences, etc. In particular, background tasks should limit
        /// the objects they communicate with to avoid hangs or other complications due to connected stand-by.
        /// </remarks>
        /// <exception cref="InvalidOperationException">TCoInterface must be an interface.</exception>
        [SupportedOSPlatform("windows5.0")]
        public static HRESULT CreateInstance<TCoInterface>(Guid rclsid,
            [AllowNull] IUnknown? pUnkOuter,
            CreateInstanceContext dwClsContext,
            Guid riid,
            [MaybeNull] out TCoInterface? ppv) where TCoInterface : IUnknown
        {
            if (!typeof(TCoInterface).IsInterface)
                throw new InvalidOperationException("TCoInterface must be an interface.");

            ppv = default;

            HRESULT hr = (HRESULT)CoCreateInstance(ref rclsid, pUnkOuter, (uint)dwClsContext, ref riid, out nint inst);

            if (Macros.Succeeded(hr))
            {
                var obj = Marshal.GetTypedObjectForIUnknown(inst, typeof(TCoInterface));

                ppv = (TCoInterface)obj;
            }

            return hr;
        }

        /// <summary>
        /// Values that are used in activation calls to indicate the execution contexts in which an object is to
        /// be run. Used in <see cref="CreateInstance"/>.
        /// </summary>
        /// <remarks>
        /// <para>Values from the <see cref="CreateInstanceContext"/> enumeration are used in activation calls (<see cref="CreateInstance"/>,
        /// CoCreateInstanceEx, CoGetClassObject, and so on) to indicate the preferred execution contexts (in-process, local, or remote) in which an
        /// object is to be run. They are also used in calls to CoRegisterClassObject to indicate the set of execution contexts in which a class object
        /// is to be made available for requests to construct instances.</para>
        /// 
        /// <para>To indicate that more than one context is acceptable, you can combine multiple values with Boolean ORs. The contexts
        /// are tried in the order in which they are listed.</para>
        /// 
        /// <para>Given a set of <see cref="CreateInstanceContext"/> flags, the execution context to be used depends on the availability of registered class
        /// codes and other parameters according to the following algorithm.</para>
        /// 
        /// <list type="number">
        /// <item>
        /// If the call specifies one of the following, <see cref="RemoteServer"/> is implied and is added to the list of flags:
        /// <list type="bullet">
        /// <item>An explicit COSERVERINFO structure indicating a machine different from the current computer.</item>
        /// <item>No explicit COSERVERINFO structure specified but the specified class is registered with either the
        /// <see href="https://learn.microsoft.com/en-us/windows/win32/com/remoteservername">RemoteServerName</see>
        /// or <see href="https://learn.microsoft.com/en-us/windows/win32/com/activateatstorage">ActivateAtStorage</see> registry value.</item>
        /// </list>
        /// The second case allows applications written prior to the release of distributed <see cref="COM"/> to be the configuration
        /// of classes for remote activation to be used by client applications available prior to DCOM and the <see cref="RemoteServer"/> flag. The cases
        /// in which there would be no explicit COSERVERINFO structure are when the value is specified as <see langword="null"/> or when it is not one of the function
        /// parameters (as in calls to <see cref="CreateInstance"/> and CoGetClassObject).
        /// </item>
        /// <item>If the explicit COSERVERINFO parameter indicates the current computer, <see cref="RemoteServer"/> is removed if present.</item>
        /// </list>
        /// 
        /// The rest of the processing proceeds by looking at the value(s) in the following sequence:
        /// <list type="number">
        /// <item>If the flags include <see cref="RemoteServer"/> and no COSERVERINFO parameter is specified and if the activation request indicates
        /// a persistent state from which to initialize the object (with CoGetInstanceFromFile, CoGetInstanceFromIStorage, or, for a file moniker,
        /// in a call to IMoniker::BindToObject) and the class has an
        /// <see href="https://learn.microsoft.com/en-us/windows/win32/com/activateatstorage">ActivateAtStorage</see> subkey or no class registry information whatsoever,
        /// the request to activate and initialize is forwarded to the computer where the persistent state resides. (Refer to the remote activation functions
        /// listed in the See Also section for details.)</item>
        /// <item>If the flags include <see cref="InprocServer"/>, the class code in the DLL found under the class's <c>InprocServer32</c>
        /// key is used if this key exists. The class code will run within the same process as the caller.</item>
        /// <item>If the flags include <see cref="InprocHandler"/>, the class code in the DLL found under the class's <c>InprocHandler32</c> key is used if this key exists.
        /// The class code will run within the same process as the caller.</item>
        /// <item>If the flags include <see cref="LocalServer"/>, the class code in the service found under the class's <c>LocalService</c> key is used if this key exists.
        /// If no service is specified but an EXE is specified under that same key, the class code associated with that EXE is used. The class code (in either case)
        /// will be run in a separate service process on the same computer as the caller.</item>
        /// <item>If the flag is set to <see cref="RemoteServer"/> and an additional COSERVERINFO parameter to the function specifies a particular remote computer,
        /// a request to activate is forwarded to this remote computer with flags modified to set to <see cref="LocalServer"/>. The class code will run in its own process
        /// on this specific computer, which must be different from that of the caller.</item>
        /// <item>Finally, if the flags include <see cref="RemoteServer"/> and no COSERVERINFO parameter is specified and if a computer name is given under the
        /// class's <see href="https://learn.microsoft.com/en-us/windows/win32/com/remoteservername">RemoteServerName</see> named-value, the request to activate
        /// is forwarded to this remote computer with the flags modified to be set to <see cref="LocalServer"/>. The class code will run in its own process on
        /// this specific computer, which must be different from that of the caller.</item>
        /// </list>
        /// 
        /// <para><i><see cref="Activate32BitServer"/> and <see cref="Activate64BitServer"/></i></para>
        /// 
        /// <para>The 64-bit versions of Windows introduce two new flags: <see cref="Activate32BitServer"/> and <see cref="Activate64BitServer"/>. On a 64-bit computer,
        /// a 32-bit and 64-bit version of the same <see cref="COM"/> server may coexist. When a client requests an activation of an out-of-process server,
        /// these <see cref="CreateInstanceContext"/> flags allow the client to specify a 32-bit or a 64-bit version of the server.</para>
        /// 
        /// <para>Usually, a client will not care whether it uses a 32-bit or a 64-bit version of the server. However, if the server itself loads an additional
        /// in-process server, then it and the in-process server must both be either 32-bit or 64-bit. For example, suppose that the client wants to use a server "A",
        /// which in turn loads an in-process server "B". If only a 32-bit version of server "B" is available, then the client must specify the 32-bit version of server "A".
        /// If only a 64-bit version of server "B" is available, then the client must specify the 64-bit version of server "A".</para>
        /// 
        /// <para>A server can specify its own architecture preference via the
        /// <see href="https://learn.microsoft.com/en-us/windows/win32/com/preferredserverbitness">PreferredServerBitness</see> registry key, but the client's preference,
        /// specified via a <see cref="Activate32BitServer"/> or <see cref="Activate64BitServer"/> flag, will override the server's preference. If the client does not specify
        /// a preference, then the server's preference will be used.</para>
        /// 
        /// If neither the client nor the server specifies a preference, then:
        /// <list type="bullet">
        /// <item>If the computer that hosts the server is running Windows Server 2003 with Service Pack 1 (SP1) or a later system, then <see cref="COM"/>
        /// will try to match the server architecture to the client architecture. In other words, for a 32-bit client, <see cref="COM"/> will activate a 32-bit server
        /// if available; otherwise it will activate a 64-bit version of the server. For a 64-bit client, <see cref="COM"/> will activate a 64-bit server if available;
        /// otherwise it will activate a 32-bit server.</item>
        /// <item>If the computer that hosts the server is running Windows XP or Windows Server 2003 without SP1 or later installed,
        /// then <see cref="COM"/> will prefer a 64-bit version of the server if available; otherwise it will activate a 32-bit version of the server.</item>
        /// </list>
        /// <para>If a <see cref="CreateInstanceContext"/> enumeration has both the <see cref="Activate32BitServer"/> and <see cref="Activate64BitServer"/> flags set,
        /// then it is invalid and the activation will return <see cref="HRESULT.E_INVALIDARG"/>.</para>
        /// 
        /// 
        /// The flags <see cref="Activate32BitServer"/> and <see cref="Activate64BitServer"/> flow across computer boundaries. If the computer that hosts
        /// the server is running the 64-bit Windows, then it will honor these flags; otherwise it will ignore them.
        /// </remarks>
        [Flags]
        public enum CreateInstanceContext
        {
            /// <summary>
            /// The code that creates and manages objects of this class is a DLL that runs in the same process as the
            /// caller of the function specifying the class context. (CLSCTX_INPROC_SERVER)
            /// </summary>
            InprocServer = 0x1,
            /// <summary>
            /// The code that manages objects of this class is an in-process handler. This is a DLL that runs in the client process and implements client-side structures
            /// of this class when instances of the class are accessed remotely. (CLSCTX_INPROC_HANDLER)
            /// </summary>
            InprocHandler = 0x2,
            /// <summary>
            /// The EXE code that creates and manages objects of this class runs on same machine but is loaded
            /// in a separate process space. (CLSCTX_LOCAL_SERVER)
            /// </summary>
            LocalServer = 0x4,
            /// <summary>
            /// Obsolete. (CLSCTX_INPROC_SERVER16)
            /// </summary>
            [Obsolete("This value is obsolete.")]
            Inproc16 = 0x8,
            /// <summary>
            /// A remote context. The LocalServer32 or LocalService code that creates and
            /// manages objects of this class is run on a different computer. (CLSCTX_REMOTE_SERVER)
            /// </summary>
            RemoteServer = 0x10,
            /// <summary>
            /// Obsolete. (CLSCTX_INPROC_HANDLER16)
            /// </summary>
            [Obsolete("This value is obsolete.")]
            InprocHandler16 = 0x20,
            /// <summary>
            /// Reserved. (CLSCTX_RESERVED1)
            /// </summary>
            [Obsolete("This value is reserved.", DiagnosticId = ErrorDiagIDs.ReservedEnumValue)]
            Reserved1 = 0x40,
            /// <summary>
            /// Reserved. (CLSCTX_RESERVED2)
            /// </summary>
            [Obsolete("This value is reserved.", DiagnosticId = ErrorDiagIDs.ReservedEnumValue)]
            Reserved2 = 0x80,
            /// <summary>
            /// Reserved. (CLSCTX_RESERVED3)
            /// </summary>
            [Obsolete("This value is reserved.", DiagnosticId = ErrorDiagIDs.ReservedEnumValue)]
            Reserved3 = 0x100,
            /// <summary>
            /// Reserved. (CLSCTX_RESERVED4)
            /// </summary>
            [Obsolete("This value is reserved.", DiagnosticId = ErrorDiagIDs.ReservedEnumValue)]
            Reserved4 = 0x200,
            /// <summary>
            /// Disables the downloading of code from the directory service or the Internet.
            /// This flag cannot be set at the same time as <see cref="EnableCodeDownload"/>. (CLSCTX_NO_CODE_DOWNLOAD)
            /// </summary>
            NoCodeDownload = 0x400,
            /// <summary>
            /// Reserved. (CLSCTX_RESERVED5)
            /// </summary>
            [Obsolete("This value is reserved.", DiagnosticId = ErrorDiagIDs.ReservedEnumValue)]
            Reserved5 = 0x800,
            /// <summary>
            /// Specify if you want the activation to fail if it uses custom marshalling. (CLSCTX_NO_CUSTOM_MARSHAL)
            /// </summary>
            NoCustomMarshal = 0x1000,
            /// <summary>
            /// Enables the downloading of code from the directory service or the Internet. This flag cannot be set at the
            /// same time as <see cref="NoCodeDownload"/>. (CLSCTX_ENABLE_CODE_DOWNLOAD)
            /// </summary>
            EnableCodeDownload = 0x2000,
            /// <summary>
            /// <para>The <see cref="NoFailureLog"/> can be used to override the logging of failures in CoCreateInstanceEx.</para>
            /// If the ActivationFailureLoggingLevel is created, the following values can determine the status of event logging:
            /// <list type="bullet">
            /// <item>0 = Discretionary logging. Log by default, but clients can override by
            /// specifying <see cref="NoFailureLog"/> in CoCreateInstanceEx.</item>
            /// <item>1 = Always log all failures no matter what the client specified.</item>
            /// <item>2 = Never log any failures no matter what client specified. If the registry entry is missing,
            /// the default is 0. If you need to control customer applications, it is recommended that you set this value to 0 and write the
            /// client code to override failures. It is strongly recommended that you do not set the value to 2. If event logging is disabled,
            /// it is more difficult to diagnose problems.</item>
            /// </list>
            /// (CLSCTX_NO_FAILURE_LOG)
            /// </summary>
            NoFailureLog = 0x4000,
            /// <summary>
            /// Disables activate-as-activator (AAA) activations for this activation only. This flag overrides the setting of the EOAC_DISABLE_AAA
            /// flag from the EOLE_AUTHENTICATION_CAPABILITIES enumeration. This flag cannot be set at the same time as <see cref="EnableActivateAsActivator"/>.
            /// Any activation where a server process would be launched under the caller's identity is known as an activate-as-activator (AAA) activation.
            /// Disabling AAA activations allows an application that runs under a privileged account (such as LocalSystem) to help prevent its identity 
            /// from being used to launch untrusted components. Library applications that use activation calls should always set this flag during those calls.
            /// This helps prevent the library application from being used in an escalation-of-privilege security attack. This is the only way to disable AAA
            /// activations in a library application because the EOAC_DISABLE_AAA flag from the EOLE_AUTHENTICATION_CAPABILITIES enumeration is applied only to
            /// the server process and not to the library application. (CLSCTX_DISABLE_AAA)
            /// </summary>
            [UnsupportedOSPlatform("windows5.0")]
            DisableActivateAsActivator = 0x8000,
            /// <summary>
            /// Enables activate-as-activator (AAA) activations for this activation only. This flag overrides the setting of the EOAC_DISABLE_AAA
            /// flag from the EOLE_AUTHENTICATION_CAPABILITIES enumeration. This flag cannot be set at the same time
            /// as <see cref="DisableActivateAsActivator"/>. Any activation where a server process would be launched under the caller's identity is known
            /// as an activate-as-activator (AAA) activation. Enabling this flag allows an application to transfer its identity to an activated component. (CLSCTX_ENABLE_AAA)
            /// </summary>
            [UnsupportedOSPlatform("windows5.0")]
            EnableActivateAsActivator = 0x10000,
            /// <summary>
            /// Begin this activation from the default context of the current apartment. (CLSCTX_FROM_DEFAULT_CONTEXT)
            /// </summary>
            FromDefaultContext = 0x20000,
            /// <summary>
            /// Activate or connect to a 32-bit version of the server; fail if one is not registered. (CLSCTX_ACTIVATE_X86_SERVER)
            /// </summary>
            ActivateX86Server = 0x40000,
            /// <summary>
            /// Activate or connect to a 32-bit version of the server; fail if one is not registered. (CLSCTX_ACTIVATE_32_BIT_SERVER)
            /// </summary>
            Activate32BitServer = ActivateX86Server,
            /// <summary>
            /// Activate or connect to a 64 bit version of the server; fail if one is not registered. (CLSCTX_ACTIVATE_64_BIT_SERVER)
            /// </summary>
            Activate64BitServer = 0x80000,
            /// <summary>
            /// When this flag is specified, <see cref="COM"/> uses the impersonation token of the thread, if one is present, for the activation request
            /// made by the thread. When this flag is not specified or if the thread does not have an impersonation token, <see cref="COM"/>
            /// uses the process token of the thread's process for the activation request made by the thread. (CLSCTX_ENABLE_CLOAKING)
            /// </summary>
            [SupportedOSPlatform("windows6.0")]
            EnableCloaking = 0x100000,
            /// <summary>
            /// Indicates activation is for an app container. (CLSCTX_APPCONTAINER)
            /// </summary>
            /// <remarks>This flag is reserved for internal use and is not intended to be used directly from your code.</remarks>
            [Obsolete("This flag is reserved.", DiagnosticId = ErrorDiagIDs.ReservedEnumValue)]
            AppContainer = 0x400000,
            /// <summary>
            /// Specify this flag for Interactive User activation behavior for As-Activator servers. A strongly named Medium IL Windows Store
            /// app can use this flag to launch an "As Activator" <see cref="COM"/> server without a strong name. Also, you can use this flag to bind to a running
            /// instance of the <see cref="COM"/> server that's launched by a desktop application. (CLSCTX_ACTIVATE_AAA_AS_IU)
            /// </summary>
            /// <remarks>
            /// <para>The client must be Medium IL, it must be strongly named, which means that it has a SysAppID in the client token, it can't be in
            /// session 0, and it must have the same user as the session ID's user in the client token.</para>
            /// 
            /// <para>If the server is out-of-process and "As Activator", it launches the server
            /// with the token of the client token's session user. This token won't be strongly named.</para>
            /// 
            /// <para>If the server is out-of-process and RunAs "Interactive User", this flag has no effect.</para>
            /// 
            /// <para>If the server is out-of-process and is any other RunAs type, the activation fails.</para>
            /// 
            /// <para>This flag has no effect for in-process servers.</para>
            /// 
            /// <para>Off-machine activations fail when they use this flag.</para>
            /// </remarks>
            ActivateActivateAsActivatorAsUI = 0x800000,
            /// <summary>
            /// This value is reserved. (CLSCTX_RESERVED6)
            /// </summary>
            [Obsolete("This value is reserved.", DiagnosticId = ErrorDiagIDs.ReservedEnumValue)]
            Reserved6 = 0x1000000,
            /// <summary>
            /// CLSCTX_ACTIVATE_ARM32_SERVER
            /// </summary>
            ActivateArm32Server = 0x2000000,
            /// <summary>
            /// Used for loading Proxy/Stub DLLs. (CLSCTX_PS_DLL)
            /// </summary>
            /// <remarks>This flag is reserved for internal use and is not intended to be used directly from your code.</remarks>
            [Obsolete("This value is reserved.", DiagnosticId = ErrorDiagIDs.ReservedEnumValue)]
            ProxyStubDll = unchecked((int)0x80000000)
        }
    }
}
