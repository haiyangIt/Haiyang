using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Log.Impl
{
    public class LogToBlob : DefaultLog
    {
        public LogToBlob()
        {
            //RegisterLogStream(new LogBlobStreamProvider());
        }
    }

    //public class LogBlobStreamProvider : LogStreamProviderBase
    //{
    //    public LogBlobStreamProvider() {

    //        BlobNameFormat = "{0}-{1}{2}";
    //    }

    //    public LogBlobStreamProvider(string blobNameFormat)
    //    {
    //        BlobNameFormat = blobNameFormat;
    //    }

    //    private static CloudStorageAccount StorageAccount = FactoryBase.GetStorageAccount();

    //    internal static CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();

    //    private DateTime LastDateTime = DateTime.MinValue.Date;
    //    private CloudAppendBlob _appendBlob = null;

    //    private volatile int _logCount = 0;
    //    private const int MaxBlocks = 50000;
    //    private int _logFileIndex = 0;

    //    protected string BlobNameFormat
    //    {
    //        get; set;
    //    }

    //    public override string GetTotalLog(DateTime date)
    //    {
    //        CloudBlobContainer container = BlobClient.GetContainerReference("logs");
    //        StringBuilder sb = new StringBuilder();
    //        if (container.Exists())
    //        {
    //            CloudAppendBlob appBlob = null;
    //            int index = 0;
    //            do
    //            {
    //                appBlob = container.GetAppendBlobReference(
    //                    string.Format(BlobNameFormat, date.ToString("yyyyMMdd"), index++, ".log")
    //                );

    //                if (appBlob.Exists())
    //                {
    //                    sb.AppendLine(appBlob.DownloadText());
    //                }
    //                else
    //                {
    //                    appBlob = null;
    //                }

    //            } while (appBlob != null);
    //        }
    //        else
    //            sb.AppendLine("No log exists.");
    //        return sb.ToString();
    //    }

    //    public override void Write(string information)
    //    {
    //        WriteToAppendBlob(information);
    //    }

    //    public override void WriteLine(string information)
    //    {
    //        WriteToAppendBlob(information);
    //    }

    //    private void WriteToAppendBlob(string msg)
    //    {
    //        //var currentDay = DateTime.Now.Date;
    //        //if (currentDay != LastDateTime)
    //        //{
    //        //    _appendBlob = null;
    //        //}

    //        //if (Interlocked.CompareExchange(ref _logCount, 0, MaxBlocks) != _logCount)
    //        //{
    //        //    _appendBlob = null;
    //        //}

    //        //if (_appendBlob == null)
    //        //{
    //        //    lock (SyncObj)
    //        //    {
    //        //        if (_appendBlob == null)
    //        //        {
    //        //            CloudBlobContainer container = BlobClient.GetContainerReference("logs");
    //        //            container.CreateIfNotExists();
    //        //            LastDateTime = currentDay;
    //        //            CloudAppendBlob appBlob = container.GetAppendBlobReference(
    //        //                string.Format(BlobNameFormat, DateTime.Now.ToString("yyyyMMdd"), _logFileIndex++, ".log")
    //        //            );

    //        //            if (!appBlob.Exists())
    //        //            {
    //        //                appBlob.CreateOrReplace();
    //        //            }

    //        //            _appendBlob = appBlob;
    //        //        }
    //        //    }
    //        //}

    //        //msg = msg + "\r\nArcserve";
    //        //_appendBlob.AppendText(msg);
    //        //Interlocked.Increment(ref _logCount);
    //    }
    //}

    public class LogToBlobEwsTrace : LogToBlob
    {
        public LogToBlobEwsTrace() : base()
        {
            //RegisterLogStream(new LogBlobStreamProvider("{0}EwsTrace-{1}{2}"));
        }
    }
}
