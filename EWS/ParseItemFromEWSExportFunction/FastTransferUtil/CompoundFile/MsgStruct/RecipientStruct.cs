using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using FastTransferUtil.CompoundFile.MsgStruct.Helper;


namespace FastTransferUtil.CompoundFile.MsgStruct
{
    public class RecipientStruct : BaseStruct
    {
        protected override string Name
        {
            get
            {
                return string.Format("Recipient_{0}", _recpIndex);
            }
        }

        private readonly int _recpIndex;

        public RecipientStruct(BaseStruct parentStruct, int index) : base(parentStruct)
        {
            _recpIndex = index;
            Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(index), true, parentStruct.Storage);
        }

        private string GetStorageName(int index)
        {
            return string.Format("__recip_version1.0_#{0:X8}", index);
        }

        protected override void BuildHeader(IStream propertyStream)
        {
            // 1.1.1 Set 8 bytes reserve.
            propertyStream.WriteZero(8);
        }

        protected override void Release(bool hasError)
        {
            if (!hasError)
                Storage.Commit(0);
            CompoundFileUtil.Instance.ReleaseComObj(Storage);
        }
    }
}
