namespace Test_Client
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            textBox1 = new TextBox();
            button1 = new Button();
            button2 = new Button();
            textBox2 = new TextBox();
            listBox1 = new ListBox();
            textBox3 = new TextBox();
            button3 = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 11);
            label1.Name = "label1";
            label1.Size = new Size(59, 15);
            label1.TabIndex = 0;
            label1.Text = "서버 주소";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(75, 7);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(169, 23);
            textBox1.TabIndex = 1;
            textBox1.Text = "127.0.0.1";
            // 
            // button1
            // 
            button1.Location = new Point(326, 6);
            button1.Name = "button1";
            button1.Size = new Size(56, 25);
            button1.TabIndex = 2;
            button1.Text = "접속";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(384, 7);
            button2.Name = "button2";
            button2.Size = new Size(56, 25);
            button2.TabIndex = 3;
            button2.Text = "끊기";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(250, 8);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(66, 23);
            textBox2.TabIndex = 4;
            textBox2.Text = "40000";
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(15, 93);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(438, 199);
            listBox1.TabIndex = 5;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(15, 36);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(301, 23);
            textBox3.TabIndex = 6;
            textBox3.Text = "test_msg_001";
            // 
            // button3
            // 
            button3.Location = new Point(326, 36);
            button3.Name = "button3";
            button3.Size = new Size(56, 25);
            button3.TabIndex = 7;
            button3.Text = "보내기";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(471, 304);
            Controls.Add(button3);
            Controls.Add(textBox3);
            Controls.Add(listBox1);
            Controls.Add(textBox2);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Name = "MainForm";
            Text = "NetCode Client";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private Button button1;
        private Button button2;
        private TextBox textBox2;
        private ListBox listBox1;
        private TextBox textBox3;
        private Button button3;
    }
}
