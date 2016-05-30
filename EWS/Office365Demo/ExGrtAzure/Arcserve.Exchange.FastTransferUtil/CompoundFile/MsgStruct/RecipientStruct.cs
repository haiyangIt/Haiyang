using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Arcserve.Exchange.FastTransferUtil.CompoundFile.MsgStruct.Helper;


namespace Arcserve.Exchange.FastTransferUtil.CompoundFile.MsgStruct
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
            if (Storage != null)
            {
                if (!hasError && !isParser)
                    Storage.Commit(0);
                CompoundFileUtil.Instance.ReleaseComObj(Storage);
            }
        }

        protected override void CreateSelfStorageForBuild()
        {
            if (Storage == null)
            {
                Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(_recpIndex), true, ParentStruct.Storage);
            }
        }

        protected override void ParserHeader(IStream propertyHeaderStream, ref int readCount)
        {
            propertyHeaderStream.ReadInt64(ref readCount);
        }

        protected override void GetStorageForParser()
        {
            if (Storage == null)
            {
                Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(_recpIndex), false, ParentStruct.Storage);
            }
        }
    }
}
