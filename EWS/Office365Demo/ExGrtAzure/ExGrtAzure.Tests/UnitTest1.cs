using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EwsFrame;
using System.Threading;
using System.Reflection;
using DataProtectImpl;
using EwsDataInterface;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class UnitTest1
    {
        //[TestMethod]
        //public void TestFolderContainerMappingSerialize()
        //{
        //    List<FolderContainerMapping> list = new List<FolderContainerMapping>(2);

        //    FolderContainerMapping map1 = new FolderContainerMapping();
        //    map1.ContainerInfo = new ContainerCount();
        //    map1.ContainerInfo.ContainerName = "container1";
        //    map1.ContainerInfo.UsedCount = 1;
        //    map1.FolderId = "1234567";
        //    list.Add(map1);

        //    map1 = new FolderContainerMapping();
        //    map1.ContainerInfo = new ContainerCount();
        //    map1.ContainerInfo.ContainerName = "container2";
        //    map1.ContainerInfo.UsedCount = 1;
        //    map1.FolderId = "8765432";
        //    list.Add(map1);

        //    byte[] buffer;
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        FolderContainerMapping.SerializeList(list, stream);
        //        buffer = new byte[stream.Length];
        //        stream.Seek(0, SeekOrigin.Begin);
        //        int readCount = stream.Read(buffer, 0, (int)stream.Length);

        //    }

        //    List<FolderContainerMapping> result1 = null;
        //    using (MemoryStream readStream = new MemoryStream(buffer))
        //    {
        //        result1 = FolderContainerMapping.DeSerializeList(readStream);
        //    }


        //}

        //[TestMethod]
        //public void TestGetBlobCount()
        //{
        //    int size11 = 10 * 4 * 1024 * 1024 + 1;
        //    int size10 = 10 * 4 * 1024 * 1024 - 1;
        //    int size = BlobDataAccess.GetBlobCount(size11);
        //    Assert.AreEqual(size, 11);
        //    size = BlobDataAccess.GetBlobCount(size10);
        //    Assert.AreEqual(size, 10);
        //}

        [TestMethod]
        public void TestMD5()
        {
            string testStr = "2345asabasdfewtqgasdfwertqgasdgqwetbasdweqgasdfaweqegsdgasdgwegasgqgtasdfgasdgaweqfasdgfasdfwqgerhtykukotjeu69i87okke56";
            StringBuilder sBuilder = new StringBuilder();
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(testStr));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.


                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
            }

            Thread.Sleep(1000);
            string result = MD5Utility.ConvertToMd5(testStr);
            string result2 = sBuilder.ToString();
            Assert.AreEqual(result, result2);

        }

        //[TestMethod]
        public void ResetBlobData()
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            directory = Path.Combine(directory, "..\\..\\..\\lib");
            CatalogFactory.LibPath = directory;
            ServiceContext context = new ServiceContext("haiyang.ling@arcserve.com", "", "", "Arcserve", DataProtectInterface.TaskType.Catalog);
            context.CurrentContext.CurrentMailbox = "haiyang.ling@arcserve.com";
            var dataAccess = CatalogFactory.Instance.NewCatalogDataAccess();
            dataAccess.ResetAllStorage();
            dataAccess.ResetAllStorage("Arcserve");
        }

        [TestMethod]
        public void TestTree()
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            directory = Path.Combine(directory, "..\\..\\..\\lib");
            var dataProtectPath = Path.Combine(directory, "DataProtectImpl.dll");
            var DataProtectImplAssembly = Assembly.LoadFrom(dataProtectPath);
            Type type = DataProtectImplAssembly.GetType("DataProtectImpl.RestoreServiceBase.TreeNode");

            var rootFolderId = "1";
            var folderLevel11 = new FolderData() { ParentFolderId = rootFolderId, FolderId = "11", DisplayName = "Display-11" };
            var folderLevel111 = new FolderData() { ParentFolderId = folderLevel11.FolderId, FolderId = "111", DisplayName = "Display-111" };
            var folderLevel112 = new FolderData() { ParentFolderId = folderLevel11.FolderId, FolderId = "112", DisplayName = "Display-112" };
            var folderLevel113 = new FolderData() { ParentFolderId = folderLevel11.FolderId, FolderId = "113", DisplayName = "Display-113" };
            var folderLevel1121 = new FolderData() { ParentFolderId = folderLevel112.FolderId, FolderId = "1121", DisplayName = "Display-1121" };
            var folderLevel1131 = new FolderData() { ParentFolderId = folderLevel113.FolderId, FolderId = "1131", DisplayName = "Display-1131" };
            var folderLevel1132 = new FolderData() { ParentFolderId = folderLevel113.FolderId, FolderId = "1132", DisplayName = "Display-1132" };
            var folderLevel1133 = new FolderData() { ParentFolderId = folderLevel113.FolderId, FolderId = "1133", DisplayName = "Display-1133" };
            var folderLevel12 = new FolderData() { ParentFolderId = rootFolderId, FolderId = "12", DisplayName = "Display-12" };
            var folderLevel121 = new FolderData() { ParentFolderId = folderLevel12.FolderId, FolderId = "121", DisplayName = "Display-121" };
            var folderLevel122 = new FolderData() { ParentFolderId = folderLevel12.FolderId, FolderId = "122", DisplayName = "Display-122" };
            var folderLevel1211 = new FolderData() { ParentFolderId = folderLevel121.FolderId, FolderId = "1211", DisplayName = "Display-1211" };
            var folderLevel1212 = new FolderData() { ParentFolderId = folderLevel121.FolderId, FolderId = "1212", DisplayName = "Display-1212" };

            var folderLevel1212_Path = new List<string>() { folderLevel12.DisplayName, folderLevel121.DisplayName, folderLevel1212.DisplayName };
            var folderLevel11_Path = new List<string>() { folderLevel11.DisplayName };

            var folderLevel12_AllChildFolderId = new List<string>() {
                folderLevel12.FolderId,
                folderLevel121.FolderId,
                folderLevel122.FolderId,
                folderLevel1211.FolderId
            , folderLevel1212.FolderId};

            List<IFolderData> allFolder = new List<IFolderData>()
            {
                 folderLevel11,
                 folderLevel111,
                 folderLevel112 ,
                 folderLevel113 ,
                 folderLevel1121 ,
                 folderLevel1131,
                 folderLevel1132,
                 folderLevel1133 ,
                 folderLevel12,
                 folderLevel121,
                 folderLevel122 ,
                 folderLevel1211,
                 folderLevel1212
            };

            var rootNode = RestoreServiceBase.TreeNode.CreateTree(allFolder);
            List<string> allFolderIds = RestoreServiceBase.TreeNode.GetAllFoldersAndChildFolders(rootNode, "12");

            var dic = RestoreServiceBase.TreeNode.GetEachFolderPath(rootNode);

            var path1212 = dic["1212"];
            var path11 = dic["11"];

            AssertList(allFolderIds, folderLevel12_AllChildFolderId, false);
            AssertList(path1212, folderLevel1212_Path, true);
            AssertList(path11, folderLevel11_Path, true);
        }

        private void AssertList(List<string> left1, List<string> right1, bool isEqualByOrder)
        {
            var left = new List<string>(left1);
            var right = new List<string>(right1);
            Assert.AreEqual(left.Count, right.Count);
            if(isEqualByOrder)
            {
                for(int i = 0; i < right.Count; i++)
                {
                    Assert.AreEqual(left[i], right[i]);
                }
            }
            else
            {
                for(int i = 0; i < left.Count; i++)
                {
                    int j = right.IndexOf(left[i]);
                    Assert.IsTrue(j >= 0);
                    right.RemoveAt(j);
                }
            }
        }

        class FolderData : IFolderData
        {
            public int ChildFolderCount
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public int ChildItemCount
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public string DisplayName
            {
                get; set;
            }

            public string FolderId
            {
                get; set;
            }

            public string FolderType
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public string Location
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public string MailboxAddress
            {
                get; set;
            }

            public string ParentFolderId
            {
                get; set;
            }
        }
    }
}
