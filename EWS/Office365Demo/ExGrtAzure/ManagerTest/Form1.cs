using EwsFrame;
using EwsFrame.Manager.IF;
using EwsFrame.Manager.Impl;
using LogInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagerTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Thread.CurrentThread.Name = "MainGUI";
            InitializeComponent();
            workThread = new Thread(workfunc);
            workThread.Start();
        }

        Thread workThread ;
        AutoResetEvent startEvent = new AutoResetEvent(false);
        AutoResetEvent endEvent = new AutoResetEvent(false);

        private void workfunc()
        {
            startEvent.WaitOne();
            LogFactory.LogInstance.RegisterLogStream(new TestboxStreamProvider(this));
            JobFactoryServer.OnStart();
            JobFactoryServer.Instance.ProgressManager.NewProgressEvent += ProgressManager_NewProgressEvent;

            endEvent.WaitOne();
            JobFactoryServer.OnStop();
        }

        private List<TestJob> _allJobs = new List<TestJob>();

        private void ProgressManager_NewProgressEvent(object sender, EwsFrame.Manager.Data.ProgressArgs e)
        {
            UpdateListbox();
            TestProgressInfo info = e.ProgressInfo as TestProgressInfo;
            AppendToTextbox1(string.Format("{0}\t{1}\t{2}\t{3}/{4}\r\n", info.Time, info.Job.JobId, info.Job.Status, info.CurrentStep, info.TotalStep));
        }


        bool EnsureMainGuiThread()
        {
            if (Thread.CurrentThread.Name != "MainGUI")
            {
                return false;
            }
            return true;
        }

        public void UpdateListbox()
        {
            if (EnsureMainGuiThread())
            {
                UpdateList(_allJobs);
                return;
            }
            
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action(UpdateListbox));
                return;
            }

            UpdateList(_allJobs);
        }

        private void UpdateList(List<TestJob> list)
        {
            JobList.Items.Clear();
            foreach(var item in list)
            {
                JobList.Items.Add(item);
            }
        }

        public void AddListbox(TestJob job)
        {
            _allJobs.Add(job);
            UpdateListbox();
        }

        public void AppendToTextbox1(string text)
        {
            if (EnsureMainGuiThread())
            {
                textBox1.AppendText(text);
                return;
            }
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendToTextbox1), new object[] { text });
                return;
            }
            
            textBox1.AppendText(text);
        }
        

        private void BtnAddJob_Click(object sender, EventArgs e)
        {
            TestJob job = new TestJob();
            
            JobFactoryServer.Instance.JobManager.AddJob(job);
            AddListbox(job);
            job.JobStatusChangedEvent += Job_JobStatusChangedEvent;
        }

        private void Job_JobStatusChangedEvent(object sender, JobStatusChangedEventArgs e)
        {
            UpdateListbox();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            startEvent.Set();
        }

        private void BtnEnd_Click(object sender, EventArgs e)
        {
            endEvent.Set();
        }

        private void BtnCancelJob_Click(object sender, EventArgs e)
        {
            IArcJob job = (TestJob)JobList.SelectedItem;
            job = JobFactoryServer.Instance.JobManager.GetJob(job.JobId);
            job.CancelJob();
        }

        public class TestboxStreamProvider : ILogStreamProvider
        {
            private Form1 _form;
            private Guid _streamId = Guid.NewGuid();
            public TestboxStreamProvider(Form1 form)
            {
                _form = form;
            }

            public Guid StreamId
            {
                get
                {
                    return _streamId;
                }
            }

            public object SyncObj
            {
                get
                {
                    return _form;
                }
            }

            public string GetTotalLog(DateTime date)
            {
                return _form.textBox1.Text;
            }

            public void Write(string information)
            {
                Debug.Write(information);
                _form.AppendToTextbox1(information + "\r\n");
            }

            public void WriteLine(string information)
            {
                Debug.WriteLine(information);
                _form.AppendToTextbox1(information + "\r\n");
            }
        }

        private void AddLine_Click(object sender, EventArgs e)
        {
            AppendToTextbox1("\r\n");
        }
    }
}
