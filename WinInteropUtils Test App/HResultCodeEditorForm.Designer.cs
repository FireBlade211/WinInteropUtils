namespace WinInteropUtils_Test_App
{
    partial class HResultCodeEditorForm
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
            comboBox1 = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            numericUpDown1 = new NumericUpDown();
            button1 = new Button();
            numericUpDown2 = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            SuspendLayout();
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(12, 32);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 23);
            comboBox1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 10);
            label1.Name = "label1";
            label1.Size = new Size(47, 15);
            label1.TabIndex = 1;
            label1.Text = "Facility:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(159, 9);
            label2.Name = "label2";
            label2.Size = new Size(51, 15);
            label2.TabIndex = 3;
            label2.Text = "Severity:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(13, 69);
            label3.Name = "label3";
            label3.Size = new Size(34, 15);
            label3.TabIndex = 4;
            label3.Text = "Base:";
            // 
            // numericUpDown1
            // 
            numericUpDown1.Hexadecimal = true;
            numericUpDown1.Location = new Point(19, 92);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(114, 23);
            numericUpDown1.TabIndex = 5;
            // 
            // button1
            // 
            button1.DialogResult = DialogResult.OK;
            button1.Location = new Point(209, 131);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 6;
            button1.Text = "OK";
            button1.UseVisualStyleBackColor = true;
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new Point(170, 32);
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new Size(114, 23);
            numericUpDown2.TabIndex = 7;
            // 
            // HResultCodeEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(numericUpDown2);
            Controls.Add(button1);
            Controls.Add(numericUpDown1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(comboBox1);
            Name = "HResultCodeEditorForm";
            Size = new Size(296, 166);
            Load += HResultCodeEditorForm_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private Label label2;
        private Label label3;
        public ComboBox comboBox1;
        public NumericUpDown numericUpDown1;
        public NumericUpDown numericUpDown2;
        public Button button1;
    }
}