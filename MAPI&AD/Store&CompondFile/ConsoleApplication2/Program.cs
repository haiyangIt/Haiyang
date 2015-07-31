using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FTStreamUtil;
using System.Xml;
using System.Diagnostics;
using FTStreamUtil.FTStream;

namespace ConsoleApplication2
{
    class Program
    {
        class FxConfig
        {
            UInt32 Flag;
            UInt32 TransferMethod;

            public static FxConfig ParseFxConfig(byte[] buffer)
            {
                return null;
                //FxConfig config = new FxConfig();
                //int pos = 0;
                //config.Flag = (UInt32)ParseSerialize.ParseInt32(buffer, pos);
                //pos += 4;
                //config.TransferMethod = (UInt32)ParseSerialize.ParseInt32(buffer, pos);
                //return config;
            }

            public override string ToString()
            {
                return string.Format("Flag:{0},TransferMethod:{1}.", Flag.ToString("X8"), TransferMethod.ToString("X8"));
            }
        }

        class FxInterfaceOKConfig
        {
            UInt32 Flags;
            Guid Refiid;
            UInt32 TransferMethod;

            public static FxInterfaceOKConfig ParseFxInterfaceOKConfig(byte[] buffer)
            {
                return null;
                //FxInterfaceOKConfig config = new FxInterfaceOKConfig();
                //int pos = 0;
                //config.Flags = (UInt32)ParseSerialize.ParseInt32(buffer, pos);
                //pos += 4;
                //byte[] guidbuffer = new byte[0x10];
                //Array.Copy(buffer, pos, guidbuffer, 0, 0x10);
                //config.Refiid = new Guid(guidbuffer);
                //pos += 0x10;
                //config.TransferMethod = (UInt32)ParseSerialize.ParseInt32(buffer, pos);
                //return config;
            }

            public override string ToString()
            {
                return string.Format("Flag:{0},Refiid:{1},TransferMethod:{2}", Flags.ToString("X8"), Refiid.ToString(), TransferMethod.ToString("X8"));
            }
        }


        internal enum FxOpcodes
        {
            None = 0,
            Config = 1,
            TransferBuffer = 2,
            IsInterfaceOk = 3,
            TellPartnerVersion = 4,
            StartMdbEventsImport = 11,
            FinishMdbEventsImport = 12,
            AddMdbEvents = 13,
            SetWatermarks = 14,
            SetReceiveFolder = 15,
            SetPerUser = 0x10,
            SetProps = 0x11
        }
        static void Main(string[] args)
        {
            //string fileName = "D:\\simple.bin";
            //string fileName = "D:\\no attach simple mail.bin";
            //string fileName = "D:\\withEmbedAttach.bin";
            string fileName = "D:\\EWSTest\\simple.bin";
            const string templateFile = "D:\\EWSTest\\Template.bin";
            if (File.Exists(templateFile))
                File.Delete(templateFile);
            FileStream templateStream = new FileStream(templateFile, FileMode.CreateNew);
            BinaryWriter templateWriter = new BinaryWriter(templateStream);
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                byte[] buffer = new byte[fileStream.Length];
                byte[] subBuffer;
                fileStream.Read(buffer, 0, (int)fileStream.Length);
                int readedCount = 0;
                int byteCount = 0;
                int iGuessIsTag = 0;
                int subBufferCount = 0;
                FxOpcodes opcodes = 0;
                string str;
                string str2;
                XmlNode node2;
                XmlNode node = null; //no itemid, so can't overwrite;
                //proxy = item.MapiMessage.GetFxProxyCollector();
                string iGuessTagStr = "";
                try
                {
                    //buffer = Base64StringConverter.Parse(node3.InnerText);
                    readedCount = 0;
                    byteCount = buffer.Length;
                    while (readedCount < byteCount)
                    {
                        if ((readedCount + 8) <= byteCount)
                        {
                            iGuessIsTag = BitConverter.ToInt32(buffer, readedCount);
                            templateWriter.Write(iGuessIsTag);
                            readedCount += 4;
                            subBufferCount = BitConverter.ToInt32(buffer, readedCount);
                            templateWriter.Write(subBufferCount);
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
                                        Debug.WriteLine(string.Format("OpCode:{0}, Count:{1}", opcodes.ToString(), subBufferCount));
                                        AppendBuffer(subBuffer);
                                    }

                                    else
                                    {
                                        templateWriter.Write(subBuffer);
                                        if (opcodes == FxOpcodes.Config)
                                        {
                                            var config = FxConfig.ParseFxConfig(subBuffer);
                                            
                                            //Debug.WriteLine(config.ToString());
                                        }
                                        else if (opcodes == FxOpcodes.IsInterfaceOk)
                                        {
                                            var interfaceConfig = FxInterfaceOKConfig.ParseFxInterfaceOKConfig(subBuffer);
                                            
                                            //Debug.WriteLine(interfaceConfig.ToString());
                                        }

                                        StringBuilder sb = new StringBuilder();

                                        foreach (byte temp in subBuffer)
                                        {
                                            sb.Append(temp.ToString("X2"));
                                        }

                                        Debug.WriteLine(string.Format("OpCode:{0}, value:{1}", opcodes.ToString(), sb.ToString()));
                                    }
                                    //iGuessTagStr = opcodes.ToString("X8");
                                    //proxy.ProcessRequest(opcodes, subBuffer);
                                    readedCount += subBufferCount;
                                }
                                else
                                {
                                    throw new NotImplementedException();
                                    //str = node2.Attributes.GetNamedItem("Id").Value;
                                    //str2 = "null";
                                    //if (node == null)
                                    //{
                                    //    goto Label_0232;
                                    //}
                                    //str2 = node.Attributes.GetNamedItem("Id").Value;
                                    //Label_0232:
                                    //base.CallContext.ProtocolLog.AppendGenericError("BufferInfo", string.Format("[Length:{0}; Index:{1}]", (int) byteCount, (int) readedCount));
                                    //base.CallContext.ProtocolLog.AppendGenericError("ItemID", str2);
                                    //base.CallContext.ProtocolLog.AppendGenericError("ParentFolderID", str);
                                    //throw new CorruptDataException(new LocalizedString("UploadItems - corrupted data encountered"));
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

            templateWriter.Close();
            templateStream.Close();

            ProcessTransferBuffer();

        }

        private static List<byte[]> allBuffer = new List<byte[]>();
        private static void AppendBuffer(byte[] buffer)
        {
            allBuffer.Add(buffer);
        }

        private static void ProcessTransferBuffer()
        {
            const string oldTransferBin = "D:\\EWSTest\\OldTransfer.bin";
            const string oldTransferTxt = "D:\\EWSTest\\OldTransfer.txt";
            const string newTransferBin = "D:\\EWSTest\\NewTransfer.bin";
            const string newTransferTxt = "D:\\EWSTest\\NewTransfer.txt";
            const string outputFile = "D:\\EWSTest\\Output.bin";
            const string templateFile = "D:\\EWSTest\\Template.bin";

            if (File.Exists(oldTransferBin))
                File.Delete(oldTransferBin);
            if (File.Exists(oldTransferTxt))
                File.Delete(oldTransferTxt);
            if (File.Exists(newTransferBin))
                File.Delete(newTransferBin);
            if (File.Exists(newTransferTxt))
                File.Delete(newTransferTxt);
            if (File.Exists(outputFile))
                File.Delete(outputFile);

            var length = 0;
            foreach (byte[] temp in allBuffer)
            {
                length += temp.Length;
            }

            byte[] buffer = new byte[length];
            length = 0;
            foreach (byte[] temp in allBuffer)
            {
                Array.Copy(temp, 0, buffer, length, temp.Length);
                length += temp.Length;
            }


            using (FileStream fileStream = new FileStream(oldTransferBin, FileMode.OpenOrCreate))
            {
                fileStream.Write(buffer, 0, buffer.Length);
            }
            
            var split = "\t";
            int index = 0;
            using (StreamWriter writer = new StreamWriter(oldTransferTxt))
            {
                writer.Write("Num");
                writer.Write(split);
                writer.Write("Hex");
                writer.Write(split);
                writer.Write("Dec");
                writer.Write(split);
                writer.Write("Ch");
                writer.WriteLine();
                foreach (byte b in buffer)
                {
                    writer.Write(index++);
                    writer.Write(split);
                    writer.Write(b.ToString("X2"));
                    writer.Write(split);
                    writer.Write((int)b);
                    writer.Write(split);
                    writer.Write((char)b);
                    writer.WriteLine();
                }
            }
                
            

            FTBufferRead reader = new FTBufferRead(allBuffer);
            FTStreamParseContext.NewContext(reader);
            FTMessageTreeRoot root = new FTMessageTreeRoot();
            using (FTStreamParseContext.Instance)
            {
                root.Parse();
            }

            using (IFTStreamWriter writer = new FTStreamWriter(newTransferBin))
            {
                root.GetByteData(writer);
            }

            index = 0;
            using (FileStream readerAfterWriter = new FileStream(newTransferBin, FileMode.Open))
            {
                using (StreamWriter writer = new StreamWriter(newTransferTxt))
                {
                    writer.Write("Num");
                    writer.Write(split);
                    writer.Write("Hex");
                    writer.Write(split);
                    writer.Write("Dec");
                    writer.Write(split);
                    writer.Write("Ch");
                    writer.WriteLine();
                    while (true)
                    {
                        if (readerAfterWriter.Length == readerAfterWriter.Position)
                        {
                            break;
                        }
                        var b = readerAfterWriter.ReadByte();
                        writer.Write(index++);
                        writer.Write(split);
                        writer.Write(b.ToString("X2"));
                        writer.Write(split);
                        writer.Write((int)b);
                        writer.Write(split);
                        writer.Write((char)b);
                        writer.WriteLine();
                    }
                        
                }
            }
            
            
            using (FileStream readerCom = new FileStream(oldTransferBin, FileMode.Open))
            {
                using (FileStream readerAfterWriter = new FileStream(newTransferBin, FileMode.Open))
                {
                    int readed = 0;
                    while (true)
                    {
                        if (readerCom.Length == readerCom.Position || readerAfterWriter.Length == readerAfterWriter.Position)
                        {
                            break;
                        }

                        var b1 = readerCom.ReadByte();
                        var b2 = readerAfterWriter.ReadByte();
                        readed++;

                        if (b1 != b2)
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }

            IList<IFTTransferUnit> allTransferUnit = new List<IFTTransferUnit>();
            root.GetAllTransferUnit(allTransferUnit);

            int allUnitCount = 0;
            foreach(IFTTransferUnit unit in allTransferUnit)
            {
                allUnitCount += unit.BytesCount;
            }
            if (allUnitCount != root.BytesCount)
                throw new NotSupportedException();
            
            using (FileStream outputStream = new FileStream(outputFile, FileMode.CreateNew))
            {
                using (FTFileStream ftFileStream = new FTFileStream(templateFile))
                {
                    using (BinaryWriter writer = new BinaryWriter(outputStream))
                    {
                        ftFileStream.WriteConfigs(writer);
                        ftFileStream.WriteTransferData(allTransferUnit, writer);
                    }
                }
            }

        }

    }
}
