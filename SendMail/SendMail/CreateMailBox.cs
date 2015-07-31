using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SendMail
{
    public partial class CreateMailBox : Form
    {
        private static char[] Split = ";".ToCharArray();
        public CreateMailBox()
        {
            InitializeComponent();
        }

        public string CreateMailsName = "";
        private bool IsCreateSuccess = false;

        public UserInfo UserObj;

        private void SetUserAndSendInfo()
        {
            UserObj.UserName = UserName.Text;
            UserObj.Psw = Psw.Text;
            UserObj.DomainName = DomainName.Text;
        }

        private void InitUserAndSendInfo()
        {
            UserName.Text = UserObj.UserName;
            Psw.Text = UserObj.Psw;
            DomainName.Text = UserObj.DomainName;
        }

        private void CreateMailBoxBtn_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(UserName.Text) || string.IsNullOrEmpty(Psw.Text) || 
                string.IsNullOrEmpty(DomainName.Text) || string.IsNullOrEmpty(MailAddress.Text) || 
                string.IsNullOrEmpty(DbText.Text))
            {
                MessageBox.Show("请输入正确的数据，不能为空！");
                return;
            }

            SetUserAndSendInfo();
            string[] names = MailAddress.Text.Split(Split, StringSplitOptions.RemoveEmptyEntries);
            CreateMailsName = MailAddress.Text;
            string message = EmsSession.CreateMails(names, DomainName.Text, DbText.Text, out IsCreateSuccess);
            ResultTxt.Text = message;
           
        }

        private void CreateMailBox_Load(object sender, EventArgs e)
        {
            InitUserAndSendInfo();
            MailAddress.Text = CreateMailsName;
        }

        private void CreateMailBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsCreateSuccess)
                this.DialogResult = System.Windows.Forms.DialogResult.Yes;
            else
                this.DialogResult = System.Windows.Forms.DialogResult.No;
        }

    }
}
