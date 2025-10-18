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
            slider1 = new FireBlade.WinInteropUtils.WinForms.Slider();
            slider2 = new FireBlade.WinInteropUtils.WinForms.Slider();
            slider3 = new FireBlade.WinInteropUtils.WinForms.Slider();
            label2 = new Label();
            label1 = new Label();
            slider4 = new FireBlade.WinInteropUtils.WinForms.Slider();
            numericUpDown2 = new NumericUpDown();
            button2 = new Button();
            button3 = new Button();
            hotKeyBox1 = new FireBlade.WinInteropUtils.WinForms.HotKeyBox();
            hotKeyBox2 = new FireBlade.WinInteropUtils.WinForms.HotKeyBox();
            label3 = new Label();
            label4 = new Label();
            slider5 = new FireBlade.WinInteropUtils.WinForms.Slider();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
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
            // slider1
            // 
            slider1.Location = new Point(651, 21);
            slider1.Name = "slider1";
            slider1.Size = new Size(137, 35);
            slider1.TabIndex = 6;
            slider1.Text = "slider1";
            // 
            // slider2
            // 
            slider2.Location = new Point(651, 62);
            slider2.Maximum = 10;
            slider2.Name = "slider2";
            slider2.SelectionRangeEnd = 7;
            slider2.SelectionRangeStart = 3;
            slider2.ShowSelectionRange = true;
            slider2.Size = new Size(137, 35);
            slider2.TabIndex = 7;
            slider2.Text = "slider2";
            slider2.TickFrequency = 1;
            slider2.TickMode = FireBlade.WinInteropUtils.WinForms.SliderTickMode.Custom;
            slider2.Value = 5;
            slider2.ValueChanging += slider2_ValueChanging;
            // 
            // slider3
            // 
            slider3.LeftBuddy = label2;
            slider3.Location = new Point(610, 21);
            slider3.Name = "slider3";
            slider3.Orientation = Orientation.Vertical;
            slider3.RightBuddy = label1;
            slider3.Size = new Size(35, 82);
            slider3.TabIndex = 8;
            slider3.Text = "slider3";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(570, 4);
            label2.Name = "label2";
            label2.Size = new Size(101, 15);
            label2.TabIndex = 10;
            label2.Text = "More - top buddy";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(562, 105);
            label1.Name = "label1";
            label1.Size = new Size(117, 15);
            label1.TabIndex = 9;
            label1.Text = "Less - bottom buddy";
            // 
            // slider4
            // 
            slider4.Location = new Point(570, 131);
            slider4.Name = "slider4";
            slider4.Size = new Size(159, 35);
            slider4.TabIndex = 11;
            slider4.Text = "slider4";
            slider4.TickFrequency = 30;
            slider4.TickMode = FireBlade.WinInteropUtils.WinForms.SliderTickMode.Custom;
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new Point(738, 135);
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new Size(50, 23);
            numericUpDown2.TabIndex = 12;
            // 
            // button2
            // 
            button2.Location = new Point(738, 173);
            button2.Name = "button2";
            button2.Size = new Size(59, 23);
            button2.TabIndex = 13;
            button2.Text = "Add tick";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(662, 173);
            button3.Name = "button3";
            button3.Size = new Size(70, 23);
            button3.TabIndex = 14;
            button3.Text = "Clear ticks";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // hotKeyBox1
            // 
            hotKeyBox1.Keys = Keys.Control | Keys.A;
            hotKeyBox1.Location = new Point(12, 225);
            hotKeyBox1.Name = "hotKeyBox1";
            hotKeyBox1.Size = new Size(108, 23);
            hotKeyBox1.TabIndex = 15;
            hotKeyBox1.Text = "hotKeyBox1";
            // 
            // hotKeyBox2
            // 
            hotKeyBox2.FallbackValue = FireBlade.WinInteropUtils.WinForms.HotKeyBoxModifiers.Shift;
            hotKeyBox2.Location = new Point(81, 253);
            hotKeyBox2.Name = "hotKeyBox2";
            hotKeyBox2.Rules = FireBlade.WinInteropUtils.WinForms.HotKeyBoxRules.Control;
            hotKeyBox2.Size = new Size(108, 23);
            hotKeyBox2.TabIndex = 16;
            hotKeyBox2.Text = "hotKeyBox2";
            hotKeyBox2.HotKeyChanged += hotKeyBox2_HotKeyChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(13, 256);
            label3.Name = "label3";
            label3.Size = new Size(62, 15);
            label3.TabIndex = 17;
            label3.Text = "Restricted:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(131, 228);
            label4.Name = "label4";
            label4.Size = new Size(58, 15);
            label4.TabIndex = 18;
            label4.Text = "Changed!";
            label4.Visible = false;
            // 
            // slider5
            // 
            slider5.Location = new Point(275, 315);
            slider5.Name = "slider5";
            slider5.SelectionRangeEnd = 75;
            slider5.SelectionRangeStart = 25;
            slider5.ShowSelectionRange = true;
            slider5.Size = new Size(187, 36);
            slider5.TabIndex = 19;
            slider5.Text = "slider5";
            slider5.Value = 15;
            // 
            // WiuWinFormsTestForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(slider5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(hotKeyBox2);
            Controls.Add(hotKeyBox1);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(numericUpDown2);
            Controls.Add(slider4);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(slider3);
            Controls.Add(slider2);
            Controls.Add(slider1);
            Controls.Add(button1);
            Controls.Add(numericUpDown1);
            Controls.Add(progressBarEx4);
            Controls.Add(progressBarEx3);
            Controls.Add(progressBarEx2);
            Controls.Add(progressBarEx1);
            Name = "WiuWinFormsTestForm";
            Text = "WinInteropUtils.WinForms - Test";
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FireBlade.WinInteropUtils.WinForms.ProgressBarEx progressBarEx1;
        private FireBlade.WinInteropUtils.WinForms.ProgressBarEx progressBarEx2;
        private FireBlade.WinInteropUtils.WinForms.ProgressBarEx progressBarEx3;
        private System.Windows.Forms.Timer timer1;
        private FireBlade.WinInteropUtils.WinForms.ProgressBarEx progressBarEx4;
        private NumericUpDown numericUpDown1;
        private Button button1;
        private FireBlade.WinInteropUtils.WinForms.Slider slider1;
        private FireBlade.WinInteropUtils.WinForms.Slider slider2;
        private FireBlade.WinInteropUtils.WinForms.Slider slider3;
        private Label label2;
        private Label label1;
        private FireBlade.WinInteropUtils.WinForms.Slider slider4;
        private NumericUpDown numericUpDown2;
        private Button button2;
        private Button button3;
        private FireBlade.WinInteropUtils.WinForms.HotKeyBox hotKeyBox1;
        private FireBlade.WinInteropUtils.WinForms.HotKeyBox hotKeyBox2;
        private Label label3;
        private Label label4;
        private FireBlade.WinInteropUtils.WinForms.Slider slider5;
        //private FireBlade.WinInteropUtils.WinForms.HyperLink hyperLink1;
    }
}