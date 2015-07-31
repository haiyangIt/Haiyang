namespace SendMail
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.MailAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Start = new System.Windows.Forms.Button();
            this.End = new System.Windows.Forms.Button();
            this.DomainName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.SendMail = new System.Windows.Forms.Button();
            this.HostName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.UserName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.Psw = new System.Windows.Forms.TextBox();
            this.TimeSpan = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SentCount = new System.Windows.Forms.TextBox();
            this.FinishCount = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.CreateMailBox = new System.Windows.Forms.Button();
            this.ContainAttachment = new System.Windows.Forms.CheckBox();
            this.IsRefreshLog = new System.Windows.Forms.CheckBox();
            this.ExceptionText = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.AttachPathText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ErrorCountTxt = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.RecvMaxCountText = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.SendCountText = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // MailAddress
            // 
            this.MailAddress.Location = new System.Drawing.Point(85, 75);
            this.MailAddress.Multiline = true;
            this.MailAddress.Name = "MailAddress";
            this.MailAddress.Size = new System.Drawing.Size(986, 61);
            this.MailAddress.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-1, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 40;
            this.label1.Text = "AccountNames:";
            // 
            // Start
            // 
            this.Start.Location = new System.Drawing.Point(85, 266);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(75, 23);
            this.Start.TabIndex = 10;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // End
            // 
            this.End.Location = new System.Drawing.Point(166, 266);
            this.End.Name = "End";
            this.End.Size = new System.Drawing.Size(75, 23);
            this.End.TabIndex = 11;
            this.End.Text = "End";
            this.End.UseVisualStyleBackColor = true;
            this.End.Click += new System.EventHandler(this.End_Click);
            // 
            // DomainName
            // 
            this.DomainName.Location = new System.Drawing.Point(549, 154);
            this.DomainName.Name = "DomainName";
            this.DomainName.Size = new System.Drawing.Size(160, 20);
            this.DomainName.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(497, 157);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 43;
            this.label2.Text = "Domain:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(11, 345);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(527, 303);
            this.textBox1.TabIndex = 18;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(544, 345);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(529, 303);
            this.textBox2.TabIndex = 19;
            // 
            // SendMail
            // 
            this.SendMail.Location = new System.Drawing.Point(358, 266);
            this.SendMail.Name = "SendMail";
            this.SendMail.Size = new System.Drawing.Size(75, 23);
            this.SendMail.TabIndex = 13;
            this.SendMail.Text = "Test";
            this.SendMail.UseVisualStyleBackColor = true;
            this.SendMail.Click += new System.EventHandler(this.SendMail_Click);
            // 
            // HostName
            // 
            this.HostName.Location = new System.Drawing.Point(85, 189);
            this.HostName.Name = "HostName";
            this.HostName.Size = new System.Drawing.Size(216, 20);
            this.HostName.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 192);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 44;
            this.label4.Text = "HostName:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 157);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 41;
            this.label5.Text = "UserName:";
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(85, 154);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(152, 20);
            this.UserName.TabIndex = 1;
            this.UserName.Text = "administrator";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(249, 157);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 42;
            this.label6.Text = "Password:";
            // 
            // Psw
            // 
            this.Psw.Location = new System.Drawing.Point(311, 155);
            this.Psw.Name = "Psw";
            this.Psw.PasswordChar = '*';
            this.Psw.Size = new System.Drawing.Size(146, 20);
            this.Psw.TabIndex = 2;
            // 
            // TimeSpan
            // 
            this.TimeSpan.Location = new System.Drawing.Point(85, 225);
            this.TimeSpan.Name = "TimeSpan";
            this.TimeSpan.Size = new System.Drawing.Size(152, 20);
            this.TimeSpan.TabIndex = 7;
            this.TimeSpan.Text = "2";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(1, 228);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(81, 13);
            this.label7.TabIndex = 46;
            this.label7.Text = "SendInterval(s):";
            // 
            // SentCount
            // 
            this.SentCount.Location = new System.Drawing.Point(85, 305);
            this.SentCount.Name = "SentCount";
            this.SentCount.Size = new System.Drawing.Size(152, 20);
            this.SentCount.TabIndex = 14;
            // 
            // FinishCount
            // 
            this.FinishCount.Location = new System.Drawing.Point(311, 305);
            this.FinishCount.Name = "FinishCount";
            this.FinishCount.Size = new System.Drawing.Size(146, 20);
            this.FinishCount.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(19, 307);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 13);
            this.label8.TabIndex = 49;
            this.label8.Text = "SentCount:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(240, 307);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 13);
            this.label9.TabIndex = 50;
            this.label9.Text = "FinishCount:";
            // 
            // CreateMailBox
            // 
            this.CreateMailBox.Location = new System.Drawing.Point(252, 266);
            this.CreateMailBox.Name = "CreateMailBox";
            this.CreateMailBox.Size = new System.Drawing.Size(100, 23);
            this.CreateMailBox.TabIndex = 12;
            this.CreateMailBox.Text = "CreateMailBox";
            this.CreateMailBox.UseVisualStyleBackColor = true;
            this.CreateMailBox.Click += new System.EventHandler(this.CreateMailBox_Click);
            // 
            // ContainAttachment
            // 
            this.ContainAttachment.AutoSize = true;
            this.ContainAttachment.Location = new System.Drawing.Point(311, 192);
            this.ContainAttachment.Name = "ContainAttachment";
            this.ContainAttachment.Size = new System.Drawing.Size(116, 17);
            this.ContainAttachment.TabIndex = 5;
            this.ContainAttachment.Text = "ContainAttachment";
            this.ContainAttachment.UseVisualStyleBackColor = true;
            // 
            // IsRefreshLog
            // 
            this.IsRefreshLog.AutoSize = true;
            this.IsRefreshLog.Location = new System.Drawing.Point(724, 306);
            this.IsRefreshLog.Name = "IsRefreshLog";
            this.IsRefreshLog.Size = new System.Drawing.Size(89, 17);
            this.IsRefreshLog.TabIndex = 17;
            this.IsRefreshLog.Text = "IsRefreshLog";
            this.IsRefreshLog.UseVisualStyleBackColor = true;
            // 
            // ExceptionText
            // 
            this.ExceptionText.Location = new System.Drawing.Point(11, 654);
            this.ExceptionText.Multiline = true;
            this.ExceptionText.Name = "ExceptionText";
            this.ExceptionText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ExceptionText.Size = new System.Drawing.Size(1062, 167);
            this.ExceptionText.TabIndex = 20;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(480, 192);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(63, 13);
            this.label12.TabIndex = 45;
            this.label12.Text = "AttachPath:";
            // 
            // AttachPathText
            // 
            this.AttachPathText.Location = new System.Drawing.Point(549, 189);
            this.AttachPathText.Name = "AttachPathText";
            this.AttachPathText.Size = new System.Drawing.Size(160, 20);
            this.AttachPathText.TabIndex = 6;
            this.AttachPathText.Text = "C:\\Attach.zip";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(478, 308);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 51;
            this.label3.Text = "ErrorCount:";
            // 
            // ErrorCountTxt
            // 
            this.ErrorCountTxt.Location = new System.Drawing.Point(549, 305);
            this.ErrorCountTxt.Name = "ErrorCountTxt";
            this.ErrorCountTxt.Size = new System.Drawing.Size(160, 20);
            this.ErrorCountTxt.TabIndex = 16;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(459, 228);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(84, 13);
            this.label10.TabIndex = 48;
            this.label10.Text = "RecvMaxCount:";
            // 
            // RecvMaxCountText
            // 
            this.RecvMaxCountText.Location = new System.Drawing.Point(549, 225);
            this.RecvMaxCountText.Name = "RecvMaxCountText";
            this.RecvMaxCountText.Size = new System.Drawing.Size(160, 20);
            this.RecvMaxCountText.TabIndex = 9;
            this.RecvMaxCountText.Text = "20";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(242, 228);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(63, 13);
            this.label11.TabIndex = 47;
            this.label11.Text = "SendCount:";
            // 
            // SendCountText
            // 
            this.SendCountText.Location = new System.Drawing.Point(311, 225);
            this.SendCountText.Name = "SendCountText";
            this.SendCountText.Size = new System.Drawing.Size(146, 20);
            this.SendCountText.TabIndex = 8;
            this.SendCountText.Text = "1000";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(9, 9);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(68, 13);
            this.label13.TabIndex = 52;
            this.label13.Text = "Please Note:";
            // 
            // textBox4
            // 
            this.textBox4.Enabled = false;
            this.textBox4.Location = new System.Drawing.Point(87, 6);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(986, 63);
            this.textBox4.TabIndex = 53;
            this.textBox4.Text = resources.GetString("textBox4.Text");
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1083, 832);
            this.Controls.Add(this.ExceptionText);
            this.Controls.Add(this.IsRefreshLog);
            this.Controls.Add(this.ContainAttachment);
            this.Controls.Add(this.AttachPathText);
            this.Controls.Add(this.CreateMailBox);
            this.Controls.Add(this.ErrorCountTxt);
            this.Controls.Add(this.FinishCount);
            this.Controls.Add(this.SentCount);
            this.Controls.Add(this.SendCountText);
            this.Controls.Add(this.RecvMaxCountText);
            this.Controls.Add(this.TimeSpan);
            this.Controls.Add(this.HostName);
            this.Controls.Add(this.Psw);
            this.Controls.Add(this.UserName);
            this.Controls.Add(this.DomainName);
            this.Controls.Add(this.SendMail);
            this.Controls.Add(this.End);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.Start);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.MailAddress);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox MailAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.Button End;
        private System.Windows.Forms.TextBox DomainName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button SendMail;
        private System.Windows.Forms.TextBox HostName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox Psw;
        private System.Windows.Forms.TextBox TimeSpan;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox SentCount;
        private System.Windows.Forms.TextBox FinishCount;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button CreateMailBox;
        private System.Windows.Forms.CheckBox ContainAttachment;
        private System.Windows.Forms.CheckBox IsRefreshLog;
        private System.Windows.Forms.TextBox ExceptionText;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox AttachPathText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ErrorCountTxt;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox RecvMaxCountText;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox SendCountText;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBox4;
    }
}

