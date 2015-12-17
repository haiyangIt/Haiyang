using FTStreamUtil.FTStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace MyInterop
{
    public delegate Stream ReadTransferStreamFunc();
    public class MailItemUtil
    {
        private IStreamUtil _streamUtil;
        public MailItemUtil(IStreamUtil stream)
        {
            _streamUtil = stream;
        }

        private List<byte[]> allBuffer = new List<byte[]>();
        private void AppendBuffer(byte[] buffer)
        {
            allBuffer.Add(buffer);
        }

        public void Parse()
        {
            //FileStream templateStream = new FileStream(templateFile, FileMode.CreateNew);
            //BinaryWriter templateWriter = new BinaryWriter(templateStream);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                Stream stream = null;
                while((stream = _streamUtil.GetNextStream()) != null){
                    stream.Seek(0, SeekOrigin.Begin);
                    byte[] buffer = new byte[stream.Length];
                    int readCount = stream.Read(buffer, 0, (int)stream.Length);
                    memoryStream.Write(buffer, 0, readCount);
                }
            }

            using (FileStream fileStream = new FileStream(exportFileName, FileMode.Open))
            {
                byte[] buffer = new byte[fileStream.Length];
                byte[] subBuffer;
                fileStream.Read(buffer, 0, (int)fileStream.Length);
                int readedCount = 0;
                int byteCount = 0;
                int iGuessIsTag = 0;
                int subBufferCount = 0;
                FxOpcodes opcodes = 0;
                try
                {
                    readedCount = 0;
                    byteCount = buffer.Length;
                    int outputIndex = 0;
                    while (readedCount < byteCount)
                    {
                        outputIndex++;
                        if ((readedCount + 8) <= byteCount)
                        {
                            iGuessIsTag = BitConverter.ToInt32(buffer, readedCount);
                            //templateWriter.Write(iGuessIsTag);
                            readedCount += 4;
                            subBufferCount = BitConverter.ToInt32(buffer, readedCount);
                            //templateWriter.Write(subBufferCount);
                            readedCount += 4;
                            if ((readedCount + subBufferCount) <= byteCount)
                            {
                                subBuffer = (subBufferCount > 0) ? new byte[subBufferCount] : null;
                                if (subBufferCount > 0)
                                {
                                    Array.ConstrainedCopy(buffer, readedCount, subBuffer, 0, subBufferCount);

                                }
                                //FaultInjection.GenerateFault(-943313603);
                                opcodes = (FxOpcodes)iGuessIsTag;
                                if (opcodes != FxOpcodes.TransferBuffer || subBuffer != null)
                                {
                                    if (opcodes == FxOpcodes.TransferBuffer)
                                    {
                                        Debug.WriteLine(string.Format("Section{2}: FxOpCodes:{0:-30}\t\tCount:{1}", opcodes.ToString(), subBufferCount, outputIndex));
                                        AppendBuffer(subBuffer);
                                    }

                                    else
                                    {
                                        //templateWriter.Write(subBuffer);

                                        StringBuilder sb = new StringBuilder();

                                        foreach (byte temp in subBuffer)
                                        {
                                            sb.Append(temp.ToString("X2"));
                                        }

                                        Debug.Write(string.Format("Section{3}: FxOpCodes:{0:-30}\t\tCount:{1}\t\tvalue:{2}\t\t", opcodes.ToString(), subBuffer.Length, sb.ToString(), outputIndex));

                                        if (opcodes == FxOpcodes.Config)
                                        {
                                            var config = FxConfig.ParseFxConfig(subBuffer);

                                            Debug.Write(config.ToString());
                                        }
                                        else if (opcodes == FxOpcodes.IsInterfaceOk)
                                        {
                                            var interfaceConfig = FxInterfaceOKConfig.ParseFxInterfaceOKConfig(subBuffer);

                                            Debug.Write(interfaceConfig.ToString());
                                        }

                                        Debug.WriteLine("");

                                    }
                                    readedCount += subBufferCount;
                                }
                                else
                                {
                                    throw new NotImplementedException();
                                }
                            }
                            else
                                throw new ApplicationException(); //CorruptDataException(new LocalizedString("FastTransferUpload, Uploaded Data"));
                        }
                        else
                            throw new ApplicationException();// CorruptDataException(new LocalizedString("FastTransferUpload, Uploaded Data"));
                    }
                }
                catch (Exception e) { }
            }

            //templateWriter.Close();
            //templateStream.Close();

            ProcessTransferBuffer();
            
        }
    }

    public interface IStreamUtil
    {
        public Stream GetNextStream();
    }
}
