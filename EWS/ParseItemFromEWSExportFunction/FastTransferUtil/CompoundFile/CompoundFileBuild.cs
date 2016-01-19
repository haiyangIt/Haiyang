/*
In C++ code, there are a few bugs:
1. If the type is CLSID or GUID, it must be treated as variable.
2. In bin, Embed Message may don't contain any content.
3. The number format in stream name must be X8 (Hex).
4. 

*/

using FTStreamUtil;
using FTStreamUtil.Item;
using FTStreamUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using FTStreamUtil.FTStream;
using FastTransferUtil.CompoundFile.MsgStruct;

namespace FastTransferUtil.CompoundFile
{
    public class CompoundFileBuild
    {
        public CompoundFileBuild()
        {

        }

        public IStorage RootStorage;
        private TopLevelStruct _topPropStruct;
        private BaseStruct _currentPropCollection;
        private Stack<BaseStruct> PropCollection = new Stack<BaseStruct>(4);

        internal void SetRootStorage(IStorage storage)
        {
            RootStorage = storage;
        }

        private BaseStruct CurrentPropCollection
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
}
