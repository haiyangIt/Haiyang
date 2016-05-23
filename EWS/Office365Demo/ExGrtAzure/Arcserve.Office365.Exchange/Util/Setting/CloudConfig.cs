using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Arcserve.Office365.Exchange.Util.Setting
{
    public class CloudConfig
    {

        public static CloudConfig Instance = new CloudConfigCache();

        public virtual CloudStorageAccount StorageAccount
        {
            get
            {
                return CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionStringRunning"));
            }
            protected set { }
        }

        protected CloudConfig() { }

        public virtual string DbConnectString
        {
            get
            {
                //using (StreamWriter writer = new StreamWriter("E:\\Log.txt", true))
                //{
                //    writer.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //    writer.WriteLine(string.Format("ConnectString:{0}", CloudConfigurationManager.GetSetting("Organization")));
                //    writer.WriteLine(this.GetType().FullName);
                //}
                //throw new NotImplementedException();
                return CloudConfigurationManager.GetSetting("Organization");
            }
            protected set { }
        }

        public virtual string DbDefaultConnectString
        {
            get
            {
                //using (StreamWriter writer = new StreamWriter("E:\\Log.txt", true))
                //{
                //    writer.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //    writer.WriteLine(string.Format("ConnectString:{0}", CloudConfigurationManager.GetSetting("DefaultConnection")));
                //    writer.WriteLine(this.GetType().FullName);
                //}
                //throw new NotImplementedException();
                return CloudConfigurationManager.GetSetting("DefaultConnection");
            }
            protected set { }
        }


        public static bool IsRunningOnAzure()
        {
            var isDebugAzure = CloudConfigurationManager.GetSetting("ForDebugAzure");
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")) || isDebugAzure == "1";
        }

        public virtual string LogPath
        {
            get
            {
                return CloudConfigurationManager.GetSetting("LogPath");
            }
            protected set { }
        }

        public virtual bool IsLog
        {
            get
            {
                return CloudConfigurationManager.GetSetting("IsLog") == "1";
            }
            protected set { }
        }

        public virtual string ServiceBusConnectionString
        {
            get
            {
                return CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            }
            protected set { }
        }

        public virtual string ServiceBusNameSpace
        {
            get
            {
                return CloudConfigurationManager.GetSetting("ServiceBusNameSpace");
            }
            protected set { }
        }

        public virtual string ServiceBusQueueName
        {
            get
            {
                return CloudConfigurationManager.GetSetting("ServiceBusQueueName");
            }
            protected set { }
        }

        public virtual int ServiceBusQueueMaxSize
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["ServiceBusQueueTTL"], out result))
                    return 5120;
                return result;
            }
            protected set { }
        }

        public virtual int ServiceBusQueueTTL
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["ServiceBusQueueTTL"], out result))
                    return 5;
                return result;
            }
            protected set { }
        }

        public virtual string ServiceBusTopicName
        {
            get
            {
                return CloudConfigurationManager.GetSetting("ServiceBusTopicName");
            }
            protected set { }
        }

        public virtual int ServiceBusTopicMaxSize
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["ServiceBusTopicMaxSize"], out result))
                    return 5120;
                return result;
            }
            protected set { }
        }

        public virtual int ServiceBusTopicTTL
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["ServiceBusTopicTTL"], out result))
                    return 5;
                return result;
            }
            protected set { }
        }

        public virtual string SubscriptionNameForAzure
        {
            get
            {
                return CloudConfigurationManager.GetSetting("SubscriptionNameForScheduler");
            }
            protected set { }
        }

        public virtual int ExportItemTimeOut
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["ExportItemTimeOut"], out result))
                {
                    result = 120;
                }
                return result * 1000;
            }
            protected set { }
        }

        public virtual int RequestTimeOut
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["RequestTimeOut"], out result))
                {
                    result = 120;
                }
                return result * 1000;
            }
            protected set { }
        }

        public virtual int MaxItemChangesReturn
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["MaxItemChangesReturn"], out result))
                {
                    result = 10;
                }
                return result;
            }
            protected set { }
        }

        public virtual bool IsRewriteDataIfReadFlagChanged
        {
            get
            {
                bool result = false;
                if (bool.TryParse(ConfigurationManager.AppSettings["IsRewriteDataIfReadFlagChanged"], out result))
                {
                    return result;
                }
                return false;
            }
            protected set { }
        }

        public virtual int BatchExportImportItemMaxCount
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["BatchExportImportItemMaxCount"], out result))
                {
                    result = 10;
                }
                return result;
            }
            protected set { }
        }

        /// <summary>
        /// one item if his size is equal this value, then will start a request for this item. not put it to the request for batch items.
        /// </summary>
        /// <remarks>
        /// if item larger than this value, we still support. but if larger than the SupportMaxSize, we can't not support it.
        /// </remarks>

        public virtual int BatchExportImportItemMaxSizeForSingleMB
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["BatchExportImportItemMaxSizeForSingleMB"], out result))
                {
                    result = 10;
                }
                return result * 1024 * 1024;
            }
            protected set { }
        }

        public virtual int SupportMaxSize
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["SupportMaxSize"], out result))
                {
                    result = 15;
                }
                return result * 1024 * 1024;
            }
            protected set { }
        }

        public virtual int BatchExportImportMaxAddCount
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["BatchExportImportMaxAddCount"], out result))
                {
                    result = 50;
                }
                return result;
            }
            protected set { }
        }

        public virtual int BatchExportImportSmallCountInPartition
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["BatchExportImportSmallCountInPartition"], out result))
                {
                    result = 7;
                }
                return result;
            }
            protected set { }
        }
        public virtual int BatchExportImportLargeCountInPartition
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["BatchExportImportLargeCountInPartition"], out result))
                {
                    result = 3;
                }
                return result;
            }
            protected set { }
        }

        public virtual int BatchLoadPropertyItemCount
        {
            get
            {
                int result = 0;
                if (!int.TryParse(ConfigurationManager.AppSettings["BatchLoadPropertyItemCount"], out result))
                {
                    result = 10;
                }
                return result;
            }
            protected set { }
        }

        public virtual int LogFileMaxRecordCount
        {
            get
            {
                int result = 500;
                if (int.TryParse(ConfigurationManager.AppSettings["LogFileMaxRecordCount"], out result))
                    return result;
                return 500;
            }
            protected set { }
        }
    }

    public class CloudConfigCache : CloudConfig
    {
        public override CloudStorageAccount StorageAccount
        {
            get; protected set;
        }

        internal CloudConfigCache()
        {
            StorageAccount = base.StorageAccount;
            DbConnectString = base.DbConnectString;

            DbDefaultConnectString = base.DbDefaultConnectString;

            LogPath = base.LogPath;

            IsLog = base.IsLog;

            ServiceBusConnectionString = base.ServiceBusConnectionString;

            ServiceBusNameSpace = base.ServiceBusNameSpace;

            ServiceBusQueueName = base.ServiceBusQueueName;

            ServiceBusQueueMaxSize = base.ServiceBusQueueMaxSize;

            ServiceBusQueueTTL = base.ServiceBusQueueTTL;

            ServiceBusTopicName = base.ServiceBusTopicName;

            ServiceBusTopicMaxSize = base.ServiceBusTopicMaxSize;

            ServiceBusTopicTTL = base.ServiceBusTopicTTL;

            SubscriptionNameForAzure = base.SubscriptionNameForAzure;

            ExportItemTimeOut = base.ExportItemTimeOut;

            RequestTimeOut = base.RequestTimeOut;

            MaxItemChangesReturn = base.MaxItemChangesReturn;

            IsRewriteDataIfReadFlagChanged = base.IsRewriteDataIfReadFlagChanged;

            BatchExportImportItemMaxCount = base.BatchExportImportItemMaxCount;

            BatchExportImportItemMaxSizeForSingleMB = base.BatchExportImportItemMaxSizeForSingleMB;

            SupportMaxSize = base.SupportMaxSize;

            BatchExportImportMaxAddCount = base.BatchExportImportMaxAddCount;

            BatchExportImportSmallCountInPartition = base.BatchExportImportSmallCountInPartition;

            BatchExportImportLargeCountInPartition = base.BatchExportImportLargeCountInPartition;

            BatchLoadPropertyItemCount = base.BatchLoadPropertyItemCount;
            LogFileMaxRecordCount = base.LogFileMaxRecordCount;
        }

        public override int LogFileMaxRecordCount
        {
            get; protected set;
        }

        public override string DbConnectString
        {
            get; protected set;
        }

        public override string DbDefaultConnectString
        {
            get; protected set;
        }


        public static bool IsRunningOnAzure()
        {
            var isDebugAzure = CloudConfigurationManager.GetSetting("ForDebugAzure");
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")) || isDebugAzure == "1";
        }

        public override string LogPath
        {
            get; protected set;
        }

        public override bool IsLog
        {
            get; protected set;
        }

        public override string ServiceBusConnectionString
        {
            get; protected set;
        }

        public override string ServiceBusNameSpace
        {
            get; protected set;
        }

        public override string ServiceBusQueueName
        {
            get; protected set;
        }

        public override int ServiceBusQueueMaxSize
        {
            get; protected set;
        }

        public override int ServiceBusQueueTTL
        {
            get; protected set;
        }

        public override string ServiceBusTopicName
        {
            get; protected set;
        }

        public override int ServiceBusTopicMaxSize
        {
            get; protected set;
        }

        public override int ServiceBusTopicTTL
        {
            get; protected set;
        }

        public override string SubscriptionNameForAzure
        {
            get; protected set;
        }

        public override int ExportItemTimeOut
        {
            get; protected set;
        }

        public override int RequestTimeOut
        {
            get; protected set;
        }

        public override int MaxItemChangesReturn
        {
            get; protected set;
        }

        public override bool IsRewriteDataIfReadFlagChanged
        {
            get; protected set;
        }

        public override int BatchExportImportItemMaxCount
        {
            get; protected set;
        }

        /// <summary>
        /// one item if his size is equal this value, then will start a request for this item. not put it to the request for batch items.
        /// </summary>
        /// <remarks>
        /// if item larger than this value, we still support. but if larger than the SupportMaxSize, we can't not support it.
        /// </remarks>

        public override int BatchExportImportItemMaxSizeForSingleMB
        {
            get; protected set;
        }

        public override int SupportMaxSize
        {
            get; protected set;
        }

        public override int BatchExportImportMaxAddCount
        {
            get; protected set;
        }

        public override int BatchExportImportSmallCountInPartition
        {
            get; protected set;
        }
        public override int BatchExportImportLargeCountInPartition
        {
            get; protected set;
        }

        public override int BatchLoadPropertyItemCount
        {
            get; protected set;
        }
    }

    //internal class ConfigInAppconfigOrWebConfig : CloudConfig
    //{
    //    internal ConfigInAppconfigOrWebConfig() { }
    //    public override string DbConnectString
    //    {
    //        get
    //        {
    //            //using (StreamWriter writer = new StreamWriter("E:\\Log.txt", true))
    //            //{
    //            //    writer.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    //            //    writer.WriteLine(string.Format("ConnectString:{0}", ConfigurationManager.AppSettings["Organization"]));
    //            //    writer.WriteLine(this.GetType().FullName);
    //            //    writer.WriteLine(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
    //            //}
    //            //throw new NotImplementedException();
    //            return ConfigurationManager.AppSettings["Organization"];
    //        }
    //    }

    //    public override string DbDefaultConnectString
    //    {
    //        get
    //        {
    //            //using (StreamWriter writer = new StreamWriter("E:\\Log.txt", true))
    //            //{
    //            //    writer.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    //            //    writer.WriteLine(string.Format("ConnectString:{0}", ConfigurationManager.AppSettings["DefaultConnection"]));
    //            //    writer.WriteLine(this.GetType().FullName);
    //            //    writer.WriteLine(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
    //            //}
    //            //throw new NotImplementedException();
    //            return ConfigurationManager.AppSettings["DefaultConnection"];
    //        }
    //    }


    //    public override string LogPath
    //    {
    //        get
    //        {
    //            return ConfigurationManager.AppSettings["LogPath"];
    //        }
    //    }

    //    public override string ServiceBusConnectionString
    //    {
    //        get
    //        {
    //            return ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
    //        }
    //    }

    //    public override string ServiceBusNameSpace
    //    {
    //        get
    //        {
    //            return ConfigurationManager.AppSettings["ServiceBusNameSpace"];
    //        }
    //    }

    //    public override string ServiceBusQueueName
    //    {
    //        get
    //        {
    //            return ConfigurationManager.AppSettings["ServiceBusQueueName"];
    //        }
    //    }

    //    public override int ServiceBusQueueMaxSize
    //    {
    //        get
    //        {
    //            return Convert.ToInt32(ConfigurationManager.AppSettings["ServiceBusQueueMaxSize"]);
    //        }
    //    }

    //    public override int ServiceBusQueueTTL
    //    {
    //        get
    //        {
    //            return Convert.ToInt32(ConfigurationManager.AppSettings["ServiceBusQueueTTL"]);
    //        }
    //    }

    //    public override string ServiceBusTopicName
    //    {
    //        get
    //        {
    //            return ConfigurationManager.AppSettings["ServiceBusTopicName"];
    //        }
    //    }

    //    public override int ServiceBusTopicMaxSize
    //    {
    //        get
    //        {
    //            return Convert.ToInt32(ConfigurationManager.AppSettings["ServiceBusTopicMaxSize"]);
    //        }
    //    }

    //    public override int ServiceBusTopicTTL
    //    {
    //        get
    //        {
    //            return Convert.ToInt32(ConfigurationManager.AppSettings["ServiceBusTopicTTL"]);
    //        }
    //    }

    //    public override string SubscriptionNameForAzure
    //    {
    //        get
    //        {
    //            return ConfigurationManager.AppSettings["SubscriptionNameForScheduler"];
    //        }
    //    }
    //}
}
