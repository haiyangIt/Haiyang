using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Arcserve.Office365.Exchange.Com;
using Arcserve.Office365.Exchange.Com.Impl;
using System.Diagnostics;

namespace ExGrtAzure.Tests.SyncBackup
{
    [TestClass]
    public class TestCom
    {
        [TestMethod]
        public void TestComForQuery()
        {
            string Catalogname = "D:\\test\\Catalog.sqlite";
            uint nCnt = 0;
            uint totalCount = 0;
            int resultIndex = 0;
            string queryString = "#sortfield#:#subject# #descentant#:#0# #ignorecase#:#1#";

            IOffice365ComFactory factory = new Office365ComFactory();
            var queryCondition = factory.CreateQueryCondition();
            var queryCatalog = factory.CreateQueryCatalog();

            queryCondition.SortField = "subject";
            queryCondition.SearchString = "";
            queryCondition.ContentFilter = ContentFilter.CONTENT_ALL;

            queryCatalog.SetCatalogFile(Catalogname);

            var result = queryCatalog.Query(queryCondition, 0, 1, 0, 25);

            var count = result.QueryCount;
            var firstMailbox = result.GetResult(0);
            for (int i = 0; i < count; i++)
            {
                var mailboxResult = result.GetResult((uint)i);
                Debug.WriteLine(string.Format("[{1}] mailbox:{0}", mailboxResult.DisplayName, i));
                Mailbox(Catalogname, factory, mailboxResult);
            }
        }

        private static void Mailbox(string Catalogname, IOffice365ComFactory factory, IResult mailboxResult)
        {
            var mailboxqueryCondition = factory.CreateQueryCondition();
            var mailboxqueryCatalog = factory.CreateQueryCatalog();

            mailboxqueryCondition.ContentFilter = ContentFilter.CONTENT_FOLDER;
            mailboxqueryCatalog.SetCatalogFile(Catalogname);
            var mailboxresult = mailboxqueryCatalog.Query(mailboxqueryCondition, mailboxResult.HId, mailboxResult.LId, 0, 25);
            var mailboxcount = mailboxresult.QueryCount;
            for (int mailboxIndex = 0; mailboxIndex < mailboxcount; mailboxIndex++)
            {
                var folderResult = mailboxresult.GetResult((uint)mailboxIndex);
                Debug.WriteLine(string.Format("  [{1}] folder:{0}", folderResult.DisplayName, mailboxIndex));
                folder(Catalogname, factory, folderResult);
            }
        }

        private static void folder(string Catalogname, IOffice365ComFactory factory, IResult folderResult)
        {
            var folderqueryCondition = factory.CreateQueryCondition();
            var folderqueryCatalog = factory.CreateQueryCatalog();

            folderqueryCondition.ContentFilter = ContentFilter.CONTENT_MAIL;
            folderqueryCatalog.SetCatalogFile(Catalogname);
            var itemResultArray = folderqueryCatalog.Query(folderqueryCondition, folderResult.HId, folderResult.LId, 0, 25);
            var itemcount = itemResultArray.QueryCount;
            for (int j = 0; j < itemcount; j++)
            {
                var itemResult = itemResultArray.GetResult((uint)j);
                Debug.WriteLine(string.Format("     [{1}] item:{0}", itemResult.DisplayName, j));
            }

            Mailbox(Catalogname, factory, folderResult);
        }
    }
}
