namespace WinInteropUtils_Test_App
{
    public partial class SettingsForm : Form
    {
        /// <summary>
        /// Fires when the settings are applied.
        /// </summary>
        public event EventHandler? OnApplied;

        private bool canEnableApply = true;

        public SettingsForm()
        {
            InitializeComponent();

            LoadConfig();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (!canEnableApply) return;

            button1.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!canEnableApply) return;

            button1.Enabled = true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (!canEnableApply) return;

            button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ApplyChanges();
            button1.Enabled = false;
        }

        private void ApplyChanges()
        {
            Properties.Settings.Default.ArgPanelUseLargeIcons = radioButton2.Checked;
            Properties.Settings.Default.ArgPanelIsHelpShown = checkBox1.Checked;
            Properties.Settings.Default.ArgPanelToolbarVisibility = checkBox2.Checked;

            Properties.Settings.Default.Save();

            OnApplied?.Invoke(this, new EventArgs());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        private void LoadConfig()
        {
            canEnableApply = false;

            radioButton1.Checked = !Properties.Settings.Default.ArgPanelUseLargeIcons;
            radioButton2.Checked = Properties.Settings.Default.ArgPanelUseLargeIcons;

            checkBox1.Checked = Properties.Settings.Default.ArgPanelIsHelpShown;
            checkBox2.Checked = Properties.Settings.Default.ArgPanelToolbarVisibility;

            canEnableApply = true;
        }
    }
}
