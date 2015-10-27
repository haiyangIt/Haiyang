using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsDataInterface
{
    public interface IItemData
    {
        string ParentFolderId { get; }
        string DisplayName { get; }
        DateTime? CreateTime { get; }
        string ItemId { get; }
        object Data { get; }
        string ItemClass { get; }
        int Size { get; }

        int ActualSize { get; }
        string Location { get; }
    }
    
}
