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
using EwsFrame.Util;
using System.Diagnostics;
using DataProtectInterface;
using SqlDbImpl;
using System.IO.Compression;
using System.Net.Mail;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;

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
        public void ExportLog()
        {
            DateTime time = DateTime.Now;
            var yesterday0413 = time.AddDays(-1).Date;
            var str = LogFactory.LogInstance.GetTotalLog(yesterday0413);
            using (StreamWriter writer = new StreamWriter(time.ToString("yyyyMMddHHmmssfff") + "log.txt"))
            {
                writer.Write(str);
            }

            using (StreamWriter writer = new StreamWriter(time.ToString("yyyyMMddHHmmssfff") + "Tracelog.txt"))
            {
                str = LogFactory.EwsTraceLogInstance.GetTotalLog(yesterday0413);
                writer.Write(str);
            }

        }

        [TestMethod]
        public void TestStream()
        {
            using(FileStream stream = new FileStream("a.txt", FileMode.Append))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine("aaa");
                }
            }
        }

        [TestMethod]
        public void FormatString()
        {
            int i = 10;
            string test = string.Format("{0:X8},{0:X4},{0:X2}", i);
        }

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

        [TestMethod]
        public void ResetBlobData()
        {
            //var directory = AppDomain.CurrentDomain.BaseDirectory;
            //directory = Path.Combine(directory, "..\\..\\..\\lib");
            //CatalogFactory.LibPath = directory;
            //IServiceContext context = ServiceContext.NewServiceContext("haiyang.ling@arcserve.com", "", "", "Arcserve", DataProtectInterface.TaskType.Catalog);
            //context.CurrentContext.CurrentMailbox = "haiyang.ling@arcserve.com";
            //var dataAccess = ServiceContext.GetDataAccessInstance(TaskType.Catalog, context.Argument, "Arcserve");
            //dataAccess.ResetAllStorage();
            //dataAccess.ResetAllStorage("Arcserve");
        }

        [TestMethod]
        public void CreateBlob()
        {
            CloudStorageAccount StorageAccount = FactoryBase.GetStorageAccount();

            CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = BlobClient.GetContainerReference("aaaa");
            container.CreateIfNotExists();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("block.log");
            if (!blockBlob.Exists())
            {
                blockBlob.UploadText("ceshiyixia");
            }

            CloudAppendBlob appBlob = container.GetAppendBlobReference("abc.log");

            if (!appBlob.Exists())
            {
                appBlob.CreateOrReplace();
            }

        }

        [TestMethod]
        public void TestSendMail()
        {
            SendMailHelper helper = new SendMailHelper();
            helper.AddDownloadUrl("http://127.0.0.1:10000/devstoreaccount1/b4c939eb-a481-4e0d-bde1-49abd6eafade?sv=2015-02-21&sr=c&sig=9nwy8J0CbhbPEvvhpDVMSijq%2BBteQqdX8hQYQy9cnQ4%3D&st=2015-12-14T08%3A37%3A54Z&se=2015-12-18T09%3A07%3A54Z&sp=rl&restype=container&comp=list");
            helper.AddDownloadUrl("http://127.0.0.1:10000/devstoreaccount1/b4c939eb-a481-4e0d-bde1-49abd6eafade/1.zip?sv=2015-02-21&sr=c&sig=9nwy8J0CbhbPEvvhpDVMSijq%2BBteQqdX8hQYQy9cnQ4%3D&st=2015-12-14T08%3A37%3A54Z&se=2015-12-18T09%3A07%3A54Z&sp=rl&restype=container&comp=list");
            string htmlbody = helper.GetHtmlBody();

            var client = Config.MailConfigInstance.Client();
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            msg.To.Add("haiyang.ling@arcserve.com");
            msg.From = new MailAddress(Config.MailConfigInstance.Sender);
            msg.Subject = "Restore Finish";
            msg.Body = htmlbody;
            msg.IsBodyHtml = true;

            try
            {
                client.Send(msg);
            }
            catch (Exception e)
            {

            }
        }


        [TestMethod]
        public void TestMonth()
        {
            DateTime time = new DateTime(2015, 2, 28);
            DateTime time13 = time.AddDays(13 * 7);
            DateTime time27 = time.AddDays(28 * 7 - 1);
        }

        [TestMethod]
        public void TestCompress()
        {
            StreamReader readerFile1 = new StreamReader(@"D:\21GitHub\Haiyang\EWS\Office365Demo\ExGrtAzure\ExGrtAzure.Tests\bin\Debug\00MsgFile\11.msg");
            StreamReader readerFile2 = new StreamReader(@"D:\21GitHub\Haiyang\EWS\Office365Demo\ExGrtAzure\ExGrtAzure.Tests\bin\Debug\00MsgFile\test.msg");

            using (MemoryStream mStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(mStream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry(@"Dir1\Dir3\11.msg");
                    using (var stream = entry.Open())
                    {
                        readerFile1.BaseStream.CopyTo(stream);
                    }

                    var entry2 = archive.CreateEntry(@"Dir2\Dir4\test.msg");
                    using (var stream2 = entry2.Open())
                    {
                        readerFile2.BaseStream.CopyTo(stream2);
                    }

                }
                mStream.Seek(0, SeekOrigin.Begin);

                using (StreamWriter writer = new StreamWriter(@"D:\21GitHub\Haiyang\EWS\Office365Demo\ExGrtAzure\ExGrtAzure.Tests\bin\Debug\00MsgFile\zip.zip"))
                {
                    mStream.CopyTo(writer.BaseStream);
                }

            }


            //MailLocation binLocation = new MailLocation(ExportType.TransferBin, @"http://www.baidu.com/baidu?wd=support+oauth+2.0+file+share&tn=monline_4_dg",100);
            //MailLocation emlLocation = new MailLocation(ExportType.Eml, @"http://dict.youdao.com/search?le=eng&q=lj%3A%20%E5%90%8C%E6%AD%A5&keyfrom=dict.top",200);
            //List<MailLocation> lists = new List<MailLocation>(2);
            //lists.Add(binLocation);
            //lists.Add(emlLocation);

            //string saveString = LocationUtil.LocationsToString(lists, 0);
            //var val = LocationUtil.StringToLocations(saveString, 0);

            //string otherString = LocationUtil.LocationsToString(lists, 1);
            //var otherVal = LocationUtil.StringToLocations(otherString, 1);

            //Assert.AreEqual(binLocation.Path, otherVal[0].Path);
            //Assert.AreEqual(binLocation.Type, otherVal[0].Type);
            //Assert.AreEqual(emlLocation.Path, otherVal[1].Path);
            //Assert.AreEqual(emlLocation.Type, otherVal[1].Type);

            //var locationLenght = (binLocation.Path.Length + emlLocation.Path.Length);
            //var jsonLength = saveString.Length;
            //var compressLength = otherString.Length;


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

            var folderLevel1212_Path = new List<IFolderDataBase>() {
                new FolderDataBaseDefault() { DisplayName = folderLevel12.DisplayName },
                new FolderDataBaseDefault() { DisplayName = folderLevel121.DisplayName },
                new FolderDataBaseDefault() { DisplayName = folderLevel1212.DisplayName }
            };
            var folderLevel11_Path = new List<IFolderDataBase>() { new FolderDataBaseDefault() { DisplayName = folderLevel11.DisplayName } };

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

            var rootNode = TreeNode.CreateTree(allFolder);
            List<string> allFolderIds = TreeNode.GetAllFoldersAndChildFolders(rootNode, "12");

            var dic = TreeNode.GetEachFolderPath(rootNode);

            var path1212 = dic["1212"];
            var path11 = dic["11"];

            AssertList(allFolderIds, folderLevel12_AllChildFolderId, false);
            AssertList(path1212, folderLevel1212_Path, true);
            AssertList(path11, folderLevel11_Path, true);
        }

        private void AssertList(List<IFolderDataBase> left1, List<IFolderDataBase> right1, bool isEqualByOrder)
        {
            var left = new List<IFolderDataBase>(left1);
            var right = new List<IFolderDataBase>(right1);
            Assert.AreEqual(left.Count, right.Count);
            if (isEqualByOrder)
            {
                for (int i = 0; i < right.Count; i++)
                {
                    Assert.AreEqual(left[i], right[i]);
                }
            }
            else
            {
                for (int i = 0; i < left.Count; i++)
                {
                    int j = right.IndexOf(left[i]);
                    Assert.IsTrue(j >= 0);
                    right.RemoveAt(j);
                }
            }
        }

        private void AssertList<T>(List<T> left1, List<T> right1, bool isEqualByOrder)
        {
            var left = new List<T>(left1);
            var right = new List<T>(right1);
            Assert.AreEqual(left.Count, right.Count);
            if (isEqualByOrder)
            {
                for (int i = 0; i < right.Count; i++)
                {
                    Assert.AreEqual(left[i], right[i]);
                }
            }
            else
            {
                for (int i = 0; i < left.Count; i++)
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

            public string Id
            {
                get
                {
                    return FolderId;
                }

                set
                {
                    FolderId = value;
                }
            }

            public ItemKind ItemKind
            {
                get
                {
                    return ItemKind.Folder;
                }

                set
                {
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

            int IFolderData.ChildFolderCount
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

            int IFolderData.ChildItemCount
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

            public IFolderData Clone()
            {
                throw new NotImplementedException();
            }
        }
    }

    public static class ZipArchiveExtension
    {
        public static ZipArchiveDirectory CreateDirectory(this ZipArchive @this, string directoryPath)
        {

            return new ZipArchiveDirectory(@this, directoryPath);
        }
    }

    public class ZipArchiveDirectory
    {
        private readonly string _directory;
        private ZipArchive _archive;

        internal ZipArchiveDirectory(ZipArchive archive, string directory)
        {
            _archive = archive;
            _directory = directory;
        }

        public ZipArchive Archive { get { return _archive; } }

        public ZipArchiveEntry CreateEntry(string entry)
        {
            return _archive.CreateEntry(_directory + "/" + entry);
        }

        public ZipArchiveEntry CreateEntry(string entry, CompressionLevel compressionLevel)
        {
            return _archive.CreateEntry(_directory + "/" + entry, compressionLevel);
        }


        [TestMethod]
        public void TestRSA()
        {
        }
    }

    
}
