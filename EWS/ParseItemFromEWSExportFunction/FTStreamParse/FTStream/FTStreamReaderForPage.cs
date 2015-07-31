using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.FTStream
{
    public class FTStreamReaderForPage : IFTStreamReader
    {
        private FTStreamReader _reader;
        private IFTPage _ftPage;

        public FTStreamReaderForPage(IFTPage ftPage)
        {
            _ftPage = ftPage;
            if (ftPage != null)
                CanReadData(0);
        }

        private bool CanReadData(int length)
        {
            if(_reader == null || _reader.Position + length > _reader.Length)
            {
                if (_ftPage.IsLastPage)
                {
                    return false;
                }
                else
                {
                    int reading = 0;
                    if(_reader == null)
                        reading = length;
                    else
                    {
                        reading = length - (int)(_reader.Length - _reader.Position);
                    }
                    List<byte[]> bufferList = new List<byte[]>(2);
                    int readedCount = 0;
                    do
                    {
                        LogWriter.Instance.WriteLine("---------------------------------------------------------------------------------------------------");
                        LogWriter.Instance.WriteLine("current page is end. will read next page");
                        LogWriter.Instance.WriteLine("---------------------------------------------------------------------------------------------------");

                        byte[] bufferTemp = _ftPage.GetNextPageBuffer();
                        bufferList.Add(bufferTemp);
                        reading -= bufferTemp.Length;
                        readedCount += bufferTemp.Length;

                        LogWriter.Instance.WriteLine("---------------------------------------------------------------------------------------------------");
                        LogWriter.Instance.Write("next page bytes index is :");
                        LogWriter.Instance.Write(_ftPage.CurrentPageIndex.ToString());
                        LogWriter.Instance.Write(". Length is :");
                        LogWriter.Instance.WriteLine(bufferTemp.Length.ToString());
                        LogWriter.Instance.WriteLine("---------------------------------------------------------------------------------------------------");
                    } while (reading > 0);

                    byte[] buffer = new byte[readedCount];
                    reading = 0;
                    foreach (byte[] eachBuffer in bufferList)
                    {
                        Array.Copy(eachBuffer, 0, buffer, reading, eachBuffer.Length);
                        reading += eachBuffer.Length;
                    }

                    if (_reader == null)
                    {
                        _reader = new FTStreamReader(buffer);
                    }
                    else if (_reader.Position == _reader.Length)
                    {
                        _reader.Dispose();
                        _reader = new FTStreamReader(buffer);
                    }
                    else
                    {
                        byte[] lastBuffer = _reader.ReadBytes((int)(_reader.Length - _reader.Position));
                        byte[] allBuffer = new byte[lastBuffer.Length + buffer.Length];
                        Array.Copy(lastBuffer, 0, allBuffer, 0, lastBuffer.Length);
                        Array.Copy(buffer, 0, allBuffer, lastBuffer.Length, buffer.Length);
                        _reader.Dispose();
                        _reader = new FTStreamReader(allBuffer);
                    }

                       
                }
            }
            return true;
        }

        private bool CanReadString()
        {
            if (_ftPage.IsLastPage)
                return false;
            byte[] buffer = _ftPage.GetNextPageBuffer();
            if (_reader != null)
            {
                _reader.Dispose();
            }
            _reader = new FTStreamReader(buffer);
            return true;
        }

        public ushort ReadUInt16()
        {
            if (CanReadData(FTStreamConst.UInt16Size))
            {
                return _reader.ReadUInt16();
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public uint ReadUInt32()
        {
            if (CanReadData(FTStreamConst.UInt32Size))
            {
                return _reader.ReadUInt32();
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }


        public uint ReadUInt32(bool isPositionChange)
        {
            if (CanReadData(FTStreamConst.UInt32Size))
            {
                return _reader.ReadUInt32(isPositionChange);
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public ulong ReadUInt64()
        {
            if (CanReadData(FTStreamConst.UInt64Size))
            {
                return _reader.ReadUInt64();
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public float ReadSingle()
        {
            if (CanReadData(FTStreamConst.FloatSize))
            {
                return _reader.ReadSingle();
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public double ReadDouble()
        {
            if (CanReadData(FTStreamConst.DoubleSize))
            {
                return _reader.ReadDouble();
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public double ReadCurrency()
        {
            if (CanReadData(FTStreamConst.CurrencySize))
            {
                return _reader.ReadCurrency();
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public Guid ReadGuid()
        {
            if (CanReadData(FTStreamConst.GuidSize))
            {
                return _reader.ReadGuid();
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public byte[] ReadBytes(int length)
        {
            if (CanReadData(length))
            {
                return _reader.ReadBytes(length);
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public byte ReadByte()
        {
            if (CanReadData(FTStreamConst.ByteSize))
            {
                return _reader.ReadByte();
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public bool ReadBoolean()
        {
            if (CanReadData(FTStreamConst.BooleanSize))
            {
                return _reader.ReadBoolean();
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public string ReadAnsiString(out bool isReadStringTerminate)
        {
            return ReadUnicodeAndAnsiString(false, out isReadStringTerminate);
        }

        public string ReadAnsiString(int length)
        {
            if (CanReadData(length))
            {
                return _reader.ReadAnsiString(length);
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        private string ReadUnicodeAndAnsiString(bool isUnicode, out bool isReadStringTerminate)
        {
            StringBuilder sb = new StringBuilder();
            isReadStringTerminate = false;
            while (true)
            {
                if (_reader == null || _reader.IsEnd)
                {
                    if (!CanReadString())
                    {
                        break;
                    }
                }
                if (isUnicode)
                    sb.Append(_reader.ReadUnicodeString(out isReadStringTerminate));
                else
                    sb.Append(_reader.ReadAnsiString(out isReadStringTerminate));
                if (isReadStringTerminate)
                    break;
            }

            if (!isReadStringTerminate)
                throw new ArgumentException("Read string error for no terminate char.");
            return sb.ToString();
        }

        public string ReadUnicodeString(out bool isReadStringTerminate)
        {
            return ReadUnicodeAndAnsiString(true, out isReadStringTerminate);
        }

        public string ReadUnicodeString(int length)
        {
            if (CanReadData(length))
            {
                return _reader.ReadUnicodeString(length);
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public string ReadUnicodeStringWithCodePage(int length, int codePage)
        {
            if (CanReadData(length))
            {
                return _reader.ReadUnicodeStringWithCodePage(length, codePage);
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public DateTime ReadDateTime()
        {
            if (CanReadData(FTStreamConst.DateTimeSize))
            {
                return _reader.ReadDateTime();
            }
            else
                throw new OutOfMemoryException("The index is end of buffer.");
        }

        public long Position
        {
            get { return _reader.Position; }
        }

        public long Length
        {
            get { return _reader.Length; }
        }

        public void Dispose()
        {
            if (_reader != null)
                _reader.Dispose();
        }

        public bool IsEnd
        {
            get {
                return _reader.IsEnd && _ftPage.IsLastPage; 
            }
        }


        public Item.PropertyTag ReadPropertyTag()
        {
            return _reader.ReadPropertyTag();
        }
    }
}
