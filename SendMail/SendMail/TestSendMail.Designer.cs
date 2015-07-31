namespace SendMail
{
    partial class TestSendMail
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
            this.HostName = new System.Windows.Forms.TextBox();
            this.Psw = new System.Windows.Forms.TextBox();
            this.UserName = new System.Windows.Forms.TextBox();
            this.SendAddress = new System.Windows.Forms.TextBox();
            this.DomainName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.NormalTest = new System.Windows.Forms.Button();
            this.EwsTest = new System.Windows.Forms.Button();
            this.ResultText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // HostName
            // 
            this.HostName.Location = new System.Drawing.Point(225, 136);
            this.HostName.Name = "HostName";
            this.HostName.Size = new System.Drawing.Size(146, 20);
            this.HostName.TabIndex = 4;
            // 
            // Psw
            // 
            this.Psw.Location = new System.Drawing.Point(225, 62);
            this.Psw.Name = "Psw";
            this.Psw.PasswordChar = '*';
            this.Psw.Size = new System.Drawing.Size(146, 20);
            this.Psw.TabIndex = 2;
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(225, 23);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(152, 20);
            this.UserName.TabIndex = 1;
            this.UserName.Text = "administrator";
            // 
            // SendAddress
            // 
            this.SendAddress.Location = new System.Drawing.Point(225, 176);
            this.SendAddress.Name = "SendAddress";
            this.SendAddress.Size = new System.Drawing.Size(152, 20);
            this.SendAddress.TabIndex = 5;
            // 
            // DomainName
            // 
            this.DomainName.Location = new System.Drawing.Point(225, 99);
            this.DomainName.Name = "DomainName";
            this.DomainName.Size = new System.Drawing.Size(100, 20);
            this.DomainName.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(167, 65);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "Password:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(159, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "UserName:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(163, 139);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "HostName:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(151, 179);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "SendAddress:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(176, 102);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "Domain:";
            // 
            // NormalTest
            // 
            this.NormalTest.Location = new System.Drawing.Point(170, 223);
            this.NormalTest.Name = "NormalTest";
            this.NormalTest.Size = new System.Drawing.Size(75, 23);
            this.NormalTest.TabIndex = 6;
            this.NormalTest.Text = "Test";
            this.NormalTest.UseVisualStyleBackColor = true;
            this.NormalTest.Click += new System.EventHandler(this.NormalTest_Click);
            // 
            // EwsTest
            // 
            this.EwsTest.Location = new System.Drawing.Point(266, 223);
            this.EwsTest.Name = "EwsTest";
            this.EwsTest.Size = new System.Drawing.Size(75, 23);
            this.EwsTest.TabIndex = 7;
            this.EwsTest.Text = "EwsTest";
            this.EwsTest.UseVisualStyleBackColor = true;
            this.EwsTest.Click += new System.EventHandler(this.EwsTest_Click);
            // 
            // ResultText
            // 
            this.ResultText.Location = new System.Drawing.Point(61, 281);
            this.ResultText.Multiline = true;
            this.ResultText.Name = "ResultText";
            this.ResultText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ResultText.Size = new System.Drawing.Size(429, 193);
            this.ResultText.TabIndex = 8;
            // 
            // TestSendMail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 530);
            this.Controls.Add(this.ResultText);
            this.Controls.Add(this.EwsTest);
            this.Controls.Add(this.NormalTest);
            this.Controls.Add(this.HostName);
            this.Controls.Add(this.Psw);
            this.Controls.Add(this.UserName);
            this.Controls.Add(this.SendAddress);
            this.Controls.Add(this.DomainName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Name = "TestSendMail";
            this.Text = "TestSendMail";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestSendMail_FormClosing);
            this.Load += new System.EventHandler(this.TestSendMail_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox HostName;
        private System.Windows.Forms.TextBox Psw;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.TextBox SendAddress;
        private System.Windows.Forms.TextBox DomainName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button NormalTest;
        private System.Windows.Forms.Button EwsTest;
        private System.Windows.Forms.TextBox ResultText;
    }
}