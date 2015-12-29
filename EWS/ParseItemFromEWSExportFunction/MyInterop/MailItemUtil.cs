using FTStreamUtil;
using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace MyInterop
{
    public class MailItemUtil
    {
        public MailItemUtil(Stream stream)
        {
            _stream = stream;
            
        }

        private readonly Stream _stream;
        private List<byte[]> allBuffer = new List<byte[]>();
        private void AppendBuffer(byte[] buffer)
        {
            allBuffer.Add(buffer);
        }

        public void Parse()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            using (BinaryReader reader = new BinaryReader(_stream))
            {
                int iGuessIsTag = reader.ReadInt32();
                int subBufferCount = reader.ReadInt32();
                byte[] subBuffer = null;
                if(reader.BaseStream.Position + subBufferCount <= reader.BaseStream.Length)
                {
                    if (subBufferCount > 0)
                        subBuffer = reader.ReadBytes(subBufferCount);
                    FxOpcodes opcodes = (FxOpcodes)iGuessIsTag;
                    if(opcodes != FxOpcodes.TransferBuffer || (subBuffer != null && subBuffer.Length > 0))
                    {
                        if (opcodes == FxOpcodes.TransferBuffer)
                        {
                            AppendBuffer(subBuffer);
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();

                            foreach (byte temp in subBuffer)
                            {
                                sb.Append(temp.ToString("X2"));
                            }

                            Debug.Write(string.Format("FxOpCodes:{0:-30}\t\tCount:{1}\t\tvalue:{2}\t\t", opcodes.ToString(), subBuffer.Length, sb.ToString()));
                        }
                    }
                }
            }
            _stream.Seek(0, SeekOrigin.Begin);
        }

        private void CreateMsg()
        {
            FTParserUtil util = new FTParserUtil(allBuffer);
            var root = util.Parser();

            //root.CreateMsg()
        }
    }
}
