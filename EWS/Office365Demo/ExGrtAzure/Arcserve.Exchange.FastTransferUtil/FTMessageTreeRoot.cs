using Arcserve.Exchange.FastTransferUtil.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arcserve.Exchange.FastTransferUtil.CompoundFile;

namespace Arcserve.Exchange.FastTransferUtil
{
    public class FTMessageTreeRoot : FTNodeBase
    {
        internal MessageContent MessageContent;
        public FTMessageTreeRoot()
            : base()
        {
            MessageContent = FTFactory.Instance.CreateMessageContent();
            Children.Add(MessageContent);
        }

        private static readonly Guid CLSID_MailMessage = new Guid(0x00020D0B, 0x0000, 0x0000, 0xC0, 0x00, 0x0, 0x00, 0x0, 0x00, 0x00, 0x46);
        public override void WriteToCompoundFile(CompoundFileBuild build)
        {
            NativeDll.WriteClassStg(build.RootStorage, CLSID_MailMessage);
            base.WriteToCompoundFile(build);
        }
    }
}
