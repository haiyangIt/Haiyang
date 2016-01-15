using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.IO;
using System.Diagnostics;
using FastTransferUtil.CompoundFile;

namespace TestBuildMessage
{
    [TestClass]
    public class CompoundBuildTest
    {
        [TestMethod]
        public void TestGlobal()
        {
            
        }

        [TestMethod]
        public void TestWriteStorageToMemoryToFile()
        {
            ILockBytes lockBytes = null;
            IStorage rootStorage = null;
            IStorage childStorage = null;
            IStream childStream = null;
            System.Runtime.InteropServices.ComTypes.STATSTG iLockBytesStat;
            try
            {
                var result = NativeDll.CreateILockBytesOnHGlobal(IntPtr.Zero, true, out lockBytes);

                result = NativeDll.StgCreateDocfileOnILockBytes(lockBytes, STGM.READWRITE | STGM.TRANSACTED | STGM.CREATE, 0, out rootStorage);

                rootStorage.CreateStorage("test", (uint)(STGM.CREATE | STGM.SHARE_EXCLUSIVE | STGM.READWRITE), 0, 0, out childStorage);

                childStorage.CreateStream("stream1", (uint)(STGM.CREATE | STGM.SHARE_EXCLUSIVE | STGM.READWRITE), 0, 0, out childStream);

                IntPtr actualLength = Marshal.AllocHGlobal(sizeof(int));
                byte[] myArray = new byte[] { 0, 1, 2, 3 };
                childStream.Write(myArray, myArray.Length, actualLength);
                int acb = Marshal.ReadInt32(actualLength);
                Marshal.FreeHGlobal(actualLength);

                lockBytes.Flush();
                childStream.Commit((int)STGC.STGC_DEFAULT);
                childStorage.Commit((int)STGC.STGC_DEFAULT);
                rootStorage.Commit((int)STGC.STGC_DEFAULT);

                iLockBytesStat = new System.Runtime.InteropServices.ComTypes.STATSTG();
                lockBytes.Stat(out iLockBytesStat, 1);
                int iLockBytesSize = (int)iLockBytesStat.cbSize;


                byte[] array = new byte[iLockBytesSize];
                lockBytes.ReadAt(0, array, iLockBytesSize, null);

                using (FileStream writer = new FileStream(@"D:\22GitHubOtherProject\04GlobalAlloc\1.msg", FileMode.Create))
                {
                    writer.Write(array, 0, iLockBytesSize);
                }
            }
            finally
            {
                if (childStream != null)
                {
                    Marshal.ReleaseComObject(childStream);
                    childStream = null;
                }

                if (childStorage != null)
                {
                    Marshal.ReleaseComObject(childStorage);
                    childStorage = null;
                }

                if (rootStorage != null)
                {
                    Marshal.ReleaseComObject(rootStorage);
                    rootStorage = null;
                }

                if (lockBytes != null)
                {
                    Marshal.ReleaseComObject(lockBytes);
                    lockBytes = null;
                }
            }
        }

        [TestMethod]
        public void TestCopyMsg()
        {
            string srcMsgFile = @"D:\22GitHubOtherProject\04GlobalAlloc\1.msg";
            IStorage srcRootStorage = CompoundFileUtil.Instance.GetRootStorage(srcMsgFile, false);
            CompoundFileUtil.Instance.DisplayStorage(srcRootStorage, 1);

            string desMsgFile = @"D:\22GitHubOtherProject\04GlobalAlloc\3.msg";
            IStorage desRootStorage = CompoundFileUtil.Instance.GetRootStorage(desMsgFile, true);
            CompoundFileUtil.Instance.CopyStorage(srcRootStorage, desRootStorage);
        }

        [TestMethod]
        public void TestCopyToMemory()
        {
            string srcMsgFile = @"D:\22GitHubOtherProject\04GlobalAlloc\23.msg";
            IStorage srcRootStorage = null;
            ILockBytes lockBytes = null;
            IStorage desMemStorage = null;
            try
            {
                desMemStorage = CompoundFileUtil.Instance.CreateStorageInMemory(out lockBytes);
                srcRootStorage = CompoundFileUtil.Instance.GetRootStorage(srcMsgFile, false);
                CompoundFileUtil.Instance.CopyStorage(srcRootStorage, desMemStorage);

                lockBytes.Flush();
                desMemStorage.Commit(0);

                byte[] allData = CompoundFileUtil.Instance.ReadAllByteInlockBytes(lockBytes);
                using (FileStream writer = new FileStream(@"D:\22GitHubOtherProject\04GlobalAlloc\2.msg", FileMode.Create))
                {
                    writer.Write(allData, 0, allData.Length);
                }
            }
            finally
            {
                CompoundFileUtil.Instance.ReleaseComObj(srcRootStorage);
                CompoundFileUtil.Instance.ReleaseComObj(desMemStorage);
                CompoundFileUtil.Instance.ReleaseComObj(lockBytes);
            }
        }
        
        [TestMethod]
        public void TestCreateMsgFromBin()
        {
            string binFilePath = @"D:\21GitHub\Haiyang\EWS\99TestFile\Old\Test.bin";
            string msgFilePath = string.Format( @"D:\21GitHub\Haiyang\EWS\99TestFile\New\{0}.msg", DateTime.Now.ToString("yyyyMMddHHmmss"));
            ILockBytes memory = null;
            try {
                using (StreamReader reader = new StreamReader(binFilePath))
                {
                    CompoundFileUtil.Instance.BuildMsgInMemoryWithBin(reader.BaseStream, out memory);
                    var bytes = CompoundFileUtil.Instance.ReadAllByteInlockBytes(memory);
                    using (FileStream writer = new System.IO.FileStream(msgFilePath, FileMode.Create))
                    {
                        writer.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            finally
            {
                CompoundFileUtil.Instance.ReleaseComObj(memory);
            }
        }
    }
}
