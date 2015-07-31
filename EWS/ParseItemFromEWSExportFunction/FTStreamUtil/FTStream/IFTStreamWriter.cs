using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.FTStream
{
    public interface IFTStreamWriter : IDisposable
    {
        int Write(UInt16 value);
        int Write(UInt32 value);
        int Write(UInt64 value);
        int Write(float value);

        int Write(double data);

        int Write(DateTime data);

        int Write(bool data);

        int Write(Guid data);

        int WriteUnicodeStringWithCodePage(string data, uint length, uint codepage);

        int WriteAnsiString(string data, uint length);

        int WriteBytes(byte[] data, uint length);

        int Write(byte data);

        int WriteUnicodeString(string data);
    }
}
