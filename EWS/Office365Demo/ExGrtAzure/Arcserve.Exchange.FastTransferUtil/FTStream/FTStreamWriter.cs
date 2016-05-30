using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.FTStream
{
    public class FTStreamWriter : IFTStreamWriter , IDisposable
    {
        private BinaryWriter _writer;
        private Stream _stream;
        public FTStreamWriter(byte[] buffer)
        {
            _stream = new MemoryStream(buffer);
            _writer = new BinaryWriter(_stream);
        }

        public FTStreamWriter(string fileName)
        {
            _stream = new FileStream(fileName, FileMode.CreateNew);
            _writer = new BinaryWriter(_stream);
        }

        public int Write(ushort value)
        {
            _writer.Write(value);
            return FTStreamConst.UInt16Size;
        }

        public int Write(uint value)
        {
            _writer.Write(value);
            return FTStreamConst.UInt32Size;
        }

        public int Write(ulong value)
        {
            _writer.Write(value);
            return FTStreamConst.UInt64Size;
        }

        public int Write(float value)
        {
            _writer.Write(value);
            return FTStreamConst.FloatSize;
        }

        public int Write(double data)
        {
            _writer.Write(data);
            return FTStreamConst.DoubleSize;
        }

        public int Write(DateTime data)
        {
            var value = data.ToFileTimeUtc();
            _writer.Write(value);
            return FTStreamConst.UInt64Size;
        }

        public int Write(bool data)
        {
            _writer.Write(data ? FTStreamConst.BoolenTrueInFT : FTStreamConst.BoolenFalseInFT);
            return FTStreamConst.BoolenSizeInFT;
        }

        public int Write(Guid data)
        {
            _writer.Write(data.ToByteArray());
            return FTStreamConst.GuidSize;
        }

        public int WriteUnicodeStringWithCodePage(string data, uint length, uint codepage)
        {
            int count = 0;
            var value = Encoding.GetEncoding((int)codepage).GetBytes(data);
            //todo judge terminate byte. and length;
            _writer.Write(value);
            if (value.Length + 2 == length)
            {
                _writer.Write((UInt16)0x0000);
                count += FTStreamConst.UInt16Size;
            }
            else if (value.Length != length)
                throw new NotImplementedException();
            return count + value.Length;
        }

        public int WriteAnsiString(string data, uint length)
        {
            int count = 0;
            foreach (char c in data)
            {
                _writer.Write((byte)c);
                count++;
            }
            if (count + 1 == length)
            {
                _writer.Write((byte)0x00);
                count++;
            }
            else if (count != length)
                throw new NotImplementedException();
            return count;
        }

        public int WriteBytes(byte[] data, uint length)
        {
            _writer.Write(data, 0, (int)length);
            return (int)length;
        }

        public int Write(byte data)
        {
            _writer.Write(data);
            return FTStreamConst.ByteSize;
        }

        public int WriteUnicodeString(string data)
        {
            var value = Encoding.Unicode.GetBytes(data);
            //todo judge terminate byte. and length;
            _writer.Write(value);
            _writer.Write((UInt16)0x0000);
            return value.Length + FTStreamConst.UInt16Size;
        }

        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
                _stream.Close();
                _stream.Dispose();
                _stream = null;
            }
        }
    }
}
