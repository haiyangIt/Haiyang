using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.FTStream
{
    public class ByteArrayTransferUnit : IFTTransferUnit
    {
        private byte[] _buffer;
        public ByteArrayTransferUnit(byte[] buffer)
        {
            _buffer = buffer;
        }

        public int BytesCount
        {
            get { return _buffer.Length; }
        }

        public byte[] Bytes
        {
            get { return _buffer; }
        }


        public uint Tag
        {
            get { return (uint)BitConverter.ToInt32(Bytes, 0); }
        }
    }

}
