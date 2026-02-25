namespace WinInteropUtils_Test_App
{
    partial class VisualStyleTestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            headingTextControl1 = new HeadingTextControl();
            textBox1 = new TextBox();
            visualStyleExControl1 = new VisualStyleExControl();
            SuspendLayout();
            // 
            // headingTextControl1
            // 
            headingTextControl1.Dock = DockStyle.Top;
            headingTextControl1.Location = new Point(0, 0);
            headingTextControl1.Name = "headingTextControl1";
            headingTextControl1.Size = new Size(321, 35);
            headingTextControl1.TabIndex = 0;
            headingTextControl1.Text = "Heading";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(77, 70);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(166, 23);
            textBox1.TabIndex = 1;
            textBox1.Text = "Heading";
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // visualStyleExControl1
            // 
            visualStyleExControl1.Location = new Point(84, 109);
            visualStyleExControl1.Name = "visualStyleExControl1";
            visualStyleExControl1.Size = new Size(159, 42);
            visualStyleExControl1.TabIndex = 2;
            visualStyleExControl1.Text = "visualStyleExControl1";
            visualStyleExControl1.UseVisualStyle = true;
            // 
            // VisualStyleTestForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(321, 201);
            Controls.Add(visualStyleExControl1);
            Controls.Add(textBox1);
            Controls.Add(headingTextControl1);
            Name = "VisualStyleTestForm";
            Text = "VisualStyleTestForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private HeadingTextControl headingTextControl1;
        private TextBox textBox1;
        private VisualStyleExControl visualStyleExControl1;
    }
}