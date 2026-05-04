using FireBlade.WinInteropUtils.ComponentObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace FireBlade.WinInteropUtils.Memory
{
    /// <summary>
    /// Represents a smart COM pointer that automatically releases its associated COM interface on dispose.
    /// </summary>
    /// <typeparam name="T">The COM interface to wrap.</typeparam>
    /// <remarks>This is a managed version of the <c>CComPtr</c> class from the ATL (Active Template Library).</remarks>
    public sealed class ComSmartPointer<T> : IDisposable where T : notnull
    {
        private T _interface;

        /// <summary>
        /// The COM interface.
        /// </summary>
        public T Interface => IsDisposed ? throw new ObjectDisposedException("The COM interface was disposed.") : _interface;

        /// <summary>
        /// Creates a new instance of the <see cref="ComSmartPointer{T}"/> class from an existing instance of
        /// an interface.
        /// </summary>
        /// <param name="it">The interface to wrap.</param>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> must be an interface.</exception>
        public ComSmartPointer(T it)
        {
            if (!typeof(T).IsInterface || !Marshal.IsComObject(it))
                throw new ArgumentException("T must be a COM interface.");

            _interface = it;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ComSmartPointer{T}"/> by creating a new instance of an interface.
        /// </summary>
        /// <param name="rclsid">The CLSID associated with the data and code that will be used to create the object.</param>
        /// <param name="pUnkOuter">If <see langword="null"/>, indicates that the object is not being created as part of an aggregate.
        /// If non-<see langword="null"/>, pointer to the aggregate object's <c>IUnknown</c> interface
        /// (the controlling <c>IUnknown</c>).</param>
        /// <param name="dwClsContext">Context in which the code that manages the newly created object will run.
        /// A bitwise combination of <see cref="COM.CreateInstanceContext"/> values.</param>
        /// <exception cref="ArgumentException">This exception can get thrown in 2 cases:
        /// <list type="bullet">
        ///     <item><typeparamref name="T"/> must be an interface.</item>
        ///     <item><paramref name="pUnkOuter"/> must be a COM object or <see langword="null"/>.</item>
        /// </list>
        /// </exception>
        /// <exception cref="COMException">This exception can get thrown in 3 cases:
        /// <list type="bullet">
        ///     <item>Failed to create the COM interface.</item>
        ///     <item>A specified class is not registered in the registration database. Also can indicate that the
        ///     type of server you requested in the <see cref="COM.CreateInstanceContext"/> enumeration is not registered
        ///     or the values for the server types in the registry are corrupt.</item>
        ///     <item>This class cannot be created as part of an aggregate.</item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidCastException">The specified class does not implement the requested interface,
        /// or the controlling <c>IUnknown</c> does not expose the requested interface.</exception>
        /// <exception cref="UnauthorizedAccessException">The request may be denied because the COM object cannot be created in
        /// UWP applications.</exception>
        public ComSmartPointer(Guid rclsid, [AllowNull] object? pUnkOuter, COM.CreateInstanceContext dwClsContext)
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("T must be an interface.");

            _interface = COM.CreateInstance<T>(rclsid, pUnkOuter, dwClsContext) ??
                throw new COMException("Failed to create COM interface: " + typeof(T).Name);
        }

        private bool _disposed;

        /// <summary>
        /// Gets a value indicating whether the COM interface was disposed.
        /// </summary>
        public bool IsDisposed => _disposed;

        /// <summary>
        /// Releases the COM interface.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Marshal.ReleaseComObject(_interface);
        }

        /// <summary>
        /// Converts this <see cref="ComSmartPointer{T}"/> to its COM interface.
        /// </summary>
        /// <param name="ptr">The <see cref="ComSmartPointer{T}"/> to extract the interface from.</param>
        public static implicit operator T(ComSmartPointer<T> ptr) => ptr.Interface;
    }
}
