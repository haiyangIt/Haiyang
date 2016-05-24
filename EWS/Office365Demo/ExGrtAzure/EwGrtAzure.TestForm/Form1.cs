using DataProtectInterface;
using EwsFrame;
using EwsFrame.Util;
using FTStreamUtil;
using Newtonsoft.Json;
using SqlDbImpl;
using SqlDbImpl.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EwGrtAzure.TestForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            if (!FactoryBase.IsRunningOnAzure())
            {
                string logFolder = ConfigurationManager.AppSettings["LogPath"];
                var logPath = Path.Combine(logFolder, string.Format("{0}Trace.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")));
                Trace.Listeners.Add(new TextWriterTraceListener(logPath));
            }
            InitializeComponent();
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            Thread.CurrentThread.Name = "MainGUI";
            LogFactory.LogInstance.WriteLogMsgEvent += LogInstance_WriteLogMsgEvent;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try {
                if (JobProgressManager.Instance.Count > 1)
                    return;
                this.txtLog.Text = "";
                AppendToTextbox1("Clear Environment.");
                BeforeBackup();
                TestCatalog(@"AllSelectItems.txt");
                AfterBackup();
            }
            catch(Exception ex)
            {
                AppendToTextbox1(ex.Message + " \r\n " + ex.StackTrace);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (JobProgressManager.Instance.Count > 1)
                return;
            this.txtLog.Text = "";
            AppendToTextbox1("Clear Environment.");
            BeforeBackup();
            TestCatalog(@"PartSelectItems.txt");
            AfterBackup();
        }

        private void BeforeBackup()
        {
            
            IServiceContext serviceContext = ServiceContext.NewServiceContext(txtAdminUser.Text, "", "", txtOrg.Text, DataProtectInterface.TaskType.Catalog);
            serviceContext.CurrentMailbox = txtAdminUser.Text;
            var dataAccess = CatalogFactory.Instance.NewCatalogDataAccessInternal(serviceContext.Argument, serviceContext.AdminInfo.OrganizationName);
            dataAccess.ResetAllStorage(serviceContext.CurrentMailbox);

            if (!FactoryBase.IsRunningOnAzureOrStorageInAzure())
            {

                using (CatalogDbContext dbContext = new CatalogDbContext(new SqlDbImpl.Model.OrganizationModel() { Name = txtOrg.Text }))
                {
                    AppendToTextbox1(dbContext.Database.Connection.ConnectionString);
                    dbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE CatalogInformation");
                    dbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE MailboxInformation");
                    dbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE ItemLocation");
                    dbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE ItemInformation");
                    dbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE FolderInformation");
                    dbContext.SaveChanges();
                }
            }

            
            
        }

        private void LogInstance_WriteLogMsgEvent(object sender, string e)
        {
            AppendToTextbox1(e);
        }

        public void AppendToTextbox1(string text)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendToTextbox1), new object[] { text });
                return;
            }

            using (_wait.LockWhile(() =>
             {
                 //if (txtLog.Lines.Length > 500)
                 //{
                 //    string[] newLines = new string[500];
                 //    Array.Copy(txtLog.Lines, 1, newLines, 0, 500);
                 //    txtLog.Lines = newLines;
                 //}
                 txtLog.AppendText(text + "\r\n");
             }))
            { }
        }

        bool EnsureMainGuiThread()
        {
            if (Thread.CurrentThread.Name != "MainGUI")
            {
                return false;
            }
            return true;
        }

        private void AfterBackup()
        {
        }

        private void TestCatalog(string file)
        {
            var service = CatalogFactory.Instance.NewCatalogService(txtAdminUser.Text, txtAdminPsw.Text, "", txtOrg.Text);
            var partSelect = "";
            using (StreamReader reader = new StreamReader(file))
            {
                partSelect = reader.ReadToEnd();
            }
            LoadedTreeItem selectedItem = JsonConvert.DeserializeObject<LoadedTreeItem>(partSelect);
            service.CatalogJobName = string.Format("Test Catalog job {0}", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
            IFilterItem filterObj = CatalogFactory.Instance.NewFilterItemBySelectTree(selectedItem);
            var serviceId = Guid.NewGuid();
            JobProgressManager.Instance[serviceId] = (IDataProtectProgress)service;
            ThreadPool.QueueUserWorkItem(ThreadBackup, new CatalogArgs() { Service = service, FilterItem = filterObj });
        }

        private static void ThreadBackup(object arg)
        {
            var argument = arg as CatalogArgs;
            try
            {
                using (argument.Service)
                    argument.Service.GenerateCatalog(argument.FilterItem);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                LogFactory.LogInstance.WriteException(LogInterface.LogLevel.ERR, "Backup job failed.", e, e.Message);
            }
            finally
            {
                JobProgressManager.Instance.Clear();
            }

        }

        class CatalogArgs
        {
            public ICatalogService Service;
            public IFilterItem FilterItem;
        }

        private static AutoResetEvent _wait = new AutoResetEvent(false);
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogFactory.LogInstance.Dispose();
            LogFactory.EwsTraceLogInstance.Dispose();
            EwsRequestGate.Instance.Dispose();
            _wait.Dispose();
        }
        

        private void button5_Click(object sender, EventArgs e)
        {
            if (JobProgressManager.Instance.Count > 1)
                return;
            this.txtLog.Text = "";
            AppendToTextbox1("Clear Environment.");
            BeforeBackup();
            TestCatalog(@"FourMailboxPartial.txt");
            AfterBackup();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (JobProgressManager.Instance.Count > 1)
                return;
            this.txtLog.Text = "";
            AppendToTextbox1("Clear Environment.");
            BeforeBackup();
            TestCatalog(@"FourMailboxAll.txt");
            AfterBackup();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            HashSet<string> filePaths = new HashSet<string>();
            ReadLogs((line, filePath, lineIndex) =>
            {
                if (line.IndexOf("\tE\t") >= 0)
                {
                    if (!filePaths.Contains(filePath))
                    {
                        AppendToTextbox1(filePath);
                        filePaths.Add(filePath);
                    }

                    AppendToTextbox1(string.Format("[{0,4}] {1}", lineIndex, line));
                }
            });
        }

        private void ReadLogs(Action<string, string, int> findOperator)
        {
            var files = GetTraceFiles();

            foreach (var filePath in files)
            {

                int lineIndex = 0;
                using (StreamReader reader = new StreamReader(filePath))
                {
                    do
                    {
                        lineIndex++;
                        var line = reader.ReadLine();
                        findOperator(line, filePath, lineIndex);
                    } while (!reader.EndOfStream);
                }
            }
        }

        private ICollection<string> GetTraceFiles()
        {
            string folder = @"D:\21GitHub\Haiyang\EWS\Office365Demo\ExGrtAzure\EwGrtAzure.TestForm\bin\Debug\FourLogs";
            return Directory.EnumerateFiles(folder).ToList();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (JobProgressManager.Instance.Count > 1)
                return;
            this.txtLog.Text = "";
            AppendToTextbox1("Clear Environment.");
            BeforeBackup();
            TestCatalog(txtFile.Text);
            AfterBackup();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            BeforeBackup();
        }

        private void btnSwitchCount_Click(object sender, EventArgs e)
        {
            txtAdminUser.Text = "ArcserveJacky@ArcserveJacky.onmicrosoft.com";
            txtAdminPsw.Text = "Arcserve1!";
            txtOrg.Text = "ArcserveJacky";
        }
    }
}
