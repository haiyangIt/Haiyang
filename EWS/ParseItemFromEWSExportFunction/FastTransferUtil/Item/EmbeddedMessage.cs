using FTStreamUtil.FTStream;
using FTStreamUtil.Item.Marker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FastTransferUtil.CompoundFile;

namespace FTStreamUtil.Item
{
    public class EmbeddedMessage : FTNodeBase, IContentProcess
    {
        internal StartEmbedMarker StartEmbedMarker;
        internal MessageContent MessageContent;
        internal EndEmbedMarker EndEmbedMarker;
        private bool _hasData = false;

        public EmbeddedMessage()
            : base()
        {
            StartEmbedMarker = FTFactory.Instance.CreateStartEmbedMarker();
            MessageContent = FTFactory.Instance.CreateMessageContent();
            EndEmbedMarker = FTFactory.Instance.CreateEndEmbedMarker();

            Children.Add(StartEmbedMarker);
            Children.Add(MessageContent);
            Children.Add(EndEmbedMarker);
        }

        protected override void ParseNode(IFTStreamReader reader)
        {
            var propertyTag = reader.ReadPropertyTag();
            if (IsTagRight(propertyTag))
            {
                _hasData = true;
                base.ParseNode(reader);
            }
        }

        public override int GetByteData(IFTStreamWriter writer)
        {
            if (_hasData)
            {
                return base.GetByteData(writer);
            }
            return 0;
        }

        public override int BytesCount
        {
            get
            {
                if (_hasData)
                {
                    return base.BytesCount;
                }
                else
                    return 0;
            }
        }

        public override void GetAllTransferUnit(IList<FTStream.IFTTransferUnit> allTransferUnit)
        {
            if (_hasData)
                base.GetAllTransferUnit(allTransferUnit);
        }

        public bool IsTagRight(PropertyTag propertyTag)
        {
            return propertyTag.Data == PropertyTag.StartEmbed;
        }

        public override void WriteToCompoundFile(CompoundFileBuild build)
        {
            build.StartWriteEmbed();
            base.WriteToCompoundFile(build);
            build.EndWriteEmbed();
        }
    }
}
