using FastTransferUtil.CompoundFile;
using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FTStreamUtil
{
    public abstract class FTNodeBase : IFTTreeNode,IFTParseEvent
    {
        protected FTNodeBase()
        {
            _children = new List<IFTTreeNode>(2);
        }

        private IList<IFTTreeNode> _children;

        public void Parse(IFTStreamReader reader)
        {
            BeginParseOutput();
            BeginParse();

            ParseNode(reader);

            AfterParse();
            AfterParseOutput();
        }

        protected virtual void ParseNode(IFTStreamReader reader)
        {
            var children = Children;
            foreach (IFTTreeNode child in children)
            {
                child.Parse(reader);
            }
        }

        public virtual IList<IFTTreeNode> Children
        {
            get
            {
                return _children;
            }
        }

        public virtual void BeginParse()
        {
        }

        public virtual void BeginParseOutput()
        {
            LogWriter.Instance.IncrementIndent();
            LogWriter.Instance.Write(LogWriter.Instance.GetIndent());
            LogWriter.Instance.WriteLine(string.Format("{0} parse start:", this.GetType().Name));
        }

        public virtual void AfterParse()
        {
        }

        public virtual void AfterParseOutput()
        {
            LogWriter.Instance.Write(LogWriter.Instance.GetIndent());
            if (this is IFTTreeLeaf)
            {
                LogWriter.Instance.WriteLine(((IFTTreeLeaf)this).GetLeafString());
            }
            else
            {
                LogWriter.Instance.WriteLine(string.Format("{0} parse end.totalCount:{1}.", this.GetType().Name, this.Children.Count));
            }
            LogWriter.Instance.ResetIndent();
        }

        public virtual int GetByteData(IFTStreamWriter writer)
        {
            var children = Children;
            int count = 0;

            if(this is IFTTreeLeaf)
            {
                if (Children.Count != 0)
                    throw new NotSupportedException();
                count += ((IFTTreeLeaf)this).WriteLeafData(writer);
            }

            foreach (IFTTreeNode child in children)
            { 
                if (child is IFTTreeLeaf)
                {
                    count += ((IFTTreeLeaf)child).WriteLeafData(writer);
                }
                else
                    count += child.GetByteData(writer);
            }
            if (count != BytesCount)
                throw new NotSupportedException();
            return count;
        }

        private int _bytesCount = 0;
        public virtual int BytesCount
        {
            get 
            {
                if(_bytesCount == 0)
                {
                    if (this is IFTTreeLeaf)
                    {
                        if (Children.Count != 0)
                            throw new NotSupportedException();
                        _bytesCount += ((IFTTreeLeaf)this).BytesCount;
                    }

                    var children = Children;
                    foreach (IFTTreeNode child in children)
                    {
                        if (child is IFTTreeLeaf)
                        {
                            _bytesCount += ((IFTTreeLeaf)child).BytesCount;
                        }
                        else
                            _bytesCount += child.BytesCount;
                    }
                }
                return _bytesCount;
            }
        }

        public virtual void GetAllTransferUnit(IList<IFTTransferUnit> allTransferUnit)
        {
            if (this is IFTTreeLeaf)
            {
                if (Children.Count != 0)
                    throw new NotSupportedException();
                allTransferUnit.Add((IFTTransferUnit)this);
            }

            var children = Children;
            foreach (IFTTreeNode child in children)
            {
                if (child is IFTTransferUnit)
                {
                    allTransferUnit.Add((IFTTransferUnit)child);
                }
                else
                    child.GetAllTransferUnit(allTransferUnit);
            }
        }

        private byte[] _bytes;
        public byte[] Bytes
        {
            get
            {
                if (_bytes == null)
                {
                    _bytes = new byte[BytesCount];
                    using (FTStreamWriter bwriter = new FTStreamWriter(_bytes))
                    {
                        GetByteData(bwriter);
                    }
                }
                return _bytes;
            }
        }

        public uint Tag
        {
            get { return (uint)BitConverter.ToInt32(Bytes, 0); }
        }

        public virtual void WriteToCompoundFile(CompoundFileBuild build)
        {
            if(Children != null && Children.Count > 0)
            {
                foreach(var child in Children)
                {
                    child.WriteToCompoundFile(build);
                }
            }
        }
    }
}
