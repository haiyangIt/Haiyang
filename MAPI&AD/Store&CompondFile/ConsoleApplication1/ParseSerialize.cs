using System;
using System.Runtime.InteropServices;
using System.Text;

public class ParseException : ApplicationException
{
    public ParseException(uint lid, string message):base(message)
    { }

}


public static class CTSGlobals
{
    public static Encoding AsciiEncoding;
    public const int ReadBufferSize = 0x4000;
    public static Encoding UnicodeEncoding;

    static CTSGlobals()
    {
        AsciiEncoding = Encoding.GetEncoding("us-ascii");
        UnicodeEncoding = Encoding.GetEncoding("utf-16");
        return;
    }
}


internal static class ExBitConverter
{
    public static string ReadAsciiString(byte[] buffer, int offset)
    {
        int num;
        StringBuilder builder;
        int num2;
        num = 0;
        num = offset;
        goto Label_000A;
    Label_0006:
        num += 1;
    Label_000A:
        if ((num < ((int)buffer.Length)) && (buffer[num] != 0))
        {
            goto Label_0006;
        }
        builder = new StringBuilder(num - offset);
        num2 = offset;
        goto Label_003E;
    Label_0022:
        builder.Append((buffer[num2] < 0x80) ? buffer[num2] : 0x3f);
        num2 += 1;
    Label_003E:
        if (num2 < num)
        {
            goto Label_0022;
        }
        return builder.ToString();
    }

    public static byte[] ReadByteArray(byte[] buffer, int offset, int length)
    {
        byte[] buffer2;
        buffer2 = new byte[length];
        Array.Copy(buffer, offset, buffer2, 0, length);
        return buffer2;
    }

    public static unsafe Guid ReadGuid(byte[] buffer, int offset)
    {
        byte[] bytes = new byte[16];
        Array.Copy(buffer, offset, bytes, 0, 16);
        return new Guid(bytes);
    }

    //public static unsafe Guid ReadGuid(byte[] buffer, int offset)
    //{
    //    Guid guid = Guid.Empty;
    //    byte* numPtr;
    //    int num;
    //    if (offset < 0)
    //    {
    //        goto Label_0011;
    //    }
    //    if (offset <= (((int)buffer.Length) - sizeof(Guid)))
    //    {
    //        goto Label_001C;
    //    }
    //Label_0011:
    //    throw new ArgumentOutOfRangeException("offset");
    //Label_001C:
    //    IntPtr pGUID = Marshal.AllocHGlobal(Marshal.SizeOf(guid));

    //    Marshal.StructureToPtr(guid, pGUID, false);
    //    numPtr = (byte*)(void*)pGUID;
    //    num = 0;
    //    goto Label_0055;
    //Label_0024:
    //    *((sbyte*)(numPtr + num)) = (sbyte)buffer[offset];
    //    *((sbyte*)(numPtr + (num + 1))) = (sbyte)buffer[offset + 1];
    //    *((sbyte*)(numPtr + (num + 2))) = (sbyte)buffer[offset + 2];
    //    *((sbyte*)(numPtr + (num + 3))) = (sbyte)buffer[offset + 3];
    //    offset += 4;
    //    num += 4;
    //Label_0055:
    //    if (num < sizeof(Guid))
    //    {
    //        goto Label_0024;
    //    }
    //    return guid;
    //}

    public static int Write(char value, byte[] buffer, int offset)
    {
        return Write((short)value, buffer, offset);
    }

    public static unsafe int Write(double value, byte[] buffer, int offset)
    {
        return Write(*((long*)&value), buffer, offset);
    }

    public static unsafe int Write(Guid value, byte[] buffer, int offset)
    {
        var bytes = value.ToByteArray();
        Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        return bytes.Length;
    //    byte* numPtr;
    //    int num;

    //    IntPtr pGUID = Marshal.AllocHGlobal(Marshal.SizeOf(value));

    //    Marshal.StructureToPtr(value, pGUID, false);

    //    numPtr = (byte*)(void*)(pGUID);
    //    num = 0;
    //    goto Label_0039;
    //Label_0008:
    //    buffer[offset] = *((byte*)(numPtr + num));
    //    buffer[offset + 1] = *((byte*)(numPtr + (num + 1)));
    //    buffer[offset + 2] = *((byte*)(numPtr + (num + 2)));
    //    buffer[offset + 3] = *((byte*)(numPtr + (num + 3)));
    //    offset += 4;
    //    num += 4;
    //Label_0039:
    //    if (num < 0x10)
    //    {
    //        goto Label_0008;
    //    }
    //    return 0x10;
    }

    public static int Write(short value, byte[] buffer, int offset)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        return 2;
    }

    public static int Write(int value, byte[] buffer, int offset)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 0x10);
        buffer[offset + 3] = (byte)(value >> 0x18);
        return 4;
    }

    public static int Write(long value, byte[] buffer, int offset)
    {
        Write((int)value, buffer, offset);
        Write((int)(value >> 0x20), buffer, offset + 4);
        return 8;
    }

    public static unsafe int Write(float value, byte[] buffer, int offset)
    {
        return Write(*((int*)&value), buffer, offset);
    }

    public static int Write(ushort value, byte[] buffer, int offset)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        return 2;
    }

    public static int Write(uint value, byte[] buffer, int offset)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 0x10);
        buffer[offset + 3] = (byte)(value >> 0x18);
        return 4;
    }

    public static int Write(ulong value, byte[] buffer, int offset)
    {
        return Write(value, buffer, offset);
    }

    public static int Write(string value, bool unicode, byte[] buffer, int offset)
    {
        return Write(value, value.Length, unicode, buffer, offset);
    }

    public static int Write(string value, int maxCharCount, bool unicode, byte[] buffer, int offset)
    {
        int num;
        int num2;
        int num3;
        num = Math.Min(value.Length, maxCharCount);
        if (unicode != false)
        {
            goto Label_004D;
        }
        num2 = 0;
        goto Label_003B;
    Label_0014:
        buffer[offset++] = (byte)((value[num2] < 0x80) ? ((byte)value[num2]) : 0x3f);
        num2 += 1;
    Label_003B:
        if (num2 < num)
        {
            goto Label_0014;
        }
        buffer[offset++] = 0;
        return (num + 1);
    Label_004D:
        num3 = CTSGlobals.UnicodeEncoding.GetBytes(value, 0, num, buffer, offset);
        num3 += Write(0, buffer, offset + num3);
        return num3;
    }
}



public static class ParseSerialize
{
    private static readonly byte[] emptyByteArray;
    public static readonly long MaxFileTime;
    public static readonly long MinFileTime;
    public static readonly DateTime MinFileTimeDateTime;
    public const int SizeOfByte = 1;
    public const int SizeOfDouble = 8;
    public const int SizeOfFileTime = 8;
    public const int SizeOfGuid = 0x10;
    public const int SizeOfInt16 = 2;
    public const int SizeOfInt32 = 4;
    public const int SizeOfInt64 = 8;
    public const int SizeOfSingle = 4;
    public const int SizeOfUnicodeChar = 2;

    static unsafe ParseSerialize()
    {
        MinFileTime = 0L;
        MaxFileTime = DateTime.MaxValue.ToFileTimeUtc();
        MinFileTimeDateTime = DateTime.FromFileTimeUtc(MinFileTime);
        emptyByteArray = new byte[0];
        return;
    }

    public static void CheckBounds(int pos, int posMax, int sizeNeeded)
    {
        if (TryCheckBounds(pos, posMax, sizeNeeded) != false)
        {
            goto Label_001F;
        }
        throw new ParseException(0xa478, "Request would overflow buffer");
    Label_001F:
        return;
    }

    public static void CheckBounds(int pos, byte[] buffer, int sizeNeeded)
    {
        if (buffer == null)
        {
            goto Label_000D;
        }
        CheckBounds(pos, (int)buffer.Length, sizeNeeded);
    Label_000D:
        return;
    }

    internal static void CheckCount(uint count, int elementSize, int availableSize)
    {
        if (TryCheckCount(count, elementSize, availableSize) != false)
        {
            goto Label_001F;
        }
        throw new ParseException(0xe478, "TryCheckCount failed");
    Label_001F:
        return;
    }

    public static bool CheckOffsetLength(int maxOffset, int offset, int length)
    {
        if (offset < 0)
        {
            goto Label_0016;
        }
        if (length < 0)
        {
            goto Label_0016;
        }
        if (offset > maxOffset)
        {
            goto Label_0016;
        }
        return ((length > (maxOffset - offset)) == false);
    Label_0016:
        return false;
    }

    public static bool CheckOffsetLength(byte[] buffer, int offset, int length)
    {
        return CheckOffsetLength((int)buffer.Length, offset, length);
    }

    public static unsafe bool GetBoolean(byte[] buff, ref int pos, int posMax)
    {
        bool flag;
        if (TryGetBoolean(buff, ref pos, posMax, out flag) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xe9dc, "Request would overflow buffer");
    Label_0021:
        return flag;
    }

    public static unsafe byte GetByte(byte[] buff, ref int pos, int posMax)
    {
        byte num;
        if (TryGetByte(buff, ref pos, posMax, out num) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0x8ddc, "Request would overflow buffer");
    Label_0021:
        return num;
    }

    public static unsafe byte[] GetByteArray(byte[] buff, ref int pos, int posMax)
    {
        byte[] buffer;
        if (TryGetByteArray(buff, ref pos, posMax, out buffer) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xdfdc, "Request would overflow buffer");
    Label_0021:
        return buffer;
    }

    public static unsafe double GetDouble(byte[] buff, ref int pos, int posMax)
    {
        double num;
        if (TryGetDouble(buff, ref pos, posMax, out num) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xc5dc, "Request would overflow buffer");
    Label_0021:
        return num;
    }

    public static unsafe uint GetDword(byte[] buff, ref int pos, int posMax)
    {
        uint num;
        if (TryGetDword(buff, ref pos, posMax, out num) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xc1dc, "Request would overflow buffer");
    Label_0021:
        return num;
    }

    public static unsafe float GetFloat(byte[] buff, ref int pos, int posMax)
    {
        float num;
        if (TryGetFloat(buff, ref pos, posMax, out num) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xd1dc, "Request would overflow buffer");
    Label_0021:
        return num;
    }

    public static unsafe Guid GetGuid(byte[] buff, ref int pos, int posMax)
    {
        Guid guid;
        if (TryGetGuid(buff, ref pos, posMax, out guid) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0x81dc, "Request would overflow buffer");
    Label_0021:
        return guid;
    }

    public static int GetLengthOfUtf8String(byte[] buffer, int offset, int length)
    {
        if (length == 0)
        {
            goto Label_0011;
        }
        return Encoding.UTF8.GetCharCount(buffer, offset, length);
    Label_0011:
        return 0;
    }

    public static unsafe byte[][] GetMVBinary(byte[] buff, ref int pos, int posMax)
    {
        byte[][] bufferArray;
        if (TryGetMVBinary(buff, ref pos, posMax, out bufferArray) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xe1dc, "Request would overflow buffer");
    Label_0021:
        return bufferArray;
    }

    public static unsafe Guid[] GetMVGuid(byte[] buff, ref int pos, int posMax)
    {
        Guid[] guidArray;
        if (TryGetMVGuid(buff, ref pos, posMax, out guidArray) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xb1dc, "Request would overflow buffer");
    Label_0021:
        return guidArray;
    }

    public static unsafe short[] GetMVInt16(byte[] buff, ref int pos, int posMax)
    {
        short[] numArray;
        if (TryGetMVInt16(buff, ref pos, posMax, out numArray) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0x91dc, "Request would overflow buffer");
    Label_0021:
        return numArray;
    }

    public static unsafe int[] GetMVInt32(byte[] buff, ref int pos, int posMax)
    {
        int[] numArray;
        if (TryGetMVInt32(buff, ref pos, posMax, out numArray) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0x99dc, "Request would overflow buffer");
    Label_0021:
        return numArray;
    }

    public static unsafe long[] GetMVInt64(byte[] buff, ref int pos, int posMax)
    {
        long[] numArray;
        if (TryGetMVInt64(buff, ref pos, posMax, out numArray) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0x89dc, "Request would overflow buffer");
    Label_0021:
        return numArray;
    }

    public static unsafe double[] GetMVR8(byte[] buff, ref int pos, int posMax)
    {
        double[] numArray;
        if (TryGetMVR8(buff, ref pos, posMax, out numArray) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xa9dc, "Request would overflow buffer");
    Label_0021:
        return numArray;
    }

    public static unsafe float[] GetMVReal32(byte[] buff, ref int pos, int posMax)
    {
        float[] numArray;
        if (TryGetMVReal32(buff, ref pos, posMax, out numArray) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xf9dc, "Request would overflow buffer");
    Label_0021:
        return numArray;
    }

    public static unsafe string[] GetMVString8(byte[] buff, ref int pos, int posMax)
    {
        string[] strArray;
        if (TryGetMVString8(buff, ref pos, posMax, out strArray) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0x9fdc, "Request would overflow buffer");
    Label_0021:
        return strArray;
    }

    public static unsafe DateTime[] GetMVSysTime(byte[] buff, ref int pos, int posMax)
    {
        DateTime[] timeArray;
        if (TryGetMVSysTime(buff, ref pos, posMax, out timeArray) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xc9dc, "Request would overflow buffer");
    Label_0021:
        return timeArray;
    }

    public static unsafe string[] GetMVUnicode(byte[] buff, ref int pos, int posMax)
    {
        string[] strArray;
        if (TryGetMVUnicode(buff, ref pos, posMax, out strArray) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xafdc, "Request would overflow buffer");
    Label_0021:
        return strArray;
    }

    public static unsafe ulong GetQword(byte[] buff, ref int pos, int posMax)
    {
        ulong num;
        if (TryGetQword(buff, ref pos, posMax, out num) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0x85dc, "Request would overflow buffer");
    Label_0021:
        return num;
    }

    public static unsafe string GetStringFromASCII(byte[] buff, ref int pos, int posMax)
    {
        string str;
        if (TryGetStringFromASCII(buff, ref pos, posMax, out str) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xa1dc, "Request would overflow buffer");
    Label_0021:
        return str;
    }

    public static unsafe string GetStringFromASCII(byte[] buff, ref int pos, int posMax, int charCount)
    {
        string str;
        if (TryGetStringFromASCII(buff, ref pos, posMax, charCount, out str) != false)
        {
            goto Label_0022;
        }
        throw new ParseException(0x8ddc, "Request would overflow buffer");
    Label_0022:
        return str;
    }

    public static unsafe string GetStringFromASCIINoNull(byte[] buff, ref int pos, int posMax, int charCount)
    {
        string str;
        if (TryGetStringFromASCIINoNull(buff, ref pos, posMax, charCount, out str) != false)
        {
            goto Label_0022;
        }
        throw new ParseException(0xbfdc, "Request would overflow buffer");
    Label_0022:
        return str;
    }

    public static unsafe string GetStringFromUnicode(byte[] buff, ref int pos, int posMax)
    {
        string str;
        if (TryGetStringFromUnicode(buff, ref pos, posMax, out str) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xcddc, "Request would overflow buffer");
    Label_0021:
        return str;
    }

    public static unsafe string GetStringFromUnicode(byte[] buff, ref int pos, int posMax, int byteCount)
    {
        string str;
        if (TryGetStringFromUnicode(buff, ref pos, posMax, byteCount, out str) != false)
        {
            goto Label_0022;
        }
        throw new ParseException(0xb5dc, "Request would overflow buffer");
    Label_0022:
        return str;
    }

    public static unsafe DateTime GetSysTime(byte[] buff, ref int pos, int posMax)
    {
        DateTime time;
        if (TryGetSysTime(buff, ref pos, posMax, out time) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xb9dc, "Request would overflow buffer");
    Label_0021:
        return time;
    }

    public static unsafe ushort GetWord(byte[] buff, ref int pos, int posMax)
    {
        ushort num;
        if (TryGetWord(buff, ref pos, posMax, out num) != false)
        {
            goto Label_0021;
        }
        throw new ParseException(0xd9dc, "Request would overflow buffer");
    Label_0021:
        return num;
    }

    public static string ParseAsciiString(byte[] buffer, int offset, int length)
    {
        if (length == 0)
        {
            goto Label_0011;
        }
        return CTSGlobals.AsciiEncoding.GetString(buffer, offset, length);
    Label_0011:
        return string.Empty;
    }

    public static byte[] ParseBinary(byte[] buffer, int offset, int length)
    {
        byte[] buffer2;
        if (length != 0)
        {
            goto Label_0009;
        }
        return emptyByteArray;
    Label_0009:
        buffer2 = new byte[length];
        Buffer.BlockCopy(buffer, offset, buffer2, 0, length);
        return buffer2;
    }

    public static double ParseDouble(byte[] buffer, int offset)
    {
        return BitConverter.ToDouble(buffer, offset);
    }

    public static unsafe DateTime ParseFileTime(byte[] buffer, int offset)
    {
        DateTime time;
        TryParseFileTime(buffer, offset, out time);
        return time;
    }

    public static Guid ParseGuid(byte[] buffer, int offset)
    {
        return ExBitConverter.ReadGuid(buffer, offset);
    }

    public static short ParseInt16(byte[] buffer, int offset)
    {
        return (short)(buffer[offset] | (buffer[offset + 1] << 8));
    }

    public static int ParseInt32(byte[] buffer, int offset)
    {
        return (((buffer[offset] | (buffer[offset + 1] << 8)) | (buffer[offset + 2] << 0x10)) | (buffer[offset + 3] << 0x18));
    }

    public static long ParseInt64(byte[] buffer, int offset)
    {
        uint num;
        uint num2;
        num = (uint)(((buffer[offset] | (buffer[offset + 1] << 8)) | (buffer[offset + 2] << 0x10)) | (buffer[offset + 3] << 0x18));
        num2 = (uint)(((buffer[offset + 4] | (buffer[offset + 5] << 8)) | (buffer[offset + 6] << 0x10)) | (buffer[offset + 7] << 0x18));
        return (long)((((ulong)num) | (((ulong)num2) << 0x20)));
    }

    public static float ParseSingle(byte[] buffer, int offset)
    {
        return BitConverter.ToSingle(buffer, offset);
    }

    public static string ParseUcs16String(byte[] buffer, int offset, int length)
    {
        if (length == 0)
        {
            goto Label_0011;
        }
        return Encoding.Unicode.GetString(buffer, offset, length);
    Label_0011:
        return string.Empty;
    }

    public static string ParseUtf8String(byte[] buffer, int offset, int length)
    {
        if (length == 0)
        {
            goto Label_0011;
        }
        return Encoding.UTF8.GetString(buffer, offset, length);
    Label_0011:
        return string.Empty;
    }

    public static unsafe byte PeekByte(byte[] buff, int pos, int posMax)
    {
        byte num;
        if (TryPeekByte(buff, pos, posMax, out num))
        {
            goto Label_0021;
        }
        throw new ParseException(0xf1dc, "Request would overflow buffer");
    Label_0021:
        return num;
    }

    public static int SerializeAsciiString(string value, byte[] buffer, int offset)
    {
        CTSGlobals.AsciiEncoding.GetBytes(value, 0, value.Length, buffer, offset);
        return value.Length;
    }

    public static int SerializeDouble(double value, byte[] buffer, int offset)
    {
        ExBitConverter.Write((double)value, buffer, offset);
        return 8;
    }

    public static unsafe int SerializeFileTime(DateTime dateTime, byte[] buffer, int offset)
    {
        long num;
        if ((dateTime < MinFileTimeDateTime) == false)
        {
            goto Label_0012;
        }
        num = 0L;
        goto Label_0033;
    Label_0012:
        if ((dateTime == DateTime.MaxValue) == false)
        {
            goto Label_002B;
        }
        num = 0x7fffffffffffffffL;
        goto Label_0033;
    Label_002B:
        num = dateTime.ToFileTimeUtc();
    Label_0033:
        SerializeInt64(num, buffer, offset);
        return 8;
    }

    public static int SerializeGuid(Guid value, byte[] buffer, int offset)
    {
        ExBitConverter.Write(value, buffer, offset);
        return 0x10;
    }

    public static int SerializeInt16(short value, byte[] buffer, int offset)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        return 2;
    }

    public static int SerializeInt32(int value, byte[] buffer, int offset)
    {
        ExBitConverter.Write(value, buffer, offset);
        return 4;
    }

    public static int SerializeInt64(long value, byte[] buffer, int offset)
    {
        ExBitConverter.Write(value, buffer, offset);
        return 8;
    }

    public static int SerializeSingle(float value, byte[] buffer, int offset)
    {
        ExBitConverter.Write((float)value, buffer, offset);
        return 4;
    }

    public static unsafe void SetASCIIString(byte[] buff, ref int pos, string str)
    {
        CheckBounds(pos, buff, str.Length + 1);
        if (buff == null)
        {
            goto Label_0035;
        }
        CTSGlobals.AsciiEncoding.GetBytes(str, 0, str.Length, buff, pos);
        buff[*(((int*)pos)) + str.Length] = 0;
    Label_0035:
        pos += str.Length + 1;
        return;
    }

    public static void SetBoolean(byte[] buff, ref int pos, bool value)
    {
        SetByte(buff, ref pos, (byte)(value? 1 : 0));
        return;
    }

    public static unsafe void SetByte(byte[] buff, ref int pos, byte b)
    {
        CheckBounds(pos, buff, 1);
        if (buff == null)
        {
            goto Label_0011;
        }
        buff[pos] = b;
    Label_0011:
        pos += 1;
        return;
    }

    public static unsafe void SetByteArray(byte[] buff, ref int pos, byte[] byteArray)
    {
        CheckBounds(pos, buff, 2 + ((int)byteArray.Length));
        if (buff == null)
        {
            goto Label_002C;
        }
        SerializeInt16((short)((int)byteArray.Length), buff, pos);
        Buffer.BlockCopy(byteArray, 0, buff, *(((int*)pos)) + 2, (int)byteArray.Length);
    Label_002C:
        pos += 2 + ((int)byteArray.Length);
        return;
    }

    public static unsafe void SetDouble(byte[] buff, ref int pos, double dbl)
    {
        CheckBounds(pos, buff, 8);
        if (buff == null)
        {
            goto Label_0016;
        }
        SerializeDouble(dbl, buff, pos);
    Label_0016:
        pos += 8;
        return;
    }

    public static unsafe void SetDword(byte[] buff, ref int pos, int dw)
    {
        CheckBounds(pos, buff, 4);
        if (buff == null)
        {
            goto Label_0016;
        }
        SerializeInt32(dw, buff, pos);
    Label_0016:
        pos += 4;
        return;
    }

    public static void SetDword(byte[] buff, ref int pos, uint dw)
    {
        SetDword(buff, ref pos, dw);
        return;
    }

    public static unsafe void SetFloat(byte[] buff, ref int pos, float fl)
    {
        CheckBounds(pos, buff, 4);
        if (buff == null)
        {
            goto Label_0016;
        }
        SerializeSingle(fl, buff, pos);
    Label_0016:
        pos += 4;
        return;
    }

    public static unsafe void SetGuid(byte[] buff, ref int pos, Guid guid)
    {
        CheckBounds(pos, buff, 0x10);
        if (buff == null)
        {
            goto Label_0017;
        }
        SerializeGuid(guid, buff, pos);
    Label_0017:
        pos += 0x10;
        return;
    }

    public static void SetMVBinary(byte[] buff, ref int pos, byte[][] values)
    {
        int num;
        SetDword(buff, ref pos, (int)values.Length);
        num = 0;
        goto Label_001C;
    Label_000E:
        SetByteArray(buff, ref pos, values[num]);
        num += 1;
    Label_001C:
        if (num < ((int)values.Length))
        {
            goto Label_000E;
        }
        return;
    }

    public static unsafe void SetMVGuid(byte[] buff, ref int pos, Guid[] values)
    {
        int num;
        CheckBounds(pos, buff, 4 + (((int)values.Length) * 0x10));
        if (buff == null)
        {
            goto Label_0049;
        }
        SerializeInt32((int)values.Length, buff, pos);
        num = 0;
        goto Label_0043;
    Label_0023:
        SerializeGuid(values[num], buff, (*(((int*)pos)) + 4) + (num * 0x10));
        num += 1;
    Label_0043:
        if (num < ((int)values.Length))
        {
            goto Label_0023;
        }
    Label_0049:
        pos += 4 + (((int)values.Length) * 0x10);
        return;
    }

    public static unsafe void SetMVInt16(byte[] buff, ref int pos, short[] values)
    {
        int num;
        CheckBounds(pos, buff, 4 + (((int)values.Length) * 2));
        if (buff == null)
        {
            goto Label_003E;
        }
        SerializeInt32((int)values.Length, buff, pos);
        num = 0;
        goto Label_0038;
    Label_0022:
        SerializeInt16(values[num], buff, (*(((int*)pos)) + 4) + (num * 2));
        num += 1;
    Label_0038:
        if (num < ((int)values.Length))
        {
            goto Label_0022;
        }
    Label_003E:
        pos += 4 + (((int)values.Length) * 2);
        return;
    }

    public static unsafe void SetMVInt32(byte[] buff, ref int pos, int[] values)
    {
        int num;
        CheckBounds(pos, buff, 4 + (((int)values.Length) * 4));
        if (buff == null)
        {
            goto Label_003E;
        }
        SerializeInt32((int)values.Length, buff, pos);
        num = 0;
        goto Label_0038;
    Label_0022:
        SerializeInt32(values[num], buff, (*(((int*)pos)) + 4) + (num * 4));
        num += 1;
    Label_0038:
        if (num < ((int)values.Length))
        {
            goto Label_0022;
        }
    Label_003E:
        pos += 4 + (((int)values.Length) * 4);
        return;
    }

    public static unsafe void SetMVInt64(byte[] buff, ref int pos, long[] values)
    {
        int num;
        CheckBounds(pos, buff, 4 + (((int)values.Length) * 8));
        if (buff == null)
        {
            goto Label_003E;
        }
        SerializeInt32((int)values.Length, buff, pos);
        num = 0;
        goto Label_0038;
    Label_0022:
        SerializeInt64(values[num], buff, (*(((int*)pos)) + 4) + (num * 8));
        num += 1;
    Label_0038:
        if (num < ((int)values.Length))
        {
            goto Label_0022;
        }
    Label_003E:
        pos += 4 + (((int)values.Length) * 8);
        return;
    }

    public static unsafe void SetMVReal32(byte[] buff, ref int pos, float[] values)
    {
        int num;
        CheckBounds(pos, buff, 4 + (((int)values.Length) * 4));
        if (buff == null)
        {
            goto Label_003E;
        }
        SerializeInt32((int)values.Length, buff, pos);
        num = 0;
        goto Label_0038;
    Label_0022:
        SerializeSingle(values[num], buff, (*(((int*)pos)) + 4) + (num * 4));
        num += 1;
    Label_0038:
        if (num < ((int)values.Length))
        {
            goto Label_0022;
        }
    Label_003E:
        pos += 4 + (((int)values.Length) * 4);
        return;
    }

    public static unsafe void SetMVReal64(byte[] buff, ref int pos, double[] values)
    {
        int num;
        CheckBounds(pos, buff, 4 + (((int)values.Length) * 8));
        if (buff == null)
        {
            goto Label_003E;
        }
        SerializeInt32((int)values.Length, buff, pos);
        num = 0;
        goto Label_0038;
    Label_0022:
        SerializeDouble(values[num], buff, (*(((int*)pos)) + 4) + (num * 8));
        num += 1;
    Label_0038:
        if (num < ((int)values.Length))
        {
            goto Label_0022;
        }
    Label_003E:
        pos += 4 + (((int)values.Length) * 8);
        return;
    }

    public static unsafe void SetMVSystime(byte[] buff, ref int pos, DateTime[] values)
    {
        int num;
        CheckBounds(pos, buff, 4 + (((int)values.Length) * 8));
        if (buff == null)
        {
            goto Label_0047;
        }
        SerializeInt32((int)values.Length, buff, pos);
        num = 0;
        goto Label_0041;
    Label_0022:
        SerializeFileTime(values[num], buff, (*(((int*)pos)) + 4) + (num * 8));
        num += 1;
    Label_0041:
        if (num < ((int)values.Length))
        {
            goto Label_0022;
        }
    Label_0047:
        pos += 4 + (((int)values.Length) * 8);
        return;
    }

    public static void SetMVUnicode(byte[] buff, ref int pos, string[] values)
    {
        int num;
        SetDword(buff, ref pos, (int)values.Length);
        num = 0;
        goto Label_001C;
    Label_000E:
        SetUnicodeString(buff, ref pos, values[num]);
        num += 1;
    Label_001C:
        if (num < ((int)values.Length))
        {
            goto Label_000E;
        }
        return;
    }

    public static unsafe void SetQword(byte[] buff, ref int pos, long qw)
    {
        CheckBounds(pos, buff, 8);
        if (buff == null)
        {
            goto Label_0016;
        }
        SerializeInt64(qw, buff, pos);
    Label_0016:
        pos += 8;
        return;
    }

    public static void SetQword(byte[] buff, ref int pos, ulong qw)
    {
        SetQword(buff, ref pos, qw);
        return;
    }

    public static unsafe void SetSysTime(byte[] buff, ref int pos, DateTime value)
    {
        CheckBounds(pos, buff, 8);
        if (buff == null)
        {
            goto Label_0016;
        }
        SerializeFileTime(value, buff, pos);
    Label_0016:
        pos += 8;
        return;
    }

    public static unsafe void SetUnicodeString(byte[] buff, ref int pos, string str)
    {
        CheckBounds(pos, buff, (str.Length + 1) * 2);
        if (buff == null)
        {
            goto Label_0049;
        }
        Encoding.Unicode.GetBytes(str, 0, str.Length, buff, pos);
        buff[*(((int*)pos)) + (str.Length * 2)] = 0;
        buff[(*(((int*)pos)) + (str.Length * 2)) + 1] = 0;
    Label_0049:
        pos += (str.Length + 1) * 2;
        return;
    }

    public static unsafe void SetWord(byte[] buff, ref int pos, short w)
    {
        CheckBounds(pos, buff, 2);
        if (buff == null)
        {
            goto Label_0016;
        }
        SerializeInt16(w, buff, pos);
    Label_0016:
        pos += 2;
        return;
    }

    public static void SetWord(byte[] buff, ref int pos, ushort w)
    {
        SetWord(buff, ref pos, (short)w);
        return;
    }

    public static bool TryCheckBounds(int pos, int posMax, int sizeNeeded)
    {
        return CheckOffsetLength(posMax, pos, sizeNeeded);
    }

    internal static bool TryCheckCount(uint count, int elementSize, int availableSize)
    {
        if (count >= 0)
        {
            goto Label_0015;
        }
        //DiagnosticContext.TraceLocation((LID)0x9bdc);
        return false;
    Label_0015:
        //if ((((ulong)count) * ((long)elementSize)) <= ((long)availableSize))
        if ((((long)count) * ((long)elementSize)) <= ((long)availableSize))
        {
            goto Label_002F;
        }
        //DiagnosticContext.TraceLocation((LID)0xdbdc);
        return false;
    Label_002F:
        //if ((((ulong)count) * ((long)elementSize)) <= 0x1fffffffL)
        if ((((long)count) * ((long)elementSize)) <= 0x1fffffffL)
        {
            goto Label_004D;
        }
        //DiagnosticContext.TraceLocation((LID)0xabdc);
        return false;
    Label_004D:
        return true;
    }

    public static bool TryConvertFileTime(long fileTime, out DateTime dateTime)
    {
        bool flag;
        flag = false;
        if (fileTime < MinFileTime)
        {
            goto Label_0012;
        }
        if (fileTime < MaxFileTime)
        {
            goto Label_002C;
        }
    Label_0012:
        //*(dateTime) = DateTime.MaxValue;
        dateTime = DateTime.MaxValue;
        flag = fileTime == 0x7fffffffffffffffL;
        goto Label_004E;
    Label_002C:
        if (fileTime != 0L)
        {
            goto Label_0040;
        }
        //*(dateTime) = DateTime.MinValue;
        dateTime = DateTime.MinValue;
        flag = true;
        goto Label_004E;
    Label_0040:
        //*(dateTime) = DateTime.FromFileTimeUtc(fileTime);
        dateTime = DateTime.FromFileTimeUtc(fileTime);
        flag = true;
    Label_004E:
        return flag;
    }

    public static unsafe bool TryGetBoolean(byte[] buff, ref int pos, int posMax, out bool result)
    {
        byte num;
        if (TryGetByte(buff, ref pos, posMax, out num))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0xa3dc);
        //*((sbyte*)result) = 0;
        result = false;
        return false;
    Label_0020:
        //*((sbyte*)result) = (num == 0) == 0;
        result = ((num == 0) == false);
        return true;
    }

    public static unsafe bool TryGetByte(byte[] buff, ref int pos, int posMax, out byte result)
    {
        if (TryCheckBounds(pos, posMax, 1))
        {
            goto Label_001F;
        }
        //DiagnosticContext.TraceLocation((LID)0xebdc);
        result = 0;
        return false;
    Label_001F:
        //*((sbyte*)result) = buff[pos];
        result = buff[pos];
        //pos += 1;
        pos += 1;
        return true;
    }

    public static unsafe bool TryGetByteArray(byte[] buff, ref int pos, int posMax, out byte[] result)
    {
        ushort num;
        if (TryGetWord(buff, ref pos, posMax, out num))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0xc0dc);
        //*(result) = null;
        result = null;
        return false;
    Label_0020:
        if (TryCheckBounds(pos, posMax, num))
        {
            goto Label_003F;
        }
        //DiagnosticContext.TraceLocation((LID)0xdedc);
        //*(result) = null;
        result = null;
        return false;
    Label_003F:
        //*(result) = ParseBinary(buff, pos, num);
        result = ParseBinary(buff, pos, num);
        pos += num;
        return true;
    }

    public static unsafe bool TryGetDouble(byte[] buff, ref int pos, int posMax, out double result)
    {
        if (TryCheckBounds(pos, posMax, 8) )
        {
            goto Label_0027;
        }
        //DiagnosticContext.TraceLocation((LID)0x93dc);
        //*((double*)result) = 0.0;
        result = 0.0;
        return false;
    Label_0027:
        //*((double*)result) = ParseDouble(buff, pos);
        result = ParseDouble(buff, pos);
        pos += 8;
        return true;
    }

    public static unsafe bool TryGetDword(byte[] buff, ref int pos, int posMax, out uint result)
    {
        if (TryCheckBounds(pos, posMax, 4))
        {
            goto Label_001F;
        }
        //DiagnosticContext.TraceLocation((LID)0x8bdc);
        result = 0;
        return false;
    Label_001F:
        result = (uint)ParseInt32(buff, pos);
        pos += 4;
        return true;
    }

    public static unsafe bool TryGetFloat(byte[] buff, ref int pos, int posMax, out float result)
    {
        if (TryCheckBounds(pos, posMax, 4))
        {
            goto Label_0023;
        }
        //DiagnosticContext.TraceLocation((LID)0xb3dc);
        //*((float*)result) = 0f;
        result = 0f;
        return false;
    Label_0023:
        //*((float*)result) = ParseSingle(buff, pos);
        result = ParseSingle(buff, pos);
        pos += 4;
        return true;
    }

    public static unsafe bool TryGetGuid(byte[] buff, ref int pos, int posMax, out Guid result)
    {
        if (TryCheckBounds(pos, posMax, 0x10))
        {
            goto Label_0024;
        }
        //DiagnosticContext.TraceLocation((LID)0xe3dc);
        result = new Guid();
        return false;
    Label_0024:
        //*(result) = ParseGuid(buff, pos);
        result = ParseGuid(buff, pos);
        pos += 0x10;
        return true;
    }

    public static unsafe bool TryGetMVBinary(byte[] buff, ref int pos, int posMax, out byte[][] result)
    {
        uint num;
        byte[][] bufferArray;
        int num2;
        if (TryGetDword(buff, ref pos, posMax, out num) )
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0x83dc);
        //*(result) = null;
        result = null;
        return false;
    Label_0020:
        if (TryCheckCount(num, 2, posMax - *(((int*)pos))))
        {
            goto Label_0041;
        }
        //DiagnosticContext.TraceLocation((LID)0xc3dc);
        //*(result) = null;
        result = null;
        return false;
    Label_0041:
        bufferArray = new byte[num][];
        num2 = 0;
        goto Label_0076;
    Label_004D:
        if (TryGetByteArray(buff, ref pos, posMax,out bufferArray[num2]))
        {
            goto Label_0072;
        }
        //DiagnosticContext.TraceLocation((LID)0xbcdc);
        //*(result) = null;
        result = null;
        return false;
    Label_0072:
        num2 += 1;
    Label_0076:
        //if (((long)num2) < ((ulong)num))
        if(num2 < num)
        {
            goto Label_004D;
        }
        //*(result) = bufferArray;
        result = bufferArray;
        return true;
    }

    public static unsafe bool TryGetMVGuid(byte[] buff, ref int pos, int posMax, out Guid[] result)
    {
        uint num;
        Guid[] guidArray;
        int num2;
        if (TryGetDword(buff, ref pos, posMax, out num))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0xd8dc);
        //*(result) = null;
        result = null;
        return false;
    Label_0020:
        if (TryCheckCount(num, 0x10, posMax - *(((int*)pos))))
        {
            goto Label_0042;
        }
        //DiagnosticContext.TraceLocation((LID)0xa8dc);
        //*(result) = null;
        result = null;
        return false;
    Label_0042:
        guidArray = new Guid[num];
        num2 = 0;
        goto Label_0077;
    Label_004E:
        if (TryGetGuid(buff, ref pos, posMax, out guidArray[num2]))
        {
            goto Label_0073;
        }
        //DiagnosticContext.TraceLocation((LID)0xe8dc);
        //*(result) = null;
        result = null;
        return false;
    Label_0073:
        num2 += 1;
    Label_0077:
        //if (((long)num2) < ((ulong)num))
        if(num2 < num)
        {
            goto Label_004E;
        }
        //*(result) = guidArray;
        result = guidArray;
        return true;
    }

    public static unsafe bool TryGetMVInt16(byte[] buff, ref int pos, int posMax, out short[] result)
    {
        uint num;
        short[] numArray;
        int num2;
        ushort num3;
        if (TryGetDword(buff, ref pos, posMax, out num))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0xfcdc);
        //*(result) = null;
        result = null;
        return false;
    Label_0020:
        if (TryCheckCount(num, 2, posMax - *(((int*)pos))))
        {
            goto Label_0041;
        }
        //DiagnosticContext.TraceLocation((LID)0x9cdc);
        //*(result) = null;
        result = null;
        return false;
    Label_0041:
        //numArray = new short[num];
        numArray = new short[num];
        num2 = 0;
        goto Label_0076;
    Label_004D:
        //if (TryGetWord(buff, ref pos, posMax, &num3) != null)
        if (TryGetWord(buff, ref pos, posMax, out num3))
        {
            goto Label_006D;
        }
        ////DiagnosticContext.TraceLocation((LID)0xdcdc);
        //*(result) = null;
        result = null;
        return false;
    Label_006D:
        numArray[num2] = (short)num3;
        num2 += 1;
    Label_0076:
        //if (((long)num2) < ((ulong)num))
        if(num2 < num)
        {
            goto Label_004D;
        }
        //*(result) = numArray;
        result = numArray;
        return true;
    }

    public static unsafe bool TryGetMVInt32(byte[] buff, ref int pos, int posMax, out int[] result)
    {
        uint num;
        int[] numArray;
        int num2;
        uint num3;
        if (TryGetDword(buff, ref pos, posMax, out num))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0xacdc);
        //*(result) = null;
        result = null;
        return false;
    Label_0020:
        if (TryCheckCount(num, 4, posMax - *(((int*)pos))))
        {
            goto Label_0041;
        }
        //DiagnosticContext.TraceLocation((LID)0xecdc);
        //*(result) = null;
        result = null;
        return false;
    Label_0041:
        numArray = new int[num];
        num2 = 0;
        goto Label_0075;
    Label_004D:
        if (TryGetDword(buff, ref pos, posMax, out num3))
        {
            goto Label_006D;
        }
        //DiagnosticContext.TraceLocation((LID)0x8cdc);
        //*(result) = null;
        result = null;
        return false;
    Label_006D:
        numArray[num2] = (int)num3;
        num2 += 1;
    Label_0075:
        //if (((long)num2) < ((ulong)num))
        if(num2 < num)
        {
            goto Label_004D;
        }
        result = numArray;
        return true;
    }

    public static unsafe bool TryGetMVInt64(byte[] buff, ref int pos, int posMax, out long[] result)
    {
        uint num;
        long[] numArray;
        int num2;
        ulong num3;
        if (TryGetDword(buff, ref pos, posMax, out num))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0xe4dc);
        result = null;
        return false;
    Label_0020:
        if (TryCheckCount(num, 8, posMax - *(((int*)pos))))
        {
            goto Label_0041;
        }
        //DiagnosticContext.TraceLocation((LID)0x84dc);
        result = null;
        return false;
    Label_0041:
        numArray = new long[num];
        num2 = 0;
        goto Label_0075;
    Label_004D:
        if (TryGetQword(buff, ref pos, posMax, out num3))
        {
            goto Label_006D;
        }
        //DiagnosticContext.TraceLocation((LID)0xc4dc);
        result = null;
        return false;
    Label_006D:
        numArray[num2] = (long)num3;
        num2 += 1;
    Label_0075:
        if(num2 < num)
        {
            goto Label_004D;
        }
        result = numArray;
        return true;
    }

    public static unsafe bool TryGetMVR8(byte[] buff, ref int pos, int posMax, out double[] result)
    {
        uint num;
        double[] numArray;
        int num2;
        if (TryGetDword(buff, ref pos, posMax, out num))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0x94dc);
        result = null;
        return false;
    Label_0020:
        if (TryCheckCount(num, 8, posMax - *(((int*)pos))))
        {
            goto Label_0041;
        }
        //DiagnosticContext.TraceLocation((LID)0xd4dc);
        result = null;
        return false;
    Label_0041:
        numArray = new double[num];
        num2 = 0;
        goto Label_0076;
    Label_004D:
        if (TryGetDouble(buff, ref pos, posMax, out numArray[num2]))
        {
            goto Label_0072;
        }
        //DiagnosticContext.TraceLocation((LID)0xa4dc);
        result = null;
        return false;
    Label_0072:
        num2 += 1;
    Label_0076:
        if(num2 < num)
        {
            goto Label_004D;
        }
        result = numArray;
        return true;
    }

    public static unsafe bool TryGetMVReal32(byte[] buff, ref int pos, int posMax, out float[] result)
    {
        uint num;
        float[] numArray;
        int num2;
        if (TryGetDword(buff, ref pos, posMax, out num))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0xccdc);
        result = null;
        return false;
    Label_0020:
        if (TryCheckCount(num, 4, posMax - *(((int*)pos))))
        {
            goto Label_0041;
        }
        //DiagnosticContext.TraceLocation((LID)0xb4dc);
        result = null;
        return false;
    Label_0041:
        numArray = new float[num];
        num2 = 0;
        goto Label_0076;
    Label_004D:
        if (TryGetFloat(buff, ref pos, posMax, out numArray[num2]))
        {
            goto Label_0072;
        }
        //DiagnosticContext.TraceLocation((LID)0xf4dc);
        result = null;
        return false;
    Label_0072:
        num2 += 1;
    Label_0076:
        if(num2 < num)
        {
            goto Label_004D;
        }
        result = numArray;
        return true;
    }

    public static unsafe bool TryGetMVString8(byte[] buff, ref int pos, int posMax, out string[] result)
    {
        uint num;
        string[] strArray;
        int num2;
        if (TryGetDword(buff, ref pos, posMax, out num) )
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0x9edc);
        result = null;
        return false;
    Label_0020:
        if (TryCheckCount(num, 1, posMax - *(((int*)pos))) )
        {
            goto Label_0041;
        }
        //DiagnosticContext.TraceLocation((LID)0x8edc);
        result = null;
        return false;
    Label_0041:
        strArray = new string[num];
        num2 = 0;
        goto Label_0076;
    Label_004D:
        if (TryGetStringFromASCII(buff, ref pos, posMax, out strArray[num2]) )
        {
            goto Label_0072;
        }
        //DiagnosticContext.TraceLocation((LID)0x86dc);
        result = null;
        return false;
    Label_0072:
        num2 += 1;
    Label_0076:
        if(num2 < num)
        {
            goto Label_004D;
        }
        result = strArray;
        return true;
    }

    public static unsafe bool TryGetMVSysTime(byte[] buff, ref int pos, int posMax, out DateTime[] result)
    {
        uint num;
        DateTime[] timeArray;
        int num2;
        if (TryGetDword(buff, ref pos, posMax, out num))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0xb8dc);
        result = null;
        return false;
    Label_0020:
        if (TryCheckCount(num, 8, posMax - *(((int*)pos))) )
        {
            goto Label_0041;
        }
        //DiagnosticContext.TraceLocation((LID)0xf8dc);
        result = null;
        return false;
    Label_0041:
        timeArray = new DateTime[num];
        num2 = 0;
        goto Label_0076;
    Label_004D:
        if (TryGetSysTime(buff, ref pos, posMax, out timeArray[num2]))
        {
            goto Label_0072;
        }
        //DiagnosticContext.TraceLocation((LID)0x98dc);
        result = null;
        return false;
    Label_0072:
        num2 += 1;
    Label_0076:
        if(num2 < num)
        {
            goto Label_004D;
        }
        result = timeArray;
        return true;
    }

    public static unsafe bool TryGetMVUnicode(byte[] buff, ref int pos, int posMax, out string[] result)
    {
        uint num;
        string[] strArray;
        int num2;
        if (TryGetDword(buff, ref pos, posMax, out num))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0xfedc);
        result = null;
        return false;
    Label_0020:
        if (TryCheckCount(num, 2, posMax - *(((int*)pos))) )
        {
            goto Label_0041;
        }
        //DiagnosticContext.TraceLocation((LID)0xe0dc);
        result = null;
        return false;
    Label_0041:
        strArray = new string[num];
        num2 = 0;
        goto Label_0076;
    Label_004D:
        if (TryGetStringFromUnicode(buff, ref pos, posMax, out strArray[num2]) )
        {
            goto Label_0072;
        }
        //DiagnosticContext.TraceLocation((LID)0xbedc);
        result = null;
        return false;
    Label_0072:
        num2 += 1;
    Label_0076:
        if(num2 < num)
        {
            goto Label_004D;
        }
        result = strArray;
        return true;
    }

    public static unsafe bool TryGetQword(byte[] buff, ref int pos, int posMax, out ulong result)
    {
        if (TryCheckBounds(pos, posMax, 8))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0xf3dc);
        result = 0L;
        return false;
    Label_0020:
        result = (ulong)ParseInt64(buff, pos);
        pos += 8;
        return true;
    }

    public static unsafe bool TryGetStringFromASCII(byte[] buff, ref int pos, int posMax, out string result)
    {
        int num;
        num = 0;
        goto Label_0008;
    Label_0004:
        num += 1;
    Label_0008:
        if ((*(((int*)pos)) + num) >= posMax)
        {
            goto Label_0017;
        }
        if (buff[*(((int*)pos)) + num] != 0)
        {
            goto Label_0004;
        }
    Label_0017:
        if ((*(((int*)pos)) + num) < posMax)
        {
            goto Label_0032;
        }
        //DiagnosticContext.TraceLocation((LID)0xefdc);
        result = null;
        return false;
    Label_0032:
        return TryGetStringFromASCII(buff, ref pos, posMax, num + 1,out result);
    }

    public static unsafe bool TryGetStringFromASCII(byte[] buff, ref int pos, int posMax, int charCount, out string result)
    {
        if (TryCheckBounds(pos, posMax, charCount))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0x8fdc);
        result = null;
        return false;
    Label_0020:
        result = CTSGlobals.AsciiEncoding.GetString(buff, pos, charCount - 1);
        pos += charCount;
        return true;
    }

    public static unsafe bool TryGetStringFromASCIINoNull(byte[] buff, ref int pos, int posMax, int charCount, out string result)
    {
        if (TryCheckBounds(pos, posMax, charCount))
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0x80dc);
        result = null;
        return false;
    Label_0020:
        result = CTSGlobals.AsciiEncoding.GetString(buff, pos, charCount);
        pos += charCount;
        return true;
    }

    public static unsafe bool TryGetStringFromUnicode(byte[] buff, ref int pos, int posMax, out string result)
    {
        int num;
        num = 0;
        if (TryCheckBounds(pos, posMax, 2) )
        {
            goto Label_0046;
        }
        //DiagnosticContext.TraceLocation((LID)0x88dc);
        result = null;
        return false;
    Label_0021:
        num += 2;
        if (TryCheckBounds(*(((int*)pos)) + num, posMax, 2) )
        {
            goto Label_0046;
        }
        //DiagnosticContext.TraceLocation((LID)0xc8dc);
        result = null;
        return false;
    Label_0046:
        if (buff[*(((int*)pos)) + num] != 0)
        {
            goto Label_0021;
        }
        if (buff[(*(((int*)pos)) + num) + 1] != 0)
        {
            goto Label_0021;
        }
        return TryGetStringFromUnicode(buff, ref pos, posMax, num + 2, out result);
    }

    public static unsafe bool TryGetStringFromUnicode(byte[] buff, ref int pos, int posMax, int byteCount, out string result)
    {
        if (TryCheckBounds(pos, posMax, byteCount) )
        {
            goto Label_0020;
        }
        //DiagnosticContext.TraceLocation((LID)0xb0dc);
        result = null;
        return false;
    Label_0020:
        result = Encoding.Unicode.GetString(buff, pos, byteCount - 2);
        pos += byteCount;
        return true;
    }

    public static unsafe bool TryGetSysTime(byte[] buff, ref int pos, int posMax, out DateTime result)
    {
        if (TryCheckBounds(pos, posMax, 8))
        {
            goto Label_0023;
        }
        //DiagnosticContext.TraceLocation((LID)0xd3dc);
        result = new DateTime();
        return false;
    Label_0023:
        result = ParseFileTime(buff, pos);
        pos += 8;
        return true;
    }

    public static unsafe bool TryGetWord(byte[] buff, ref int pos, int posMax, out ushort result)
    {
        if (TryCheckBounds(pos, posMax, 2))
        {
            goto Label_001F;
        }
        //DiagnosticContext.TraceLocation((LID)0xcbdc);
        result = 0;
        return false;
    Label_001F:
        result = (ushort)ParseInt16(buff, pos);
        pos += 2;
        return true;
    }

    public static bool TryParseFileTime(byte[] buffer, int offset, out DateTime dateTime)
    {
        return TryConvertFileTime(ParseInt64(buffer, offset),out dateTime);
    }

    public static unsafe bool TryPeekByte(byte[] buff, int pos, int posMax, out byte result)
    {
        if (TryCheckBounds(pos, posMax, 1) )
        {
            goto Label_001E;
        }
        //DiagnosticContext.TraceLocation((LID)0xffdc);
        result = 0;
        return false;
    Label_001E:
        result = buff[pos];
        return true;
    }
}
