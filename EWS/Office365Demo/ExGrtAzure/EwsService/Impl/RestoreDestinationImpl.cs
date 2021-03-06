﻿using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;
using EwsServiceInterface;
using EwsFrame;
using DataProtectInterface.Util;

namespace EwsService.Impl
{
    public class RestoreDestinationImpl : IRestoreDestination
    {
        public RestoreDestinationImpl(EwsServiceArgument argument, IDataAccess dataAccess)
        {
            _argument = argument;
            _dataAccess = dataAccess;
        }

        private readonly EwsServiceArgument _argument;
        private readonly IDataAccess _dataAccess;

        public string DesMailboxAddress { get; set; }
        public string DesFolderDisplayNamePath
        {
            set
            {
                var eachPath = value.Split(_pathSplit, StringSplitOptions.RemoveEmptyEntries);
                DesFolderPath = new List<IFolderDataBase>(eachPath.Length);
                foreach (var pathItem in eachPath)
                {
                    DesFolderPath.Add(new FolderDataBase() { DisplayName = pathItem, FolderType = FolderClassUtil.DefaultFolderType });
                }
            }
        }
        public List<IFolderDataBase> DesFolderPath { get; set; }

        private static char[] _pathSplit = "\\".ToCharArray();
        //private string[] _desFolderPathArray;
        //private string[] DesFolderPathArray
        //{
        //    get
        //    {
        //        if(_desFolderPathArray == null)
        //        {
        //            _desFolderPathArray = DesFolderDisplayNamePath.Split(_pathSplit, StringSplitOptions.RemoveEmptyEntries);
        //        }
        //        return _desFolderPathArray;
        //    }
        //}

        public ExportType ExportType
        {
            get
            {
                return ExportType.TransferBin;
            }
        }

        private IEwsAdapter EwsAdapter { get; set; }


        private string CurrentMailbox;
        private void ConnectMailbox(string mailboxAddress)
        {
            if (CurrentMailbox != mailboxAddress)
            {
                if (CurrentMailbox == null)
                {
                    CurrentMailbox = mailboxAddress;
                }

                EwsAdapter = RestoreFactory.Instance.NewEwsAdapter();
                _argument.SetConnectMailbox(mailboxAddress);
                EwsAdapter.ConnectMailbox(_argument, mailboxAddress);
            }
        }

        //private Dictionary<string, FolderId> CreatedFolders;
        //private FolderId FindAndCreateFolder(IFolderDataBase folder, FolderId parentFolderId, StringBuilder keyBuilder)
        //{
        //    FolderId folderId = null;
        //    keyBuilder.Append(folder.DisplayName).Append("\\");
        //    var key = keyBuilder.ToString(); //string.Format("{0}-{1}", folder.DisplayName, parentFolderId.UniqueId);
        //    if (CreatedFolders.TryGetValue(key, out folderId))
        //        return folderId;

        //    folderId = EwsAdapter.FindFolder(folder, parentFolderId, 3);
        //    if (folderId == null)
        //        folderId = EwsAdapter.CreateChildFolder(folder, parentFolderId);
        //    CreatedFolders.Add(key, folderId);
        //    return folderId;
        //}

        //private FolderId CreateFoldersIfNotExist(ICollection<IFolderDataBase> folders)
        //{
        //    ConnectMailbox(DesMailboxAddress);
        //    FolderId parentFolderId = EwsAdapter.GetRootFolder().Id;
        //    StringBuilder sb = new StringBuilder();
        //    foreach (var folder in folders)
        //    {
        //        parentFolderId = FindAndCreateFolder(folder, parentFolderId, sb);
        //    }
        //    return parentFolderId;
        //}

        private List<IFolderDataBase> GetFolderPathArray(Stack<IFolderData> folders)
        {
            List<IFolderDataBase> path = new List<IFolderDataBase>(5);
            if (DesFolderPath != null)
                path.AddRange(DesFolderPath);
            if (folders != null)
            {
                path.AddRange(folders);
            }
            return path;
        }

        private List<IFolderDataBase> GetFolderPathArray(List<IFolderDataBase> folderPaths)
        {
            List<IFolderDataBase> path = new List<IFolderDataBase>(5);
            if (DesFolderPath != null)
                path.AddRange(DesFolderPath);
            path.AddRange(folderPaths);
            return path;
        }

        public void WriteItem(IRestoreItemInformation item, byte[] itemData)
        {
            //WriteItemForEachType(item, itemData);
            WriteItemToDesFolder(item, itemData);
        }

        private void WriteItemForEachType(IRestoreItemInformation item, byte[] itemData)
        {
            string folderId;
            ConnectMailbox(DesMailboxAddress);
            switch (item.ItemClass)
            {
                case DataProtectInterface.Util.ItemClass.Message:
                    {
                        List<IFolderDataBase> path = GetFolderPathArray(item.FolderPathes);
                        var folder = EwsAdapter.CreateFoldersIfNotExist(path);
                        folderId = folder;
                        break;
                    }
                case DataProtectInterface.Util.ItemClass.Contact:
                    {
                        folderId = EwsAdapter.GetContactFolderId();
                        break;
                    }
                case DataProtectInterface.Util.ItemClass.Appointment:
                    {
                        folderId = EwsAdapter.GetAppointmentFolderId();
                        break;
                    }
                case DataProtectInterface.Util.ItemClass.Task:
                    {
                        folderId = EwsAdapter.GetTaskFolderId();
                        break;
                    }
                default:
                    throw new NotSupportedException("Can not import not support itemclass.");
            }
            var argument = _argument;
            EwsAdapter.ImportItem(folderId, itemData, argument);
        }

        private void WriteItemToDesFolder(IRestoreItemInformation item, byte[] itemData)
        {
            ConnectMailbox(DesMailboxAddress);
            List<IFolderDataBase> path = GetFolderPathArray(item.FolderPathes);
            var folderId = EwsAdapter.CreateFoldersIfNotExist(path);
            var argument = _argument;
            EwsAdapter.ImportItem(folderId, itemData, argument);
        }

        public void InitOtherInformation(params object[] information)
        {
            DesMailboxAddress = information[0] as string;
            DesFolderDisplayNamePath = information[1] as string;
        }

        public void RestoreComplete(bool success, string restoreJobName, Exception ex)
        {
        }

        public void Dispose()
        {
        }


        class FolderDataBase : IFolderDataBase
        {
            public string DisplayName
            {
                get; set;
            }

            public string FolderType
            {
                get; set;
            }
        }
    }


}
