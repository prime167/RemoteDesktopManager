namespace Manager
{
    partial class FormManager
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
            this.label1 = new System.Windows.Forms.Label();
            this.BtnSleep = new System.Windows.Forms.Button();
            this.BtnShutDown = new System.Windows.Forms.Button();
            this.BtnShutDownSelf = new System.Windows.Forms.Button();
            this.BtnSleepSelf = new System.Windows.Forms.Button();
            this.pnl1 = new System.Windows.Forms.Panel();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.pnl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(12, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "16";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BtnSleep
            // 
            this.BtnSleep.Location = new System.Drawing.Point(12, 62);
            this.BtnSleep.Name = "BtnSleep";
            this.BtnSleep.Size = new System.Drawing.Size(84, 30);
            this.BtnSleep.TabIndex = 1;
            this.BtnSleep.Text = "休眠";
            this.BtnSleep.UseVisualStyleBackColor = true;
            this.BtnSleep.Click += new System.EventHandler(this.BtnSleep_Click);
            // 
            // BtnShutDown
            // 
            this.BtnShutDown.Location = new System.Drawing.Point(12, 120);
            this.BtnShutDown.Name = "BtnShutDown";
            this.BtnShutDown.Size = new System.Drawing.Size(84, 30);
            this.BtnShutDown.TabIndex = 2;
            this.BtnShutDown.Text = "关机";
            this.BtnShutDown.UseVisualStyleBackColor = true;
            this.BtnShutDown.Click += new System.EventHandler(this.BtnShutDown_Click);
            // 
            // BtnShutDownSelf
            // 
            this.BtnShutDownSelf.Location = new System.Drawing.Point(350, 384);
            this.BtnShutDownSelf.Name = "BtnShutDownSelf";
            this.BtnShutDownSelf.Size = new System.Drawing.Size(84, 30);
            this.BtnShutDownSelf.TabIndex = 4;
            this.BtnShutDownSelf.Text = "本机关机";
            this.BtnShutDownSelf.UseVisualStyleBackColor = true;
            this.BtnShutDownSelf.Click += new System.EventHandler(this.BtnShutDownSelf_Click);
            // 
            // BtnSleepSelf
            // 
            this.BtnSleepSelf.Location = new System.Drawing.Point(215, 384);
            this.BtnSleepSelf.Name = "BtnSleepSelf";
            this.BtnSleepSelf.Size = new System.Drawing.Size(84, 30);
            this.BtnSleepSelf.TabIndex = 5;
            this.BtnSleepSelf.Text = "本机休眠";
            this.BtnSleepSelf.UseVisualStyleBackColor = true;
            this.BtnSleepSelf.Click += new System.EventHandler(this.BtnSleepSelf_Click);
            // 
            // pnl1
            // 
            this.pnl1.Controls.Add(this.BtnShutDown);
            this.pnl1.Controls.Add(this.label1);
            this.pnl1.Controls.Add(this.BtnSleep);
            this.pnl1.Location = new System.Drawing.Point(23, 12);
            this.pnl1.Name = "pnl1";
            this.pnl1.Size = new System.Drawing.Size(114, 161);
            this.pnl1.TabIndex = 6;
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 17;
            this.listBox1.Location = new System.Drawing.Point(23, 205);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(650, 123);
            this.listBox1.TabIndex = 3;
            // 
            // FormManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(707, 444);
            this.Controls.Add(this.pnl1);
            this.Controls.Add(this.BtnSleepSelf);
            this.Controls.Add(this.BtnShutDownSelf);
            this.Controls.Add(this.listBox1);
            this.Name = "FormManager";
            this.Text = "远程桌面管家";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Label label1;
        private Button BtnSleep;
        private Button BtnShutDown;
        private Button BtnShutDownSelf;
        private Button BtnSleepSelf;
        private Panel pnl1;
        private ListBox listBox1;
    }
}