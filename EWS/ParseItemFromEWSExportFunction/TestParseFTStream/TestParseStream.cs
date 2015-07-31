using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using FTStreamUtil.FTStream;
using System.Collections.Generic;
using FTStreamUtil;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace TestParseFTStream
{
    [TestClass]
    public class TestParseStream
    {
        string oldTransferBin = "D:\\EWSTest\\OldTransfer.bin";
        string oldTransferTxt = "D:\\EWSTest\\OldTransfer.txt";
        string newTransferBin = "D:\\EWSTest\\NewTransfer.bin";
        string newTransferTxt = "D:\\EWSTest\\NewTransfer.txt";
        string outputFile = "D:\\EWSTest\\Output.bin";
        string templateFile = "D:\\EWSTest\\Template.bin";

        //string fileName = "D:\\simple.bin";
        //string fileName = "D:\\no attach simple mail.bin";
        //string fileName = "D:\\withEmbedAttach.bin";
        //string fileName = "D:\\EWSTest\\simple.bin";
        string exportFileName = "D:\\withEmbedAttach.bin";

        [TestInitialize]
        public void TestInit()
        {
            oldTransferBin = "D:\\EWSTest\\OldTransfer.bin";
            oldTransferTxt = "D:\\EWSTest\\OldTransfer.txt";
            newTransferBin = "D:\\EWSTest\\NewTransfer.bin";
            newTransferTxt = "D:\\EWSTest\\NewTransfer.txt";
            outputFile = "D:\\EWSTest\\Output.bin";
            templateFile = "D:\\EWSTest\\Template.bin";
            exportFileName = @"D:\11Study\04EWS-ExchangeWebService\10Com\ManagedCOM\ManagedCOM\TestBuildMessage\bin\Debug\simple test.bin";

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
            Assert.IsTrue(File.Exists(exportFileName), string.Format("file {0} is not exist", exportFileName));
        }

        private List<byte[]> allBuffer = new List<byte[]>();
        private void AppendBuffer(byte[] buffer)
        {
            allBuffer.Add(buffer);
        }

        [TestMethod]
        public void TestParseFtStream()
        {
            if (File.Exists(templateFile))
                File.Delete(templateFile);
            FileStream templateStream = new FileStream(templateFile, FileMode.CreateNew);
            BinaryWriter templateWriter = new BinaryWriter(templateStream);
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
                    int outputIndex = 0;
                    while (readedCount < byteCount)
                    {
                        outputIndex++;
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
                                        Debug.WriteLine(string.Format("Section{2}: FxOpCodes:{0:-30}\t\tCount:{1}", opcodes.ToString(), subBufferCount, outputIndex));
                                        AppendBuffer(subBuffer);
                                    }

                                    else
                                    {
                                        templateWriter.Write(subBuffer);

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

        private void ProcessTransferBuffer()
        {
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


            GenerateTransferBinFile(buffer, oldTransferBin);

            GenerateTransferTxtFile(buffer, oldTransferTxt);

            FTBufferRead reader = new FTBufferRead(allBuffer);
            FTStreamParseContext.NewContext(reader);
            FTMessageTreeRoot root = new FTMessageTreeRoot();
            using (FTStreamParseContext.Instance)
            {
                root.Parse();
            }

            GenerateMessageBin(root);

            GenerateMessageTransferTxtFile();

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

                        Assert.AreNotSame(b1, b2, string.Format("In index [2], oldByte:[0] is not equal newByte [1]", b1, b2, readed));
                    }
                }
            }

            IList<IFTTransferUnit> allTransferUnit = new List<IFTTransferUnit>();
            root.GetAllTransferUnit(allTransferUnit);

            int allUnitCount = 0;
            foreach (IFTTransferUnit unit in allTransferUnit)
            {
                allUnitCount += unit.BytesCount;
            }

            Assert.AreNotSame(allUnitCount, root.BytesCount, string.Format("allUnitCountByte:[0] is not equal message all Bytes:[1]", allUnitCount, root.BytesCount));
            Assert.AreNotSame(allUnitCount, length, string.Format("after parse, the bytes count:[0] is not equal old bin file length:[1].", allUnitCount, length));

            GenerateStreamByMessage(allTransferUnit);

        }

        private void GenerateStreamByMessage(IList<IFTTransferUnit> allTransferUnit)
        {
            using (FileStream outputStream = new FileStream(outputFile, FileMode.CreateNew))
            {
                using (BinaryWriter writer = new BinaryWriter(outputStream))
                {
                    using (FTFileStream ftFileStream = new FTFileStream(templateFile, writer))
                    {

                        ftFileStream.WriteConfigs();
                        ftFileStream.WriteTransferData(allTransferUnit);
                    }
                }
            }
        }

        private void GenerateMessageTransferTxtFile()
        {
            int index = 0;
            string split = "\t";
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
        }

        private void GenerateMessageBin(FTMessageTreeRoot root)
        {
            using (IFTStreamWriter writer = new FTStreamWriter(newTransferBin))
            {
                root.GetByteData(writer);
            }
        }

        private void GenerateTransferTxtFile(byte[] buffer, string transferTxt)
        {
            string split = "\t";
            int index = 0;
            using (StreamWriter writer = new StreamWriter(transferTxt))
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
        }

        private void GenerateTransferBinFile(byte[] buffer, string transferBin)
        {
            using (FileStream fileStream = new FileStream(transferBin, FileMode.OpenOrCreate))
            {
                fileStream.Write(buffer, 0, buffer.Length);
            }
        }

        class FxConfig
        {
            UInt32 Flag;
            UInt32 TransferMethod;


            public static FxConfig ParseFxConfig(byte[] buffer)
            {
                //return null;
                FxConfig config = new FxConfig();
                int pos = 0;
                config.Flag = (UInt32)BitConverter.ToUInt32(buffer, pos);
                pos += 4;
                config.TransferMethod = (UInt32)BitConverter.ToUInt32(buffer, pos);
                return config;
            }

            public override string ToString()
            {
                return string.Format("Flag:{0}\t\tTransferMethod:{1}.", Flag.ToString("X8"), TransferMethod.ToString("X8"));
            }
        }

        class FxInterfaceOKConfig
        {
            UInt32 Flags;
            Guid Refiid;
            UInt32 TransferMethod;

            public static FxInterfaceOKConfig ParseFxInterfaceOKConfig(byte[] buffer)
            {
                //return null;
                FxInterfaceOKConfig config = new FxInterfaceOKConfig();
                int pos = 0;
                config.Flags = (UInt32)BitConverter.ToUInt32(buffer, pos);
                pos += 4;
                byte[] guidbuffer = new byte[0x10];
                Array.Copy(buffer, pos, guidbuffer, 0, 0x10);
                config.Refiid = new Guid(guidbuffer);
                pos += 0x10;
                config.TransferMethod = (UInt32)BitConverter.ToUInt32(buffer, pos);
                return config;
            }

            public override string ToString()
            {
                return string.Format("Flag:{0}\t\tRefiid:{1}\t\tTransferMethod:{2}.", Flags.ToString("X8"), Refiid.ToString(), TransferMethod.ToString("X8"));
            }
        }
    }
}
