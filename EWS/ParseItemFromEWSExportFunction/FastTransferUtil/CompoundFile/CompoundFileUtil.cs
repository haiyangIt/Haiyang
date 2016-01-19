using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using FTStreamUtil;
using FTStreamUtil.Item;
using FTStreamUtil.Item.PropValue;
using FastTransferUtil.CompoundFile.MsgStruct;

namespace FastTransferUtil.CompoundFile
{
    public class CompoundFileUtil
    {
        public static CompoundFileUtil Instance = new CompoundFileUtil();

        #region CompoundFileInMemory
        public IStorage CreateStorageInMemory(out ILockBytes lockBytes)
        {
            NativeDll.CreateILockBytesOnHGlobal(IntPtr.Zero, true, out lockBytes);
            STGM flag = STGM.CREATE | STGM.SHARE_EXCLUSIVE | STGM.READWRITE;
            IStorage result;
            NativeDll.StgCreateDocfileOnILockBytes(lockBytes, flag, 0, out result);
            return result;
        }

        public byte[] ReadAllByteInlockBytes(ILockBytes iLockBytes)
        {
            System.Runtime.InteropServices.ComTypes.STATSTG iLockBytesStat;
            iLockBytesStat = new System.Runtime.InteropServices.ComTypes.STATSTG();
            iLockBytes.Stat(out iLockBytesStat, 1);
            int iLockBytesSize = (int)iLockBytesStat.cbSize;
            byte[] iLockBytesContent = new byte[iLockBytesSize];
            iLockBytes.ReadAt(0, iLockBytesContent, iLockBytesContent.Length, null);
            return iLockBytesContent;
        }

        public byte[] ConvertBinToMsg(byte[] binBytes)
        {
            ILockBytes memory = null;
            try {
                using (MemoryStream reader = new MemoryStream(binBytes))
                {
                    BuildMsgInMemoryWithBin(reader, out memory);
                    var bytes = ReadAllByteInlockBytes(memory);
                    return bytes;
                }
            }
            finally
            {
                CompoundFileUtil.Instance.ReleaseComObj(memory);
            }
        }
        #endregion

        #region CompoundFileStorageUtil
        public void ReleaseComObj<T>(T comObj)
        {
            if (comObj != null)
            {
                Marshal.ReleaseComObject(comObj);
                comObj = default(T);
            }
        }



        public IStorage GetRootStorage(string name, bool isCreate)
        {
            IStorage result = null;
            STGM flag = STGM.READ | STGM.DIRECT | STGM.SHARE_EXCLUSIVE;
            if (isCreate)
            {
                flag = STGM.CREATE | STGM.SHARE_EXCLUSIVE | STGM.READWRITE;
                NativeDll.StgCreateDocfile(name, (uint)flag, 0, out result);

            }
            else
                NativeDll.StgOpenStorage(name, null, flag, IntPtr.Zero, 0, out result);

            var error = Marshal.GetLastWin32Error();
            if (result == null)
            {
                throw new NullReferenceException(error.ToString("X8"));
            }
            return result;
        }

        public void CopyStorage(IStorage srcStorage, IStorage desStorage)
        {
            srcStorage.CopyTo(0, null, IntPtr.Zero, desStorage);
        }

        public IStream GetChildStream(string streamName, bool isCreate, IStorage parentStorage)
        {
            STGM flag = STGM.DIRECT | STGM.READ | STGM.SHARE_EXCLUSIVE;
            if (isCreate)
                flag = STGM.CREATE | STGM.SHARE_EXCLUSIVE | STGM.READWRITE;

            IStream result = null;

            if (isCreate)
            {
                parentStorage.CreateStream(streamName, (uint)flag, 0, 0, out result);
            }
            else
            {
                parentStorage.OpenStream(streamName, IntPtr.Zero, (uint)flag, 0, out result);
            }

            var error = Marshal.GetLastWin32Error();
            if (result == null)
            {
                throw new NullReferenceException(error.ToString("X8"));
            }
            return result;
        }

        public byte[] ReadStream(string streamName, IStorage parentStorage, int readCount)
        {
            IStream stream = null;
            try
            {
                stream = GetChildStream(streamName, false, parentStorage);
                byte[] result = new byte[readCount];
                stream.Read(result, readCount, IntPtr.Zero);
                return result;
            }
            finally
            {
                ReleaseComObj(stream);
            }
        }

        public IStorage GetChildStorage(string stgName, bool isCreate, IStorage parentStorage = null)
        {
            IStorage result = null;
            STGM flag = STGM.DIRECT | STGM.READ | STGM.SHARE_EXCLUSIVE;
            int error;
            if (isCreate)
            {
                flag = STGM.CREATE | STGM.SHARE_EXCLUSIVE | STGM.READWRITE;
            }
            if (parentStorage == null)
            {
                NativeDll.StgOpenStorage(stgName, null, flag, IntPtr.Zero, 0, out result);
                error = Marshal.GetLastWin32Error();
            }
            else
            {
                if (isCreate)
                {
                    parentStorage.CreateStorage(stgName, (uint)flag, 0, 0, out result);
                    error = Marshal.GetLastWin32Error();
                }
                else
                {
                    parentStorage.OpenStorage(stgName, null, (uint)flag, IntPtr.Zero, 0, out result);
                    error = Marshal.GetLastWin32Error();
                }
            }

            if (result == null)
            {
                throw new NullReferenceException(error.ToString("X8"));
            }
            return result;
        }

        public void DisplayStorage(IStorage srcStorage, int indent = 0)
        {
            IEnumSTATSTG elements;
            srcStorage.EnumElements(0, IntPtr.Zero, 0, out elements);
            var error = Marshal.GetLastWin32Error();

            System.Runtime.InteropServices.ComTypes.STATSTG[] stats = new System.Runtime.InteropServices.ComTypes.STATSTG[1];
            uint temp = 0;
            uint result = elements.Next(1, stats, out temp);
            error = Marshal.GetLastWin32Error();
            while (result == 0)
            {
                var stat = stats[0];
                if (stat.type == (int)STGTY.STGTY_STORAGE)
                {
                    Debug.WriteLine(string.Format("{2}Storage:{0}, clsid:{1}.", stat.pwcsName, stat.clsid, " ".PadLeft(indent)));
                    var childStorage = GetChildStorage(stat.pwcsName, false, srcStorage);
                    DisplayStorage(childStorage, indent + 2);
                }
                else if (stat.type == (int)STGTY.STGTY_STREAM)
                {
                    Debug.WriteLine(string.Format("{2}Stream:{0}, clsid:{1}.", stat.pwcsName, stat.cbSize, " ".PadLeft(indent)));
                    IStream childStream = GetChildStream(stat.pwcsName, false, srcStorage);
                    DisplayStream(childStream, indent + 2);
                }

                result = elements.Next(1, stats, out temp);
                error = Marshal.GetLastWin32Error();
            }
        }

        public void DisplayStream(IStream childStream, int indent)
        {

        }
        #endregion

        public void BuildMsgInMemoryWithBin(Stream binFileInfoStream, out ILockBytes lockBytes)
        {
            List<byte[]> allBuffer = GetContentFromBin(binFileInfoStream);
            FTParserUtil util = new FTParserUtil(allBuffer);
            var root = util.Parser();

            IStorage storage = null;
            CompoundFileBuild build = new CompoundFileBuild();
            try
            {
                storage = CreateStorageInMemory(out lockBytes);
                build.SetRootStorage(storage);
                root.WriteToCompoundFile(build);
                storage.Commit(0);
                lockBytes.Flush();
            }
            finally
            {
                ReleaseComObj(storage);
            }
        }

        public FTMessageTreeRoot ParseBin(Stream binFileInfoStream)
        {
            List<byte[]> allBuffer = GetContentFromBin(binFileInfoStream);
            FTParserUtil util = new FTParserUtil(allBuffer);
            return util.Parser();
        }

        public void CompareMsg(string sourMsg, string desMsg)
        {
            IStorage sourRootStorage = null;
            IStorage desRootStorage = null;
            try
            {
                sourRootStorage = GetRootStorage(sourMsg, false);
                desRootStorage = GetRootStorage(desMsg, false);

                var sourTopStruct = new TopLevelStruct(sourRootStorage);
                var desRootStruct = new TopLevelStruct(desRootStorage);

                sourTopStruct.Parser();
                desRootStruct.Parser();

                var stringbuilder = new StringBuilder();
                sourTopStruct.Compare(desRootStruct, stringbuilder, 0);
                Debug.WriteLine(stringbuilder.ToString());
            }
            finally
            {
                ReleaseComObj(sourRootStorage);
                ReleaseComObj(desRootStorage);
            }
        }

        private List<byte[]> GetContentFromBin(Stream stream)
        {
            List<byte[]> allBuffer = new List<byte[]>();

            stream.Seek(0, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(stream);
            {
                while (stream.Position < stream.Length)
                {
                    int iGuessIsTag = reader.ReadInt32();
                    int subBufferCount = reader.ReadInt32();
                    byte[] subBuffer = null;
                    if (reader.BaseStream.Position + subBufferCount <= reader.BaseStream.Length)
                    {
                        if (subBufferCount > 0)
                            subBuffer = reader.ReadBytes(subBufferCount);
                        FxOpcodes opcodes = (FxOpcodes)iGuessIsTag;
                        if (opcodes != FxOpcodes.TransferBuffer || (subBuffer != null && subBuffer.Length > 0))
                        {
                            if (opcodes == FxOpcodes.TransferBuffer)
                            {
                                allBuffer.Add(subBuffer);
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
            }
            stream.Seek(0, SeekOrigin.Begin);
            return allBuffer;
        }

        public enum FxOpcodes
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
    }


}
