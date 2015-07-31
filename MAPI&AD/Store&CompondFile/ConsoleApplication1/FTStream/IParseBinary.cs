using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{

    public interface IParseBinary : IDisposable
    {
        /// <summary>
        /// 2 bytes no-signed int
        /// </summary>
        /// <returns></returns>
        UInt16 ReadUInt16();

        /// <summary>
        /// 4 bytes no-signed int;
        /// </summary>
        /// <returns></returns>
        UInt32 ReadUInt32();

        /// <summary>
        /// 8 bytes no-signed int;
        /// </summary>
        /// <returns></returns>
        UInt64 ReadUInt64();

        /// <summary>
        /// 4 bytes float
        /// </summary>
        /// <returns></returns>
        float ReadSingle();

        /// <summary>
        /// 8 bytes double
        /// </summary>
        /// <returns></returns>
        double ReadDouble();

        /// <summary>
        /// 8 bytes currency;
        /// </summary>
        /// <returns></returns>
        double ReadCurrency();

        /// <summary>
        /// 16 bytes guid
        /// </summary>
        /// <returns></returns>
        Guid ReadGuid();
        byte[] ReadBytes(int length);
        byte ReadByte();
        bool ReadBoolean();
        String ReadAnsiString();
        String ReadUnicodeString();
        String ReadUnicodeString(int length);
        String ReadUnicodeStringWithCodePage(int length, int codePage);

        /// <summary>
        /// 8 bytes DateTime
        /// </summary>
        /// <returns></returns>
        DateTime ReadDateTime();

        int Read(byte[] buffer, int offset, int length);

        void Seek(long position);
        long Position { get; }
        long Length { get; }

        bool CanRead { get; }
    }
}
