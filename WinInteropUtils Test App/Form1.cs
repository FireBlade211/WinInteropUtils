using FireBlade.WinInteropUtils;
using FireBlade.WinInteropUtils.ComponentObjectModel;
using FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces;
using FireBlade.WinInteropUtils.Dialogs;
using FireBlade.WinInteropUtils.Memory;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms.Design;

namespace WinInteropUtils_Test_App
{
    public partial class Form1 : Form
    {
        private ColorPickerDialog? _colordlg;
        private uint _commDlgHelpId;

        public Form1()
        {
            InitializeComponent();
            fileDialogToolStripMenuItem.Click += fileDialogToolStripMenuItem_Click;

            LoadConfig();

            listView1.BeginUpdate();
            foreach (var c in Assembly.GetAssembly(typeof(Shell32))!.GetTypes()
                .Where(t => (t.IsClass || t.IsValueType) && t.Namespace?.StartsWith("FireBlade.WinInteropUtils") == true))
            {
                if (c.Name == "ExceptionExtensions") continue;

                var items = new List<ListViewItem>();
                foreach (var method in c.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    var item = new ListViewItem();
                    item.Tag = method;

                    var cParams = method.GetParameters().Select(x => GetTypeName(x.ParameterType));

                    item.Text = $"{GetTypeName(method.ReturnType)} {method.Name}({string.Join<string>(", ", cParams)})";

                    items.Add(item);
                }

                foreach (var ctor in c.GetConstructors(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                {
                    var item = new ListViewItem();
                    item.Tag = ctor;

                    var cParams = ctor.GetParameters().Select(x => GetTypeName(x.ParameterType));

                    item.Text = $"{GetTypeName(c)}({string.Join<string>(", ", cParams)})";
                    items.Add(item);
                }

                var found = listView1.Groups.Cast<ListViewGroup>().FirstOrDefault(x => x.Name == c.Name);
                if (found != null)
                {
                    foreach (var item in items)
                    {
                        item.Group = found;
                        found.Items.Add(item);
                    }
                }
                else
                {
                    if (c.Name.EndsWith("Extensions")) continue;

                    var group = new ListViewGroup();
                    group.Header = c.Name;
                    group.CollapsedState = ListViewGroupCollapsedState.Expanded;

                    listView1.Groups.Add(group);

                    foreach (var item in items)
                    {
                        item.Group = group;
                        group.Items.Add(item);
                    }
                }

                foreach (var item in items)
                {
                    listView1.Items.Add(item);
                }
            }

            listView1.EndUpdate();

            _commDlgHelpId = User32.RegisterWindowMessage("commdlg_help");
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == _commDlgHelpId)
            {
                var wnd = new HwndWindow(m.WParam);
                MessageBox.Show(wnd, "Help requested!", "Help");
            }
        }

        public static string GetTypeName<T>() => GetTypeName(typeof(T));

        public static string GetTypeName(Type type)
        {
            return type.Name.TrimEnd('&') switch
            {
                "Boolean" => "bool",
                "IntPtr" => "nint",
                "UIntPtr" => "nuint",
                "String" => "string",
                "Void" => "void",
                "Int32" => "int",
                "UInt32" => "uint",
                "Nullable`1" => GetTypeName(type.GetGenericArguments().First()) + "?",
                "Double" => "double",
                "Decimal" => "decimal",
                "Single" => "float",
                "UInt16" => "ushort",
                "Byte" => "byte",
                _ => type.Name.TrimEnd('&')
            };
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                propertyGrid1.SelectedObject = new MethodArgumentDescriptor((listView1.SelectedItems[0].Tag as MethodBase)!);
                callMethodToolStripMenuItem.Enabled = true;
            }
        }

        private void callMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var item = listView1.SelectedItems[0];

                if (item.Tag is MethodBase method)
                {
                    if (propertyGrid1.SelectedObject is MethodArgumentDescriptor descriptor)
                    {
                        var result = method is ConstructorInfo ctor ?
                            ctor.Invoke(descriptor.Values.Values.ToArray())
                            : method.Invoke(null, descriptor.Values.Values.ToArray());

                        var page = new TaskDialogPage
                        {
                            Buttons = [TaskDialogButton.OK],
                            Heading = "Return Value",
                            Caption = "Call Method",
                            Text = "The method completed successfully",
                            Icon = TaskDialogIcon.ShieldSuccessGreenBar
                        };

                        if (result != null)
                        {
                            page.Text += ".\n\n" +
                                "Return value:\n" +
                                result.ToString();

                            var exp = new TaskDialogExpander();
                            var sb = new StringBuilder($"{result.GetType().Name} ({result.GetType().FullName})\n\n");

                            foreach (var prop in result.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                                sb.AppendLine($"{prop.Name} = {prop.GetValue(result)?.ToString() ?? "null"}");

                            exp.Text = sb.ToString();
                            page.Expander = exp;

                            if (result is Icon icon)
                                page.Footnote = new TaskDialogFootnote
                                {
                                    Text = "This is a preview of the output icon.",
                                    Icon = new TaskDialogIcon(icon)
                                };

                            if (result is IDisposable disp)
                                disp.Dispose();
                            else if (result is IAsyncDisposable adisp)
                                _ = adisp.DisposeAsync().AsTask();
                        }
                        else
                            page.Text += " but didn't return a value.";

                        TaskDialog.ShowDialog(this, page, TaskDialogStartupLocation.CenterScreen);
                    }
                }
            }
        }

        private void viewHResultValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new EnumValuesForm().ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var config = new SettingsForm();
            config.OnApplied += (s, e) => LoadConfig();
            config.ShowDialog();
        }

        private void LoadConfig()
        {
            propertyGrid1.HelpVisible = Properties.Settings.Default.ArgPanelIsHelpShown;
            propertyGrid1.ToolbarVisible = Properties.Settings.Default.ArgPanelToolbarVisibility;
            propertyGrid1.LargeButtons = Properties.Settings.Default.ArgPanelUseLargeIcons;
        }
#pragma warning disable CS0618
        private void messageBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var wnd = Window.FromHandle(Handle);
            if (wnd == null) return;

            var msgBox = new WinMessageBox();
            msgBox.Buttons = WinMessageBoxButtons.CancelRetryContinue;
            msgBox.Caption = null;
            msgBox.Text = "This is some text";
            msgBox.DefaultButton = 2;
            msgBox.Icon = WinMessageBoxIcon.Warning;
            msgBox.ShowHelp = true;
            msgBox.Culture = new CultureInfo("en-US");
            msgBox.RightAlign = true;

            int helpCount = 0;
            msgBox.OnHelp += (s, e) => helpCount++;

            var result = msgBox.Show(wnd);

            msgBox.Caption = "Result";
            msgBox.Text =
                $"Clicked button:\n" +
                $"{result}\n" +
                $"\n" +
                $"Help button was clicked {helpCount} times";
            msgBox.Buttons = WinMessageBoxButtons.Ok;
            msgBox.DefaultButton = 1;
            msgBox.Icon = WinMessageBoxIcon.Info;
            msgBox.ShowHelp = false;
            msgBox.RightAlign = false;

            msgBox.Show(wnd);
        }
#pragma warning restore

        private void fileDialogToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            Debug.WriteLine("Code running!");

            HResult hr = COM.Initialize(COM.COMInitOptions.ApartmentThreaded);

            if (Macros.Succeeded(hr))
            {
                try
                {
                    IFileOpenDialog? dlg = COM.CreateInstance<IFileOpenDialog>(
    new Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7"),
    null,
    COM.CreateInstanceContext.InprocServer);

                    if (dlg != null)
                    {
                        COMDLG_FILTERSPEC[] filters =
                        [
                            new COMDLG_FILTERSPEC { pszName = "Text Files", pszSpec = "*.txt" },
    new COMDLG_FILTERSPEC { pszName = "All Files", pszSpec = "*.*" }
                        ];

                        int structSize = IntPtr.Size * 2; // two pointers per struct
                        nint buffer = Marshal.AllocHGlobal(structSize * filters.Length);

                        for (int i = 0; i < filters.Length; i++)
                        {
                            nint name = Marshal.StringToHGlobalUni(filters[i].pszName);
                            nint spec = Marshal.StringToHGlobalUni(filters[i].pszSpec);

                            nint slot = buffer + (i * structSize);

                            Marshal.WriteIntPtr(slot, name);
                            Marshal.WriteIntPtr(slot + IntPtr.Size, spec);
                        }

                        dlg.SetFileTypes((uint)filters.Length, buffer);
                        dlg.SetTitle("Cool File Dialog");

                        //try
                        //{
                        //    IFileDialogCustomize customize = COM.QueryInterface<IFileOpenDialog, IFileDialogCustomize>(dlg);

                        //    using (ComSmartPointer<IFileDialogCustomize> ptr = new ComSmartPointer<IFileDialogCustomize>(customize))
                        //    {
                        //        customize.StartVisualGroup(1001, "Sample:");
                        //        customize.AddComboBox(1002);

                        //        customize.AddControlItem(1002, 1003, "Test Item 1");
                        //        customize.AddControlItem(1002, 1004, "Test Item 2");
                        //        customize.AddControlItem(1002, 1005, "Test Item 3");

                        //        customize.EndVisualGroup();

                        //        hr = customize.EnableOpenDropDown(1006);

                        //        if (Macros.Succeeded(hr))
                        //        {
                        //            customize.AddControlItem(1006, 1007, "Open Dropdown Item");
                        //        }
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    Debug.WriteLine($"Failed customize: {ex.Message}");
                        //    Debugger.Break();
                        //}

                        hr = dlg.Show(Handle);

                        if (Macros.Succeeded(hr))
                        {
                            Console.WriteLine("Dialog accepted!");

                            hr = dlg.GetResult(out nint iptr);

                            if (Macros.Succeeded(hr))
                            {
                                IShellItem item = (IShellItem)Marshal.GetTypedObjectForIUnknown(iptr, typeof(IShellItem));

                                hr = item.GetDisplayName(
                                    SIGDN.SIGDN_FILESYSPATH,
                                    out nint pptr);

                                if (Macros.Succeeded(hr))
                                {
                                    string? path = Marshal.PtrToStringUni(pptr);

                                    if (path != null && Window.FromHandle(Handle) is Window wnd)
                                        WinMessageBox.Show(wnd, $"Chosen path: {path}", "File dialog test", WinMessageBoxIcon.Information);
                                    else
                                        Debug.WriteLine("Path is null!");

                                    Marshal.FreeCoTaskMem(pptr);
                                }
                                else
                                {
                                    Debug.WriteLine($"Display name get failed: {hr}");
                                }

                                Marshal.ReleaseComObject(item);
                            }
                            else
                            {
                                Debug.WriteLine($"Result get failed: {hr}");
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"File dialog show failed: {hr}");
                        }

                        Marshal.ReleaseComObject(dlg);

                        for (int i = 0; i < filters.Length; i++)
                        {
                            nint slot = buffer + (i * structSize);

                            Marshal.FreeHGlobal(Marshal.ReadIntPtr(slot));
                            Marshal.FreeHGlobal(Marshal.ReadIntPtr(slot + IntPtr.Size));
                        }

                        Marshal.FreeHGlobal(buffer);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Instance create failed: {ex.Message}");
                }

                COM.Uninitialize();
            }
            else
            {
                Debug.WriteLine($"COM init failed: {hr}");
            }
        }

        private void winInteropUtilsWinFormsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new WiuWinFormsTestForm().Show(this);
        }

        private const int WM_USER = 0x0400;
        private const int CDM_MSGBOX = WM_USER + 0; // custom message

        private void colorDialogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _colordlg ??= new ColorPickerDialog();
            _colordlg.ShowHelp = true;
            _colordlg.UseHookProc = true;

            Window? dlgWnd = null;

            _colordlg.HookProcedure = nuint (nint hDlg, uint uMsg, nuint wParam, nint lParam) =>
            {
                switch (uMsg)
                {
                    case 0x0110: // WM_INITDIALOG
                        var focusCtrl = (nint)wParam;
                        var focusWnd = Window.FromHandle(focusCtrl);
                        dlgWnd = focusWnd?.Parent;

                        dlgWnd?.PostMessage(CDM_MSGBOX, 0, 0);
                        break;
                    case CDM_MSGBOX:
                        if (dlgWnd != null)
                            WinMessageBox.Show(dlgWnd, "Dialog initialized!", "Init", WinMessageBoxIcon.Info);
                        break;
                }

                return 0;
            };

            if (Window.FromHandle(Handle) is Window wnd)
                _colordlg.Show(wnd);

        }

        private void visualStylesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new VisualStyleTestForm().Show(this);
        }

        private void windowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new WindowTestForm().Show(this);
        }

        private void winInteropUtilsWinFormsShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new WiuWinFormsShellControlsTestForm().ShowDialog();
        }

        private void memoryUtilsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new MemoryUtilsTestForm().ShowDialog();
        }

        private void winFileWinFileInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FileApiTestForm().ShowDialog();
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct COMDLG_FILTERSPEC
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszSpec;
    }

    public class MethodArgumentDescriptor : ICustomTypeDescriptor
    {
        private readonly MethodBase _method;
        public Dictionary<string, object> Values { get; } = new();

        public MethodArgumentDescriptor(MethodBase method)
        {
            _method = method;

            foreach (var param in method.GetParameters())
                Values[param.Name!] = GetDefault(param.ParameterType)!;
        }

        private object? GetDefault(Type type) =>
            type.IsValueType ? Activator.CreateInstance(type) : null;

        public PropertyDescriptorCollection GetProperties()
        {
            var props = _method.GetParameters()
                .Select(p => new MethodArgPropertyDescriptor(p, Values, _method))
                .Cast<PropertyDescriptor>()
                .ToArray();

            return new PropertyDescriptorCollection(props);
        }

        #region Forwarded ICustomTypeDescriptor methods
        public AttributeCollection GetAttributes() => AttributeCollection.Empty;
        public string GetClassName() => null!;
        public string GetComponentName() => null!;
        public TypeConverter GetConverter() => null!;
        public EventDescriptor GetDefaultEvent() => null!;
        public PropertyDescriptor GetDefaultProperty() => null!;
        public object GetEditor(Type editorBaseType) => null!;
        public EventDescriptorCollection GetEvents(Attribute[]? attributes) => EventDescriptorCollection.Empty;
        public EventDescriptorCollection GetEvents() => EventDescriptorCollection.Empty;
        public PropertyDescriptorCollection GetProperties(Attribute[]? attributes) => GetProperties();
        public object GetPropertyOwner(PropertyDescriptor? pd) => this;
        #endregion
    }

    public partial class MethodArgPropertyDescriptor(ParameterInfo param, Dictionary<string, object> store,
        MethodBase parentMethod) : PropertyDescriptor(param.Name ?? string.Empty, null)
    {
        private readonly ParameterInfo _param = param;
        private readonly Dictionary<string, object> _store = store;
        private readonly MethodBase _method = parentMethod;

        public override Type ComponentType => typeof(MethodArgumentDescriptor);
        public override Type PropertyType => _param.ParameterType;
        public override bool IsReadOnly => false;

        public override bool CanResetValue(object component) => GetValue(component) != _param.DefaultValue;
        public override void ResetValue(object component) => SetValue(component, _param.DefaultValue);

        public override object? GetValue(object? component)
        {
            return _store.TryGetValue(_param.Name!, out var val) ? val : null;
        }

        public override void SetValue(object? component, object? value)
        {
            _store[_param.Name!] = value!;
        }

        public override bool ShouldSerializeValue(object component) => true;

        public override string DisplayName => $"{_param.Name} ({Form1.GetTypeName(_param.ParameterType)})";
        public override string Category => _param.IsOut ? "Output" : (_param.ParameterType.IsByRef && !_param.IsOut) ? "Reference" : _param.IsIn ? "Input" : "Parameters";
        public override TypeConverter Converter => _param.ParameterType == typeof(nint) 
            ? new IntPtrConverter()
            : _param.ParameterType == typeof(nuint)
            ? new UIntPtrConverter()
            : _param.ParameterType == typeof(HResult)
            ? new HResultConverter()
            : base.Converter;

        public override string Description
        {
            get
            {
                if (_method.DeclaringType != null)
                {
                    XmlDocHelper.LoadXmlDoc(_method.DeclaringType.Assembly);

                    return SpaceAfterDotSentenceRegex().Replace(XmlDocHelper.GetParameterDoc(_method, _param.Name ?? string.Empty) ?? base.Description, ". ");
                }

                return base.Description;
            }
        }

        public override object? GetEditor(Type editorBaseType)
        {
            if (_param.ParameterType.IsEnum && _param.ParameterType.GetCustomAttribute(typeof(FlagsAttribute)) != null)
                return new FlagsEnumEditor();

            if (DisplayName.Contains("hWnd", StringComparison.OrdinalIgnoreCase))
                return new HwndEditor();

            if (_param.ParameterType.Equals(typeof(Window)))
                return new WindowEditor();

            if (_param.ParameterType.Equals(typeof(HResult)))
                return new HResultEditor();

            if (_param.ParameterType.Equals(typeof(int)) && _param.Name == "code"
                && _method.DeclaringType?.Equals(typeof(HResult)) == true)

                return new HResultCodeEditor();

            return base.GetEditor(editorBaseType);
        }

        [GeneratedRegex(@"(?<=\b\w)\.(?=[A-Za-z])(?!\.)")]
        private static partial Regex SpaceAfterDotSentenceRegex();

    }

    public class IntPtrConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
            => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s && long.TryParse(s, out var result))
                return new IntPtr(result);
            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is IntPtr ptr && destinationType == typeof(string))
                return ptr.ToInt64().ToString();
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class UIntPtrConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
            => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s && uint.TryParse(s, out var result))
                return new UIntPtr(result);
            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is UIntPtr ptr && destinationType == typeof(string))
                return ptr.ToUInt64().ToString();
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

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

    public class HwndEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
            => UITypeEditorEditStyle.Modal;

        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider? provider, object? value)
        {
            if (provider?.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService edSvc)
                return value;

            var dlg = new WindowPickerForm();
            
            edSvc.ShowDialog(dlg);

            return dlg.Hwnd;
        }
    }

    public class WindowEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
            => UITypeEditorEditStyle.Modal;

        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider? provider, object? value)
        {
            if (provider?.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService edSvc)
                return value;

            var dlg = new WindowPickerForm();

            edSvc.ShowDialog(dlg);

            return Window.FromHandle((nint)dlg.Hwnd!);
        }
    }

    public class HwndWindow(nint handle) : IWin32Window
    {
        public nint Handle => handle;
    }

    public class HResultEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
            => UITypeEditorEditStyle.DropDown;

        public class HResultListBoxItem
        {
            public string Text = string.Empty;
            public HResult HR;

            public override string ToString() => Text;

            public HResultListBoxItem(HResult hr, string text)
            {
                HR = hr;
                Text = text;
            }
        }

        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider,
            object? value)
        {
            if (provider?.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService edSvc)
                return value;

            var lb = new ListBox();
            lb.SelectionMode = SelectionMode.One;
            lb.Height = 256;
            //lb.IntegralHeight = false;
            lb.Click += (s, e) => edSvc.CloseDropDown();

            if (!HResultConverter._names.Any())
                HResultConverter.InitMap();

            foreach (var kvp in HResultConverter._names)
            {
                lb.Items.Add(new HResultListBoxItem(kvp.Key, kvp.Value));

                if (kvp.Key == (HResult?)value)
                    lb.SelectedIndex = lb.Items.Count - 1;
            }

            edSvc.DropDownControl(lb);

            HResultListBoxItem? item = (HResultListBoxItem?)lb.SelectedItem;

            return item?.HR ?? value;
        }
    }

    public class HResultConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
            => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public static Dictionary<HResult, string> _names = [];

        public static void InitMap()
        {
            foreach (var field in typeof(HResult).GetFields(BindingFlags.Public | BindingFlags.Static))
                if (field.GetValue(null) is HResult hr)
                    _names[hr] = field.Name;
        }

        public override object? ConvertTo(ITypeDescriptorContext? context,
            CultureInfo? culture,
            object? value, Type destinationType)
        {
            if (!_names.Any())
                InitMap();

            if (destinationType == typeof(string) && value is HResult hr)
                if (_names.TryGetValue(hr, out string? disp))
                    return disp;
                else
                    return $"0x{(int)hr:X8}";

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => false;
    }

    public class HResultCodeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
            => UITypeEditorEditStyle.DropDown;

        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value)
        {
            if (provider?.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService edSvc)
                return value;

            var form = new HResultCodeEditorForm();
            var hr = new HResult((int)value!);

            form.Load += (s, e) =>
            {
                form.comboBox1.SelectedItem = hr.Facility;
                form.numericUpDown2.Value = hr.Severity;
                form.numericUpDown1.Value = hr.BaseCode;
            };

            form.button1.Click += (s, e) => edSvc.CloseDropDown();

            edSvc.DropDownControl(form);

            hr = new HResult((int)form.numericUpDown1.Value, (Facility)form.comboBox1.SelectedItem!,
                (int)form.numericUpDown2.Value);

            return hr.FullCode;
        }
    }
}
