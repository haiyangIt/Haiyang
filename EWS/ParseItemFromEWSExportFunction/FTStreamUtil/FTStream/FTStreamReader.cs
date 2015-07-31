using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FTStreamUtil.FTStream
{
    public class FTStreamReader : IFTStreamReader
    {
        private BinaryReader _reader;
        public FTStreamReader(byte[] buffer)
        {
            MemoryStream stream = new MemoryStream(buffer);
            _reader = new BinaryReader(stream);
        }

        public ushort ReadUInt16()
        {
            return _reader.ReadUInt16();
        }

        public uint ReadUInt32()
        {
            return ReadUInt32(true);
        }

        public uint ReadUInt32(bool isPositionChange)
        {
            var result = _reader.ReadUInt32();
            if (!isPositionChange)
                _reader.BaseStream.Seek(_reader.BaseStream.Position - FTStreamConst.UInt32Size, SeekOrigin.Begin);
            return result;
        }

        public ulong ReadUInt64()
        {
            return _reader.ReadUInt64();
        }

        public float ReadSingle()
        {
            return _reader.ReadSingle();
        }

        public double ReadDouble()
        {
            return _reader.ReadDouble();
        }

        public double ReadCurrency()
        {
            return _reader.ReadDouble();
        }

        public Guid ReadGuid()
        {
            byte[] bytes = ReadBytes(FTStreamConst.GuidSize);
            return new Guid(bytes);
        }

        public byte[] ReadBytes(int length)
        {
            return _reader.ReadBytes(length);
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public bool ReadBoolean()
        {
            UInt16 result = _reader.ReadUInt16();
            return result == FTStreamConst.BoolenTrueInFT;
        }

        public string ReadAnsiString(out bool isReadStringTerminate)
        {
            StringBuilder sb = new StringBuilder();
            isReadStringTerminate = false;
            while (IsEnd)
            {
                byte b = ReadByte();
                if (b == 0x00)
                {
                    isReadStringTerminate = true;
                    break;
                }
                sb.Append((char)b);
            }
            return sb.ToString();
        }

        public string ReadAnsiString(int length)
        {
            StringBuilder sb = new StringBuilder();
            int readed = 0;
            while (readed < length)
            {
                byte b = ReadByte();
                sb.Append((char)b);
            }
            return sb.ToString();
        }

        public string ReadUnicodeString(out bool isReadStringTerminate)
        {
            List<byte> bytes = new List<byte>();
            isReadStringTerminate = false;
            while (!IsEnd)
            {
                byte byte1 = ReadByte();
                byte byte2 = ReadByte();
                if (byte1 == 0x00 && byte2 == 0x00)
                {
                    isReadStringTerminate = true;
                    break;
                }
                bytes.Add(byte1);
                bytes.Add(byte2);
            }
            return Encoding.Unicode.GetString(bytes.ToArray());
        }

        public string ReadUnicodeString(int length)
        {
            byte[] bytes = ReadBytes(length);
            return Encoding.Default.GetString(bytes);
        }

        public string ReadUnicodeStringWithCodePage(int length, int codePage)
        {
            byte[] bytes = ReadBytes(length);
            return Encoding.GetEncoding(codePage).GetString(bytes);
        }

        public DateTime ReadDateTime()
        {
            UInt64 fileTime = ReadUInt64();
            return DateTime.FromFileTimeUtc((long)fileTime);
        }

        public long Position
        {
            get { return _reader.BaseStream.Position; }
        }

        public long Length
        {
            get { return _reader.BaseStream.Length; }
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }
        }

        public bool IsEnd
        {
            get { return Position >= Length; }
        }
    }
}
