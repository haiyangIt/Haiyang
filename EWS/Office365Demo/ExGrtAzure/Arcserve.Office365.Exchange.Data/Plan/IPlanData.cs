﻿using Arcserve.Office365.Exchange.Data.Account;
using Microsoft.WindowsAzure.Scheduler.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data.Plan
{
    public interface IPlanData
    {
        string Name { get; }
        string Organization { get; }
        string CredentialInfo { get; }
        string PlanMailInfos { get; }
        DateTime FirstStartTime { get; }
        DateTime NextFullBackupTime { get; }
        string LastSyncStaus { get; }
    }

    public interface IPlanMailInfo
    {

    }

    public interface IPlanAzureInfo
    {
        Job Job { get; }
    }

    public class PlanUtil
    {
        public static string GetMailString(List<IPlanMailInfo> mails)
        {
            throw new NotImplementedException();
        }

        public static OrganizationAdminInfo GetAdminInfo(string credential)
        {
            OrganizationAdminInfo info = new OrganizationAdminInfo();
            byte[] data = Convert.FromBase64String(credential);
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    info.UserName = reader.ReadString();
                    info.UserPassword = reader.ReadString();
                    info.UserDomain = reader.ReadString();
                    info.OrganizationName = reader.ReadString();
                }
            }
            return info;
        }

        public static string GetAdminInfo(OrganizationAdminInfo adminInfo)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(adminInfo.UserName);
                    writer.Write(adminInfo.UserPassword);
                    writer.Write(adminInfo.UserDomain);
                    writer.Write(adminInfo.OrganizationName);

                    return Convert.ToBase64String(stream.ToArray());
                }

            }

        }
    }
}