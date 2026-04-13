using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireBlade.WinInteropUtils.WinForms
{
    /// <summary>
    /// Represents the base class for WinInteropUtils.WinForms controls.
    /// </summary>
    public abstract class WinInteropUtilsControlBase : Control
    {
#nullable disable
        /// <summary>
        /// Gets the WinInteropUtils <see cref="FireBlade.WinInteropUtils.Window"/> for the current control.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Window Window => Window.FromHandle(Handle);
    }
}
