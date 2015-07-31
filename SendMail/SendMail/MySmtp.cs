using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace SendMail
{
    public class SmtpFactory : IDisposable
    {
        private Dictionary<int, bool> AllSmtpClientStatus = new Dictionary<int, bool>(20);
        private List<MySmtp> AllSmtpClient = new List<MySmtp>(20);
        private object _lockObje = new object();

        public void SendException(MySmtp smtp)
        {
            lock (_lockObje)
            {
                AllSmtpClientStatus[smtp.Index] = false;
                Debug.WriteLine(" ");
                Debug.Write("Index:");
                Debug.Write(smtp.Index);
                Debug.WriteLine(" release by Exception.");
            }
        }

        internal void DisableSmtp(int smtpIndex)
        {
            lock (_lockObje)
            {
                AllSmtpClientStatus[smtpIndex] = true;
                AllSmtpClient[smtpIndex].Dispose();
                AllSmtpClient[smtpIndex] = null;
            }
        }

        public MySmtp GetInstance(string hostName, string userName, string psw, string domainName, SendCompletedEventHandler completeHandler, out int index)
        {
            lock (_lockObje)
            {
                Debug.Write("CurrentSmtpCount:");
                Debug.WriteLine(AllSmtpClient.Count);
                foreach (var keyValue in AllSmtpClientStatus)
                {
                    if (!keyValue.Value)
                    {
                        AllSmtpClientStatus[keyValue.Key] = true;
                        AllSmtpClient[keyValue.Key].CompletedHandler = completeHandler;

                        Debug.WriteLine(" ");
                        Debug.Write("Index:");
                        Debug.Write(keyValue.Key);
                        Debug.WriteLine(" use.");
                        index = keyValue.Key;
                        return AllSmtpClient[keyValue.Key];
                    }
                }


                MySmtp client = new MySmtp(hostName, userName, psw, domainName, AllSmtpClient.Count);
                client.EventSendCompleted += client_SendCompleted;
                AllSmtpClient.Add(client);
                AllSmtpClientStatus.Add(client.Index, true);

                Debug.WriteLine(" ");
                Debug.Write("Index:");
                Debug.Write(client.Index);
                Debug.WriteLine(" create and use.");
                index = client.Index;
                client.CompletedHandler = completeHandler;
                
                return client;
            }
        }

        public int GetCount()
        {
            return AllSmtpClient.Count;
        }

        void client_SendCompleted(object sender, MySmtpArgs e)
        {
            lock (_lockObje)
            {
                AllSmtpClientStatus[e.Index] = false;
                Debug.WriteLine(" ");
                Debug.Write("Index:");
                Debug.Write(e.Index);
                Debug.WriteLine(" release.");
            }
        }

        public void Dispose()
        {
            while (true)
            {
                Thread.Sleep(1000);
                bool isStillRun = false;
                foreach (var keyValue in AllSmtpClientStatus)
                {
                    if (keyValue.Value)
                    {
                        isStillRun = true;
                        break;
                    }
                }
                if (!isStillRun)
                {
                    foreach (var item in AllSmtpClient)
                    {
                        item.Dispose();
                    }
                    break;
                }
            }
        }

        
    }

    public delegate void SendCompleteDel(object sender, MySmtpArgs e);
    public class MySmtp : IDisposable
    {
        private SmtpClient _client;

        private string _name;
        private int _index;
        public int Index { get { return _index; } }
        public SendCompletedEventHandler CompletedHandler;
        public MySmtp(string hostName, string userName, string psw, string domainName, int index)
        {
            _client = new SmtpClient();
            _client.Host = hostName;
            _client.Port = 587;
            _client.EnableSsl = true;
            _client.UseDefaultCredentials = false;
            _client.Credentials = new System.Net.NetworkCredential(userName, psw, domainName);

            _client.SendCompleted += _client_SendCompleted;
            _index = index;
        }

        public void SendAsync(MailMessage mail, object token)
        {
            _client.SendAsync(mail, token);
        }

        void _client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (CompletedHandler != null)
                CompletedHandler(sender, e);
            if (EventSendCompleted != null)
            {
                EventSendCompleted.Invoke(sender, new MySmtpArgs() { OrginalArg = e, Index = _index });

            }

        }

        public event SendCompleteDel EventSendCompleted;

        public void Dispose()
        {
            _client.Dispose();
        }
    }

    public class MySmtpArgs
    {
        public AsyncCompletedEventArgs OrginalArg;
        public int Index;
    }
}
