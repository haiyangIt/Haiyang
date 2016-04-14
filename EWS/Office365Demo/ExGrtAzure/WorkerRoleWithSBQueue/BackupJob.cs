using DataProtectInterface;
using DataProtectInterface.Plan;
using EwsFrame;
using EwsFrame.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;
using Newtonsoft.Json;
using DataProtectInterface.Event;
using EwsFrame.Manager.Impl;
using EwsFrame.Manager.IF;
using EwsFrame.ServiceBus;
using EwsFrame.Manager.Data;

namespace WorkerRoleWithSBQueue
{
    public class BackupJob : ArcJobBase
    {
        public BackupJob() : base()
        {

        }

        private readonly IPlanData PlanData;
        //private readonly List<IPlanMailInfo> PlanMailInfo;
        private readonly DateTime StartTime;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobParam">
        /// 3 params:
        /// a. key:planBaseInfo
        /// b. key:planMailInfo
        /// c. key:planJobStartTime
        /// </param>
        public BackupJob(IDictionary<string, object> jobParam) : base()
        {
            PlanData = jobParam["planBaseInfo"] as IPlanData;
            //PlanMailInfo = jobParam["planMailInfo"] as List<IPlanMailInfo>;
            string timeStr = jobParam["planJobStartTime"] as string;
            StartTime = new DateTime(Convert.ToInt64(timeStr));
        }

        public override ArcJobType JobType
        {
            get
            {
                return ArcJobType.Backup;
            }
        }

        public string Organization
        {
            get { return PlanData.Organization; }
        }

        private ICatalogService _backupService;

        protected override void InternalRun()
        {
            var adminInfo = PlanUtil.GetAdminInfo(PlanData.CredentialInfo);
            _backupService = CatalogFactory.Instance.NewCatalogService(adminInfo.UserName, adminInfo.UserPassword, null, adminInfo.OrganizationName);
            _backupService.CatalogJobName = string.Format("Backup job {0} of plan {1}", StartTime.ToString("yyyyMMddHHmmss"), PlanData.Name);

            LoadedTreeItem selectedItem = JsonConvert.DeserializeObject<LoadedTreeItem>(PlanData.PlanMailInfos);
            IFilterItem filterObj = CatalogFactory.Instance.NewFilterItemBySelectTree(selectedItem);
            _backupService.ProgressChanged += Service_ProgressChanged;
            _backupService.GenerateCatalog(filterObj);
        }

        private void Service_ProgressChanged(object sender, DataProtectInterface.Event.CatalogProgressArgs e)
        {
            var progressInfo = new BackupProgressInfo(e);
            JobFactory.Instance.ProgressManager.AddProgress(progressInfo);
        }
    }

    public class FilterByPlanMailInfo : IFilterItemWithMailbox
    {
        public FilterByPlanMailInfo(IList<IPlanMailInfo> mailInfos)
        {

        }

        public List<IMailboxData> GetAllMailbox()
        {
            throw new NotImplementedException();
        }

        public int GetFolderCount()
        {
            throw new NotImplementedException();
        }

        public bool IsFilterFolder(IFolderData currentFolder, IMailboxData mailbox, Stack<IFolderData> folders)
        {
            throw new NotImplementedException();
        }

        public bool IsFilterItem(IItemData item, IMailboxData mailbox, Stack<IFolderData> folders)
        {
            throw new NotImplementedException();
        }

        public bool IsFilterMailbox(IMailboxData mailbox)
        {
            throw new NotImplementedException();
        }
    }

}
