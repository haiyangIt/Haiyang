using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public class PropertyTypeReader : IParseBinary
    {
        private const int UInt16Size = 2;
        private const int UInt32Size = 4;
        private const int UInt64Size = 8;
        private const int FloatSize = 4;
        private const int DoubleSize = 8;
        private const int GuidSize = 16;
        private const int DateTimeSize = 16;
        private const int CurrencySize = 8;

        /// <summary>
        /// Boolen size in fast transfer is 2
        /// </summary>
        private const int BoolenSizeInFT = 2;
        private const ushort BoolenTrueInFT = 0x0001;
        private const ushort BoolenFalseInFT = 0x0000;



        private byte[] _buffer;
        private BinaryReader _reader;
        public PropertyTypeReader(byte[] buffer)
        {
            _buffer = buffer;
            MemoryStream stream = new MemoryStream(_buffer);
            _reader = new BinaryReader(stream);
        }

        public ushort ReadUInt16()
        {
            return _reader.ReadUInt16();
        }

        public uint ReadUInt32()
        {
            return _reader.ReadUInt32();
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
            byte[] bytes = ReadBytes(GuidSize);
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
            return result == BoolenTrueInFT;
        }

        public string ReadAnsiString()
        {
            StringBuilder sb = new StringBuilder();
            while (CanRead)
            {
                byte b = ReadByte();
                if (b == 0x00)
                {
                    break;
                }
                sb.Append((char)b);
            }
            return sb.ToString();
        }

        public string ReadUnicodeString()
        {
            List<byte> bytes = new List<byte>();
            while (CanRead)
            {
                byte byte1 = ReadByte();
                byte byte2 = ReadByte();
                if (byte1 == 0x00 && byte2 == 0x00)
                {
                    break;
                }
                bytes.Add(byte1);
                bytes.Add(byte2);
            }
            return Encoding.Default.GetString(bytes.ToArray());
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

        public int Read(byte[] buffer, int offset, int length)
        {
            return _reader.Read(buffer, offset, length);
        }

        public void Seek(long position)
        {
            _reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        public long Position
        {
            get { return _reader.BaseStream.Position; }
        }

        public long Length
        {
            get { return _buffer.Length; }
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }
        }

        public bool CanRead { get { return _reader.BaseStream.CanRead; } }


    }
}
