﻿using EwsFrame;
using LogInterface;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using EwsFrame.Util;

namespace LogImpl
{
    public class LogToBlob : ILog
    {
        public LogToBlob()
        {
        }

        private static CloudStorageAccount StorageAccount = FactoryBase.GetStorageAccount();

        internal static CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();

        private LogToBlobThread _intance = null;
        private LogToBlobThread Instance
        {
            get
            {
                if (_intance == null)
                {
                    using (_lock.LockWhile(() =>
                    {
                        if (_intance == null)
                        {
                            _intance = new LogToBlobThread(BlobNameFormat);
                        }
                    }
                        ))
                    { }
                }
                return _intance;
            }
        }

        protected virtual string BlobNameFormat
        {
            get
            {
                return "{0}-{1}{2}";
            }
        }


        private static object _lock = new object();

        public event EventHandler<string> WriteLogMsgEvent;
        private void TriggerEvent(string msg)
        {
            try
            {
                if (WriteLogMsgEvent != null)
                {
                    WriteLogMsgEvent.Invoke(null, msg);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
            }
        }

        protected virtual void WriteToAppendBlob(string msg)
        {
            Trace.TraceInformation(msg);
            Instance.WriteToLog(msg);
            TriggerEvent(msg);
            //var currentDay = DateTime.Now.Date;
            //if (currentDay != LastDateTime)
            //{
            //    _appendBlob = null;
            //}

            //if (Interlocked.CompareExchange(ref _logCount, 0, MaxBlocks) != _logCount)
            //{
            //    _appendBlob = null;
            //}

            //if (_appendBlob == null)
            //{
            //    lock (_lockObj)
            //    {
            //        if (_appendBlob == null)
            //        {
            //            CloudBlobContainer container = BlobClient.GetContainerReference("logs");
            //            container.CreateIfNotExists();
            //            LastDateTime = currentDay;
            //            CloudAppendBlob appBlob = container.GetAppendBlobReference(
            //                string.Format(BlobNameFormat, DateTime.Now.ToString("yyyyMMdd"), _logFileIndex++, ".log")
            //            );

            //            if (!appBlob.Exists())
            //            {
            //                appBlob.CreateOrReplace();
            //            }

            //            _appendBlob = appBlob;
            //        }
            //    }
            //}

            //msg = msg + "\r\nArcserve";
            //using (MemoryStream stream = new MemoryStream())
            //{
            //    StreamWriter writer = new StreamWriter(stream);
            //    writer.Write(msg);
            //    stream.Seek(0, SeekOrigin.Begin);
            //    _appendBlob.AppendBlock(stream);
            //}
            //Interlocked.Increment(ref _logCount);
        }

        public void WriteException(LogInterface.LogLevel level, string message, Exception exception, string exMsg)
        {
            WriteToAppendBlob(DefaultLog.GetExceptionString(level, message, exception, exMsg));
        }

        public void WriteLog(LogInterface.LogLevel level, string message)
        {
            WriteToAppendBlob(DefaultLog.GetLogString(level, message));
        }

        public void WriteLog(LogInterface.LogLevel level, string message, string format, params object[] args)
        {
            WriteToAppendBlob(DefaultLog.GetLogString(level, message, format, args));
        }

        public string GetTotalLog(DateTime date)
        {
            CloudBlobContainer container = BlobClient.GetContainerReference("logs");
            StringBuilder sb = new StringBuilder();
            if (container.Exists())
            {
                CloudAppendBlob appBlob = null;
                int index = 0;
                do
                {
                    appBlob = container.GetAppendBlobReference(
                        string.Format(BlobNameFormat, date.ToString("yyyyMMdd"), index++, ".log")
                    );

                    if (appBlob.Exists())
                    {
                        sb.AppendLine(appBlob.DownloadText());
                    }
                    else
                    {
                        appBlob = null;
                    }

                } while (appBlob != null);
            }
            else
                sb.AppendLine("No log exists.");
            return sb.ToString();
        }

        public void Dispose()
        {
            if (Instance != null)
            {
                Instance.Dispose();
            }
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        class LogToBlobThread : ManageBase
        {
            public string BlobNameFormat;
            public LogToBlobThread(string blobNameFormat) : base("LogToBlobThread")
            {
                BlobNameFormat = blobNameFormat;
            }


            private DateTime LastDateTime = DateTime.MinValue.Date;
            private CloudAppendBlob _appendBlob = null;

            private volatile int _logCount = 0;
            private const int MaxBlocks = 49900;

            private ConcurrentQueue<string> MsgList = new ConcurrentQueue<string>();
            private const int FlushCount = 20;

            private int _logFileIndex = 0;

            private object _lockObj = new object();

            private CloudAppendBlob Create(DateTime currentDay)
            {
                CloudAppendBlob result = null;
                using (_lockObj.LockWhile(() =>
                {
                    CloudBlobContainer container = BlobClient.GetContainerReference("logs");
                    container.CreateIfNotExists();
                    LastDateTime = currentDay;
                    CloudAppendBlob appBlob = container.GetAppendBlobReference(
                        string.Format(BlobNameFormat, DateTime.Now.ToString("yyyyMMdd"), _logFileIndex++, ".log")
                    );

                    if (!appBlob.Exists())
                    {
                        appBlob.CreateOrReplace();
                    }

                    result = appBlob;
                }))
                { }
                return result;
            }

            protected override void Flush()
            {
                using (_lockObj.LockWhile(() =>
                {
                    WriteToAppendBlob();
                }))
                { }
            }

            private void WriteToAppendBlob()
            {
                var currentDay = DateTime.Now.Date;
                if (currentDay != LastDateTime)
                {
                    _appendBlob = null;
                }

                if (_logCount >= MaxBlocks)
                {
                    _appendBlob = null;
                }

                if (_appendBlob == null)
                //Interlocked.CompareExchange(ref _appendBlob, Create(currentDay), null);
                {
                    using (_lockObj.LockWhile(() =>
                    {
                        if (_appendBlob == null)
                        {
                            _appendBlob = Create(currentDay);
                        }
                    }))
                    { }
                }

                StringBuilder sb = new StringBuilder();
                while (MsgList.Count > 0)
                {
                    string result = null;
                    bool hasItems = MsgList.TryDequeue(out result);
                    if (hasItems)
                    {
                        sb.AppendLine(result);
                        sb.Append("Arcserve");
                    }
                }
                if (sb.Length > 0)
                {
                    using (_lockObj.LockWhile(() =>
                    {
                        _appendBlob.AppendText(sb.ToString());
                    }))
                    { }
                }

                //using (MemoryStream stream = new MemoryStream())
                //{
                //    StreamWriter writer = new StreamWriter(stream);
                //    writer.Write(msg);
                //    stream.Seek(0, SeekOrigin.Begin);
                //    _appendBlob.AppendBlock(stream);
                //}
                Interlocked.Increment(ref _logCount);
            }

            protected override void DoWriteLog(string msg)
            {
                MsgList.Enqueue(msg);

                if (MsgList.Count > FlushCount)
                {
                    using (_lockObj.LockWhile(() =>
                    {
                        WriteToAppendBlob();
                    }))
                    { }
                }
            }

            protected override void DoDispose()
            {
            }
        }
    }

    public class LogToBlobEwsTrace : LogToBlob
    {
        protected override string BlobNameFormat
        {
            get
            {
                return "{0}EwsTrace-{1}{2}";
            }
        }


        private int _logCount = 0;
        private bool IsWriteLog = true;

        protected override void WriteToAppendBlob(string msg)
        {
            Interlocked.Increment(ref _logCount);
            if (_logCount > 20)
            {
                IsWriteLog = ConfigurationManager.AppSettings["WriteEWSTrace"] == "1";
                Interlocked.Exchange(ref _logCount, 0);
            }

            if (IsWriteLog)
            {
                base.WriteToAppendBlob(msg);
            }
        }
    }
}
