using EwsDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public interface IRestoreDestination
    {
        void InitOtherInformation(params object[] information);
        void WriteItem(IRestoreItemInformation item, byte[] itemData);
    }
}
