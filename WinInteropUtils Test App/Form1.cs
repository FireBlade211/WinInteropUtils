using FireBlade.WinInteropUtils;
using FireBlade.WinInteropUtils.ComponentObjectModel;
using FireBlade.WinInteropUtils.ComponentObjectModel.Interfaces;
using FireBlade.WinInteropUtils.Dialogs;
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
        public Form1()
        {
            InitializeComponent();
            fileDialogToolStripMenuItem.Click += fileDialogToolStripMenuItem_Click;

            LoadConfig();

            listView1.BeginUpdate();
            foreach (var c in Assembly.GetAssembly(typeof(Shell32))!.GetTypes()
            .Where(t => t.IsClass && t.Namespace == "FireBlade.WinInteropUtils"))
            {
                if (c.Name == "ExceptionExtensions") continue;

                var items = new List<ListViewItem>();
                foreach (var method in c.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    var item = new ListViewItem();
                    item.Tag = method;
                    var cParams = method.GetParameters().Select(x =>
                    {
                        return GetTypeName(x.ParameterType);
                    });
                    item.Text = $"{GetTypeName(method.ReturnType)} {method.Name}({string.Join<string>(", ", cParams)})";

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
        }

        public static string GetTypeName<T>()
        {
            return GetTypeName(typeof(T));
        }

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
                _ => type.Name
            };
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                propertyGrid1.SelectedObject = new MethodArgumentDescriptor((listView1.SelectedItems[0].Tag as MethodInfo)!);
                callMethodToolStripMenuItem.Enabled = true;
            }
        }

        private void callMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var item = listView1.SelectedItems[0];

                if (item.Tag is MethodInfo method)
                {
                    if (propertyGrid1.SelectedObject is MethodArgumentDescriptor descriptor)
                    {
                        var result = method.Invoke(null, descriptor.Values.Values.ToArray());

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
                            {
                                sb.AppendLine($"{prop.Name} = {prop.GetValue(result)?.ToString() ?? "null"}");
                            }

                            exp.Text = sb.ToString();
                            page.Expander = exp;
                        }
                        else
                        {
                            page.Text += " but didn't return a value.";
                        }

                        TaskDialog.ShowDialog(this, page, TaskDialogStartupLocation.CenterScreen);
                    }
                }
            }
        }

        private void viewHRESULTValuesToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void messageBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var msgBox = new Win32MessageBox();
            msgBox.Buttons = Win32MessageBoxButtons.CancelRetryContinue;
            msgBox.Caption = null;
            msgBox.Text = "This is some text";
            msgBox.DefaultButton = 2;
            msgBox.Icon = Win32MessageBoxIcon.Warning;
            msgBox.ShowHelp = true;
            msgBox.Culture = new CultureInfo("en-US");
            msgBox.RightAlign = true;

            int helpCount = 0;
            msgBox.OnHelp += (s, e) => helpCount++;

            var result = msgBox.Show(Handle);

            msgBox.Caption = "Result";
            msgBox.Text =
                $"Clicked button:\n" +
                $"{result}\n" +
                $"\n" +
                $"Help button was clicked {helpCount} times";
            msgBox.Buttons = Win32MessageBoxButtons.Ok;
            msgBox.DefaultButton = 1;
            msgBox.Icon = Win32MessageBoxIcon.Info;
            msgBox.ShowHelp = false;
            msgBox.RightAlign = false;

            msgBox.Show(Handle);
        }

        private void fileDialogToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            Debug.WriteLine("Code running!");

            HRESULT hr = COM.Initialize(COM.COMInitOptions.ApartmentThreaded);

            if (Macros.Succeeded(hr))
            {
                hr = COM.CreateInstance(
                    new Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7"),
                    null,
                    COM.CreateInstanceContext.InprocServer,
                    new Guid("d57c7288-d4ad-4768-be02-9d969532d960"),
                    out IFileOpenDialog? dlg);

                if (Macros.Succeeded(hr) && dlg != null)
                {
                    COMDLG_FILTERSPEC[] filters =
                    [
            new COMDLG_FILTERSPEC { pszName = "Text Files", pszSpec = "*.txt" },
            new COMDLG_FILTERSPEC { pszName = "All Files", pszSpec = "*.*" }
                    ];

                    // Allocate unmanaged memory (SetFileTypes requires pointer)
                    IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<COMDLG_FILTERSPEC>() * filters.Length);

                    try
                    {
                        // Copy each struct into unmanaged memory
                        for (int i = 0; i < filters.Length; i++)
                        {
                            IntPtr structPtr = IntPtr.Add(ptr, i * Marshal.SizeOf<COMDLG_FILTERSPEC>());
                            Marshal.StructureToPtr(filters[i], structPtr, false);
                        }

                        // ptr now points to the unmanaged array
                        dlg.SetFileTypes((uint)filters.Length, ptr);
                    }
                    catch { }

                    dlg.SetTitle("Cool File Dialog");

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

                                if (path != null)
                                    Win32MessageBox.Show(Handle, $"Chosen path: {path}", "File dialog test", Win32MessageBoxIcon.Information);
                                else
                                    Debug.WriteLine("Path is null!");

                                Marshal.FreeCoTaskMem(pptr);
                            }
                            else
                            {
                                Debug.WriteLine($"Display name get failed: {hr}");
                            }

                            item.Release();
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

                    dlg.Release();
                    //Marshal.FreeHGlobal(ptr);
                }
                else
                {
                    Debug.WriteLine($"Instance create failed: {hr}");
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
        private readonly MethodInfo _method;
        public Dictionary<string, object> Values { get; } = new();

        public MethodArgumentDescriptor(MethodInfo method)
        {
            _method = method;

            foreach (var param in method.GetParameters())
            {
                Values[param.Name!] = GetDefault(param.ParameterType)!;
            }
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

    public partial class MethodArgPropertyDescriptor(ParameterInfo param, Dictionary<string, object> store, MethodInfo parentMethod) : PropertyDescriptor(param.Name ?? string.Empty, null)
    {
        private readonly ParameterInfo _param = param;
        private readonly Dictionary<string, object> _store = store;
        private readonly MethodInfo _method = parentMethod;

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
            {
                return new FlagsEnumEditor();
            }

            if (DisplayName.Contains("hWnd", StringComparison.OrdinalIgnoreCase))
            {
                return new HwndEditor();
            }

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
}
