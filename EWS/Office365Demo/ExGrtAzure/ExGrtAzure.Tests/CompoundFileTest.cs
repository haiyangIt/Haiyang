using CompoundFileImpl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class CompoundFileTest
    {
        [TestMethod]
        public void TestCompoundFile()
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            var filename = @"D:\21GitHub\Haiyang\EWS\Office365Demo\ExGrtAzure\ExGrtAzure.Tests\bin\Debug\00MsgFile\test.msg";
            IStorage storage = null;
            int childLevel = 0;
            if (CompoundFile.StgOpenStorage(
                filename,
                null,
                STGM.DIRECT | STGM.READ | STGM.SHARE_EXCLUSIVE,
                IntPtr.Zero,
                0,
                out storage) == 0)
            {

                ReadChildStorage(storage, filename, default(STATSTG), childLevel);
            }
        }

        private void ReadChildStorage(IStorage storage, string storageName, STATSTG statStg, int childLevel)
        {
            Debug.WriteLine(string.Format("{0} Storage {1}", " ".PadLeft(childLevel), storageName));

            STATSTG statstg;
            storage.Stat(out statstg, (uint)STATFLAG.STATFLAG_DEFAULT);

            IEnumSTATSTG pIEnumStatStg = null;
            storage.EnumElements(0, IntPtr.Zero, 0, out pIEnumStatStg);

            STATSTG[] regelt = { statstg };
            uint fetched = 0;
            uint res = pIEnumStatStg.Next(1, regelt, out fetched);
            STATSTG currentStatstg;
            if (res == 0)
            {
                currentStatstg = regelt[0];
                while (res != 1)
                {
                    switch (currentStatstg.type)
                    {
                        case (int)STGTY.STGTY_STORAGE:
                            {
                                IStorage pIChildStorage;
                                storage.OpenStorage(currentStatstg.pwcsName,
                                   null,
                                   (uint)(STGM.READ | STGM.SHARE_EXCLUSIVE),
                                   IntPtr.Zero,
                                   0,
                                   out pIChildStorage);

                                ReadChildStorage(pIChildStorage, currentStatstg.pwcsName, currentStatstg, childLevel + 2);
                            }
                            break;
                        case (int)STGTY.STGTY_STREAM:
                            {

                                IStream pIStream;
                                storage.OpenStream(currentStatstg.pwcsName,
                                   IntPtr.Zero,
                                   (uint)(STGM.READ | STGM.SHARE_EXCLUSIVE),
                                   0,
                                   out pIStream);
                                ReadChildStream(pIStream, currentStatstg.pwcsName, currentStatstg, childLevel + 2);
                            }
                            break;
                    }

                    if ((res = pIEnumStatStg.Next(1, regelt, out fetched)) != 1)
                    {
                        currentStatstg = regelt[0];
                    }
                }
            }
        }

        private void ReadChildStream(IStream stream, string streamName, STATSTG statStg, int childLevel)
        {
            Debug.WriteLine(string.Format("{0} Stream {1} Size [{2}]", " ".PadLeft(childLevel), streamName, statStg.cbSize));
        }



    }
}
