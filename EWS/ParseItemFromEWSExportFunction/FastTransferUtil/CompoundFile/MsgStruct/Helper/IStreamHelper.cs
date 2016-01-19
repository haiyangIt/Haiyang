using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace FastTransferUtil.CompoundFile.MsgStruct.Helper
{
    public static class IStreamHelper
    {
        public static void Write(this IStream stream, Int32 value)
        {
            stream.Write(BitConverter.GetBytes(value), sizeof(Int32), IntPtr.Zero);
        }

        public static void Write(this IStream stream, uint value)
        {
            stream.Write(BitConverter.GetBytes(value), sizeof(Int32), IntPtr.Zero);
        }

        public static void Write(this IStream stream, byte[] value)
        {
            stream.Write(value, value.Length, IntPtr.Zero);
        }

        public static void Write(this IStream stream, Guid value)
        {
            stream.Write(value.ToByteArray(), 16, IntPtr.Zero);
        }

        static byte[] zero;
        static byte[] Zero
        {
            get
            {
                if (zero == null)
                {
                    zero = new byte[16];
                    for (int i = 0; i < 16; i++)
                    {
                        zero[i] = 0;
                    }
                }
                return zero;
            }
        }
        public static void WriteZero(this IStream stream, int byteCount)
        {
            stream.Write(Zero, byteCount, IntPtr.Zero);
        }

        public static Int64 ReadInt64(this IStream stream, ref int readCount)
        {
            byte[] value = new byte[8];
            stream.Read(value, 8, IntPtr.Zero);
            readCount += 8;
            return BitConverter.ToInt64(value, 0);
        }

        public static Int32 ReadInt32(this IStream stream, ref int readCount)
        {
            byte[] value = new byte[4];
            stream.Read(value, 4, IntPtr.Zero);
            readCount += 4;
            return BitConverter.ToInt32(value, 0);
        }

        public static Int16 ReadInt16(this IStream stream, ref int readCount)
        {
            byte[] value = new byte[2];
            stream.Read(value, 2, IntPtr.Zero);
            readCount += 2;
            return BitConverter.ToInt16(value, 0);
        }
    }
}
