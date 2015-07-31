using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;

namespace SendMail
{
    public partial class TestSendMail : Form, ITraceListener
    {
        public TestSendMail()
        {
            InitializeComponent();
            
        }

        public UserInfo UserObj;
        public SendInfo SendInfoObj;
        private bool _IsTestSuccess;

        private void SetUserAndSendInfo()
        {
            UserObj.UserName = UserName.Text;
            UserObj.Psw = Psw.Text;
            UserObj.DomainName = DomainName.Text;

            SendInfoObj.HostName = HostName.Text;
        }

        private void InitUserAndSendInfo()
        {
            UserName.Text = UserObj.UserName;
            Psw.Text = UserObj.Psw;
            DomainName.Text = UserObj.DomainName;

            HostName.Text = SendInfoObj.HostName;
        }

        private void NormalTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UserName.Text) || string.IsNullOrEmpty(Psw.Text) ||
                string.IsNullOrEmpty(DomainName.Text) || string.IsNullOrEmpty(HostName.Text) ||
                string.IsNullOrEmpty(SendAddress.Text))
            {
                MessageBox.Show("请输入正确的数据，不能为空！");
                return;
            }


            SetUserAndSendInfo();

            ResultText.Text = "";
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            msg.To.Add(SendAddress.Text);
            msg.From = new MailAddress(SendAddress.Text);
            msg.Subject = "test";
            msg.Body = "test body";

            SmtpClient client = new SmtpClient();
            client.Host = HostName.Text;
            client.Port = 587;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(UserName.Text, Psw.Text, DomainName.Text);
            try
            {
                client.Send(msg);
                ResultText.Text = "success";
                _IsTestSuccess = true;
                    
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                ResultText.Text = ex.Message;
                _IsTestSuccess = false;
            }
        }

        private void EwsTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UserName.Text) || string.IsNullOrEmpty(Psw.Text) ||
                string.IsNullOrEmpty(DomainName.Text) ||
                string.IsNullOrEmpty(SendAddress.Text))
            {
                MessageBox.Show("请输入正确的数据，不能为空！");
                return;
            }

            try
            {
                SetUserAndSendInfo();

                ResultText.Text = "";
                ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2013_SP1);

                service.Credentials = new WebCredentials(UserName.Text, Psw.Text, DomainName.Text);

                service.TraceEnabled = true;
                service.TraceFlags = TraceFlags.All;
                service.TraceListener = this;

                service.AutodiscoverUrl(SendAddress.Text, RedirectionUrlValidationCallback);

                EmailMessage email = new EmailMessage(service);

                email.ToRecipients.Add(SendAddress.Text);

                email.Subject = "HelloWorld";
                email.Body = new MessageBody("This is the first email I've sent by using the EWS Managed API");

                email.Send();
            }
            catch(Exception ex)
            {
                ResultText.Text += ex.Message;
            }
        }

        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;

            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }

        public void Trace(string traceType, string traceMessage)
        {
            string result = string.Format("Type:{0},message:{1}.\r\n", traceType, traceMessage);
            AppendTextBox2(result);
        }

        public void AppendTextBox2(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox2), new object[] { value });
                return;
            }

            ResultText.Text += value;
        }

        private void TestSendMail_Load(object sender, EventArgs e)
        {
            InitUserAndSendInfo();
            SendAddress.Text = string.Format("{0}@{1}", UserName.Text, DomainName.Text);
        }

        private void TestSendMail_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_IsTestSuccess)
                this.DialogResult = System.Windows.Forms.DialogResult.Yes;
            else
                this.DialogResult = System.Windows.Forms.DialogResult.No;
        }
    }
}
