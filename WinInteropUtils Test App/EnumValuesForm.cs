using FireBlade.WinInteropUtils;
using System.ComponentModel;
using System.Reflection;
using FireBlade.WinInteropUtils.ComponentObjectModel;

namespace WinInteropUtils_Test_App
{
    public partial class EnumValuesForm : Form
    {
        private TaskDialogProgressBar? progressBar;
        private TaskDialogPage? loadingPage;
        private ManualResetEventSlim? pauseEvent;

        public EnumValuesForm()
        {
            InitializeComponent();

            var asm = Assembly.GetAssembly(typeof(COM));
            if (asm != null)
            {
                comboBox1.BeginUpdate();
                foreach (var Enum in asm.GetTypes().Where(t => t.IsEnum))
                {
                    comboBox1.Items.Add(new EnumComboBoxItem(Enum));
                }
                comboBox1.SelectedIndex = 0;
                comboBox1.EndUpdate();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            progressBar = new(TaskDialogProgressBarState.Normal);

            var btnPause = new TaskDialogButton
            {
                Text = "Pause",
                AllowCloseDialog = false
            };

            var page = new TaskDialogPage
            {
                Heading = "Loading Values",
                Buttons = [], // Pause button is kind of broken, so it got removed
                Caption = "Loading Enum Values",
                ProgressBar = progressBar,
                Text = "Loading the enum values...",
                Icon = TaskDialogIcon.ShieldBlueBar
            };

            page.Created += (s, e) =>
            {
                TaskDialog? dialog = page?.BoundDialog;
                if (dialog != null)
                {
                    IntPtr hwnd = dialog.Handle;
                    try
                    {
                        // We can update the icon using a SendMessage call. But we must specify the icon via ID, not an object or hIcon handle
                        // We do NOT use the negative of the ID, since the API is doing other stuff with the ID and handles it automatically
                        User32.SendMessage(hwnd, (uint)TDM.UPDATE_ICON, UIntPtr.Zero, new IntPtr(5335));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating icon: " + ex.Message);
                    }

                }
            };

            btnPause.Click += (s, e) =>
            {
                if (pauseEvent != null)
                {
                    if (pauseEvent.IsSet)
                    {
                        pauseEvent.Reset();
                        progressBar.State = TaskDialogProgressBarState.Paused;
                        btnPause = btnPause.SetButtonText("Resume");
                    }
                    else
                    {
                        pauseEvent.Set();
                        progressBar.State = TaskDialogProgressBarState.Normal;
                        btnPause = btnPause.SetButtonText("Pause");
                    }
                }
            };

            pauseEvent = new(true);

            backgroundWorker1.RunWorkerAsync();

            loadingPage = page;
            TaskDialog.ShowDialog(this, page, TaskDialogStartupLocation.CenterScreen);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            object? comboBoxItem = null;
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    comboBoxItem = comboBox1.SelectedItem;
                });
            }
            else
            {
                comboBoxItem = comboBox1.SelectedItem;
            }

            if (comboBoxItem is EnumComboBoxItem ecbi)
            {
                try
                {
                    if (InvokeRequired)
                    {
                        Invoke(() =>
                        {
                            listView1.Items.Clear();
                            imageList1.Images.Clear();
                        });
                    }
                    else
                    {
                        listView1.Items.Clear();
                        imageList1.Images.Clear();
                    }

                    var vals = Enum.GetValues(ecbi.Enum);

                    int i = 0;

                    foreach (var enumItem in vals)
                    {
                        pauseEvent?.Wait(-1);
                        if (enumItem != null)
                        {
                            var icon = enumItem.ToString() switch
                            {
                                string s when s.StartsWith("S_") || s.Contains("SUCCESS_") || s.Contains("_S_") || s.Contains("_I_") => SystemIcons.Information,
                                string s when s.StartsWith("E_") || s.Contains("ERROR_") || s.Equals("CloseButton", StringComparison.OrdinalIgnoreCase) ||
                                s.Contains("_E_") => SystemIcons.Error,
                                string s when s.Contains("_W_") || s.Contains("WARNING_") ||
                                s.Equals("Reserved", StringComparison.OrdinalIgnoreCase) => SystemIcons.Warning,
                                string s when s.Equals("HelpButton") => SystemIcons.Question,
                                _ => null
                            };

                            var item = new ListViewItem
                            {
                                Text = enumItem.ToString(),
                                Tag = ecbi,
                                SubItems =
                                {
                                    new ListViewItem.ListViewSubItem
                                    {
                                        Text = ((int)enumItem).ToString("X")
                                    }
                                }
                            };

                            if (icon != null)
                            {
                                if (InvokeRequired)
                                {
                                    Invoke(() =>
                                    {
                                        imageList1.Images.Add(icon);
                                    });
                                }
                                else
                                {
                                    imageList1.Images.Add(icon);
                                }
                                item.ImageIndex = imageList1.Images.Count - 1;
                            }
                            else
                            {
                                var image = enumItem.ToString() switch
                                {
                                    string s when s.Equals("TitleBar", StringComparison.OrdinalIgnoreCase) => Properties.Resources.titlebar,
                                    string s when s.Equals("MaximizeButton", StringComparison.OrdinalIgnoreCase) => Properties.Resources.titlebar_max,
                                    string s when s.Equals("MinimizeButton", StringComparison.OrdinalIgnoreCase) => Properties.Resources.titlebar_min,
                                    _ => null
                                };

                                if (image != null)
                                {
                                    if (InvokeRequired)
                                    {
                                        Invoke(() =>
                                        {
                                            imageList1.Images.Add(image);
                                        });
                                    }
                                    else
                                    {
                                        imageList1.Images.Add(image);
                                    }
                                    item.ImageIndex = imageList1.Images.Count - 1;
                                }
                            }

                            if (InvokeRequired)
                            {
                                Invoke(() =>
                                {
                                    listView1.Items.Add(item);
                                });
                            }
                            else
                            {
                                listView1.Items.Add(item);
                            }

                            backgroundWorker1.ReportProgress(i / vals.Length * 100);

                            i++;
                        }
                    }
                }
                catch (ObjectDisposedException)
                { }
                catch (Exception ex)
                {
                    e.Result = ex;
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressBar != null)
            {
                try
                {
                    progressBar.Value = e.ProgressPercentage;
                }
                catch { }

            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception ex)
            {
                throw ex;
            }
            else
            {
                loadingPage?.BoundDialog?.Close();
            }
            pauseEvent?.Set();
            pauseEvent?.Dispose();
            pauseEvent = null;
        }
    }

    public class EnumComboBoxItem(Type e)
    {
        public Type Enum { get; set; } = e;

        public override string ToString()
        {
            return Enum.Name;
        }
    }

    public enum TDM : uint
    {
        NAVIGATE_PAGE = WM.USER + 101,
        /// <summary>
        ///   "wParam = Button ID"
        /// </summary>
        CLICK_BUTTON = WM.USER + 102,
        /// <summary>
        ///   "wParam = 0 (nonMarque) wParam != 0 (Marquee)"
        /// </summary>
        SET_MARQUEE_PROGRESS_BAR = WM.USER + 103,
        /// <summary>
        ///   "wParam = new progress state"
        /// </summary>
        SET_PROGRESS_BAR_STATE = WM.USER + 104,
        /// <summary>
        ///   "lParam = MAKELPARAM(nMinRange, nMaxRange)"
        /// </summary>
        SET_PROGRESS_BAR_RANGE = WM.USER + 105,
        /// <summary>
        ///   "wParam = new position"
        /// </summary>
        SET_PROGRESS_BAR_POS = WM.USER + 106,
        /// <summary>
        ///   "wParam = 0 (stop marquee), wParam != 0 (start marquee),
        ///   lparam = speed (milliseconds between repaints)"
        /// </summary>
        SET_PROGRESS_BAR_MARQUEE = WM.USER + 107,
        /// <summary>
        ///   "wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)"
        /// </summary>
        SET_ELEMENT_TEXT = WM.USER + 108,
        /// <summary>
        ///   "wParam = Radio Button ID"
        /// </summary>
        CLICK_RADIO_BUTTON = WM.USER + 110,
        /// <summary>
        ///   "lParam = 0 (disable), lParam != 0 (enable), wParam = Button ID"
        /// </summary>
        ENABLE_BUTTON = WM.USER + 111,
        /// <summary>
        ///   "lParam = 0 (disable), lParam != 0 (enable), wParam = Radio Button ID"
        /// </summary>
        ENABLE_RADIO_BUTTON = WM.USER + 112,
        /// <summary>
        ///   "wParam = 0 (unchecked), 1 (checked), lParam = 1 (set key focus)"
        /// </summary>
        CLICK_VERIFICATION = WM.USER + 113,
        /// <summary>
        ///   "wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)"
        /// </summary>
        UPDATE_ELEMENT_TEXT = WM.USER + 114,
        /// <summary>
        ///   "wParam = Button ID, lParam = 0 (elevation not required),
        ///   lParam != 0 (elevation required)"
        /// </summary>
        SET_BUTTON_ELEVATION_REQUIRED_STATE = WM.USER + 115,
        /// <summary>
        ///   "wParam = icon element (TASKDIALOG_ICON_ELEMENTS), lParam = new icon
        ///   (hIcon if TDF_USE_HICON_* was set, PCWSTR otherwise)"
        /// </summary>
        UPDATE_ICON = WM.USER + 116,
    }

    public enum WM : uint
    {
        USER = 0x0400,
        WM_SETICON = 0x0080,
    }
}
