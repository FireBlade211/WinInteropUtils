using FireBlade.WinInteropUtils.Dialogs;
using FireBlade.WinInteropUtils.FileSystem;
using FireBlade.WinInteropUtils;

namespace WinInteropUtils_Test_App
{
    public partial class FileApiTestForm : Form
    {
        private WinFile? lastFile;

        public FileApiTestForm()
        {
            InitializeComponent();

            listBox1.BeginUpdate();

            foreach (var val in Enum.GetValues<WinFileAttributes>())
                listBox1.Items.Add(val);

            listBox1.EndUpdate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lastFile?.Dispose();

            try
            {
                lastFile = new WinFile(textBox1.Text, WinFileAccess.Read | WinFileAccess.Write);

                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
            }
            catch (Exception ex)
            {
                WinMessageBox.Show(Window.FromHandle(Handle)!, ex.Message, null, WinMessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lastFile?.Dispose();

            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = lastFile?.Content;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (lastFile != null)
                lastFile.Content = textBox2.Text;
        }

        private WinFileInfo? _lastInfo;

        private void button5_Click(object sender, EventArgs e)
        {
            _lastInfo?.Dispose();

            bool useAttribs = checkBox1.Checked;
            WinFileAttributes? attribs = useAttribs ? WinFileAttributes.None : null;

            if (useAttribs)
                foreach (var item in listBox1.SelectedItems)
                    if (item is WinFileAttributes att)
                        attribs |= att;

            _lastInfo = new WinFileInfo(textBox3.Text, WinFileInfoOptions.None, attribs);
            button6.Enabled = true;

            pictureBox1.Image = _lastInfo.SmallIcon.Icon.ToBitmap();
            pictureBox2.Image = _lastInfo.LargeIcon.Icon.ToBitmap();
            pictureBox3.Image = _lastInfo.ShellIcon.Icon.ToBitmap();

            label1.Text = $"Display name: {_lastInfo.DisplayName}";
            label2.Text = $"Type name: {_lastInfo.TypeName}";
            label3.Text = $"Icon location: {_lastInfo.IconLocation}";
            label4.Text = $"EXE type: {_lastInfo.ExecutableType}";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            _lastInfo?.Dispose();
            button6.Enabled = false;

            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
        }
    }
}
