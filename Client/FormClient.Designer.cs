namespace RemoteDesktopClient
{
    partial class FormClient
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
            this.服务器 = new System.Windows.Forms.Label();
            this.LblServerConnState = new System.Windows.Forms.Label();
            this.LblNetworkConnState = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.LblHibernateTime = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // 服务器
            // 
            this.服务器.AutoSize = true;
            this.服务器.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.服务器.Location = new System.Drawing.Point(24, 58);
            this.服务器.Name = "服务器";
            this.服务器.Size = new System.Drawing.Size(58, 21);
            this.服务器.TabIndex = 0;
            this.服务器.Text = "服务器";
            // 
            // LblServerConnState
            // 
            this.LblServerConnState.AutoSize = true;
            this.LblServerConnState.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LblServerConnState.ForeColor = System.Drawing.Color.Green;
            this.LblServerConnState.Location = new System.Drawing.Point(151, 58);
            this.LblServerConnState.Name = "LblServerConnState";
            this.LblServerConnState.Size = new System.Drawing.Size(21, 21);
            this.LblServerConnState.TabIndex = 1;
            this.LblServerConnState.Text = "√";
            // 
            // LblNetworkConnState
            // 
            this.LblNetworkConnState.AutoSize = true;
            this.LblNetworkConnState.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LblNetworkConnState.ForeColor = System.Drawing.Color.Green;
            this.LblNetworkConnState.Location = new System.Drawing.Point(151, 112);
            this.LblNetworkConnState.Name = "LblNetworkConnState";
            this.LblNetworkConnState.Size = new System.Drawing.Size(21, 21);
            this.LblNetworkConnState.TabIndex = 3;
            this.LblNetworkConnState.Text = "√";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(24, 112);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "网络";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(24, 171);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 21);
            this.label1.TabIndex = 4;
            this.label1.Text = "休眠时间";
            // 
            // LblHibernateTime
            // 
            this.LblHibernateTime.AutoSize = true;
            this.LblHibernateTime.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LblHibernateTime.Location = new System.Drawing.Point(139, 171);
            this.LblHibernateTime.Name = "LblHibernateTime";
            this.LblHibernateTime.Size = new System.Drawing.Size(42, 21);
            this.LblHibernateTime.TabIndex = 5;
            this.LblHibernateTime.Text = "--:--";
            // 
            // FormClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(252, 292);
            this.Controls.Add(this.LblHibernateTime);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LblNetworkConnState);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.LblServerConnState);
            this.Controls.Add(this.服务器);
            this.Name = "FormClient";
            this.Text = "远程桌面管家客户端";
            this.Load += new System.EventHandler(this.FormClient_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label 服务器;
        private Label LblServerConnState;
        private Label LblNetworkConnState;
        private Label label2;
        private Label label1;
        private Label LblHibernateTime;
    }
}