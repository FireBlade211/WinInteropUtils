using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FireBlade.WinInteropUtils.WinForms
{
    /// <summary>
    /// Represents a control used to enter and edit text.
    /// </summary>
    internal partial class Edit : WinInteropUtilsControlBase
    {
        private const int WS_BORDER = 0x00800000;
        private const int WS_EX_CLIENTEDGE = 0x00000200;
        private const int ES_AUTOHSCROLL = 0x0080;
        private const int ES_AUTOVSCROLL = 0x0040;
        private const int ES_MULTILINE = 0x0004;
        private const int ES_READONLY = 2048;
        private const int ES_LOWERCASE = 16;
        private const int ES_UPPERCASE = 8;
        private const int WS_EX_RIGHT = 0x00001000;
        private const int ES_LEFT = 0;
        private const int ES_CENTER = 1;
        private const int ES_RIGHT = 2;
        private const int EM_SETCUEBANNER = 0x1501;

        private BorderStyle _border = BorderStyle.Fixed3D;

        /// <summary>
        /// Gets or sets the border type of the Edit control.
        /// </summary>
        [Description("Gets or sets the border type of the Edit control.")]
        [Category("Appearance")]
        [DefaultValue(BorderStyle.Fixed3D)]
        public BorderStyle BorderStyle
        {
            get => _border;
            set
            {
                _border = value;
                RecreateHandle();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassName = "EDIT";

                cp.Style |= (int)AutoScroll;

                cp.Style &= ~WS_BORDER;
                cp.ExStyle &= ~WS_EX_CLIENTEDGE;
                
                switch (BorderStyle)
                {
                    case BorderStyle.Fixed3D:
                        cp.ExStyle |= WS_EX_CLIENTEDGE;
                        break;
                    case BorderStyle.FixedSingle:
                        cp.Style |= WS_BORDER;
                        break;
                }

                if (Multiline)
                    cp.Style |= ES_MULTILINE;

                if (ReadOnly)
                    cp.Style |= ES_READONLY;

                switch (CharacterCasing)
                {
                    case CharacterCasing.Lower:
                        cp.Style |= ES_LOWERCASE;
                        break;
                    case CharacterCasing.Upper:
                        cp.Style |= ES_UPPERCASE;
                        break;
                }

                // WS_EX_RIGHT overrides the ES_XXXX alignment styles
                cp.ExStyle &= ~WS_EX_RIGHT;

                switch (Alignment)
                {
                    case HorizontalAlignment.Left:
                        cp.Style |= ES_LEFT;
                        break;
                    case HorizontalAlignment.Center:
                        cp.Style |= ES_CENTER;
                        break;
                    case HorizontalAlignment.Right:
                        cp.Style |= ES_RIGHT;
                        break;
                }
                
                cp.Style |= (int)InputType;

                return cp;
            }
        }

#nullable disable
        /// <summary>
        /// Gets or sets the text in the Edit control.
        /// </summary>
        [Description("Gets or sets the text in the Edit control.")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }
#nullable enable

        private bool _multiline;

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="Edit"/> control supports multiple lines.
        /// </summary>
        [Description("Gets or sets a value that indicates whether the Edit control supports multiple lines.")]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool Multiline
        {
            get => _multiline;
            set
            {
                _multiline = value;
                _cueBanner = string.Empty;
                RecreateHandle();
            }
        }

        private EditAutoScroll _autoScroll = EditAutoScroll.Both;

        /// <summary>
        /// Gets or sets the auto-scroll mode for the <see cref="Edit"/> control.
        /// </summary>
        [Description("Gets or sets the auto-scroll mode for the Edit control.")]
        [DefaultValue(EditAutoScroll.Both)]
        [Category("Behavior")]
        public EditAutoScroll AutoScroll
        {
            get => _autoScroll;
            set
            {
                _autoScroll = value;
                RecreateHandle();
            }
        }

        private bool _readonly = false;

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="Edit"/> control is read-only.
        /// </summary>
        [Description("Gets or sets a value that indicates whether the Edit control is read-only.")]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get => _readonly;
            set
            {
                _readonly = value;

                OnBackColorChanged(EventArgs.Empty);
            }
        }

        private bool _backColorSet;
        private Color _backColor;

        /// <summary>
        /// Gets or sets the background color of the control.
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets the background color of the control.")]
        public override Color BackColor
        {
            get
            {
                if (!_backColorSet)
                    return ReadOnly ? SystemColors.Control : SystemColors.Window;

                return _backColor;
            }
            set
            {
                _backColorSet = true;
                _backColor = value;
                base.BackColor = value;
            }
        }

        private bool ShouldSerializeBackColor() => _backColorSet;
        private new void ResetBackColor() => _backColorSet = false;

        public Edit()
        {
            InitializeComponent();
            SetStyle(ControlStyles.StandardClick
            | ControlStyles.StandardDoubleClick
            | ControlStyles.UseTextForAccessibility
            | ControlStyles.UserPaint, false);
        }

        private CharacterCasing _casing = CharacterCasing.Normal;

        /// <summary>
        /// Gets or sets whether the <see cref="Edit"/> control modifies the case of characters as they are typed.
        /// </summary>
        [Description("Gets or sets whether the Edit control modifies the case of characters as they are typed.")]
        [Category("Behavior")]
        [DefaultValue(CharacterCasing.Normal)]
        public CharacterCasing CharacterCasing
        {
            get => _casing;
            set
            {
                _casing = value;
                RecreateHandle();
            }
        }

        private HorizontalAlignment _align = HorizontalAlignment.Left;

        /// <summary>
        /// Gets or sets how text is aligned in a <see cref="Edit"/> control.
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets how text is aligned in an Edit control.")]
        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment Alignment
        {
            get => _align;
            set
            {
                _align = value;
                RecreateHandle();
            }
        }

        private string _cueBanner = string.Empty;
        private bool _cueBannerFocus = false;

        /// <summary>
        /// Gets or sets the cue banner (placeholder text) displayed in the control.
        /// </summary>
        /// <remarks>A cue banner cannot be set on a multiline <see cref="Edit"/> control.</remarks>
        [Description("Gets or sets the cue banner (placeholder text) displayed in the control.")]
        [Category("Appearance")]
        [DefaultValue("")]
        public string CueBanner
        {
            // we cant use EM_GETCUEBANNER because it doesn't give len
            get => _cueBanner;
            set
            {
                // the designer shows an error dialog "invalid property value" when we do this
                if (Multiline)
                    throw new InvalidOperationException("A cue banner cannot be set on a multiline Edit control.");

                _cueBanner = value;
                var ptr = Marshal.StringToHGlobalUni(_cueBanner);

                Window.SendMessage(EM_SETCUEBANNER, _cueBannerFocus, ptr);

                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the cue banner is shown even while the control has focus.
        /// </summary>
        [Description("Gets or sets a value that indicates whether the cue banner is shown even while the control has focus.")]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool ShowCueBannerOnFocus
        {
            get => _cueBannerFocus;
            set
            {
                _cueBannerFocus = value;
                var ptr = Marshal.StringToHGlobalUni(_cueBanner);

                Window.SendMessage(EM_SETCUEBANNER, value, ptr);

                Marshal.FreeHGlobal(ptr);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            var ptr = Marshal.StringToHGlobalUni(_cueBanner);

            Window.SendMessage(EM_SETCUEBANNER, _cueBannerFocus, ptr);

            Marshal.FreeHGlobal(ptr);
        }

        private EditInputType _inputType = EditInputType.Text;

        /// <summary>
        /// Gets or sets the type of input accepted by the control.
        /// </summary>
        [Description("Gets or sets the type of input accepted by the control.")]
        [Category("Behavior")]
        [DefaultValue(EditInputType.Text)]
        public EditInputType InputType
        {
            get => _inputType;
            set
            {
                _inputType = value;
                RecreateHandle();
            }
        }
    }

    /// <summary>
    /// Represents the auto-scroll mode of an <see cref="Edit"/> control.
    /// </summary>
    [Flags]
    internal enum EditAutoScroll
    {
        None = 0,
        Horizontal = 0x0080,
        Vertical = 0x0040,
        Both = Horizontal | Vertical
    }

    /// <summary>
    /// Represents the input type of an <see cref="Edit"/> control.
    /// </summary>
    [Flags]
    internal enum EditInputType
    {
        /// <summary>
        /// The user can input any text.
        /// </summary>
        Text = 0,
        /// <summary>
        /// The user can input only numbers.
        /// </summary>
        Number = 0x2000,
        /// <summary>
        /// The user can input any text, but the text will be concealed with a password character.
        /// </summary>
        Password = 0x0020,
        /// <summary>
        /// The user can input only numbers, but the numbers will be concealed with password characters.
        /// </summary>
        NumberPassword = Number | Password
    }
}
