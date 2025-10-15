using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace FireBlade.WinInteropUtils.WinForms
{
    /// <summary>
    /// Represents a control that allows the user to enter a combination of keystrokes to be used as a hot key.
    /// ![Sample image](../images/hotkeys.png)
    /// </summary>
    public partial class HotKeyBox : Control
    {
        private const int WM_USER = 0x0400;
        private const int HKM_GETHOTKEY = WM_USER + 2;
        private const int HKM_SETHOTKEY = WM_USER + 1;
        private const int HKM_SETRULES = WM_USER + 3;
        private const uint WM_REFLECT = WM_USER + 0x1C00;
        private const int WM_COMMAND = 0x0111;
        private const int EN_CHANGE = 0x0300;

        private const int HOTKEYF_CONTROL = 0x02;
        private const int HOTKEYF_ALT = 0x04;
        private const int HOTKEYF_SHIFT = 0x01;
        private const int HOTKEYF_EXT = 0x08;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassName = "msctls_hotkey32";

                return cp;
            }
        }

        /// <summary>
        /// Gets or sets the keys currently in the hot key box.
        /// </summary>
        [Description("Gets or sets the keys currently in the hot key box.")]
        [Category("Behavior")]
        [DefaultValue(Keys.None)]
        public Keys Keys
        {
            get
            {
                if (!IsHandleCreated) return _keys;

                var info = User32.SendMessage(Handle, HKM_GETHOTKEY, 0, 0);
                var vk = Macros.LowByte(Macros.LowWord(info));
                var keys = (Keys)vk;

                var mods = Macros.HighByte(Macros.LowWord(info));
                if ((mods & HOTKEYF_CONTROL) != 0)
                    keys |= Keys.Control;

                if ((mods & HOTKEYF_SHIFT) != 0)
                    keys |= Keys.Shift;

                if ((mods & HOTKEYF_ALT) != 0)
                    keys |= Keys.Alt;

                return keys;
            }
            set
            {
                _keys = value;

                if (!IsHandleCreated) return;

                bool ctrl = (value & Keys.Control) != 0;
                bool shift = (value & Keys.Shift) != 0;
                bool alt = (value & Keys.Alt) != 0;

                var lowByte = (ushort)value;
                var highByte = (ushort)0u;

                if (ctrl)
                    highByte |= HOTKEYF_CONTROL;

                if (shift)
                    highByte |= HOTKEYF_SHIFT;

                if (alt)
                    highByte |= HOTKEYF_ALT;

                if (IsExKey)
                    highByte |= HOTKEYF_EXT;

                var packed = (ushort)((highByte << 8) | lowByte);

                User32.SendMessage(Handle, HKM_SETHOTKEY, packed, 0);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control is using the extended key.
        /// </summary>
        [Description("Gets or sets a value indicating whether the control is using the extended key.")]
        [DefaultValue(false)]
        [Category("Behavior")]
        public bool IsExKey
        {
            get
            {
                if (!IsHandleCreated) return _exKey;

                var info = User32.SendMessage(Handle, HKM_GETHOTKEY, 0, 0);
                var mods = Macros.HighByte(Macros.LowWord(info));

                return (mods & HOTKEYF_EXT) != 0;
            }
            set
            {
                _exKey = value;

                if (!IsHandleCreated) return;

                var keys = Keys;

                bool ctrl = (keys & Keys.Control) != 0;
                bool shift = (keys & Keys.Shift) != 0;
                bool alt = (keys & Keys.Alt) != 0;

                var lowByte = (ushort)keys;
                var highByte = (ushort)0u;

                if (ctrl)
                    highByte |= HOTKEYF_CONTROL;

                if (shift)
                    highByte |= HOTKEYF_SHIFT;

                if (alt)
                    highByte |= HOTKEYF_ALT;

                if (value)
                    highByte |= HOTKEYF_EXT;

                var packed = (ushort)((highByte << 8) | lowByte);

                User32.SendMessage(Handle, HKM_SETHOTKEY, packed, 0);
            }
        }

        // no getter message, we have to store this ourselves
        private HotKeyBoxRules _rules = HotKeyBoxRules.None;
        private HotKeyBoxModifiers _ruleFbVal = HotKeyBoxModifiers.None;
        private Keys _keys = Keys.None;
        private bool _exKey = false;

        /// <summary>
        /// Gets or sets the invalid modifiers for a hot key control.
        /// </summary>
        [Editor(typeof(FlagsEnumEditor), typeof(UITypeEditor))]
        [DefaultValue(HotKeyBoxRules.None)]
        [Category("Rules")]
        [Description("Gets or sets the invalid modifiers for a hot key control.")]
        public HotKeyBoxRules Rules
        {
            get => _rules;
            set
            {
                _rules = value;

                if (!IsHandleCreated) return;

                User32.SendMessage(Handle, HKM_SETRULES, (nuint)value, (nint)FallbackValue);
            }
        }

        /// <summary>
        /// Gets or sets the modifiers that the control should fall back to when the user inputs an invalid key combination.
        /// </summary>
        /// <remarks>
        /// When a user enters an invalid key combination, as defined by rules specified in <see cref="Rules"/>, the system uses the bitwise-OR
        /// operator to combine the keys entered by the user with the flags specified in <see cref="FallbackValue"/>. The resulting key combination
        /// is converted into a string and then displayed in the hot key control.
        /// </remarks>
        [Editor(typeof(FlagsEnumEditor), typeof(UITypeEditor))]
        [Description("Gets or sets the modifiers that the control should fall back to when the user inputs an invalid key combination.")]
        [Category("Rules")]
        [DefaultValue(HotKeyBoxModifiers.None)]
        public HotKeyBoxModifiers FallbackValue
        {
            get => _ruleFbVal;
            set
            {
                _ruleFbVal = value;

                if (!IsHandleCreated) return;

                User32.SendMessage(Handle, HKM_SETRULES, (nuint)Rules, (nint)value);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg >= WM_REFLECT)
            {
                WmReflect(m);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            User32.SendMessage(Handle, HKM_SETRULES, (nuint)Rules, (nint)FallbackValue);

            bool ctrl = (_keys & Keys.Control) != 0;
            bool shift = (_keys & Keys.Shift) != 0;
            bool alt = (_keys & Keys.Alt) != 0;

            var lowByte = (ushort)_keys;
            var highByte = (ushort)0u;

            if (ctrl)
                highByte |= HOTKEYF_CONTROL;

            if (shift)
                highByte |= HOTKEYF_SHIFT;

            if (alt)
                highByte |= HOTKEYF_ALT;

            if (IsExKey)
                highByte |= HOTKEYF_EXT;

            var packed = (ushort)((highByte << 8) | lowByte);

            User32.SendMessage(Handle, HKM_SETHOTKEY, packed, 0);
        }

        private void WmReflect(Message m)
        {
            switch (m.Msg - WM_REFLECT)
            {
                case WM_COMMAND:
                    if (Macros.HighWord(m.WParam) == EN_CHANGE && m.LParam == Handle)
                    {
                        HotKeyChanged?.Invoke(this, new EventArgs());
                    }
                    break;
            }
        }

        /// <summary>
        /// Fires when the hot key in the control changes.
        /// </summary>
        [Description("Fires when the hot key in the control changes.")]
        [Category("Action")]
        public event EventHandler? HotKeyChanged;

        public HotKeyBox()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, false);
        }
    }

    /// <summary>
    /// Specifies the invalid modifiers for a <see cref="HotKeyBox"/>.
    /// </summary>
    [Flags]
    public enum HotKeyBoxRules
    {
        /// <summary>
        /// All modifiers are allowed.
        /// </summary>
        None = 0x0001,
        /// <summary>
        /// The Alt modifier is invalid.
        /// </summary>
        Alt = 0x0008,
        /// <summary>
        /// The Ctrl modifier is invalid.
        /// </summary>
        Ctrl = 0x0004,
        /// <summary>
        /// The Ctrl modifier is invalid.
        /// </summary>
        Control = Ctrl,
        /// <summary>
        /// The combination of the Ctrl and Alt modifiers are invalid.
        /// </summary>
        CtrlAlt = 0x0040,
        /// <summary>
        /// The Shift modifier is invalid.
        /// </summary>
        Shift = 0x0002,
        /// <summary>
        /// The combination of the Shift and Alt modifiers are invalid.
        /// </summary>
        ShiftAlt = 0x0020,
        /// <summary>
        /// The combination of the Shift and Ctrl modifiers are invalid.
        /// </summary>
        ShiftCtrl = 0x0010,
        /// <summary>
        /// The combination of the Shift, Ctrl, and Alt modifiers are invalid.
        /// </summary>
        ShiftCtrlAlt = 0x0080
    }

    /// <summary>
    /// Specifies the modifier keys in a <see cref="HotKeyBox"/>.
    /// </summary>
    [Flags]
    public enum HotKeyBoxModifiers
    {
        /// <summary>
        /// No modifiers.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// The Ctrl key.
        /// </summary>
        Ctrl = 0x02,
        /// <summary>
        /// The Ctrl key.
        /// </summary>
        Control = Ctrl,
        /// <summary>
        /// The Alt key.
        /// </summary>
        Alt = 0x04,
        /// <summary>
        /// The Shift key.
        /// </summary>
        Shift = 0x01,
        /// <summary>
        /// The extended key.
        /// </summary>
        Extended = 0x08
    }

    /// <summary>
    /// Provides a type editor for enumerations with the <see cref="FlagsAttribute"/>.
    /// </summary>
    public class FlagsEnumEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
            => UITypeEditorEditStyle.DropDown;

        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider? provider, object? value)
        {
            if (provider?.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService edSvc)
                return value;

            var enumType = value?.GetType();
            if (enumType != null && value != null)
            {
                var enumValues = Enum.GetValues(enumType);

                var checkedListBox = new CheckedListBox
                {
                    BorderStyle = BorderStyle.None,
                    CheckOnClick = true,
                    Height = Math.Min(enumValues.Length * 20, 240)
                };

                int selectedValue = (int)value;

                foreach (Enum val in enumValues)
                {
                    int intVal = Convert.ToInt32(val);
                    checkedListBox.Items.Add(val, (selectedValue & intVal) == intVal && intVal != 0);
                }

                edSvc.DropDownControl(checkedListBox);

                int result = 0;
                foreach (var item in checkedListBox.CheckedItems)
                    result |= Convert.ToInt32(item);

                return Enum.ToObject(enumType, result);
            }

            return null;
        }
    }
}
