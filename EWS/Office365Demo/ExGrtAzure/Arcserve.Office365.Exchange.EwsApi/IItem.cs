using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.EwsApi
{
    /// <summary>
    /// An interface for getting item information from EWS.
    /// </summary>
    public interface IItem
    {
        List<Item> GetFolderItems(Folder folder);
        
        void ExportItem(Item item, Stream stream, EwsServiceArgument argument);
        void ImportItem(FolderId parentFolderId, byte[] itemData, EwsServiceArgument argument);
        void ImportItem(FolderId parentFolderId, Stream stream, EwsServiceArgument argument);

        bool IsItemNew(Item item, DateTime lastTime, DateTime thisTime);
        
        ExchangeService CurrentExchangeService { get; }

        void ExportEmlItem(Item itemInEws, MemoryStream emlStream, EwsServiceArgument argument);
    }
}
