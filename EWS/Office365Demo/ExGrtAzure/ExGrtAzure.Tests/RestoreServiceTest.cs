using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EwsFrame;
using DataProtectInterface;
using Microsoft.Exchange.WebServices.Data;
using System.IO;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class RestoreServiceTest
    {
        private static RestoreFactory _factory;
        private static IRestoreService _service;
        private static ExchangeService _ewsContext;

        [ClassInitialize]
        public static void TestInit(TestContext context)
        {
            //_factory = GetCatalogFactory();
            //_service = GetRestoreService();

            //var mailboxOper = _factory.NewMailboxOperatorImpl();
            //ServiceContext.ContextInstance.CurrentMailbox = "haiyang.ling@arcserve.com";
            //mailboxOper.ConnectMailbox(ServiceContext.ContextInstance.Argument, "haiyang.ling@arcserve.com");
            //_ewsContext = mailboxOper.CurrentExchangeService;
        }

        public static RestoreFactory GetCatalogFactory()
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            directory = Path.Combine(directory, "..\\..\\..\\lib");
            RestoreFactory.LibPath = directory;
            RestoreFactory factory = RestoreFactory.Instance;

            return factory;
        }

        public static IRestoreService GetRestoreService()
        {
            var service = _factory.NewRestoreService("devO365admin@arcservemail.onmicrosoft.com", "arcserve1!", null, "Arcserve");
            return service;
        }

        [TestMethod]
        public void TestRestoreMailbox()
        {
            //IRestoreService service = GetRestoreService();
            //using (IRestoreDestination destination = _factory.NewRestoreDestination(service.ServiceContext.Argument, _service.ServiceContext.DataAccessObj))
            //{
            //    service.Destination = destination;
            //    destination.InitOtherInformation("haiyang.ling@arcserve.com", "Restore1");

            //    IQueryCatalogDataAccess dataAccess = (IQueryCatalogDataAccess)ServiceContext.GetDataAccessInstance(TaskType.Restore, service.ServiceContext.Argument, "Arcserve");
            //    var allJob = dataAccess.GetAllCatalogJob();
            //    dataAccess.CatalogJob = allJob[0];
            //    service.CurrentRestoreCatalogJob = allJob[0];
            //    service.RestoreMailbox("haiyang.ling@arcserve.com");
            //}
        }

        public void TestRestoreFolder()
        {

        }

        public void TestRestoreMail()
        {

        }
    }
}
