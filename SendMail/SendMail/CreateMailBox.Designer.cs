namespace SendMail
{
    partial class CreateMailBox
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
            this.label1 = new System.Windows.Forms.Label();
            this.MailAddress = new System.Windows.Forms.TextBox();
            this.Psw = new System.Windows.Forms.TextBox();
            this.UserName = new System.Windows.Forms.TextBox();
            this.DomainName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CreateMailBoxBtn = new System.Windows.Forms.Button();
            this.ResultTxt = new System.Windows.Forms.TextBox();
            this.DbText = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "EmailAddress:";
            // 
            // MailAddress
            // 
            this.MailAddress.Location = new System.Drawing.Point(85, 12);
            this.MailAddress.Multiline = true;
            this.MailAddress.Name = "MailAddress";
            this.MailAddress.Size = new System.Drawing.Size(711, 103);
            this.MailAddress.TabIndex = 1;
            // 
            // Psw
            // 
            this.Psw.Location = new System.Drawing.Point(475, 122);
            this.Psw.Name = "Psw";
            this.Psw.PasswordChar = '*';
            this.Psw.Size = new System.Drawing.Size(146, 20);
            this.Psw.TabIndex = 4;
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(253, 121);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(152, 20);
            this.UserName.TabIndex = 3;
            this.UserName.Text = "administrator";
            // 
            // DomainName
            // 
            this.DomainName.Location = new System.Drawing.Point(84, 121);
            this.DomainName.Name = "DomainName";
            this.DomainName.Size = new System.Drawing.Size(100, 20);
            this.DomainName.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(417, 125);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "Password:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(192, 125);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "UserName:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 22;
            this.label2.Text = "Domain:";
            // 
            // CreateMailBoxBtn
            // 
            this.CreateMailBoxBtn.Location = new System.Drawing.Point(12, 173);
            this.CreateMailBoxBtn.Name = "CreateMailBoxBtn";
            this.CreateMailBoxBtn.Size = new System.Drawing.Size(173, 23);
            this.CreateMailBoxBtn.TabIndex = 6;
            this.CreateMailBoxBtn.Text = "Create";
            this.CreateMailBoxBtn.UseVisualStyleBackColor = true;
            this.CreateMailBoxBtn.Click += new System.EventHandler(this.CreateMailBoxBtn_Click);
            // 
            // ResultTxt
            // 
            this.ResultTxt.Location = new System.Drawing.Point(12, 202);
            this.ResultTxt.Multiline = true;
            this.ResultTxt.Name = "ResultTxt";
            this.ResultTxt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ResultTxt.Size = new System.Drawing.Size(784, 398);
            this.ResultTxt.TabIndex = 7;
            // 
            // DbText
            // 
            this.DbText.Location = new System.Drawing.Point(84, 147);
            this.DbText.Name = "DbText";
            this.DbText.Size = new System.Drawing.Size(100, 20);
            this.DbText.TabIndex = 5;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(56, 151);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(24, 13);
            this.label10.TabIndex = 25;
            this.label10.Text = "Db:";
            // 
            // CreateMailBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 686);
            this.Controls.Add(this.DbText);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.ResultTxt);
            this.Controls.Add(this.CreateMailBoxBtn);
            this.Controls.Add(this.Psw);
            this.Controls.Add(this.UserName);
            this.Controls.Add(this.DomainName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.MailAddress);
            this.Name = "CreateMailBox";
            this.Text = "CreateMailBox";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CreateMailBox_FormClosing);
            this.Load += new System.EventHandler(this.CreateMailBox_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox MailAddress;
        private System.Windows.Forms.TextBox Psw;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.TextBox DomainName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button CreateMailBoxBtn;
        private System.Windows.Forms.TextBox ResultTxt;
        private System.Windows.Forms.TextBox DbText;
        private System.Windows.Forms.Label label10;
    }
}