using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EwsFrame;
using DataProtectInterface;
using System.IO;
using Microsoft.Exchange.WebServices.Data;
using EwsServiceInterface;
using System.Diagnostics;
using EwsDataInterface;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class CatalogServiceTest
    {
        private static CatalogFactory _factory;
        private static ICatalogService _service;
        private static ExchangeService _ewsContext;

        [ClassInitialize]
        public static void TestInit(TestContext context)
        {
            _factory = GetCatalogFactory();
            _service = GetCatalogService();

            var mailboxOper = _factory.NewMailboxOperatorImpl();
            ServiceContext.ContextInstance.CurrentMailbox = "haiyang.ling@arcserve.com";
            mailboxOper.ConnectMailbox(ServiceContext.ContextInstance.Argument, "haiyang.ling@arcserve.com");
            _ewsContext = mailboxOper.CurrentExchangeService;
        }

        public static CatalogFactory GetCatalogFactory()
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            directory = Path.Combine(directory, "..\\..\\..\\lib");
            CatalogFactory.LibPath = directory;
            CatalogFactory factory = CatalogFactory.Instance;

            return factory;
        }

        public static ICatalogService GetCatalogService()
        {
            var service = _factory.NewCatalogService("devO365admin@arcservemail.onmicrosoft.com", "arcserve1!", null, "Arcserve");
            return service;
        }

        [TestMethod]
        [Description("Test GetAllMailboxes")]
        public void TestGetAllMailbox()
        {
            var result = _service.GetAllUserMailbox();

            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].MailAddress.ToLower(), "haiyang.ling@arcserve.com");
        }

        [TestMethod]
        [Description("Test ConnectMailbox")]
        public void TestConnectMailbox()
        {
            Assert.AreNotEqual(_ewsContext, null);
        }

        [TestMethod]
        [Description("Test folder operation include GetChildFolder GetRootFolder.")]
        public void TestFolderOperator()
        {
            var folder = _factory.NewFolderOperatorImpl(_ewsContext);
            Folder rootFolder = folder.GetRootFolder();
            IDataConvert dataConvert = _factory.NewDataConvert();
            dataConvert.StartTime = DateTime.Now;
            dataConvert.OrganizationName = ServiceContext.ContextInstance.AdminInfo.OrganizationName;
            GetChildFolder(dataConvert, folder, rootFolder, 1);
        }

        private void GetChildFolder(IDataConvert dataConvert, IFolder folder, Folder parentFolder, int level)
        {
            IFolderData folderData = dataConvert.Convert(parentFolder);

            OutputFolderData(folderData, level);
            if (parentFolder.ChildFolderCount == 0)
                Debug.WriteLine(string.Format("{0} {1} doesn't have any child folder", "".PadLeft(level * 2), parentFolder.DisplayName));

            var childFolders = folder.GetChildFolder(parentFolder);
            foreach (Folder childFolder in childFolders)
            {
                GetChildFolder(dataConvert, folder, childFolder, level + 1);
            }
        }

        private void OutputFolderData(IFolderData folderData, int level)
        {
            Debug.WriteLine(string.Format("{0} Folder: [{1}],type: [{2}],ChildFolderCount:[{3}],ChildItemCount:[{4}],Id:[{5}],ParentId:[{6}]",
                "".PadLeft(level * 2), ((IItemBase)folderData).DisplayName,
                folderData.FolderType,
                folderData.ChildFolderCount,
                folderData.ChildItemCount,
                folderData.FolderId,
                folderData.ParentFolderId));
        }

        [TestMethod]
        [Description("Test Item Operation")]
        public void TestItemOperator()
        {
            IItem itemOper = _factory.NewItemOperatorImpl(_ewsContext);
            Folder inboxFolder = Folder.Bind(_ewsContext, WellKnownFolderName.SentItems);
            var items = itemOper.GetFolderItems(inboxFolder);

            IDataConvert dataConvert = _factory.NewDataConvert();
            dataConvert.StartTime = DateTime.Now;
            dataConvert.OrganizationName = ServiceContext.ContextInstance.AdminInfo.OrganizationName;

            foreach(var item in items)
            {
                var itemData = dataConvert.Convert(item);
            }

            var path = string.Format("{0}.bin", DateTime.Now.ToString("yyyyMMddHHmmss"));
            using (StreamWriter write = new StreamWriter(path))
            {
                itemOper.ExportItem(items[0], write.BaseStream, ServiceContext.ContextInstance.Argument);
            }

            IFolder folderOper = _factory.NewFolderOperatorImpl(_ewsContext);
            var findFolderId = folderOper.FindFolder(new FolderDataBaseDefault() { DisplayName = "Test" }, inboxFolder.Id);
            if(findFolderId != null)
            {
                folderOper.DeleteFolder(findFolderId);
            }

            FolderId testFolderId = folderOper.CreateChildFolder(new FolderDataBaseDefault() { DisplayName = "Test" }, inboxFolder.Id);
            using (StreamReader reader = new StreamReader(path))
            {
                itemOper.ImportItem(testFolderId, reader.BaseStream, ServiceContext.ContextInstance.Argument);
            }
        }

        [TestMethod]
        public void TestGenerateCatalog()
        {
            //_service.GenerateCatalog("haiyang.ling@arcserve.com","Test");
        }

        [TestMethod]
        public void TestClearBlobData()
        {
            
        }
    }
}
