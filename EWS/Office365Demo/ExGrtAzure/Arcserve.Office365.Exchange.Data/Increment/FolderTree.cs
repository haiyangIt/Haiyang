using Arcserve.Office365.Exchange.Data.Mail;
using Arcserve.Office365.Exchange.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Increment
{
    public class FolderTree : Tree<IFolderDataSync, string>
    {
        public FolderTree() : base(GetParentKey, GetSelfKey, CreatRootNode())
        {
        }

        public static FolderNode CreatRootNode()
        {
            return new FolderNode() { Data = null, Key = string.Empty };
        }

        public static string GetSelfKey(IFolderDataSync data)
        {
            return data.FolderId;
        }

        public static string GetParentKey(IFolderDataSync data)
        {
            return data.ParentFolderId;
        }

        public IEnumerable<string> GetPath(IFolderDataSync data)
        {
            var parentsNode = GetParentsData(data);

            return (from m in parentsNode where m != null select ((IFolderDataBase)m).DisplayName).AsEnumerable();
        }

        protected override void AfterAdded()
        {
            base.AfterAdded();

            DFS<string>((folder, level, parentResult) =>
            {
                var result = string.Empty;
                if (level == 0)
                    result = string.Empty;
                else
                {
                    if (string.IsNullOrEmpty(parentResult))
                    {
                        result = ((IFolderDataBase)folder.Data).DisplayName.GetFolderLocation();
                    }
                    else
                    {
                        var parentDisplays = parentResult.GetFolderDisplays();
                        parentDisplays.Add(((IFolderDataBase)folder.Data).DisplayName);
                        result = parentDisplays.GetFolderLocation();
                    }
                    folder.Data.Location = result;
                    Debug.WriteLine(string.Format("{0} {1}", level, result));
                }
                return result;
            });
        }

        public string Serialize()
        {
            List<FolderBaseInfo> folders = new List<FolderBaseInfo>();
            DFS((folderNode, level) =>
            {
                if (level != 0)
                    folders.Add(new FolderBaseInfo() { Id = folderNode.Data.FolderId, PId = folderNode.Data.ParentFolderId, Name = ((IItemBase)folderNode.Data).DisplayName, Type = folderNode.Data.FolderType });
            });
            return JsonConvert.SerializeObject(folders);
        }

        public void Deserialize(string folderTreeString)
        {
            var folders = JsonConvert.DeserializeObject<List<FolderBaseInfo>>(folderTreeString);
            foreach (var folder in folders)
                this.AddNode(new FolderDataForTree() { FolderId = folder.Id, DisplayName = folder.Name, ParentFolderId = folder.PId, FolderType = folder.Type });
            AddComplete();
        }

        class FolderBaseInfo
        {
            public string Id { get; set; }
            public string PId { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }

        class FolderDataForTree : IFolderDataSync
        {
            public string MailboxId { get; set; }
            public string ParentFolderId { get; set; }
            public string MailboxAddress { get; set; }
            public string Location { get; set; }
            public int ChildItemCount { get; set; }
            public int ChildFolderCount { get; set; }
            public string FolderId { get; set; }
            public IFolderData Clone()
            {
                throw new NotSupportedException();
            }
            public string SyncStatus { get; set; }

            public string ChangeKey { get; set; }
            public string DisplayName { get; set; }
            public string FolderType { get; set; }

            public string Id
            { get; set; }

            public ItemKind ItemKind
            { get; set; }

            public long UniqueId
            {
                get; set;
            }
        }
    }

    public static class FolderPathUtil
    {
        static FolderPathUtil()
        {
            HashSet<char> charHash = new HashSet<char>(Path.GetInvalidPathChars());
            if (!charHash.Contains('\\'))
                charHash.Add('\\');
            if (!charHash.Contains('/'))
                charHash.Add('/');

            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            foreach(var charItem in invalidFileNameChars)
            {
                if (!charHash.Contains(charItem))
                {
                    charHash.Add(charItem);
                }
            }

            InvalidFolderChar = charHash.ToArray();
            InvalidFileChars = charHash.ToArray();

            Log.LogFactory.LogInstance.WriteLog(Log.LogLevel.DEBUG, string.Join(" ", InvalidFolderChar));
            Log.LogFactory.LogInstance.WriteLog(Log.LogLevel.DEBUG, string.Join(" ", InvalidFileChars));
        }

        public static string GetFolderLocation(this List<string> folderDisplays)
        {
            var str = JsonConvert.SerializeObject(folderDisplays);
            return str;
        }

        public static string GetFolderLocation(this string displayName)
        {
            return GetFolderLocation(new List<string>(1) { displayName });
        }

        public static List<string> GetFolderDisplays(this string path)
        {
            return JsonConvert.DeserializeObject<List<string>>(path);
        }

        private static readonly char[] InvalidFolderChar;

        public static string GetValidFolderName(this string folderDisplayName)
        {
            return string.Join("_", folderDisplayName.Split(InvalidFolderChar));
        }


        public static readonly char[] InvalidFileChars;
        public static string GetValidFileName(this string itemSubject)
        {
            return string.Join("_", itemSubject.Split(InvalidFileChars));
        }
    }

    public class FolderNode : TreeNode<IFolderDataSync, string>
    {

    }
}
