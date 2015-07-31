using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using System.Net.Mail;
using Microsoft.Exchange.WebServices.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SendMail
{
    public partial class Form1 : Form
    {
        private static char[] split = ";".ToCharArray();
        
        private bool IsEnd = false;
        private int SleepMillionSeconds = 5000;
        private Thread _thread = null;
        private object _lock = new object();

        private MessageFactory _messageFactory = null;

        public UserInfo UserObj = new UserInfo();
        public SendInfo SendInfoObj = new SendInfo();

        private int SendIndex = 0;

        private void SetUserAndSendInfo()
        {
            UserObj.UserName = UserName.Text;
            UserObj.Psw = Psw.Text;
            UserObj.DomainName = DomainName.Text;

            SendInfoObj.HostName = HostName.Text;
            SendInfoObj.SendInterval = (Convert.ToInt32(TimeSpan.Text)) * 1000;
            SendInfoObj.RecvMaxCount = (Convert.ToInt32(RecvMaxCountText.Text));
            SendInfoObj.SendCount = Convert.ToInt32(SendCountText.Text);
        }

        public Form1()
        {
            InitializeComponent();
            ServicePointManager.ServerCertificateValidationCallback =
    delegate(object s, X509Certificate certificate,
             X509Chain chain, SslPolicyErrors sslPolicyErrors)
    { return true; };
        }

        private Queue<string> AllSendInfo = new Queue<string>(20);
        private StringBuilder _AllSendInfoSB = new StringBuilder(4096);
        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            if (IsRefreshLog.Checked)
                textBox1.Text = value;
            //lock (AllSendInfo)
            //{
            //    AllSendInfo.Enqueue(value);
            //    if (AllSendInfo.Count >= 5)
            //    {
            //        AllSendInfo.Dequeue();
            //    }
            //    _AllSendInfoSB.Length = 0;
            //    foreach(var str in AllSendInfo)
            //    {
            //        _AllSendInfoSB.Append(str);
            //    }
            //    textBox1.Text = _AllSendInfoSB.ToString();
            //}
        }

        private Queue<string> AllFinishInfo = new Queue<string>(20);
        private StringBuilder _AllFinishInfoSB = new StringBuilder(4096);
        public void AppendTextBox2(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox2), new object[] { value });
                return;
            }

            lock (AllFinishInfo)
            {
                AllFinishInfo.Enqueue(value);
                if (AllFinishInfo.Count >=20)
                {
                    AllFinishInfo.Dequeue();
                }
                _AllFinishInfoSB.Length = 0;
                foreach (var str in AllFinishInfo)
                {
                    _AllFinishInfoSB.Append(str);
                }
                if(IsRefreshLog.Checked)
                    textBox2.Text = _AllFinishInfoSB.ToString();
            }
        }

        public void AppendTextFinishCout(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextFinishCout), new object[] { value });
                return;
            }
            FinishCount.Text = value;
        }
        
        public void AppendErrorText(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendErrorText), new object[] { value });
                return;
            }
            ErrorCountTxt.Text = value;
        }
        public void AppendTextSendCount(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextSendCount), new object[] { value });
                return;
            }
            SentCount.Text = value;
        }

        public void AppendException(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendException), new object[] { value });
                return;
            }
            ExceptionText.Text += "\r\n";
            ExceptionText.Text += value;
        }

        private void Start_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UserName.Text) || string.IsNullOrEmpty(Psw.Text) ||
                string.IsNullOrEmpty(DomainName.Text) || string.IsNullOrEmpty(HostName.Text) ||
                string.IsNullOrEmpty(MailAddress.Text) || string.IsNullOrEmpty(TimeSpan.Text) || 
                string.IsNullOrEmpty(SendCountText.Text) || string.IsNullOrEmpty(RecvMaxCountText.Text))
            {
                MessageBox.Show("请输入正确的数据，不能为空！");
                return;
            }

            if (ContainAttachment.Checked)
            {
                if (string.IsNullOrEmpty(AttachPathText.Text))
                {
                    MessageBox.Show("请输入附件路径，不能为空！");
                    return;
                }
                else
                {
                    if(!File.Exists(AttachPathText.Text))
                    {
                        MessageBox.Show("附件文件不存在，请重新输入。");
                        return;
                    }
                }
            }



            SetUserAndSendInfo();

            string[] names = MailAddress.Text.Split(split, StringSplitOptions.RemoveEmptyEntries);
            _messageFactory = new MessageFactory(names, UserObj.DomainName, SendInfoObj.RecvMaxCount);
            IsEnd = false;
            textBox1.Text = "";
            textBox2.Text = "";
            if (_thread == null)
            {
                lock (_lock)
                {
                    if (_thread == null)
                    {
                        ThreadStart func = new ThreadStart(StartThread);
                        _thread = new Thread(func);
                        _thread.Start();
                    }
                }
            }
        }

        private void StartThread()
        {
            while (true)
            {
                Thread.Sleep(SendInfoObj.SendInterval);
                if (_isClosed)
                    break;
                if (IsEnd)
                    continue;
                int index = SendIndex++;
                index += 1;
                if (index > SendInfoObj.SendCount)
                {
                    IsEnd = true;
                    continue;
                }

                string msg = SendEmail();
                AppendTextSendCount(index.ToString());
            }
        }

        SmtpFactory ClientSmtp = new SmtpFactory();
        private string SendEmail()
        {
            System.Net.Mail.MailMessage msg = null;
            if(ContainAttachment.Checked)
            {
                msg = _messageFactory.CreateMailMessage(AttachPathText.Text);
            }
            else
            {
                msg = _messageFactory.CreateMailMessage();
            }

            string resultMessage = "";
            MySmtp smtp = null;
            int smtpIndex = 0;
            try
            {
                smtp = ClientSmtp.GetInstance(SendInfoObj.HostName, UserObj.UserName, UserObj.Psw, UserObj.DomainName, client_SendCompleted, out smtpIndex);
                smtp.SendAsync(msg, msg);
                Debug.Write("Get Index:");
                Debug.WriteLine(smtp.Index);
                resultMessage = "success";
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                ClientSmtp.DisableSmtp(smtpIndex);

                if (smtp != null)
                    ClientSmtp.SendException(smtp);
                AppendException(ex.Message);
                long errorValue = Interlocked.Increment(ref ErrorIndex);
                AppendErrorText(errorValue.ToString());
                resultMessage = ex.Message;
                long finishValue = Interlocked.Increment(ref FinishedIndex);
                AppendTextFinishCout(finishValue.ToString());
            }
            Output(msg, resultMessage);
            return resultMessage;
        }

        private long FinishedIndex = 0;
        private long ErrorIndex = 0;
        private void client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MailMessage messege = (MailMessage)e.UserState;
            string token = messege.Subject;

            long value = Interlocked.Increment(ref FinishedIndex);
            string result;
            if (e.Cancelled)
            {
                result = string.Format("[{1}][{0}] Send canceled.\r\n", token, value);
            }
            if (e.Error != null)
            {
                long errorValue = Interlocked.Increment(ref ErrorIndex);
                result = string.Format("[{2}][{0}] {1}.\r\n", token, e.Error.ToString(), value);
                AppendException(result);
                AppendErrorText(errorValue.ToString());
            }
            else
            {
                result = string.Format("[{1}]{0} Message sent.\r\n", token, value);
            }
            //textBox2.Text += result;
            AppendTextBox2(result);
            AppendTextFinishCout(value.ToString());

            if (ContainAttachment.Checked)
            {
                foreach (var att in messege.Attachments)
                {
                    att.ContentStream.Close();
                    att.ContentStream.Dispose();
                    att.Dispose();
                }
            }

            messege.Dispose();
        }

        StringBuilder sbTemp = new StringBuilder();
        private void Output(System.Net.Mail.MailMessage message,string resultMessage)
        {
            sbTemp.Length = 0;
            sbTemp.Append("[").Append(SendIndex).Append("]");
            sbTemp.Append("Send mail ").AppendLine(resultMessage);
            sbTemp.Append("Sender:").AppendLine(message.From.Address);
            sbTemp.AppendLine();
            sbTemp.AppendLine("Receive Address:");
            foreach (var recv in message.To)
            {
                sbTemp.AppendLine(recv.Address);
            }
            sbTemp.AppendLine();
            sbTemp.Append("subject:").AppendLine(message.Subject);
            sbTemp.AppendLine();
            sbTemp.Append("body:").AppendLine(message.Body);

            AppendTextBox(sbTemp.ToString());
        }

        private void End_Click(object sender, EventArgs e)
        {
            IsEnd = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.SelectionLength = textBox2.Text.Length;
            textBox2.ScrollToCaret();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionLength = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }

        private bool _isClosed = false;
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            IsEnd = true;
            _isClosed = true;
            ClientSmtp.Dispose();
            EmsSession.Release();
        }

        private void CreateMailBox_Click(object sender, EventArgs e)
        {
            SetUserAndSendInfo();
            CreateMailBox createForm = new CreateMailBox();
            createForm.UserObj = UserObj;
            createForm.CreateMailsName = MailAddress.Text;
            createForm.ShowDialog(this);
            if(createForm.DialogResult == System.Windows.Forms.DialogResult.Yes)
            {
                if (!string.IsNullOrEmpty(createForm.CreateMailsName))
                    MailAddress.Text = createForm.CreateMailsName;
                SetUserInfo(createForm.UserObj);
            }
        }

        private const string user01Prefix = "user01";
        private const string user02Prefix = "user002";
        private const string user03Prefix = "user003";
        private const string user04Prefix = "user004";

        private void SetGroup_Click(object sender, EventArgs e)
        {
            //int index = Convert.ToInt32(GroupText.Text) * 5 + 1;
            //int end = Convert.ToInt32(GroupText.Text) * 5 + 5 ;
            //StringBuilder sb = new StringBuilder();

            //if (end > 99)
            //    return;
            //while(index <= end && index <= 99)
            //{
                
            //    sb.Append(user01Prefix);
            //    if (index < 10)
            //        sb.Append("0");
            //    sb.Append(index).Append(";");
                
            //    sb.Append(user02Prefix);
            //    if (index < 10)
            //        sb.Append("0");
            //    sb.Append(index).Append(";");

            //    sb.Append(user03Prefix);
            //    if (index < 10)
            //        sb.Append("0");
            //    sb.Append(index).Append(";");

            //    sb.Append(user04Prefix);
            //    if (index < 10)
            //        sb.Append("0");
            //    sb.Append(index).Append(";");
            //    index++;
            //}
            //MailAddress.Text = sb.ToString();
        }

        private void SendMail_Click(object sender, EventArgs e)
        {
            SetUserAndSendInfo();
            TestSendMail testForm = new TestSendMail();
            testForm.UserObj = UserObj;
            testForm.SendInfoObj = SendInfoObj;
            testForm.ShowDialog();
            if(testForm.DialogResult == System.Windows.Forms.DialogResult.Yes)
            {
                SetUserInfo(testForm.UserObj);
            }
        }

        private void SetUserInfo(UserInfo userObj)
        {
            UserObj = userObj;
            UserName.Text = UserObj.UserName;
            Psw.Text = UserObj.Psw;
            DomainName.Text = UserObj.DomainName;
        }

        private void InitSendInfo(SendInfo sendObj)
        {
            SendInfoObj = sendObj;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DomainName.Text = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            HostName.Text = string.Format("{0}.{1}", System.Environment.MachineName, DomainName.Text);
        }
    }

    public class UserInfo
    {
        public string UserName;
        public string Psw;
        public string DomainName;
    }

    public class SendInfo
    {
        public string HostName;
        public int SendInterval;
        public int RecvMaxCount;
        public int SendCount;
    }

    public class MessageFactory
    {
        private List<string> AllMailAddress = new List<string>(20);
        private List<string> AllMailName = new List<string>(20);
        private List<int> RandomList = new List<int>(20);
        private HashSet<int> EqualList = new HashSet<int>();
        private int _recvMaxCount;

        public MessageFactory(ICollection<string> candidateNames, string domainName, int recvMaxCount)
        {
            AllMailName.AddRange(candidateNames);
            StringBuilder sb = new StringBuilder();
            int i = 0;

            _recvMaxCount = recvMaxCount;
            int step = candidateNames.Count / 4;
            if (step != 0)
            {
                foreach (string name in candidateNames)
                {
                    sb.Length = 0;
                    sb.Append(name).Append("@").Append(domainName);
                    AllMailAddress.Add(sb.ToString());
                    if (i % step == 0 && i != 0)
                    {
                        EqualList.Add(i);
                    }
                    RandomList.Add(i++);
                }
            }
           
        }

        public System.Net.Mail.MailMessage CreateMailMessage()
        {
            var recvAddress = GetReceiveAddress();
            var sendAddress = GetSendAddress();
            var subject = GetSubject(sendAddress, recvAddress.Count);
            var body = GetBody(sendAddress, recvAddress);
            System.Net.Mail.MailMessage message = CreateMailMessage(recvAddress,sendAddress,subject,body);
            return message;
        }

        public System.Net.Mail.MailMessage CreateMailMessage(string attachmentPath)
        {
            System.Net.Mail.MailMessage message = CreateMailMessage();
            FileStream reader = new FileStream(attachmentPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            string fileName = Path.GetFileName(attachmentPath);
            System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(reader, fileName);
            message.Attachments.Add(attachment);
            return message;
        }

        private int GetRandom()
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            return random.Next(1, AllMailAddress.Count);
        }

        private string GetSendAddress()
        {
            return AllMailAddress[GetRandom() - 1];
        }

        private List<int> GetRandomArray()
        {
            int count = GetRandom();

            DealRandomList(count);

            List<int> result = new List<int>(count);
            int mailsCount = AllMailAddress.Count;
            for (int i = 0; i < count; i++)
            {
                result.Add(RandomList[i]);
            }
            if (result.Count == 0)
                result.Add(mailsCount - 1);

            //Debug.Write("Count:");
            //Debug.Write(string.Format("{0,2} RandomList:", count));

            //foreach (var i in RandomList)
            //{
            //    Debug.Write(string.Format("{0,3}", i));
            //}

            //Debug.Write("  Result:");
            result.Sort();
            //foreach (var i in result)
            //{
            //    Debug.Write(string.Format("{0,3}", i));
            //    Debug.Write(" ");
            //}
            //Debug.WriteLine(" ");

            return result;
        }

        private void DealRandomList(int count)
        {
            int last = RandomList.Count - 1;
            int leftMid = RandomList.Count / 2 - 2;
            int rightMid = RandomList.Count / 2 + 1;
            int first = 0;

            int temp = 0;
            int exchangeIndex = count / 2;
            while (exchangeIndex != 0)
            {
                temp = RandomList[exchangeIndex];
                RandomList[exchangeIndex] = RandomList[count - 1];
                RandomList[count - 1] = temp;
                exchangeIndex = exchangeIndex / 2;
            }

            RandomList.Reverse();

            if (EqualList.Contains(count))
            {
                if (leftMid >= 0 && rightMid <= last)
                {
                    temp = RandomList[first];
                    RandomList[first] = RandomList[leftMid];
                    RandomList[leftMid] = temp;

                    temp = RandomList[last];
                    RandomList[last] = RandomList[rightMid];
                    RandomList[rightMid] = temp;
                }
            }
        }

        private List<string> GetReceiveAddress()
        {
            var resultIndexes = GetRandomArray();
            List<string> result = new List<string>(resultIndexes.Count);
            int index = 0;
            foreach (var i in resultIndexes)
            {
                result.Add(AllMailAddress[i]);
                index++;
                if (index >= _recvMaxCount)
                    break;
            }
            return result;
        }

        StringBuilder sbTemp = new StringBuilder();
        private string GetSubject(string sender, int recCount)
        {
            sbTemp.Length = 0;
            sbTemp.Append("from ").Append(sender).Append(" recvCount:").Append(recCount).Append(" ").Append(DateTime.Now.ToString("yyyyMMddHHmmss"));
            return sbTemp.ToString();
        }

        private string GetBody(string sender, List<string> recv)
        {
            sbTemp.Length = 0;
            sbTemp.Append("from ").Append(sender).Append(" recvCount:").Append(recv.Count).Append(" ").Append(DateTime.Now.ToString("yyyyMMddHHmmss")).AppendLine();
            foreach (string rec in recv)
            {
                sbTemp.AppendLine(rec);
            }
            return sbTemp.ToString();
        }

        private static System.Net.Mail.MailMessage CreateMailMessage(ICollection<string> recvAddresses, string senderName, string subject, string body)
        {
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            foreach (var recv in recvAddresses)
            {
                msg.To.Add(recv);
            }

            msg.From = new MailAddress(senderName);
            msg.Subject = subject;
            msg.Body = body;

            
            return msg;
        }
    }
}
