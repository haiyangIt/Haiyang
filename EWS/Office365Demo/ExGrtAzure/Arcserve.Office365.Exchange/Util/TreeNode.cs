using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Util
{
    public class TreeNode<T>
    {
        public List<TreeNode<T>> Childrens;
        public T Data;

        public TreeNode(IEnumerable<T> items)
        {
        }
    }
}
