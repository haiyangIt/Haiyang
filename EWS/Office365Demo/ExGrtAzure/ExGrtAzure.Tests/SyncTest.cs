using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EwsService.Common;
using EwsServiceInterface;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Exchange.WebServices;
using Microsoft.Exchange.WebServices.Data;
using System.Diagnostics;
using DataProtectInterface.Util;
using System.Linq;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class SyncTest
    {
        [TestMethod]
        public void SyncInit()
        {
            string mailbox = "userInDb1@linha06.com";
            string userName = "linha06\\userindb1";
            string password = "123.com";
            EwsServiceArgument argument = new EwsServiceArgument();
            argument.UseDefaultCredentials = false;
            argument.ServiceCredential = new System.Net.NetworkCredential(userName, password);
            argument.RequestedExchangeVersion = ExchangeVersion.Exchange2013_SP1;
            argument.EwsUrl = new Uri("https://linha06-ex.linha06.com/EWS/Exchange.asmx");
            var ewsService = EwsProxyFactory.CreateExchangeService(argument, mailbox);
            var latestSyncState = MailboxSyncState.GetLatestSyncStatus();
            DealWholeMailbox(ewsService, latestSyncState);
        }

        string indent1 = "|-";
        string indent2 = "  |-";
        string indent3 = "    |-";

        private void DealWholeMailbox(ExchangeService ewsService, MailboxSyncState latestState)
        {
            int index = 0;
            bool hasMoreChanges = false;
            string lastSyncStatus = latestState.GetMailboxSyncStates();
            List<FolderChange> allChanges = new List<FolderChange>();
            do
            {
                var folderChanges = ewsService.SyncFolderHierarchy(PropertySet.IdOnly, lastSyncStatus);
                hasMoreChanges = folderChanges.MoreChangesAvailable;
                allChanges.AddRange(folderChanges);
                lastSyncStatus = folderChanges.SyncState;
                Debug.WriteLine(string.Format("SyncStatus:{0}", folderChanges.SyncState));
            } while (hasMoreChanges);

            var newSyncStatus = new SyncStatusInfo() { Status = lastSyncStatus, Time = DateTime.Now };
            HashSet<string> folderIdHashes = new HashSet<string>();

            foreach (var change in allChanges)
            {
                var folder = change.Folder;
                folder.Load(PropertySet.FirstClassProperties);

                if (!FolderClassUtil.IsFolderValid(folder.FolderClass))
                    continue;
                index++;
                Debug.WriteLine(string.Format("{4}[{0}]:ChangeType:{1},FolderName:{2}, FolderId:{3}",
                    index, change.ChangeType, folder.DisplayName, change.FolderId, indent1));

                DealEachChangedFolder(ewsService, folder.Id, latestState);
                folderIdHashes.Add(folder.Id.UniqueId);
            }

            var folders = new List<string>(latestState.FolderSyncStates.Keys);

            foreach (var folderId in folders)
            {
                if (!folderIdHashes.Contains(folderId))
                {
                    var folder = Folder.Bind(ewsService, new FolderId(folderId), PropertySet.FirstClassProperties);
                    if (!FolderClassUtil.IsFolderValid(folder.FolderClass))
                        continue;
                    index++;
                    Debug.WriteLine(string.Format("{4}[{0}]:ChangeType:{1},FolderName:{2}, FolderId:{3}",
                        index, "NoChange", folder.DisplayName, folderId, indent1));

                    DealEachChangedFolder(ewsService, folder.Id, latestState);
                }
            }

            latestState.UpdateMailboxSyncStates(newSyncStatus);
            MailboxSyncState.WriteStatusToFile(latestState);
        }

        private void DealEachChangedFolder(ExchangeService ewsService, FolderId folderId, MailboxSyncState latestState)
        {
            int index = 0;
            var lastSyncStatus = latestState.GetFolderSyncStates(folderId.UniqueId);
            bool hasMoreChanges = false;
            do
            {
                var itemChanges = ewsService.SyncFolderItems(folderId, PropertySet.IdOnly, null, 2, SyncFolderItemsScope.NormalItems, lastSyncStatus);
                hasMoreChanges = itemChanges.MoreChangesAvailable;
                lastSyncStatus = itemChanges.SyncState;
                Debug.WriteLine(string.Format("SyncStatus:{0}", itemChanges.SyncState));
                if (itemChanges.Count > 0)
                {
                    var changeItems = from i in itemChanges select i.Item;
                    ewsService.LoadPropertiesForItems(changeItems, PropertySet.FirstClassProperties);
                    Dictionary<string, Item> dic = new Dictionary<string, Item>();

                    foreach (var change in changeItems)
                    {
                        dic.Add(change.Id.UniqueId, change);
                    }

                    foreach (var change in itemChanges)
                    {
                        index++;
                        var item = dic[change.ItemId.UniqueId];
                        Debug.WriteLine(string.Format("{4}[{0}]:ChangeType:{1},ItemName:{2}, ItemId:{3}", index, change.ChangeType, item.Subject, change.ItemId.UniqueId, indent2));
                    }
                }

            } while (hasMoreChanges);

            var newSyncStatus = new SyncStatusInfo() { Status = lastSyncStatus, Time = DateTime.Now };
            latestState.UpdateFolderSyncStates(folderId.UniqueId, newSyncStatus);
        }

        class SyncStatusInfo
        {
            public DateTime Time;
            public string Status;
        }

        class MailboxSyncState
        {
            public SyncStatusInfo MailboxSyncStates;

            public Dictionary<string, SyncStatusInfo> FolderSyncStates;

            public MailboxSyncState()
            {
                FolderSyncStates = new Dictionary<string, SyncStatusInfo>();
            }

            public void UpdateMailboxSyncStates(SyncStatusInfo newMailboxSyncStates)
            {
                MailboxSyncStates = newMailboxSyncStates;
            }

            public string GetMailboxSyncStates()
            {
                if (MailboxSyncStates == null)
                    return string.Empty;
                return MailboxSyncStates.Status;
            }

            public string GetFolderSyncStates(string folderId)
            {
                SyncStatusInfo folderState = null;
                if (FolderSyncStates.TryGetValue(folderId, out folderState))
                {
                    return folderState.Status;
                }
                return string.Empty;
            }

            public void UpdateFolderSyncStates(string folderId, SyncStatusInfo newFolderSyncStates)
            {
                FolderSyncStates[folderId] = newFolderSyncStates;
            }

            public static string GetLatestSyncStatusFile()
            {
                var files = Directory.GetFiles(StatusFolder, "*State.txt", SearchOption.TopDirectoryOnly);
                if (files.Length == 0)
                    return string.Empty;
                else
                {
                    var lastFile = (from file in files orderby file descending select file).FirstOrDefault();
                    return lastFile;
                }
            }

            public static MailboxSyncState GetLatestSyncStatus()
            {
                var file = GetLatestSyncStatusFile();
                if (string.IsNullOrEmpty(file))
                {
                    return new MailboxSyncState();
                }
                using (StreamReader reader = new StreamReader(file))
                {
                    string json = reader.ReadToEnd();
                    return DeserializeFromJson(json);
                }
            }

            private static string StatusFolder
            {
                get
                {
                    return AppDomain.CurrentDomain.BaseDirectory;
                }
            }

            public static void WriteStatusToFile(MailboxSyncState state)
            {
                var fileName = string.Format("{0}State.txt", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                var path = Path.Combine(StatusFolder, fileName);
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(SerializeToJson(state));
                }
            }

            public static MailboxSyncState DeserializeFromJson(string jsonData)
            {
                return JsonConvert.DeserializeObject<MailboxSyncState>(jsonData);
            }

            public static string SerializeToJson(MailboxSyncState state)
            {
                return JsonConvert.SerializeObject(state);
            }

        }
        [TestMethod]
        public void TestMailboxSyncState()
        {
            MailboxSyncState state = new MailboxSyncState();
            state.UpdateMailboxSyncStates(new SyncStatusInfo() { Status = "1", Time = DateTime.Now });
            state.UpdateFolderSyncStates("123", new SyncStatusInfo() { Status = "2", Time = DateTime.UtcNow });
            state.UpdateFolderSyncStates("234", new SyncStatusInfo() { Status = "4", Time = DateTime.Now });
            state.UpdateFolderSyncStates("345", new SyncStatusInfo() { Status = "5", Time = DateTime.UtcNow });
            var json = MailboxSyncState.SerializeToJson(state);
            MailboxSyncState newState = MailboxSyncState.DeserializeFromJson(json);
        }
    }
}
