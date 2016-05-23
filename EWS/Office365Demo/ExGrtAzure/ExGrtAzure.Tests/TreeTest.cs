using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Mail;
using System.Diagnostics;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class TreeTest
    {
        [TestMethod]
        public void TestFolderTree()
        {
            var rootFolder = new FolderTest() { FolderId = "0", ParentFolderId = "-1", DisplayName = "root" };
            var inboxFolder = new FolderTest() { FolderId = "01", ParentFolderId = "0", DisplayName = "inbox" };

            var sentFolder = new FolderTest() { FolderId = "02", ParentFolderId = "0", DisplayName = "sent" };

            var inboxChild1 = new FolderTest() { FolderId = "1", ParentFolderId = "01", DisplayName = "inboxChild1" };
            var inboxChild2 = new FolderTest() { FolderId = "2", ParentFolderId = "01", DisplayName = "inboxChild2" };
            var inboxChild11 = new FolderTest() { FolderId = "11", ParentFolderId = "1", DisplayName = "inboxChild11" };
            var inboxChild111 = new FolderTest() { FolderId = "111", ParentFolderId = "11", DisplayName = "inboxChild111" };
            var inboxChild21 = new FolderTest() { FolderId = "21", ParentFolderId = "2", DisplayName = "inboxChild21" };
            var inboxChild22 = new FolderTest() { FolderId = "22", ParentFolderId = "2", DisplayName = "inboxChild22" };
            var sentChild1 = new FolderTest() { FolderId = "021", ParentFolderId = "02", DisplayName = "sentChild1" };

            var folderTree = new FolderTree();
            folderTree.AddNode(inboxChild1);
            folderTree.AddNode(inboxChild11);
            folderTree.AddNode(sentChild1);
            folderTree.AddNode(inboxChild2);
            folderTree.AddNode(inboxChild111);
            folderTree.AddNode(inboxFolder);
            folderTree.AddNode(rootFolder);
            folderTree.AddNode(inboxChild22);
            folderTree.AddNode(inboxChild21);
            folderTree.AddNode(sentFolder);
            folderTree.AddComplete();

            folderTree.DFS((folder, level) =>
            {
                if (level != 0)
                    Debug.WriteLine(folder.Data.Location);
                else
                {

                }
            });

            Assert.AreEqual(rootFolder.Location, "root");
            Assert.AreEqual(inboxFolder.Location, @"root\inbox");
            Assert.AreEqual(sentFolder.Location, @"root\sent");
            Assert.AreEqual(inboxChild1.Location, @"root\inbox\inboxChild1");
            Assert.AreEqual(inboxChild2.Location, @"root\inbox\inboxChild2");
            Assert.AreEqual(inboxChild11.Location, @"root\inbox\inboxChild1\inboxChild11");
            Assert.AreEqual(inboxChild111.Location, @"root\inbox\inboxChild1\inboxChild11\inboxChild111");
            Assert.AreEqual(inboxChild21.Location, @"root\inbox\inboxChild2\inboxChild21");
            Assert.AreEqual(inboxChild22.Location, @"root\inbox\inboxChild2\inboxChild22");
            Assert.AreEqual(sentChild1.Location, @"root\sent\sentChild1");

        }


        [TestMethod]
        public void TestInvalidFolderTree()
        {
            var rootFolder = new FolderTest() { FolderId = "0", ParentFolderId = "-1", DisplayName = "root" };
            var inboxFolder = new FolderTest() { FolderId = "01", ParentFolderId = "0", DisplayName = "inbox" };

            var sentFolder = new FolderTest() { FolderId = "02", ParentFolderId = "0", DisplayName = "sent" };

            var inboxChild1 = new FolderTest() { FolderId = "1", ParentFolderId = "01", DisplayName = "inboxChild1" };
            var inboxChild2 = new FolderTest() { FolderId = "2", ParentFolderId = "01", DisplayName = "inboxChild2" };
            var inboxChild11 = new FolderTest() { FolderId = "11", ParentFolderId = "1", DisplayName = "inboxChild11" };
            var inboxChild111 = new FolderTest() { FolderId = "111", ParentFolderId = "11", DisplayName = "inboxChild111" };
            var inboxChild21 = new FolderTest() { FolderId = "21", ParentFolderId = "2", DisplayName = "inboxChild21" };
            var inboxChild22 = new FolderTest() { FolderId = "22", ParentFolderId = "2", DisplayName = "inboxChild22" };
            var sentChild1 = new FolderTest() { FolderId = "021", ParentFolderId = "02", DisplayName = "sentChild1" };

            var folderTree = new FolderTree();
            folderTree.AddNode(inboxChild1);
            folderTree.AddNode(inboxChild11);
            folderTree.AddNode(sentChild1);
            folderTree.AddNode(inboxChild2);
            folderTree.AddNode(inboxChild111);
            folderTree.AddNode(inboxFolder);
            folderTree.AddNode(rootFolder);
            folderTree.AddNode(inboxChild22);
            folderTree.AddNode(inboxChild21);
            folderTree.AddNode(sentFolder);

            folderTree.AddNode(sentFolder);

            folderTree.AddComplete();

            folderTree.DFS((folder, level) =>
            {
                if (level != 0)
                    Debug.WriteLine(folder.Data.Location);
                else
                {

                }
            });

            Assert.AreEqual(rootFolder.Location, "root");
            Assert.AreEqual(inboxFolder.Location, @"root\inbox");
            Assert.AreEqual(sentFolder.Location, @"root\sent");
            Assert.AreEqual(inboxChild1.Location, @"root\inbox\inboxChild1");
            Assert.AreEqual(inboxChild2.Location, @"root\inbox\inboxChild2");
            Assert.AreEqual(inboxChild11.Location, @"root\inbox\inboxChild1\inboxChild11");
            Assert.AreEqual(inboxChild111.Location, @"root\inbox\inboxChild1\inboxChild11\inboxChild111");
            Assert.AreEqual(inboxChild21.Location, @"root\inbox\inboxChild2\inboxChild21");
            Assert.AreEqual(inboxChild22.Location, @"root\inbox\inboxChild2\inboxChild22");
            Assert.AreEqual(sentChild1.Location, @"root\sent\sentChild1");

        }
    }

    public class FolderTest : IFolderDataSync
    {
        public string ChangeKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int ChildFolderCount
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

        public int ChildItemCount
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
                throw new NotImplementedException();
            }
        }

        public ItemKind ItemKind
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Location
        {
            get; set;
        }

        public string MailboxAddress
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string MailboxId
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

        public string ParentFolderId
        {
            get; set;
        }

        public string SyncStatus
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
