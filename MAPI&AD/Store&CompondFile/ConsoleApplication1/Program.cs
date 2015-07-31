using ConsoleApplication1.FTStream;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ConsoleApplication1
{

    class Program
    {
        private static List<byte[]> allBuffer = new List<byte[]>();
        private static void AppendBuffer(byte[] buffer)
        {
            allBuffer.Add(buffer);
        }

        private static void ProcessTransferBuffer()
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


            var path = "D:\\transferBuffer.bin";
            if (!File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }

            var readPath = "D:\\trasferBuffer.txt";
            var split = "\t";
            int index = 0;
            if(!File.Exists(readPath))
            {
                using(StreamWriter writer = new StreamWriter(readPath))
                {
                    writer.Write("Num");
                    writer.Write(split);
                    writer.Write("Hex");
                    writer.Write(split);
                    writer.Write("Dec");
                    writer.Write(split);
                    writer.Write("Ch");
                    writer.WriteLine();
                    foreach(byte b in buffer)
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

            long readedOffset = 0;
            var result = FTStreamRootParse.ParseFTStreamRoot(buffer);
            FTStreamRootParse.Output(result);
            //FTStream.Parser(buffer);
        }

        

        static void Main(string[] args)
        {
            //string fileName = "D:\\simple.bin";
            //string fileName = "D:\\no attach simple mail.bin";
            string fileName = "D:\\mytest.bin";
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
                    byteCount = buffer.GetLength(0);
                    while(readedCount < byteCount){
                        
                        if ((readedCount + 8) <= byteCount)
                        {
                            iGuessIsTag = BitConverter.ToInt32(buffer, readedCount);
                            readedCount += 4;
                            subBufferCount = BitConverter.ToInt32(buffer, readedCount);
                            readedCount += 4;
                            if ((readedCount + subBufferCount) <= byteCount)
                            {
                                subBuffer = (subBufferCount > 0) ? new byte[subBufferCount] : null;
                                if(subBufferCount > 0)
                                {
                                    Array.ConstrainedCopy(buffer, readedCount, subBuffer, 0, subBufferCount);
                                }
                                //FaultInjection.GenerateFault(-943313603);
                                opcodes = (FxOpcodes)iGuessIsTag;
                                if (opcodes != FxOpcodes.TransferBuffer || subBuffer != null)
                                {

                                    


                                    if(opcodes == FxOpcodes.TransferBuffer)
                                    {
                                        Debug.WriteLine(string.Format("OpCode:{0}, Count:{1}", opcodes.ToString(), subBufferCount));
                                        AppendBuffer(subBuffer);
                                    }

                                    else
                                    {
                                        if (opcodes == FxOpcodes.Config)
                                        {
                                            var config = FxConfig.ParseFxConfig(subBuffer);
                                            Debug.WriteLine(config.ToString());
                                        }
                                        else if(opcodes == FxOpcodes.IsInterfaceOk)
                                        {
                                            var interfaceConfig = FxInterfaceOKConfig.ParseFxInterfaceOKConfig(subBuffer);
                                            Debug.WriteLine(interfaceConfig.ToString());
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

            ProcessTransferBuffer();
        
        }

        class FxConfig
        {
            UInt32 Flag;
            UInt32 TransferMethod;

            public static FxConfig ParseFxConfig(byte[] buffer)
            {
                FxConfig config = new FxConfig();
                int pos = 0;
                config.Flag = (UInt32)ParseSerialize.ParseInt32(buffer, pos);
                pos += 4;
                config.TransferMethod = (UInt32)ParseSerialize.ParseInt32(buffer, pos);
                return config;
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
                FxInterfaceOKConfig config = new FxInterfaceOKConfig();
                int pos = 0;
                config.Flags = (UInt32)ParseSerialize.ParseInt32(buffer, pos);
                pos += 4;
                byte[] guidbuffer = new byte[0x10];
                Array.Copy(buffer,pos,guidbuffer,0,0x10);
                config.Refiid = new Guid(guidbuffer);
                pos += 0x10;
                config.TransferMethod = (UInt32)ParseSerialize.ParseInt32(buffer, pos);
                return config;
            }

            public override string ToString()
            {
                return string.Format("Flag:{0},Refiid:{1},TransferMethod:{2}", Flags.ToString("X8"), Refiid.ToString(),TransferMethod.ToString("X8"));
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

        //private void DoOperation(FxOpcodes opCode, byte[] request)
        //{
        //    <>c__DisplayClassb classb;
        //    <>c__DisplayClassd classd;
        //    <>c__DisplayClassf classf;
        //    <>c__DisplayClass11 class2;
        //    <>c__DisplayClass14 class3;
        //    <>c__DisplayClass16 class4;
        //    <>c__DisplayClass18 class5;
        //    FxOpcodes opcodes;
        //    if (ComponentTrace<MapiNetTags>.CheckEnabled(0x24) != null)
        //    {
        //        ComponentTrace<MapiNetTags>.Trace<string, string>(0xb50d, 0x24, (long) this.GetHashCode(), "MapiFxProxy.DoOperation: opCode={0}, request={1}", ((FxOpcodes) opCode).ToString(), TraceUtils.DumpArray(request));
        //    }
    
        //    opcodes = opCode;
        //    switch (opCode)
        //    {
        //        case FxOpcodes.Config : // config
        //            classb = new <>c__DisplayClassb();
        //            classb.flags = 0;
        //            classb.transferMethod = 0;
        //            BinaryDeserializer.Deserialize(request, new BinaryDeserializer.DeserializeDelegate(classb.<DoOperation>b__3));
        //            this.fxCollector.Config(classb.flags, classb.transferMethod);
        //            break;

        //        case FxOpcodes.TransferBuffer: // TransferBuffer
        //            /*
        //             * 
        //             * // Used with OpenProperty to get interface for folders, messages, attachmentson
        //                #define PR_FAST_TRANSFER				PROP_TAG( PT_OBJECT, pidStoreMin+0x17)
        //             * #define		 INTERFACE	IExchangeFastTransfer
        //                DECLARE_MAPI_INTERFACE_(IExchangeFastTransfer, IUnknown)
        //                {
        //                    MAPI_IUNKNOWN_METHODS(PURE)
        //                    EXCHANGE_IEXCHANGEFASTTRANSFER_METHODS(PURE)
        //                };
        //             */

        //            this.fxCollector.TransferBuffer(request); // This function is call 
        //            break;

        //        case FxOpcodes.IsInterfaceOk: // IsInterfaceOk
        //            classd = new <>c__DisplayClassd();
        //            classd.transferMethod = 0;
        //            classd.refiid = Guid.Empty;
        //            classd.flags = 0;
        //            BinaryDeserializer.Deserialize(request, new BinaryDeserializer.DeserializeDelegate(classd.<DoOperation>b__4));
        //            this.fxCollector.IsInterfaceOk(classd.transferMethod, classd.refiid, classd.flags);
        //            break;

        //        case FxOpcodes.TellPartnerVersion: // TellPartnerVersion
        //            this.fxCollector.TellPartnerVersion(request);
        //            break;

        //        case FxOpcodes.StartMdbEventsImport: //StartMdbEventsImport
        //            this.fxCollector.StartMdbEventsImport();
        //            break;

        //        case FxOpcodes.FinishMdbEventsImport: //FinishMdbEventsImport
        //            classf = new <>c__DisplayClassf();
        //            classf.success = 0;
        //            BinaryDeserializer.Deserialize(request, new BinaryDeserializer.DeserializeDelegate(classf.<DoOperation>b__5));
        //            this.fxCollector.FinishMdbEventsImport(classf.success);
        //            break;

        //                case FxOpcodes.AddMdbEvents: //AddMdbEvents
        //            this.fxCollector.AddMdbEvents(request);
        //            break;

        //        case FxOpcodes.SetWatermarks: //SetWatermarks
        //            class2 = new <>c__DisplayClass11();
        //            class2.WMs = null;
        //            BinaryDeserializer.Deserialize(request, new BinaryDeserializer.DeserializeDelegate(class2.<DoOperation>b__6));
        //            this.fxCollector.SetWatermarks(class2.WMs);
        //            break;

        //        case FxOpcodes.SetReceiveFolder: //SetReceiveFolder
        //            class3 = new <>c__DisplayClass14();
        //            class3.entryId = null;
        //            class3.messageClass = null;
        //            BinaryDeserializer.Deserialize(request, new BinaryDeserializer.DeserializeDelegate(class3.<DoOperation>b__8));
        //            this.fxCollector.SetReceiveFolder(class3.entryId, class3.messageClass);
        //            break;

        //        case FxOpcodes.SetPerUser: //SetPerUser
        //            class4 = new <>c__DisplayClass16();
        //            class4.ltid = new MapiLtidNative();
        //            class4.guidReplica = Guid.Empty;
        //            class4.lib = 0;
        //            class4.pb = null;
        //            class4.fLast = 0;
        //            BinaryDeserializer.Deserialize(request, new BinaryDeserializer.DeserializeDelegate(class4.<DoOperation>b__9));
        //            this.fxCollector.SetPerUser(class4.ltid, class4.guidReplica, class4.lib, class4.pb, class4.fLast);
        //            break;

        //        case FxOpcodes.SetProps: //SetProps
        //            class5 = new <>c__DisplayClass18();
        //            class5.pva = null;
        //            BinaryDeserializer.Deserialize(request, new BinaryDeserializer.DeserializeDelegate(class5.<DoOperation>b__a));
        //            this.fxCollector.SetProps(class5.pva);
        //            break;
        //        default:
        //            throw new ArgumentException("Invalid FxOpcode", -2147024809);
        //            //MapiExceptionHelper.ThrowIfError("Invalid FxOpcode", -2147024809);
        //    }
    
        //    if (ComponentTrace<MapiNetTags>.CheckEnabled(0x24) != null)
        //    {
        //        ComponentTrace<MapiNetTags>.Trace(0xf50d, 0x24, (long) this.GetHashCode(), "MapiFxProxy.DoOperation succeeded");
        //    }
    
        //}


        static void Test(string[] args)
        {
            using (FileStream fileStream = new FileStream("D:\\zhongwen.bin", FileMode.Open))
            {
                byte[] buffer = new byte[fileStream.Length];
                byte[] subBuffer;
                fileStream.Read(buffer, 0, (int)fileStream.Length);
                int readedCount = 0;
                int byteCount = 0;
                int iGuessIsTag = 0;
                int subBufferCount = 0;
                int opcodes = 0;
                string str;
                string str2;
                XmlNode node2;
                XmlNode node = null; //no itemid, so can't overwrite;
            //proxy = item.MapiMessage.GetFxProxyCollector();
            Label_0147:
                try
                {
                    //buffer = Base64StringConverter.Parse(node3.InnerText);
                    readedCount = 0;
                    byteCount = buffer.GetLength(0);
                    goto Label_02AF;
                Label_0166:
                    if ((readedCount + 8) <= byteCount)
                    {
                        goto Label_017E;
                    }
                    throw new ApplicationException();// CorruptDataException(new LocalizedString("FastTransferUpload, Uploaded Data"));
                Label_017E:
                    iGuessIsTag = BitConverter.ToInt32(buffer, readedCount);
                    readedCount += 4;
                    subBufferCount = BitConverter.ToInt32(buffer, readedCount);
                    readedCount += 4;
                    if ((readedCount + subBufferCount) <= byteCount)
                    {
                        goto Label_01B9;
                    }
                    throw new ApplicationException(); //CorruptDataException(new LocalizedString("FastTransferUpload, Uploaded Data"));
                Label_01B9:
                    subBuffer = (subBufferCount > 0) ? new byte[subBufferCount] : null;
                    if (subBufferCount <= 0)
                    {
                        goto Label_01DD;
                    }
                    Array.ConstrainedCopy(buffer, readedCount, subBuffer, 0, subBufferCount);
                Label_01DD:
                    //FaultInjection.GenerateFault(-943313603);
                    opcodes = iGuessIsTag;
                    if (opcodes != 2)
                    {
                        goto Label_DealSubBuffer; //deal subbuffer
                    }
                    if (subBuffer != null)
                    {
                        goto Label_DealSubBuffer; // deal subbuffer
                    }
                    throw new NotImplementedException();
                //str = node2.Attributes.GetNamedItem("Id").Value;
                //str2 = "null";
                //if (node == null)
                //{
                //    goto Label_0232;
                //}
                //str2 = node.Attributes.GetNamedItem("Id").Value;
                Label_0232:
                //base.CallContext.ProtocolLog.AppendGenericError("BufferInfo", string.Format("[Length:{0}; Index:{1}]", (int) byteCount, (int) readedCount));
                //base.CallContext.ProtocolLog.AppendGenericError("ItemID", str2);
                //base.CallContext.ProtocolLog.AppendGenericError("ParentFolderID", str);
                //throw new CorruptDataException(new LocalizedString("UploadItems - corrupted data encountered"));
                Label_DealSubBuffer:
                    //proxy.ProcessRequest(opcodes, subBuffer);
                    readedCount += subBufferCount;
                Label_02AF:
                    if (readedCount < byteCount)
                    {
                        goto Label_0166;
                    }

                }
                catch (Exception e) { }
            }

        }
    }

    public enum TagType : uint
    {
        MV_FLAG = 0x1000,

        PT_UNSPECIFIED = 0,
        PT_NULL = 1,	/* NULL property value */
        PT_I2 = 2,	/* Signed 16-bit value */
        PT_LONG = 3,	/* Signed 32-bit value */
        PT_R4 = 4,	/* 4-byte floating point */
        PT_DOUBLE = 5,	/* Floating point double */
        PT_CURRENCY = 6,	/* Signed 64-bit int (decimal w/	4 digits right of decimal pt, */
        PT_APPTIME = 7,	/* Application time */
        PT_ERROR = 10,	/* 32-bit error value */
        PT_BOOLEAN = 11,	/* 16-bit boolean (non-zero true) */
        PT_OBJECT = 13,	/* Embedded object in a property */
        PT_I8 = 20,	/* 8-byte signed integer */
        PT_STRING8 = 30,	/* Null terminated 8-bit character string */
        PT_UNICODE = 31,	/* Null terminated Unicode string */
        PT_SYSTIME = 64,	/* FILETIME 64-bit int w/ number of 100ns periods since Jan 1,1601 */
        PT_CLSID = 72,	/* OLE GUID */
        PT_BINARY = 258,	/* Uninterpreted (counted byte array) */
        PT_SERVERID = 0x00FB,


        PT_MV_I2 = MV_FLAG | PT_I2,
        PT_MV_LONG = MV_FLAG | PT_LONG,
        PT_MV_R4 = MV_FLAG | PT_R4,
        PT_MV_DOUBLE = MV_FLAG | PT_DOUBLE,
        PT_MV_CURRENCY = MV_FLAG | PT_CURRENCY,
        PT_MV_APPTIME = MV_FLAG | PT_APPTIME,
        PT_MV_SYSTIME = MV_FLAG | PT_SYSTIME,
        PT_MV_STRING8 = MV_FLAG | PT_STRING8,
        PT_MV_BINARY = MV_FLAG | PT_BINARY,
        PT_MV_UNICODE = MV_FLAG | PT_UNICODE,
        PT_MV_CLSID = MV_FLAG | PT_CLSID,
        PT_MV_I8 = MV_FLAG | PT_I8
    }

    public class PTypMVInteger16 
    {

        
    }

    public class PTypMVInteger32
    {

    }

    public class PTypMVInteger64 
    {
    }

    public class PTypMVFloat32 
    {

    }

    public class PTypMVFloat64 
    {
    }

    public class PTypMVCurrency 
    {

    }

    public class PTypMVAppTime 
    {

    }

    public class PTypMVSysTime
    {

    }

    public class PTypMVString8 
    {

    }

    public class PTypMVBinary 
    {

    }

    public class PTypMVUnicode  
    {
    }

    public class PTypMVGuid
    {

    }

    


    

    //public class FTStream
    //{

    //    public List<IElement> Elements;

    //    public FTStream() {
    //        Elements = new List<IElement>();
    //    }

    //    public void AddElement(IElement element)
    //    {
    //        Elements.Add(element);
    //    }

    //    public static FTStream Parser(byte[] buffer)
    //    {
    //        StreamFactory.Initialize();
    //        int readedCount = 0;
    //        var result = new FTStream();

    //        while (readedCount < buffer.Length)
    //        {
    //            IMarker marker = null;
    //            if (PTypInteger32.JudgeIsMarker(buffer, ref readedCount, out marker))
    //            {
    //                result.AddElement(marker);
    //            }
    //            else
    //            {
    //                IPropValue propValue = StreamFactory.ParsePropValue(buffer, ref readedCount);
    //                result.AddElement(propValue);
    //            }
    //        }

    //        throw new NotImplementedException();
    //    }

    //    public static byte[] ToByte(FTStream ftStream)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    



   

    
    

    

    public interface IElement
    { }

    public interface IMarker : IElement,ILog
    {
    }

    

    

    

    public interface IFixSizeValue_Length_VarSizeValue
    {

    }

    public class Length_VarSizeValue : IFixSizeValue_Length_VarSizeValue
    {
        internal ILength Length;
        internal IVarSizeValue VarSizeValue;
    }

    public class PropInfoUtil
    {

    }

    

    public interface IFixedSizeValue : IFixSizeValue_Length_VarSizeValue
    {
        void ParseValue(byte[] buffer, ref int pos);
    }

    public class PTypByte
    {
        public readonly byte Value;

        private PTypByte(byte value){
            Value = value;
        }

        public static byte GetX00OrX01(byte[] buffer, ref int pos)
        {
            byte[] values = ParseSerialize.ParseBinary(buffer, pos, 1);
            pos += 1;
            return values[0];
        }

        public override string ToString()
        {
            return string.Format("Byte:[{0}]", Value.ToString("X2"));
        }
    }

    public class PTypFloating32 : ValueBase<UInt32>, IFixedSizeValue
    {
        private PTypFloating32(UInt32 value) : base(value) { }

        private PTypFloating32() : base() { }

        protected override ParseFixValue<UInt32> ParseFixValueFunc
        {
            get { return SignToUnSign; }
        }

        private UInt32 SignToUnSign(byte[] buffer, int pos)
        {
            return (UInt32)ParseSerialize.ParseInt32(buffer, pos);
        }

        protected override int PosAddNum
        {
            get { return 4; }
        }

        internal static IFixedSizeValue CreateFixedPropValue()
        {
            return new PTypFloating32();
        }

        public override string ToString()
        {
            return string.Format("Float32:[{0}]", Value.ToString("X8"));
        }
    }


    public class PtypFloating64 : ValueBase<UInt64>, IFixedSizeValue
    {
        private PtypFloating64(UInt64 value) : base(value) { }

        private PtypFloating64() : base() { }

        protected override ParseFixValue<UInt64> ParseFixValueFunc
        {
            get { return SignToUnSign; }
        }

        private UInt64 SignToUnSign(byte[] buffer, int pos)
        {
            return (UInt64)ParseSerialize.ParseInt64(buffer, pos);
        }

        protected override int PosAddNum
        {
            get { return 8; }
        }

        internal static IFixedSizeValue CreateFixedPropValue()
        {
            return new PtypFloating64();
        }

        public override string ToString()
        {
            return string.Format("Float64:[{0}]", Value.ToString("X8"));
        }
    }

    public class PtypCurrency : ValueBase<UInt64>, IFixedSizeValue
    {
        private PtypCurrency(UInt64 value) : base(value) { }

        private PtypCurrency() : base() { }

        protected override ParseFixValue<UInt64> ParseFixValueFunc
        {
            get { return SignToUnSign; }
        }

        private UInt64 SignToUnSign(byte[] buffer, int pos)
        {
            return (UInt64)ParseSerialize.ParseInt64(buffer, pos);
        }

        protected override int PosAddNum
        {
            get { return 8; }
        }

        internal static IFixedSizeValue CreateFixedPropValue()
        {
            return new PtypCurrency();
        }

        public override string ToString()
        {
            return string.Format("Currency:[{0}]", Value.ToString("X8"));
        }
    }

    public class PtypFloatingTime : PtypTime, IFixedSizeValue
    {
        private PtypFloatingTime(DateTime value) : base(value) { }

        private PtypFloatingTime() : base() { }

        public static IFixedSizeValue CreateFixedPropValue()
        {
            return new PtypFloatingTime();
        }

        public override string ToString()
        {
            return string.Format("Time:[{0}]", Value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

    class PtypBoolean : ValueBase<UInt16>, IFixedSizeValue
    {
        private PtypBoolean(UInt16 value) : base(value) { }

        private PtypBoolean() : base() { }

        protected override ParseFixValue<UInt16> ParseFixValueFunc
        {
            get { return SignToUnSign; }
        }

        private UInt16 SignToUnSign(byte[] buffer, int pos)
        {
            return (UInt16)ParseSerialize.ParseInt16(buffer, pos);
        }

        protected override int PosAddNum
        {
            get { return 2; }
        }

        internal static IFixedSizeValue CreateFixedPropValue()
        {
            return new PtypBoolean();
        }

        public override string ToString()
        {
            return string.Format("Bool:[{0}]", Value.ToString("X4"));
        }
    }

    class PtypInteger64 : ValueBase<UInt64>, IFixedSizeValue
    {
        private PtypInteger64(UInt64 value) : base(value) { }

        private PtypInteger64() : base() { }

        protected override ParseFixValue<UInt64> ParseFixValueFunc
        {
            get { return SignToUnSign; }
        }

        private UInt64 SignToUnSign(byte[] buffer, int pos)
        {
            return (UInt64)ParseSerialize.ParseInt64(buffer, pos);
        }

        protected override int PosAddNum
        {
            get { return 8; }
        }

        internal static IFixedSizeValue CreateFixedPropValue()
        {
            return new PtypInteger64();
        }

        public override string ToString()
        {
            return string.Format("Int64:[{0}]", Value.ToString("X8"));
        }
    }
    public class PtypTime : ValueBase<DateTime>, IFixedSizeValue
    {
        public UInt64 IntValue;

        protected PtypTime(DateTime value) : base(value) { }

        protected PtypTime() : base() { }

        protected override ParseFixValue<DateTime> ParseFixValueFunc
        {
            get { return ParseSerialize.ParseFileTime; }
        }

        protected override int PosAddNum
        {
            get { return 8; }
        }

        public static IFixedSizeValue CreateFixedPropValue()
        {
            return new PtypTime();
        }

        public override string ToString()
        {
            return string.Format("Time:[{0}]", Value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

    public interface IVarSizeValue
    {
        void ParseValue(byte[] buffer, ref int pos, uint p);
    }

    public class PTypStrWithCodePage : PTypStr ,IVarSizeValue
    {
        private IVarPropType _varPropType;
        private UInt16 _codePage
        {
            get
            {
                return (UInt16)(((PtypInteger16)_varPropType).Value & ((UInt16)0x0FFF));
            }
        }
        private PTypStrWithCodePage(IVarPropType propType) : base() {
            _varPropType = propType;
        }

        private PTypStrWithCodePage(UnicodeChar[] value, IVarPropType propType) : base(value) {
            _varPropType = propType;
        }

        public static IVarSizeValue CreateVarPropValue(IVarPropType propType)
        {
            return new PTypStrWithCodePage(propType);
        }

        private string _toStringResult;
        public override string ToString()
        {
            if (string.IsNullOrEmpty(_toStringResult))
            {
                //UnicodeChar[] valueStr = Value as UnicodeChar[];
                //if (valueStr == null)
                //    return string.Empty;
                //StringBuilder sb = new StringBuilder(valueStr.Length * 2);
                //sb.Append("StringWithCodePage:codePage:[").Append(_codePage.ToString("X4")).Append("] String:[");
                //foreach (UnicodeChar chars in valueStr)
                //{
                //    sb.Append(chars.ToString());
                //}
                //sb.Append("]");
                //_toStringResult = sb.ToString();
                var coding = Encoding.GetEncoding(_codePage);
                _toStringResult = coding.GetString(allBytes);
                _toStringResult = _toStringResult.Substring(0, _toStringResult.Length - 1);
            }
            return _toStringResult;
        }
    }

    public class PTypObject : PTypBinary, IVarSizeValue
    {
        private PTypObject() : base() { }

        private PTypObject(byte[] value) : base(value) { }

        public static IVarSizeValue CreateVarPropValue()
        {
            return new PTypObject();
        }

        private string _toString;
        public override string ToString()
        {
            if (string.IsNullOrEmpty(_toString))
            {
                StringBuilder sb = new StringBuilder(Value.Length + 10);
                sb.Append("Object:[");
                foreach (byte b in Value)
                {
                    sb.Append(b);
                }
                sb.Append("]");
                _toString = sb.ToString();
            }
            return _toString;
        }
    }

    public class PTypBinary : VarValueBase<byte[]>, IVarSizeValue
    {
        protected PTypBinary() : base() { }

        protected PTypBinary(byte[] value) : base(value) { }

        protected override ParseVarValue<byte[]> ParseVarValueFunc
        {
            get { return ParseByteValue; }
        }

        private byte[] ParseByteValue(byte[] buffer, int pos, uint length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = buffer[pos + i];
            }

            CheckParseByte(result);
            return result;
        }

        protected virtual void CheckParseByte(byte[] result) { }

        internal static IVarSizeValue CreateVarPropValue()
        {
            return new PTypBinary();
        }

        private string _toString;
        public override string ToString()
        {
            if(string.IsNullOrEmpty(_toString))
            {
                StringBuilder sb = new StringBuilder(Value.Length + 10);
                sb.Append("Binary:[");
                foreach (byte b in Value)
                {
                    sb.Append(b);
                }
                sb.Append("]");
                _toString = sb.ToString();
            }
            return _toString;
        }
    }

    public class PTypServerId : PTypBinary, IVarSizeValue
    {
        private PTypServerId() : base() { }

        private PTypServerId(byte[] value) : base(value) { }

        internal static IVarSizeValue CreateVarPropValue()
        {
            return new PTypServerId();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }

    public class ThreeByte
    {
        public byte OneByte;
        public UInt16 TwoByte;
    }

    public class PtypString8 : PTypBinary, IVarSizeValue
    {
        private PtypString8() : base() { }

        private PtypString8(byte[] value) : base(value) { }



        protected override void CheckParseByte(byte[] result)
        {
            if (result[result.Length - 1] != 0x00)
                throw new ArgumentException("Terminate characters is invalid.");
        }

        
        public override string ToString()
        {
            throw new NotImplementedException();   
        }
    }

    public class NamedPropInfo
    {
        public IPropertySet PropertySet;
        public IX00Or01 X00Or01;

        public void ParseNamedPropInfo(byte[] buffer, ref int pos)
        {
            PropertySet = PTypGuid.CreatePropertySet(buffer, ref pos);
            PropertySet.ParsePropertySet(buffer, ref pos);
            X00Or01 = StreamUtil.CreateX00Or01(buffer, ref pos);
            X00Or01.ParseDispIdOrName(buffer, ref pos);
        }

        private string _toString;
        public override string ToString()
        {
            if (string.IsNullOrEmpty(_toString))
            {
                StringBuilder sb = new StringBuilder(64);
                sb.Append("NamePropInfo:[PropertySet:[").Append(PropertySet.ToString()).Append("] X00Or01:[").Append(X00Or01.ToString()).Append("]]");
                _toString = sb.ToString();
            }
            return _toString;
        }
    }

    public interface IX00Or01
    {
        void ParseDispIdOrName(byte[] buffer, ref int pos);
    }

    public class X00 : IX00Or01
    {
        public readonly static byte Value = 0x00;
        public IDispId DispId { get; private set; }

        private X00() { }

        public void ParseDispIdOrName(byte[] buffer, ref int pos)
        {
            if (DispId == null)
                DispId = PTypInteger32.CreateDispId(buffer, ref pos);
        }

        internal static IX00Or01 CreateX00()
        {
            return new X00();
        }

        private string _tostring;
        public override string ToString()
        {
            if (string.IsNullOrEmpty(_tostring))
            {
                StringBuilder sb = new StringBuilder(32);
                sb.Append("%X00 Dispid:[").Append(DispId.ToString()).Append("]");
                _tostring = sb.ToString();
            }
            return _tostring;
        }
    }

    public class X01 : IX00Or01
    {
        public readonly static byte Value = 0x01;
        public IName Name { get; private set; }

        private X01() { }

        public void ParseDispIdOrName(byte[] buffer, ref int pos)
        {
            if (Name == null)
                Name = PTypStr.CreateName(buffer, ref pos);
        }

        internal static IX00Or01 CreateX01()
        {
            return new X01();
        }

        private string _tostring;
        public override string ToString()
        {
            if(string.IsNullOrEmpty(_tostring))
            {
                StringBuilder sb = new StringBuilder(32);
                sb.Append("%X01 Name:[").Append(Name.ToString()).Append("]");
                _tostring = sb.ToString();
            }
            return _tostring;
        }
    }

    public interface IPropertySet
    {
        void ParsePropertySet(byte[] buffer, ref int pos);
    }

    public class PTypGuid : ValueBase<Guid>, IPropertySet, IFixedSizeValue
    {
        public byte[] Guid16Bytes;

        private PTypGuid(Guid guid) : base(guid)
        {
        }

        private PTypGuid() : base() { }

        protected override ParseFixValue<Guid> ParseFixValueFunc
        {
            get { return ParseSerialize.ParseGuid; }
        }

        protected override int PosAddNum
        {
            get { return 16; }
        }

        public void ParsePropertySet(byte[] buffer, ref int pos)
        {
            ParseValue(buffer, ref pos);
        }

        public static IPropertySet CreatePropertySet(byte[] buffer, ref int pos)
        {
            var guid = ParseSerialize.ParseGuid(buffer, pos);
            pos += 16;
            return new PTypGuid(guid);
        }

        internal static IFixedSizeValue CreateFixedPropValue()
        {
            return new PTypGuid();
        }

        public override string ToString()
        {
            return string.Format("Guid:[{0}]",Value.ToString());
        }
    }

    public interface IDispId
    {
        void ParseDispId(byte[] buffer, ref int pos);
    }

    public interface IName
    {
        void ParseName(byte[] buffer, ref int pos);
    }

    public class UnicodeChar
    {
        public UInt16 Char;
        public UnicodeChar(UInt16 strChar)
        {
            Char = strChar;
        }

        public override string ToString()
        {
            throw new NotImplementedException();
            StringBuilder sb = new StringBuilder(6);
            sb.Append((char)((byte)((Char >> 8) & 0xFF))).Append((char)((byte)(Char & 0xFF)));
            return sb.ToString();
        }
    }

    public class PTypStr : VarValueBase<UnicodeChar[]>, IName, IVarSizeValue
    {
        protected byte[] allBytes;

        protected PTypStr(UnicodeChar[] value) : base(value) { }

        protected PTypStr() : base() { }

        protected override ParseVarValue<UnicodeChar[]> ParseVarValueFunc
        {
            get { return ParseUnicodeCharValue; }
        }

        private UnicodeChar[] ParseUnicodeCharValue(byte[] buffer, int pos, uint length)
        {
            allBytes = new byte[length];
            Array.Copy(buffer, pos, allBytes, 0, length);

            UnicodeChar[] result = new UnicodeChar[length / 2];
            for(int i = 0 ; i < length; i += 2)
            {
                result[i / 2] = new UnicodeChar((UInt16)ParseSerialize.ParseInt16(buffer, pos + i));
            }

            if (result[result.Length - 1].Char != 0x0000)
                throw new ArgumentException("Terminate characters is invalid.");
            return result;
        }

        internal static IName CreateName(byte[] buffer, ref int pos)
        {
            List<UnicodeChar> result = new List<UnicodeChar>();
            int i = 0;
            while (true)
            {
                var temp = new UnicodeChar((UInt16)ParseSerialize.ParseInt16(buffer, pos + i));
                result.Add(temp);
                i += 2;
                if (temp.Char == 0x0000)
                {
                    break;
                }
            }


            var strResult = new PTypStr(result.ToArray());
            strResult.allBytes = new byte[i];
            Array.Copy(buffer, pos, strResult.allBytes, 0, i);

            pos += i;
            return strResult;
        }

        internal static IVarSizeValue CreateVarPropValue()
        {
            return new PTypStr();
        }

        public void ParseName(byte[] buffer, ref int pos)
        {
        }

        private string _toStringResult;
        public override string ToString()
        {
            if (string.IsNullOrEmpty(_toStringResult))
            {
                //UnicodeChar[] valueStr = Value as UnicodeChar[];
                //if(valueStr == null)
                //    return string.Empty;
                //StringBuilder sb = new StringBuilder(valueStr.Length * 2);
                //sb.Append("UnicodeString:[");
                //foreach(UnicodeChar chars in valueStr)
                //{
                //    sb.Append(chars.ToString());
                //}
                //sb.Append("]");
                //_toStringResult = sb.ToString();
                _toStringResult = Encoding.Unicode.GetString(allBytes);
                _toStringResult = _toStringResult.Substring(0, _toStringResult.Length - 1);
            }
            return _toStringResult;
        }
    }

    public interface INamedPropId // greater or equal to 0x8000
    {
    }

    public interface ITaggedPropId : IPropInfo // less than 0x8000
    {
        
    }

    public interface IPropertyId : ITaggedPropId, INamedPropId
    { 
    }

    public interface ILength // must greater 0
    {
        
    }

    

    

    

    public interface IPropType
    {
        void ParsePropType(byte[] buffer, ref int pos);
    }

    public interface IFixedPropType : IPropType
    {
        
    }

    public interface IVarPropType : IPropType
    {
        
    }

    public interface IMvPropType : IPropType
    {
        
    }

    public delegate T ParseFixValue<T>(byte[] buffer, int pos);
    public abstract class ValueBase<T>
    {
        private bool _isInit;
        private T _value;
        public T Value
        {
            get { return _value; }
            private set { _value = value; _isInit = true; }
        }

        protected abstract int PosAddNum{get;}
        protected abstract ParseFixValue<T> ParseFixValueFunc{get;}

        protected ValueBase(T value)
        {
            Value = value;
        }

        protected ValueBase()
        {

        }

        public void ParseValue(byte[] buffer, ref int pos)
        {
            if (!_isInit)
            {
                Value = ParseFixValueFunc(buffer, pos);
                pos += PosAddNum;
            }
        }
    }

    public delegate T ParseVarValue<T>(byte[] buffer, int pos, uint length);
    public abstract class VarValueBase<T>
    {
        private bool _isInit;
        private T _value;
        public T Value
        {
            get { return _value; }
            private set { _value = value; _isInit = true; }
        }

        protected abstract ParseVarValue<T> ParseVarValueFunc { get; }

        protected VarValueBase(T value)
        {
            Value = value;
        }

        protected VarValueBase()
        {

        }

        public void ParseValue(byte[] buffer, ref int pos, uint length)
        {
            if (!_isInit)
            {
                Value = ParseVarValueFunc(buffer, pos, length);
                pos += (int)length;
            }
        }
    }

    public class PtypInteger16 : ValueBase<UInt16>, IMvPropType, IVarPropType, IFixedPropType, IPropertyId, IFixedSizeValue
    {
        private PtypInteger16(UInt16 value)
            : base(value)
        {
        }

        private PtypInteger16() :base()
        {
        }

        protected override int PosAddNum { get { return 2; } }
        protected override ParseFixValue<UInt16> ParseFixValueFunc
        {
            get
            {
                return SignToUnSign;
            }
        }

        private UInt16 SignToUnSign(byte[] buffer, int pos)
        {
            return (UInt16)ParseSerialize.ParseInt16(buffer, pos);
        }

        public static IFixedPropType CreateFixedPropType(UInt16 type)
        {
            return new PtypInteger16(type);
        }

        public static IVarPropType CreateVarPropType(UInt16 type)
        {
            return new PtypInteger16(type);
        }

        public static IVarPropType CreateMvPropType(UInt16 type)
        {
            return new PtypInteger16(type);
        }

        public static UInt16 GetPropType(byte[] buffer, ref int pos)
        {
            UInt16 type = (UInt16)ParseSerialize.ParseInt16(buffer, pos);
            pos += 2;
            return type;
        }

        public static UInt16 GetPropId(byte[] buffer, ref int pos)
        {
            UInt16 type = (UInt16)ParseSerialize.ParseInt16(buffer, pos);
            pos += 2;
            return type;
        }

        public static INamedPropId CreateNamedPropId(UInt16 propId)
        {
            if ((UInt16)propId < 0x8000)
                throw new ArgumentException("propid must be greater than or equal to 0x8000");
            return new PtypInteger16(propId);
        }

        public static ITaggedPropId CreateTagPropId(UInt16 propId)
        {
            if ((UInt16)propId >= 0x8000)
                throw new ArgumentException("propid must be less than 0x8000");
            return new PtypInteger16(propId);
        }

        internal static IFixedSizeValue CreateFixedPropValue()
        {
            return new PtypInteger16();
        }

        public void ParsePropType(byte[] buffer, ref int pos)
        {
            ParseValue(buffer,ref pos);
        }

        public void ParsePropInfo(byte[] buffer, ref int pos)
        {
            ParseValue(buffer,ref pos);
        }

        public override string ToString()
        {
            return string.Format("Int16:[{0}]",Value.ToString("X4"));
        }


        public ushort PropId
        {
            get { return Value; }
        }
    }

    //public class BaseByte{
    //    protected readonly byte[] Values;

    //    protected abstract int ByteCount{get;}
    //    protected BaseByte(byte[] bytes, int offset){
    //        Values = new byte[ByteCount];
    //        for(int i = 0 ; i < ByteCount ; i++)
    //        {
    //            Values[i] = bytes[i+offset];
    //        }
    //    }
    //}

    //public class OneByte : BaseByte
    //{
    //    public OneByte(byte[] bytes,int offset) : base(bytes, offset)
    //    {
    //    }
    //}

    //public class TwoByte : BaseByte
    //{
    //    public TwoByte(byte[] bytes,int offset) : base(bytes, offset)
    //    {
            
    //    }

    //    public static implicit operator UInt16(TwoByte obj){
    //        return (UInt16)ParseSerialize.ParseInt16(obj.Values, 0);
    //    }

    //    public static implicit operator Int16(TwoByte obj)
    //    {
    //        return ParseSerialize.ParseInt16(obj.Values, 0);
    //    }
    //}

    //public class ThreeByte : BaseByte
    //{
    //    public ThreeByte(byte[] bytes,int offset) : base(bytes, offset)
    //    {
            
    //    }
    //}

    //public class FourByte : BaseByte
    //{
    //   public FourByte(byte[] bytes,int offset) : base(bytes, offset)
    //    {
            
    //    }

    //   public static implicit operator UInt32(FourByte obj)
    //   {
    //       return (UInt32)ParseSerialize.ParseInt32(obj.Values, 0);
    //   }

    //   public static implicit operator Int32(FourByte obj)
    //   {
    //       return ParseSerialize.ParseInt32(obj.Values, 0);
    //   }
    //}

    //public class EightByte : BaseByte
    //{
    //    public EightByte(byte[] bytes, int offset)
    //        : base(bytes, offset)
    //    {
            
    //    }

    //    public static implicit operator UInt64(EightByte obj)
    //    {
    //        return (UInt64)ParseSerialize.ParseInt64(obj.Values, 0);
    //    }

    //    public static implicit operator Int64(EightByte obj)
    //    {
    //        return ParseSerialize.ParseInt64(obj.Values, 0);
    //    }
    //}
}
