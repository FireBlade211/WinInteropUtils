using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WinInteropUtils_Test_App
{
    public static class XmlDocHelper
    {
        private static XDocument? _xmlDoc;

        public static void LoadXmlDoc(Assembly asm)
        {
            var xmlPath = Path.ChangeExtension(asm.Location, ".xml");
            if (File.Exists(xmlPath))
                _xmlDoc = XDocument.Load(xmlPath);
        }

        public static string? GetParameterDoc(MethodInfo method, string paramName)
        {
            if (_xmlDoc == null)
                return null;

            string memberName = GetMemberElementName(method);
            var member = _xmlDoc.Descendants("member")
                .FirstOrDefault(x => x.Attribute("name")?.Value == memberName);

            var paramElement = member?.Elements("param")
                .FirstOrDefault(p => p.Attribute("name")?.Value == paramName);

            return paramElement != null ? GetXmlDocumentationText(paramElement)
                .TrimLines()
                .Replace("\r\n", string.Empty)
                .Replace("\n", string.Empty)
                .Replace("	", string.Empty)
                : null;
        }

        private static string GetXmlDocumentationText(XElement element)
        {
            var sb = new StringBuilder();

            foreach (var node in element.Nodes())
            {
                if (node is XText textNode)
                {
                    sb.Append(textNode.Value);
                }
                else if (node is XElement el)
                {
                    switch (el.Name.LocalName)
                    {
                        case "see":
                            var cref = el.Attribute("cref")?.Value;
                            if (!string.IsNullOrWhiteSpace(cref))
                            {
                                // Strip prefix (like "T:" or "M:")
                                var display = cref.TrimStart('T', 'M', 'P', 'F', 'E', ':');
                                sb.Append(display);
                            }
                            else
                            {
                                var langword = el.Attribute("langword")?.Value;

                                if (!string.IsNullOrWhiteSpace(langword))
                                {
                                    sb.Append(langword);
                                }
                            }
                            break;

                        case "paramref":
                            var name = el.Attribute("name")?.Value;
                            if (!string.IsNullOrWhiteSpace(name))
                                sb.Append(name);
                            break;

                        default:
                            // Unknown tag — treat as plain text fallback
                            sb.Append(el.Value);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }

        private static string GetMemberElementName(MethodInfo method)
        {
            var parameters = method.GetParameters();
            string paramTypes = string.Join(",", parameters.Select(p => GetTypeNameForXmlDoc(p.ParameterType)));
            string prefix = method.IsConstructor ? "M:" + method.DeclaringType!.FullName + ".#ctor" : "M:" + method.DeclaringType!.FullName + "." + method.Name;
            return parameters.Length == 0 ? prefix : $"{prefix}({paramTypes})";
        }

        private static string GetTypeNameForXmlDoc(Type type)
        {
            if (type.IsByRef)
                return GetTypeNameForXmlDoc(type.GetElementType()!) + "@";

            if (type.IsGenericType)
            {
                var typeDef = type.GetGenericTypeDefinition();
                var genericArgs = string.Join(",", type.GetGenericArguments().Select(GetTypeNameForXmlDoc));
                return $"{typeDef.FullName!.Split('`')[0]}{{{genericArgs}}}";
            }

            if (type.IsArray)
                return GetTypeNameForXmlDoc(type.GetElementType()!) + "[]";

            return type.FullName!;
        }

    }

    public static class StringExtensions
    {
        public static string TrimLines(this string input)
        {
            return string.Join(
                Environment.NewLine,
                input.Split(["\r\n", "\n", "\r" ], StringSplitOptions.None)
                     .Select(line => line.Trim())
            );
        }
    }

    public static class TaskDialogControlExtensions
    {
        public static TaskDialogButton SetButtonText(this TaskDialogButton button, string newText)
        {
            if (button.BoundPage != null)
            {
                var copy = button.BoundPage.DeepClonePage();

                var newButton = new TaskDialogButton
                {
                    AllowCloseDialog = button.AllowCloseDialog,
                    Enabled = button.Enabled,
                    ShowShieldIcon = button.ShowShieldIcon,
                    Tag = button.Tag,
                    Text = newText,
                    Visible = button.Visible
                };

                var b = copy.Buttons.FirstOrDefault(b => Equals(b.Tag, button.Tag) || b.Text == button.Text);
                if (b != null)
                {
                    var i = copy.Buttons.IndexOf(b);
                    if (i >= 0)
                    {
                        copy.Buttons.RemoveAt(i);
                        copy.Buttons.Insert(i, newButton);
                    }
                }

                button.BoundPage.Navigate(copy);
                return newButton;
            }

            return button;
        }

        public static TaskDialogPage DeepClonePage(this TaskDialogPage original)
        {
            var newPage = new TaskDialogPage
            {
                AllowCancel = original.AllowCancel,
                AllowMinimize = original.AllowMinimize,
                Caption = original.Caption,
                EnableLinks = original.EnableLinks,
                Heading = original.Heading,
                Icon = original.Icon,
                RightToLeftLayout = original.RightToLeftLayout,
                SizeToContent = original.SizeToContent,
                Text = original.Text,
                ProgressBar = original.ProgressBar != null ? new TaskDialogProgressBar
                {
                    Minimum = original.ProgressBar.Minimum,
                    Maximum = original.ProgressBar.Maximum,
                    State = original.ProgressBar.State,
                    Value = original.ProgressBar.Value,
                    MarqueeSpeed = original.ProgressBar.MarqueeSpeed,
                    Tag = original.ProgressBar.Tag,
                } : null,
                Verification = original.Verification != null ? new TaskDialogVerificationCheckBox
                {
                    Checked = original.Verification.Checked,
                    Tag = original.Verification.Tag,
                    Text = original.Verification.Text,
                } : null,
                Expander = original.Expander != null ? new TaskDialogExpander
                {
                    CollapsedButtonText = original.Expander.CollapsedButtonText,
                    Expanded = original.Expander.Expanded,
                    ExpandedButtonText = original.Expander.ExpandedButtonText,
                    Position = original.Expander.Position,
                    Tag = original.Expander.Tag,
                    Text = original.Expander.Text
                } : null,
                Footnote = original.Footnote != null ? new TaskDialogFootnote
                {
                    Icon = original.Footnote.Icon,
                    Tag = original.Footnote.Tag,
                    Text = original.Footnote.Text
                } : null
            };

            // Deep copy buttons
            foreach (var button in original.Buttons)
            {
                var nBtn = new TaskDialogButton
                {
                    AllowCloseDialog = button.AllowCloseDialog,
                    Enabled = button.Enabled,
                    ShowShieldIcon = button.ShowShieldIcon,
                    Tag = button.Tag,
                    Text = button.Text,
                    Visible = button.Visible
                };
                newPage.Buttons.Add(nBtn);

                if (original.DefaultButton == button)
                {
                    newPage.DefaultButton = nBtn;
                }
            }

            // Deep copy radio buttons (if any)
            foreach (var radio in original.RadioButtons)
            {
                newPage.RadioButtons.Add(new TaskDialogRadioButton
                {
                    Tag = radio.Tag,
                    Text = radio.Text,
                    Enabled = radio.Enabled,
                    Checked = radio.Checked
                });
            }

            return newPage;
        }
    }
}