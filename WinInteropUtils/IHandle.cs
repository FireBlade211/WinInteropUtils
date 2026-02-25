using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireBlade.WinInteropUtils
{
    /// <summary>
    /// Represents an interface for classes wrapping unmanaged handles.
    /// </summary>
    public interface IHandle
    {
        /// <summary>
        /// The handle of the object.
        /// </summary>
        public nint Handle { get; }
    }
}
