using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EwsDataInterface
{
    public interface IItemData : IItemBase
    {
        string ParentFolderId { get; }
        DateTime? CreateTime { get; }
        string ItemId { get; }
        object Data { get; }
        string ItemClass { get; }
        int Size { get; }
        int ActualSize { get; }
        string Location { get; }

        IItemData Clone();
    }

    public enum ExportType : byte
    {
        TransferBin = 1,
        Eml = 2,
        Msg = 3
    }

}
