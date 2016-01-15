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
    }
}
