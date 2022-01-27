namespace RemoteDesktopManager
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.BtnShutDownSelf = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(56, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "16";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BtnSleep
            // 
            this.BtnSleep.Location = new System.Drawing.Point(56, 79);
            this.BtnSleep.Name = "BtnSleep";
            this.BtnSleep.Size = new System.Drawing.Size(84, 30);
            this.BtnSleep.TabIndex = 1;
            this.BtnSleep.Text = "休眠";
            this.BtnSleep.UseVisualStyleBackColor = true;
            this.BtnSleep.Click += new System.EventHandler(this.BtnSleep_Click);
            // 
            // BtnShutDown
            // 
            this.BtnShutDown.Location = new System.Drawing.Point(56, 137);
            this.BtnShutDown.Name = "BtnShutDown";
            this.BtnShutDown.Size = new System.Drawing.Size(84, 30);
            this.BtnShutDown.TabIndex = 2;
            this.BtnShutDown.Text = "关机";
            this.BtnShutDown.UseVisualStyleBackColor = true;
            this.BtnShutDown.Click += new System.EventHandler(this.BtnShutDown_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 17;
            this.listBox1.Location = new System.Drawing.Point(56, 202);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(717, 327);
            this.listBox1.TabIndex = 3;
            // 
            // BtnShutDownSelf
            // 
            this.BtnShutDownSelf.Location = new System.Drawing.Point(356, 560);
            this.BtnShutDownSelf.Name = "BtnShutDownSelf";
            this.BtnShutDownSelf.Size = new System.Drawing.Size(84, 30);
            this.BtnShutDownSelf.TabIndex = 4;
            this.BtnShutDownSelf.Text = "本机关机";
            this.BtnShutDownSelf.UseVisualStyleBackColor = true;
            this.BtnShutDownSelf.Click += new System.EventHandler(this.BtnShutDownSelf_Click);
            // 
            // FormManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 614);
            this.Controls.Add(this.BtnShutDownSelf);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.BtnShutDown);
            this.Controls.Add(this.BtnSleep);
            this.Controls.Add(this.label1);
            this.Name = "FormManager";
            this.Text = "远程桌面管家";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Label label1;
        private Button BtnSleep;
        private Button BtnShutDown;
        private ListBox listBox1;
        private Button BtnShutDownSelf;
    }
}