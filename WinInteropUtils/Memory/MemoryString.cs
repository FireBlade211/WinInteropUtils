using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace FireBlade.WinInteropUtils.Memory
{
    /// <summary>
    /// Represents the encoding of a <see cref="MemoryString"/>.
    /// </summary>
    public enum MemoryStringEncoding
    {
        /// <summary>
        /// The string should be allocated as a Unicode (UTF-16) string.
        /// </summary>
        Unicode,
        /// <summary>
        /// The string should be allocated as an ANSI string.
        /// </summary>
        Ansi,
        ///// <summary>
        ///// The encoding should be detected automatically.
        ///// </summary>
        //Auto
    }

    /// <summary>
    /// Represents a string allocated in memory.
    /// </summary>
    public sealed class MemoryString : IDisposable
    {
        private nint _ptr;
        private string _str;
        private MemoryStringEncoding _encode;

        /// <summary>
        /// Gets the in-memory pointer to the string.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The string was disposed.</exception>
        // we throw a disposed exception so that the user doesnt get a random av later from using a disposed pointer
        public nint Pointer => _disposed ? throw new ObjectDisposedException(nameof(MemoryString)) : _ptr;

        /// <summary>
        /// Gets the length, in Unicode <see cref="char">chars</see>, of the allocated string.
        /// </summary>
        public int Length => _str.Length;

        /// <summary>
        /// Gets the length, in bytes, of the allocated string.
        /// </summary>
        public int ByteCount => _encode switch
        {
            MemoryStringEncoding.Unicode => (_str.Length + 1) * sizeof(char),
            MemoryStringEncoding.Ansi => System.Text.Encoding.Default.GetByteCount(_str) + 1,
            //MemoryStringEncoding.Auto => Marshal.SystemDefaultCharSize,
            _ => throw new InvalidOperationException()
        };

        /// <summary>
        /// Gets the original string.
        /// </summary>
        public string OriginalString => _str;

        /// <summary>
        /// Gets the encoding of the string.
        /// </summary>
        public MemoryStringEncoding Encoding => _encode;

        /// <summary>
        /// Allocates a new memory pointer to a string.
        /// </summary>
        /// <param name="str">The string to allocate a pointer to.</param>
        /// <param name="encode">The encoding to encode the string with.</param>
        /// <exception cref="ArgumentNullException">The string was <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The encoding specified is invalid.</exception>
        public MemoryString(string str, MemoryStringEncoding encode = MemoryStringEncoding.Unicode)
        {
            ArgumentNullException.ThrowIfNull(str);
            
            _str = str;
            _encode = encode;
            _ptr = encode switch
            {
                MemoryStringEncoding.Unicode => Marshal.StringToHGlobalUni(str),
                MemoryStringEncoding.Ansi => Marshal.StringToHGlobalAnsi(str),
                //MemoryStringEncoding.Auto => Marshal.StringToHGlobalAuto(str),
                _ => throw new ArgumentException("The encoding specified is invalid.")
            };
        }

        private bool _disposed = false;

        private void Free()
        {
            if (_disposed)
                return;

            Marshal.FreeHGlobal(_ptr);
            _ptr = nint.Zero;

            _disposed = true;
        }

        /// <summary>
        /// Releases the memory string.
        /// </summary>
        public void Dispose()
        {
            Free();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the memory string.
        /// </summary>
        ~MemoryString() => Free();

        /// <summary>
        /// Converts this <see cref="MemoryString"/> to its pointer.
        /// </summary>
        /// <param name="str">The <see cref="MemoryString"/> whose pointer is to be retrieved.</param>
        public static implicit operator nint(MemoryString str) => str.Pointer;

        /// <summary>
        /// Returns the original string.
        /// </summary>
        /// <returns>The original allocated string.</returns>
        public override string ToString() => OriginalString;
    }
}
