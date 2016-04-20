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

namespace EwsFrame.Util.Setting
{
    public class CloudConfig
    {

        public static CloudConfig Instance = new CloudConfig();

        public virtual CloudStorageAccount StorageAccount
        {
            get
            {
                return CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionStringRunning"));
            }
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
        }

        public virtual string ServiceBusConnectionString
        {
            get
            {
                return CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            }
        }

        public virtual string ServiceBusNameSpace
        {
            get
            {
                return CloudConfigurationManager.GetSetting("ServiceBusNameSpace");
            }
        }

        public virtual string ServiceBusQueueName
        {
            get
            {
                return CloudConfigurationManager.GetSetting("ServiceBusQueueName");
            }
        }

        public virtual int ServiceBusQueueMaxSize
        {
            get
            {
                return Convert.ToInt32(CloudConfigurationManager.GetSetting("ServiceBusQueueMaxSize"));
            }
        }

        public virtual int ServiceBusQueueTTL
        {
            get
            {
                return Convert.ToInt32(CloudConfigurationManager.GetSetting("ServiceBusQueueTTL"));
            }
        }

        public virtual string ServiceBusTopicName
        {
            get
            {
                return CloudConfigurationManager.GetSetting("ServiceBusTopicName");
            }
        }

        public virtual int ServiceBusTopicMaxSize
        {
            get
            {
                return Convert.ToInt32(CloudConfigurationManager.GetSetting("ServiceBusTopicMaxSize"));
            }
        }

        public virtual int ServiceBusTopicTTL
        {
            get
            {
                return Convert.ToInt32(CloudConfigurationManager.GetSetting("ServiceBusTopicTTL"));
            }
        }

        public virtual string SubscriptionNameForAzure
        {
            get
            {
                return CloudConfigurationManager.GetSetting("SubscriptionNameForScheduler");
            }
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
