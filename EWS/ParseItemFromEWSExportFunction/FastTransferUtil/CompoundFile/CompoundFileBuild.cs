using FTStreamUtil;
using FTStreamUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace FastTransferUtil.CompoundFile
{
    public class CompoundFileBuild
    {
        public CompoundFileBuild()
        {

        }

        public IStorage RootStorage;
        private TopLevelStruct _topPropStruct;
        private PropertyCollection _currentPropCollection;
        private Stack<PropertyCollection> PropCollection;

        private PropertyCollection CurrentPropCollection
        {
            get
            {
                if (_currentPropCollection == null)
                {
                    _currentPropCollection = PropCollection.Pop();
                }
                return _currentPropCollection;
            }
            set
            {
                if (_currentPropCollection != null)
                {
                    PropCollection.Push(_currentPropCollection);
                }

                _currentPropCollection = value;
            }
        }

        internal void EndWriteMessageContent()
        {
            CurrentPropCollection.Build();
            _currentPropCollection = null;
        }

        internal void StartWriteMessageContent()
        {
            if (_topPropStruct == null)
            {
                _topPropStruct = new TopLevelStruct(RootStorage);
                CurrentPropCollection = _topPropStruct;
            }
            else
            {
                CurrentPropCollection = ((EmbedStruct)_currentPropCollection).CreateMessageContent();
            }
        }

        internal void AddProperty(IPropValue propValue)
        {
            CurrentPropCollection.AddProperties(propValue);
        }

        internal void EndWriteAttachment()
        {
            CurrentPropCollection.Build();
            _currentPropCollection = null;
        }

        internal void EndWriteEmbed()
        {
            CurrentPropCollection.Build();
            _currentPropCollection = null;
        }

        internal void EndWriteRecipient()
        {
            CurrentPropCollection.Build();
            _currentPropCollection = null;
        }

        internal void SetRootStorage(IStorage storage)
        {
            RootStorage = storage;
        }

        internal void StartWriteAttachment()
        {
            CurrentPropCollection = ((MessageContentStruct)CurrentPropCollection).CreateAttachment();
        }

        internal void StartWriteEmbed()
        {
            CurrentPropCollection = ((AttachmentStruct)CurrentPropCollection).CreateEmbedStruct();
        }

        internal void StartWriteRecipient()
        {
            CurrentPropCollection = ((MessageContentStruct)CurrentPropCollection).CreateRecipient();
        }
    }

    public class TopLevelStruct : MessageContentStruct
    {
        private IStorage rootStorage;

        private Dictionary<int, IPropValue> _namedProperties;
        protected override Dictionary<int, IPropValue> NamedProperties
        {
            get
            {
                return _namedProperties;
            }
        }

        public TopLevelStruct(IStorage rootStorage) : base(null)
        {
            this.rootStorage = rootStorage;
            Storage = rootStorage;
        }

        public override void Build()
        {
            throw new NotImplementedException();
        }
    }

    public class MessageContentStruct : PropertyCollection
    {
        public MessageContentStruct(PropertyCollection parentStruct) : base(parentStruct)
        {

        }

        public List<AttachmentStruct> AttachmentProperties = new List<AttachmentStruct>();
        public List<RecipientStruct> RecipientProperties = new List<RecipientStruct>();
        public AttachmentStruct CreateAttachment()
        {
            var attachmentStruct = new AttachmentStruct(this, AttachmentProperties.Count);
            AttachmentProperties.Add(attachmentStruct);
            return attachmentStruct;
        }

        public RecipientStruct CreateRecipient()
        {
            var recipientStruct = new RecipientStruct(this, RecipientProperties.Count);
            RecipientProperties.Add(recipientStruct);
            return recipientStruct;
        }

        public override void Build()
        {
            
        }
    }

    public abstract class PropertyCollection
    {
        protected PropertyCollection(PropertyCollection parentStruct)
        {
            ParentStruct = parentStruct;
        }

        public Props Properties;

        protected virtual Dictionary<int, IPropValue> NamedProperties
        {
            get
            {
                return this.ParentStruct.NamedProperties;
            }
        }

        public virtual void AddProperties(IPropValue property)
        {
            Properties.AddProperty(property);
            if(property.PropInfo is NamePropInfo)
            {
                NamedProperties[property.PropertyId] = property;
            }
        }

        public abstract void Build();

        protected PropertyCollection ParentStruct
        {
            get; set;
        }

        internal virtual IStorage Storage
        {
            get; set;
        }

        protected virtual void BuildProperties()
        {
            IStream propertyStream = CreatePropertyStream();
            BuildHeader(propertyStream);
            foreach(IPropValue property in Properties.Properties)
            {
                BuildEachProperty(Storage, propertyStream, property);
            }
        }

        private void BuildEachProperty(IStorage storage, IStream propertyStream, IPropValue property)
        {
            throw new NotImplementedException();
        }

        private void BuildHeader(IStream propertyStream)
        {
            throw new NotImplementedException();
        }

        private IStream CreatePropertyStream()
        {
            throw new NotImplementedException();
        }
    }

    public class Props
    {
        public List<IPropValue> Properties = new List<IPropValue>();
        public void AddProperty(IPropValue property)
        {
            Properties.Add(property);
        }
    }

    public class AttachmentStruct : PropertyCollection
    {
        public EmbedStruct Embed;

        public AttachmentStruct(PropertyCollection parentStruct, int attachIndex) : base(parentStruct)
        {
            Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(attachIndex), true, parentStruct.Storage);
        }

        private string GetStorageName(int index)
        {
            return string.Format("__attach_version1.0_#{0}", index.ToString("X8"));
        }

        public override void Build()
        {
            throw new NotImplementedException();
        }

        public EmbedStruct CreateEmbedStruct()
        {
            Embed = new EmbedStruct(this);
            return Embed;
        }
    }

    public class RecipientStruct : PropertyCollection
    {
        public RecipientStruct(PropertyCollection parentStruct, int index) : base(parentStruct)
        {
            Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(index), true, parentStruct.Storage);
        }

        public override void Build()
        {
            throw new NotImplementedException();
        }

        private string GetStorageName(int index)
        {
            return string.Format("__recip_version1.0_#{0}", index.ToString("X8"));
        }
    }

    public class EmbedStruct : PropertyCollection
    {
        public MessageContentStruct MessageContent;

        public EmbedStruct(PropertyCollection parentStruct) : base(parentStruct)
        {
            Storage = CompoundFileUtil.Instance.GetChildStorage(GetStorageName(), true, parentStruct.Storage);
        }

        private string GetStorageName()
        {
            return "__substg1.0_3701000D";
        }

        public override void Build()
        {
            throw new NotImplementedException();
        }

        public MessageContentStruct CreateMessageContent()
        {
            MessageContent = new MessageContentStruct(this);
            MessageContent.Storage = Storage;
            return MessageContent;
        }
    }
}
