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

        private static char[] _invalidFolderChar;
        private static object _lockObj = new object();
        private static char[] InvalidFolderChar
        {
            get
            {
                if (_invalidFolderChar == null)
                {
                    using (_lockObj.LockWhile(() =>
                    {
                        if (_invalidFolderChar == null)
                        {
                            HashSet<char> charHash = new HashSet<char>(Path.GetInvalidPathChars());
                            if (!charHash.Contains('\\'))
                                charHash.Add('\\');
                            _invalidFolderChar = charHash.ToArray();
                        }
                    }))
                    { }
                }
                return _invalidFolderChar;
            }
        }

        public static string GetValidFolderName(this string folderDisplayName)
        {
            return string.Join("_", folderDisplayName.Split(InvalidFolderChar));
        }


        private static char[] _invalidFileChars;
        public static char[] InvalidFileChars
        {
            get
            {
                if (_invalidFileChars == null)
                {
                    HashSet<char> charHash = new HashSet<char>(Path.GetInvalidFileNameChars());
                    if (!charHash.Contains('\\'))
                        charHash.Add('\\');
                    _invalidFileChars = charHash.ToArray();
                }
                return _invalidFileChars;
            }
        }
        public static string GetValidFileName(this string itemSubject)
        {
            return string.Join("_", itemSubject.Split(InvalidFileChars));
        }
    }

    public class FolderNode : TreeNode<IFolderDataSync, string>
    {

    }
}
