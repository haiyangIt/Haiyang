using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public interface IRestoreDestination : IDisposable
    {
        void InitOtherInformation(params object[] information);
        void WriteItem(IRestoreItemInformation item, byte[] itemData);
        void RestoreComplete(bool success, Exception ex);
        ExportType ExportType { get; }
    }

    public interface IRestoreDestinationEx: IDisposable
    {
        void SetOtherInformation(params object[] args);
        void DealItem(string id, string displayName, byte[] itemData, Stack<IItemBase> dealItemStack);
        void DealFolder(string displayName, Stack<IItemBase> dealItemStack);
        void DealMailbox(string displayName, Stack<IItemBase> dealItemStack);
        void DealOrganization(string organization, Stack<IItemBase> dealItemStack);

        void RestoreComplete(bool success, Exception ex);

        ExportType ExportType { get; }
    }
}
