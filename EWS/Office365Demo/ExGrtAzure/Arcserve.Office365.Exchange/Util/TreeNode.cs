using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Util
{
    public class Tree<T, TKey>
    {
        public TreeNode<T, TKey> Root;
        private Func<T, TKey> FuncGetParentTKey;
        private Func<T, TKey> FuncGetSelfKey;

        Dictionary<TKey, TreeNode<T, TKey>> _dic = new Dictionary<TKey, TreeNode<T, TKey>>();
        public Tree(Func<T, TKey> funcGetParentTKey, Func<T, TKey> funcGetSelfKey, TreeNode<T, TKey> tempRootNode)
        {
            Root = tempRootNode;
            _dic.Add(Root.Key, Root);
            FuncGetParentTKey = funcGetParentTKey;
            FuncGetSelfKey = funcGetSelfKey;
        }

        public void AddNode(T data)
        {
            CreateNode(data);
        }

        public void AddComplete()
        {
            List<TreeNode<T, TKey>> invalidNode = new List<TreeNode<T, TKey>>();
            foreach (var node in _dic.Values)
            {
                if (object.Equals(node.Data, default(T)) && !node.Key.Equals(Root.Key)) // not root node.
                {
                    invalidNode.Add(node);
                }
            }

            foreach (var node in invalidNode)
            {
                _dic.Remove(node.Key);
                foreach (var childNode in node.Childrens)
                {
                    Root.Childrens.Add(childNode);
                    childNode.ParentNode = Root;
                }
            }

            AfterAdded();
        }

        protected virtual void AfterAdded()
        {

        }

        public void DFS(Action<TreeNode<T, TKey>, int> ActionDo)
        {
            DFSEach(Root, 0, ActionDo);
        }

        private void DFSEach(TreeNode<T, TKey> node, int level, Action<TreeNode<T, TKey>, int> ActionDo)
        {
            ActionDo(node, level);
            foreach (var child in node.Childrens)
            {
                DFSEach(child, level + 1, ActionDo);
            }
        }

        public void DFS<TResult>(Func<TreeNode<T, TKey>, int, TResult, TResult> FuncDo)
        {
            var result = FuncDo(Root, 0, default(TResult));
            foreach (var child in Root.Childrens)
            {
                DFSEach(child, 1, result, FuncDo);
            }
        }

        private void DFSEach<TResult>(TreeNode<T, TKey> node, int level, TResult parentResultFromFunc, Func<TreeNode<T, TKey>, int, TResult, TResult> FuncDo)
        {
            var result = FuncDo(node, level, parentResultFromFunc);
            foreach (var child in node.Childrens)
            {
                DFSEach(child, level + 1, result, FuncDo);
            }
        }

        private TreeNode<T, TKey> CreateNode(T data)
        {
            var parentKey = FuncGetParentTKey(data);
            TreeNode<T, TKey> parentNode = null;

            if (_dic.TryGetValue(parentKey, out parentNode))
            {

            }
            else
            {
                parentNode = new TreeNode<T, TKey>() { Key = parentKey };
                _dic.Add(parentKey, parentNode);
            }

            var selfKey = FuncGetSelfKey(data);
            TreeNode<T, TKey> selfNode = null;
            if (_dic.TryGetValue(selfKey, out selfNode))
            {
                selfNode.Data = data;
            }
            else
            {
                selfNode = new TreeNode<T, TKey>()
                {
                    Data = data,
                    Key = selfKey
                };
                _dic.Add(selfKey, selfNode);
            }

            var isContains = (from item in parentNode.Childrens where item.Key.Equals(selfNode.Key) select item).Count() > 0;
            if (!isContains)
            {
                parentNode.Childrens.Add(selfNode);
                selfNode.ParentNode = parentNode;
            }

            return selfNode;
        }

        public TreeNode<T, TKey> GetNode(T data)
        {
            TreeNode<T, TKey> result;
            if (_dic.TryGetValue(FuncGetSelfKey(data), out result))
            {
                return result;
            }
            return null;
        }

        public TreeNode<T, TKey> GetParentNode(T data)
        {
            TreeNode<T, TKey> result = GetNode(data);
            if (result == null)
                throw new ArgumentException("data is not an element of the tree.");

            if (_dic.TryGetValue(FuncGetParentTKey(data), out result))
            {
                return result;
            }
            return null;
        }

        public T GetParentNodeData(T data)
        {
            var resultNode = GetParentNode(data);
            return resultNode == null ? default(T) : resultNode.Data;
        }

        public IEnumerable<TreeNode<T, TKey>> GetChildren(T data)
        {
            var resultNode = GetNode(data);
            return resultNode == null ? new List<TreeNode<T, TKey>>(0) : resultNode.Childrens;
        }

        public IEnumerable<T> GetChildrenData(T data)
        {
            var resultNode = GetNode(data);
            return resultNode == null ? new List<T>(0) : (from d in resultNode.Childrens select d.Data).AsEnumerable();
        }

        public IEnumerable<TreeNode<T, TKey>> GetParents(T data)
        {
            TreeNode<T, TKey> result = GetNode(data);
            if (result == null)
                throw new ArgumentException("data is not an element of the tree.");

            LinkedList<TreeNode<T, TKey>> parents = new LinkedList<TreeNode<T, TKey>>();

            do
            {
                result = GetParentNode(data);
                if (result == null)
                    throw new InvalidOperationException("The tree is not right.");
                parents.AddFirst(result);
            } while (!result.Key.Equals(Root.Key));
            return parents;
        }

        public IEnumerable<T> GetParentsData(T data)
        {
            var result = GetParents(data);
            return (from n in result select n.Data).AsEnumerable();
        }
    }

    /// <summary>
    /// a virtual root tree node.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TreeNode<T, TKey>
    {
        public List<TreeNode<T, TKey>> Childrens = new List<TreeNode<T, TKey>>(4);
        public T Data;
        public TKey Key;
        public TreeNode<T, TKey> ParentNode;
    }
}
