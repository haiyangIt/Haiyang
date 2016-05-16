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
            IServiceContext serviceContext = ServiceContext.NewServiceContext("haiyang.ling@arcserve.com", "", "", "Arcserve", DataProtectInterface.TaskType.Catalog);
            serviceContext.CurrentMailbox = "haiyang.ling@arcserve.com";
            serviceContext.DataAccessObj.ResetAllStorage(serviceContext.CurrentMailbox);

            if (!FactoryBase.IsRunningOnAzureOrStorageInAzure())
            {

                using (CatalogDbContext dbContext = new CatalogDbContext(new SqlDbImpl.Model.OrganizationModel() { Name = "Arcserve" }))
                {
                    dbContext.Folders.RemoveRange(dbContext.Folders);
                    dbContext.ItemLocations.RemoveRange(dbContext.ItemLocations);
                    dbContext.Items.RemoveRange(dbContext.Items);
                    dbContext.Catalogs.RemoveRange(dbContext.Catalogs);
                    dbContext.Mailboxes.RemoveRange(dbContext.Mailboxes);

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
            var service = CatalogFactory.Instance.NewCatalogService("devO365admin@arcservemail.onmicrosoft.com", "JackyMao1!", "", "arcserve");
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

        private void button3_Click(object sender, EventArgs e)
        {
            AppendToTextbox1("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbcccccccccccccccccccccccccccccccccccccccccccc  ");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try {
                using (CatalogDbContext context = new CatalogDbContext(new OrganizationModel() { Name = "Arcserve" }))
                {
                    AppendToTextbox1(context.Database.Connection.ConnectionString);
                }
            }
            catch(Exception ex)
            {
                AppendToTextbox1(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}
