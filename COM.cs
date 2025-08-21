using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FireBlade.WinInteropUtils
{
    /// <summary>
    /// Provides COM (Component Object Model) methods from <c>Ole32.dll</c>.
    /// </summary>
    public static partial class COM
    {
        [LibraryImport("ole32.dll", EntryPoint = "CoInitializeEx")]
        private static partial int CoInitializeExW(IntPtr pvReserved, uint dwCoInit);

        /// <summary>
        /// Initializes the COM library for use by the calling thread, sets the thread's concurrency model, and creates a new apartment for the thread if one is required.
        /// </summary>
        /// <param name="coInit">The concurrency model and initialization options for the thread. Any combination of values from <see cref="CoInit"/> can be used, except
        /// that the <see cref="CoInit.ApartmentThreaded"/> and <see cref="CoInit.MultiThreaded"/> flags cannot both be set.
        /// The default is <see cref="CoInit.MultiThreaded"/>.</param>
        /// <returns>This function can return either <see cref="HRESULT.S_OK"/> or <see cref="HRESULT.S_FALSE"/>.</returns>
        /// <remarks>
        /// <para><see cref="CoInitializeEx(CoInit)"/> must be called at least once, and is usually called only once, for each thread that uses the COM library. Multiple calls
        /// to <see cref="CoInitializeEx(CoInit)"/> by the same thread are allowed as long as they pass the same concurrency flag, but subsequent valid calls return
        /// <see cref="HRESULT.S_FALSE"/>. If the concurrency flag does not match, then the call fails and returns <see cref="HRESULT.RPC_E_CHANGED_MODE"/>. 
        /// (For the purpose of this rule, a call to CoInitialize is equivalent to calling <see cref="CoInitializeEx(CoInit)"/> with the <see cref="CoInit.ApartmentThreaded"/> flag.) To
        /// uninitialize the COM library gracefully on a thread, each successful call to CoInitialize or <see cref="CoInitializeEx(CoInit)"/>, including any call that
        /// returns <see cref="HRESULT.S_FALSE"/>, must be balanced by a corresponding call to <see cref="CoUninitialize"/>. Once COM has been uninitialized on a thread,
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
        /// an STA or the MTA, that thread transfers to the NTA. If this thread subsequently calls <see cref="CoInitializeEx(CoInit)"/>, the call fails and returns
        /// <see cref="HRESULT.RPC_E_CHANGED_MODE"/>.</para>
        ///
        /// <para>Because OLE technologies are not thread-safe, the OleInitialize function calls <see cref="CoInitializeEx(CoInit)"/> with the <see cref="CoInit.ApartmentThreaded"/> flag. As a result,
        /// an apartment that is initialized for multithreaded object concurrency cannot use the features enabled by OleInitialize.</para>
        ///
        /// Because there is no way to control the order in which in-process servers are loaded or unloaded, do not call CoInitialize, <see cref="CoInitializeEx(CoInit)"/>,
        /// or <see cref="CoUninitialize"/> from the <c>DllMain</c> function.
        /// </remarks>
        public static HRESULT CoInitializeEx(CoInit coInit)
        {
            return (HRESULT)CoInitializeExW(nint.Zero, (uint)coInit);
        }

        /// <summary>
        /// Closes the COM library on the current thread, unloads all DLLs loaded by the thread,
        /// frees any other resources that the thread maintains, and forces all RPC connections on the thread to close.
        /// </summary>
        /// <remarks>
        /// <para>A thread must call <see cref="CoUninitialize"/> once for each successful call it has made to the CoInitialize or <see cref="CoInitializeEx(CoInit)"/> function,
        /// including any call that returns S_FALSE. Only the <see cref="CoUninitialize"/> call corresponding to the CoInitialize or <see cref="CoInitializeEx(CoInit)"/> call that initialized the library
        /// can close it.</para>
        ///
        /// <para>Calls to OleInitialize must be balanced by calls to OleUninitialize. The OleUninitialize function calls <see cref="CoUninitialize"/> internally, so applications
        /// that call OleUninitialize do not also need to call <see cref="CoUninitialize"/>.</para>
        ///
        ///
        /// <para><see cref="CoUninitialize"/> should be called on application shutdown, as the last call made to the COM library after the application hides
        /// its main windows and falls through its main message loop. If there are open conversations remaining, <see cref="CoUninitialize"/>
        /// starts a modal message loop and dispatches any pending messages from the containers or server for this COM application.
        /// By dispatching the messages, <see cref="CoUninitialize"/> ensures that the application does not quit before receiving all of its pending messages.
        /// Non-COM messages are discarded.</para>
        ///
        ///
        /// Because there is no way to control the order in which in-process servers are loaded or unloaded, do not call CoInitialize, <see cref="CoInitializeEx(CoInit)"/>,
        /// or <see cref="CoUninitialize"/> from the <c>DllMain</c> function.
        /// </remarks>
        [LibraryImport("ole32.dll")]
        public static partial void CoUninitialize();

        /// <summary>
        /// Specifies flags for <see cref="CoInitializeEx(CoInit)"/>.
        /// </summary>
        /// <remarks>
        /// <para>When a thread is initialized through a call to <see cref="CoInitializeEx(CoInit)"/>, you choose whether to initialize it as apartment-threaded or
        /// multithreaded by designating one of the members of <see cref="CoInit"/> as its parameter. This designates how incoming calls to any object created by that
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
        public enum CoInit
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
    }
}
