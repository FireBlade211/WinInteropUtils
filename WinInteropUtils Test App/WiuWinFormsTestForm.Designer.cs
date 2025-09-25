namespace WinInteropUtils_Test_App
{
    partial class WiuWinFormsTestForm
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
            components = new System.ComponentModel.Container();
            progressBarEx1 = new FireBlade.WinInteropUtils.WinForms.ProgressBarEx();
            progressBarEx2 = new FireBlade.WinInteropUtils.WinForms.ProgressBarEx();
            progressBarEx3 = new FireBlade.WinInteropUtils.WinForms.ProgressBarEx();
            timer1 = new System.Windows.Forms.Timer(components);
            progressBarEx4 = new FireBlade.WinInteropUtils.WinForms.ProgressBarEx();
            numericUpDown1 = new NumericUpDown();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            SuspendLayout();
            // 
            // progressBarEx1
            // 
            progressBarEx1.Location = new Point(12, 21);
            progressBarEx1.MarqueeSpeed = 0;
            progressBarEx1.Name = "progressBarEx1";
            progressBarEx1.Size = new Size(201, 35);
            progressBarEx1.TabIndex = 0;
            progressBarEx1.Text = "progressBarEx1";
            // 
            // progressBarEx2
            // 
            progressBarEx2.Location = new Point(12, 80);
            progressBarEx2.Name = "progressBarEx2";
            progressBarEx2.Size = new Size(201, 35);
            progressBarEx2.TabIndex = 1;
            progressBarEx2.Text = "progressBarEx2";
            progressBarEx2.Type = FireBlade.WinInteropUtils.WinForms.ProgressBarExType.Marquee;
            // 
            // progressBarEx3
            // 
            progressBarEx3.Location = new Point(12, 131);
            progressBarEx3.Name = "progressBarEx3";
            progressBarEx3.Size = new Size(201, 35);
            progressBarEx3.Style = FireBlade.WinInteropUtils.WinForms.ProgressBarExStyle.Paused;
            progressBarEx3.TabIndex = 2;
            progressBarEx3.Text = "progressBarEx3";
            progressBarEx3.Value = 50;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 750;
            timer1.Tick += timer1_Tick;
            // 
            // progressBarEx4
            // 
            progressBarEx4.Location = new Point(12, 184);
            progressBarEx4.MarqueeSpeed = 0;
            progressBarEx4.Name = "progressBarEx4";
            progressBarEx4.Size = new Size(201, 35);
            progressBarEx4.TabIndex = 3;
            progressBarEx4.Text = "progressBarEx4";
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(219, 173);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(45, 23);
            numericUpDown1.TabIndex = 4;
            // 
            // button1
            // 
            button1.Location = new Point(219, 202);
            button1.Name = "button1";
            button1.Size = new Size(54, 23);
            button1.TabIndex = 5;
            button1.Text = "DELTA";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // WiuWinFormsTestForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button1);
            Controls.Add(numericUpDown1);
            Controls.Add(progressBarEx4);
            Controls.Add(progressBarEx3);
            Controls.Add(progressBarEx2);
            Controls.Add(progressBarEx1);
            Name = "WiuWinFormsTestForm";
            Text = "WiuWinFormsTestForm";
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private FireBlade.WinInteropUtils.WinForms.ProgressBarEx progressBarEx1;
        private FireBlade.WinInteropUtils.WinForms.ProgressBarEx progressBarEx2;
        private FireBlade.WinInteropUtils.WinForms.ProgressBarEx progressBarEx3;
        private System.Windows.Forms.Timer timer1;
        private FireBlade.WinInteropUtils.WinForms.ProgressBarEx progressBarEx4;
        private NumericUpDown numericUpDown1;
        private Button button1;
    }
}