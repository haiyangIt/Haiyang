using FTStreamUtil.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.FTStream
{
    public interface IFTStreamReader : IDisposable
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

        UInt32 ReadUInt32(bool isPositionChange);

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
        String ReadAnsiString(out bool isReadStringTerminate);
        String ReadAnsiString(int length);
        String ReadUnicodeString(out bool isReadStringTerminate);
        String ReadUnicodeString(int length);
        String ReadUnicodeStringWithCodePage(int length, int codePage);

        /// <summary>
        /// 8 bytes DateTime
        /// </summary>
        /// <returns></returns>
        DateTime ReadDateTime();

        PropertyTag ReadPropertyTag();

        long Position { get; }
        long Length { get; }
        bool IsEnd { get; }
    }
}
