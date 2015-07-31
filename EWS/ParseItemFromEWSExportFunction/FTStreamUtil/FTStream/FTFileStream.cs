using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FTStreamUtil.FTStream
{
    public class FTFileStream : IDisposable
    {
        private string _templateFile;
        private BinaryWriter _writer;
        private Container _container;
        public FTFileStream(string templateFile, BinaryWriter writer)
        {
            _templateFile = templateFile;
            _writer = writer;
            _container = new Container(_writer);
        }

        //class FTFilePart
        //{
        //    Int32 FxOpcodes;
        //    Int32 SubBufferLength;
        //    byte[] SubBuffer;
        //}

        public void WriteConfigs()
        {
            using (FileStream fileStream = new FileStream(_templateFile, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    int i = 0;
                    while (i < 3)
                    {
                        _writer.Write(reader.ReadInt32()); // write FxOpCodes;
                        var length = reader.ReadInt32();
                        _writer.Write(length);            // write SubBufferLength;

                        for (int index = 0; index < length; index++)
                        {
                            _writer.Write(reader.ReadByte());
                        }

                        i++;
                    }
                    

                }
            }
        }

        public void WriteConfigs(UInt16[] versions)
        {

        }

        public void Dispose()
        {
            _container.Dispose();
        }

        const int pageCount = 31680;

        public void WriteTransferData(IList<IFTTransferUnit> transferUnit)
        {
            foreach (IFTTransferUnit unit in transferUnit)
            {
                WriteTransferData(unit);
            }
            
        }
        public void WriteTransferData(IFTTransferUnit transferUnit)
        {
            _container.AddUnit(transferUnit);
        }


        class Container : IDisposable
        {
            private BinaryWriter _writer;
            private PageContainer _container;
            public Container(BinaryWriter writer)
            {
                _writer = writer;
                NewContainer();
            }

            class PageContainer : IDisposable
            {
                public int LeftCount { get { return pageCount - _usedCount; } }
                public int UsedCount { get { return _usedCount; } }

                private int _usedCount = 0;
                public byte[] Buffer { get; private set; }
                private BinaryWriter _writer;
                public PageContainer()
                {
                    Buffer = new byte[pageCount];
                    MemoryStream stream = new MemoryStream(Buffer);
                    _writer = new BinaryWriter(stream);
                }


                public void Write(byte[] unitBuffer)
                {
                    _writer.Write(unitBuffer);
                    _usedCount += unitBuffer.Length;
                }

                internal void WriteLeftCount(byte[] unitBuffer)
                {
                    byte[] leftBuffer = new byte[LeftCount];
                    Array.Copy(unitBuffer, 0, leftBuffer, 0, LeftCount);
                    Write(leftBuffer);
                }

                internal void WritePageCount(byte[] unitBuffer, int unitIndex)
                {
                    byte[] pageBuffer = new byte[pageCount];
                    Array.Copy(unitBuffer, unitIndex, pageBuffer, 0, pageCount);
                    Write(pageBuffer);
                }

                internal void WriteLast(byte[] unitBuffer, int unitIndex)
                {
                    byte[] lastBuffer = new byte[unitBuffer.Length - unitIndex];
                    Array.Copy(unitBuffer, unitIndex, lastBuffer, 0, lastBuffer.Length);
                    Write(lastBuffer);
                }

                public void Dispose()
                {
                    if (_writer != null)
                    {
                        _writer.Close();
                        _writer = null;
                    }
                }
            }

            public void AddUnit(IFTTransferUnit transferUnit)
            {
                var containerLeftCount = _container.LeftCount;
                var unitCount = transferUnit.BytesCount;

                byte[] buffer = new byte[unitCount];
                using (FTStreamWriter bwriter = new FTStreamWriter(buffer))
                {
                    ((IFTTreeNode)transferUnit).GetByteData(bwriter);
                }

                if (containerLeftCount > unitCount)
                {
                    _container.Write(buffer);
                }
                else if (containerLeftCount == unitCount)
                {
                    _container.Write(buffer);
                    OldContainerWrite(_container);
                    NewContainer();
                }
                else
                {

                    if (unitCount < pageCount)
                    {
                        OldContainerWrite(_container);
                        NewContainer();
                        _container.Write(buffer);
                    }
                    else
                    {
                        int unitIndex = 0;
                        _container.WriteLeftCount(buffer);
                        OldContainerWrite(_container);
                        NewContainer();
                        unitIndex += containerLeftCount;

                        while (unitCount - unitIndex >= pageCount)
                        {
                            _container.WritePageCount(buffer, unitIndex);
                            OldContainerWrite(_container);
                            NewContainer();
                            unitIndex += pageCount;
                        }

                        if (unitCount - unitIndex != 0)
                        {
                            _container.WriteLast(buffer, unitIndex);
                        }
                    }
                }
            }

            private void OldContainerWrite(PageContainer container)
            {
                _writer.Write((UInt32)FxOpcodes.TransferBuffer);
                _writer.Write((UInt32)container.UsedCount);
                _writer.Write(container.Buffer, 0, container.UsedCount);
                _writer.Flush();
            }

            private void NewContainer()
            {
                if (_container != null)
                {
                    _container.Dispose();
                    _container = null;
                }
                _container = new PageContainer();
            }

            public void Dispose()
            {
                if (_container != null)
                {
                    if (_container.UsedCount > 0)
                        OldContainerWrite(_container);
                    _container.Dispose();
                }
            }
        }
    }
}
