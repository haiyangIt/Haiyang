using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Util
{
    public delegate void FuncForDealEachNodeWhenDepthFirstTraverse(TreeNode currentNode, int depth, int inChildrenIndex);
    public class TreeNode
    {
        public List<TreeNode> Childrens;
        public IFolderData Folder;
        public string FolderId;

        public TreeNode()
        {
            Childrens = new List<TreeNode>();
        }

        public static Dictionary<string, List<string>> GetEachFolderPath(TreeNode root)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            List<string> paths = new List<string>();

            var children = root.Childrens;
            foreach (var node in children)
            {
                paths = new List<string>();
                paths.Add(node.Folder.DisplayName);
                TraverseTree(node, result, paths);
            }
            return result;
        }

        private static void TraverseTree(TreeNode child, Dictionary<string, List<string>> result, List<string> paths)
        {
            result.Add(child.Folder.FolderId, paths);
            var children = child.Childrens;
            if (children.Count > 0)
            {
                foreach (var node in children)
                {
                    var childpaths = new List<string>();
                    childpaths.AddRange(paths);
                    childpaths.Add(node.Folder.DisplayName);
                    TraverseTree(node, result, childpaths);
                }
            }
        }

        public static void DepthFirstTraverseTree(TreeNode root, FuncForDealEachNodeWhenDepthFirstTraverse func)
        {
            DepthFirstTraverseTree(root, 0, func);

        }
        private static void DepthFirstTraverseTree(TreeNode node, int depth, FuncForDealEachNodeWhenDepthFirstTraverse func)
        {
            int childCount = 0;
            foreach(var child in node.Childrens)
            {
                DepthFirstTraverseTree(child, depth + 1, func);
                childCount++;
            }
            if (func != null)
                func(node, depth, childCount);
        }

        public static List<string> GetAllFoldersAndChildFolders(TreeNode root, List<string> folderIds)
        {
            HashSet<string> result = new HashSet<string>(folderIds);
            foreach (var id in folderIds)
            {
                var eachResult = GetAllFoldersAndChildFolders(root, id);
                foreach (var eachid in eachResult)
                {
                    if (!result.Contains(eachid))
                    {
                        result.Add(eachid);
                    }
                }
            }
            return result.ToList();
        }

        public static List<string> GetAllFoldersAndChildFolders(TreeNode root, string folderId)
        {
            var findNode = FindFolderId(root, folderId);
            if (findNode == null)
                throw new NullReferenceException(string.Format("can't find folder [{0}]", folderId));
            var result = new List<string>();
            GetAllChildFolders(findNode, result);
            return result;
        }

        public static TreeNode FindFolderId(TreeNode node, string folderId)
        {
            if (node.FolderId == folderId)
                return node;
            foreach (var child in node.Childrens)
            {
                var result = FindFolderId(child, folderId);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static void GetAllChildFolders(TreeNode node, List<string> result)
        {
            if (node.Folder != null)
            {
                result.Add(node.Folder.FolderId);
                foreach (var child in node.Childrens)
                {
                    GetAllChildFolders(child, result);
                }
            }
        }

        public static TreeNode CreateTree(List<IFolderData> folders)
        {
            Dictionary<string, TreeNode> folderId2Node = new Dictionary<string, TreeNode>(folders.Count);
            TreeNode root = null;
            foreach (var folder in folders)
            {
                var parentId = folder.ParentFolderId;
                TreeNode node = CreateOrGetParentNode(parentId, folderId2Node);
                TreeNode childNode = CreateNode(folder, folderId2Node);
                node.Childrens.Add(childNode);
            }

            foreach (var keyvalue in folderId2Node)
            {
                if (keyvalue.Value.Folder == null)
                {
                    root = keyvalue.Value;
                    break;
                }
            }
            return root;
        }


        private static TreeNode CreateNode(IFolderData folder, Dictionary<string, TreeNode> folderId2Node)
        {
            TreeNode result = null;
            if (!folderId2Node.TryGetValue(folder.FolderId, out result))
            {
                result = new TreeNode();
                folderId2Node.Add(folder.FolderId, result);
            }
            result.Folder = folder;
            result.FolderId = folder.FolderId;
            return result;
        }

        private static TreeNode CreateOrGetParentNode(string parentId, Dictionary<string, TreeNode> folderId2Node)
        {
            TreeNode result = null;
            if (!folderId2Node.TryGetValue(parentId, out result))
            {
                result = new TreeNode();
                folderId2Node.Add(parentId, result);
            }
            result.FolderId = parentId;
            return result;
        }
    }
}
