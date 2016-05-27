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
            string displayName; 
            
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
    }

    public static class FolderPathUtil
    {
        static FolderPathUtil()
        {
            HashSet<char> charHash = new HashSet<char>(Path.GetInvalidPathChars());
            if (!charHash.Contains('\\'))
                charHash.Add('\\');
            InvalidFolderChar = charHash.ToArray();

            charHash = new HashSet<char>(Path.GetInvalidFileNameChars());
            if (!charHash.Contains('\\'))
                charHash.Add('\\');
            InvalidFileChars = charHash.ToArray();
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
