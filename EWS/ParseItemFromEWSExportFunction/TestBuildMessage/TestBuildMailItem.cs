using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FTStreamUtil.Item;
using FTStreamUtil;
using FTStreamUtil.Build;
using FTStreamUtil.Item.PropValue;
using System.Collections.Generic;
using FTStreamUtil.Build.Implement;
using System.Text;
using System.Runtime.InteropServices;
using FTStreamUtil.FTStream;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Diagnostics;

namespace TestBuildMessage
{
    [TestClass]
    public class TestBuildMailItem
    {
        [TestMethod]
        public void TestBuild()
        {
            if (File.Exists(BuildConst.OutFTStreamFileName))
                File.Delete(BuildConst.OutFTStreamFileName);

            int propListCount = 15;
            E15PropertyItem[] propertyTags = new E15PropertyItem[propListCount];
            tagBegin = 0x8001;
            for (int i = 0; i < propListCount; i++)
            {
                propertyTags[i] = GetProperty();
            }

            tagBegin = 0x7990;

            List<E15PropertyItem[]> recvPropertyTagsAll = new List<E15PropertyItem[]>(2);

            int recvLength = 8;
            E15PropertyItem[] recvPropertyTags = new E15PropertyItem[recvLength];
            for (int i = 0; i < recvLength; i++)
            {
                recvPropertyTags[i] = GetProperty();
            }
            recvPropertyTagsAll.Add(recvPropertyTags);

            tagBegin = 0x7990;
            int recv2Length = 7;
            E15PropertyItem[] recv2PropertyTags = new E15PropertyItem[recv2Length];
            for (int i = 0; i < recv2Length; i++)
            {
                recv2PropertyTags[i] = GetProperty();
            }
            recvPropertyTagsAll.Add(recv2PropertyTags);

            tagBegin = 0x3000;
            int attachment1Length = 4;

            List<E15PropertyItem[]> attachmentCollectionTag = new List<E15PropertyItem[]>(1);
            E15PropertyItem attachmentNumber = new E15PropertyItem();
            attachmentNumber.nTag = 0x0E210003;
            attachmentNumber.nUsID = 0x0E21;
            attachmentNumber.PropertyType = MAPIPropertyType.Int32;
            attachmentNumber.nLength = 4;
            byte[] attachNumberBuffer = BitConverter.GetBytes((Int32)1);
            IntPtr unmanagedAttachNumberPointer = Marshal.AllocHGlobal(4);
            allNeedFreePtr.Add(unmanagedAttachNumberPointer);
            Marshal.Copy(attachNumberBuffer, 0, unmanagedAttachNumberPointer,4);
            attachmentNumber.pValue = unmanagedAttachNumberPointer;

            E15PropertyItem[] attachPropList = new E15PropertyItem[attachment1Length];
            for (int i = 0; i < attachment1Length; i++)
            {
                attachPropList[i] = GetProperty();
            }

            attachmentCollectionTag.Add(attachPropList);

            //int hResult = 0;
            using (IItemBuild build = new ItemBuild())
            {
                Assert.AreEqual(build.StartBuild(), 0);

                if (propertyTags.Length > 0)
                {
                    using (IPropListBuild propList = build.BuildPropList())
                    {
                        Assert.AreEqual(propList.StartBuild(), 0);
                        Assert.AreEqual(propList.BuildProperties(GetPropertiesPtr(propertyTags, propListCount), propListCount), 0);
                        Assert.AreEqual(propList.EndBuild(), 0);
                    }
                }

                if (recvPropertyTagsAll.Count > 0)
                {
                    using (IRecipientCollectionBuild recvCollectionBuild = build.BuildRecipientCollection())
                    {
                        Assert.AreEqual(recvCollectionBuild.StartBuild(), 0);

                        int recvIndex = 0;
                        foreach (E15PropertyItem[] recvItems in recvPropertyTagsAll)
                        {
                            using (IRecipientBuild recvBuild = recvCollectionBuild.BuildRecipient(recvIndex++))
                            {
                                Assert.AreEqual(recvBuild.StartBuild(), 0);

                                Assert.AreEqual(recvBuild.BuildProperties(GetPropertiesPtr(recvItems, recvItems.Length), recvItems.Length), 0);

                                Assert.AreEqual(recvBuild.EndBuild(), 0);
                            }
                            recvIndex++;
                        }

                        Assert.AreEqual(recvCollectionBuild.EndBuild(), 0);
                    }
                }

                if (attachmentCollectionTag.Count > 0)
                {
                    using (IAttachmentCollectionBuild attachmentCollectionBuild = build.BuildAttachmentCollection())
                    {
                        int attachIndex = 0;
                        foreach (E15PropertyItem[] attachmentItems in attachmentCollectionTag)
                        {
                            Assert.AreEqual(attachmentCollectionBuild.StartBuild(), 0);
                            using (IAttachmentBuild attachmentBuild = attachmentCollectionBuild.BuildAttachment(attachIndex))
                            {
                                Assert.AreEqual(attachmentBuild.StartBuild(), 0);

                                Assert.AreEqual(attachmentBuild.BuildAttachmentNumber(GetPropertyPtr(attachmentNumber)), 0);
                                Assert.AreEqual(attachmentBuild.BuildProperties(GetPropertiesPtr(attachmentItems, attachmentItems.Length), attachmentItems.Length), 0);

                                //todo Build Embeded;

                                Assert.AreEqual(attachmentBuild.EndBuild(), 0);
                            }
                            Assert.AreEqual(attachmentCollectionBuild.EndBuild(), 0);
                            attachIndex++;
                        }
                    }
                }


                build.EndBuild();
            }

            foreach(IntPtr ptr in allNeedFreePtr)
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        private IntPtr GetPropertyPtr(E15PropertyItem property)
        {
            IntPtr propPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(E15PropertyItem)));
            Marshal.StructureToPtr(property, propPtr, false);
            allNeedFreePtr.Add(propPtr);
            return propPtr;
        }

        private IntPtr GetPropertiesPtr(E15PropertyItem[] properties, int length)
        {
            IntPtr propListPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(E15PropertyItem))*length);
            long longPtr = propListPtr.ToInt64();
            for (int i = 0; i < length; i++)
            {
                IntPtr itemPtr = new IntPtr(longPtr);
                allNeedFreePtr.Add(itemPtr);
                Marshal.StructureToPtr(properties[i], itemPtr, false);
                longPtr += Marshal.SizeOf(typeof(E15PropertyItem));
            }
            return propListPtr;
        }

        int totalData = 0;
        private Random random = new Random();
        private MAPIPropertyType GetPropertyType()
        {
            switch (totalData++ % 7)
            {
                case 0:
                    return MAPIPropertyType.BOOL;
                case 1:
                    return MAPIPropertyType.ByteArray;
                case 2:
                    return MAPIPropertyType.DateTime;
                case 3:
                    return MAPIPropertyType.Double;
                case 4:
                    return MAPIPropertyType.Int16;
                case 5:
                    return MAPIPropertyType.Int32;
                case 6:
                    return MAPIPropertyType.Int64;
                case 7:
                    return MAPIPropertyType.String;
                default:
                    return MAPIPropertyType.Int32;
            }
        }

        private int GetPropertyLength(MAPIPropertyType propertyType)
        {
            switch (propertyType)
            {
                case MAPIPropertyType.BOOL:
                    return 1;
                case MAPIPropertyType.ByteArray:
                    return random.Next() % 64;
                case MAPIPropertyType.DateTime:
                    return 8;
                case MAPIPropertyType.Double:
                    return 8;
                case MAPIPropertyType.Int16:
                    return 2;
                case MAPIPropertyType.Int32:
                    return 4;
                case MAPIPropertyType.Int64:
                    return 8;
                case MAPIPropertyType.String:
                    return random.Next() % 64;
                default:
                    return 0;
            }
        }

        void SetValue(byte[] value, int length, MAPIPropertyType propertyType)
        {
            byte[] buffer = null;
            switch (propertyType)
            {
                case MAPIPropertyType.BOOL:
                    if (totalData++ % 2 == 0)
                        value[0] = 0x00;
                    else
                        value[0] = 0x01;
                    break;
                case MAPIPropertyType.ByteArray:
                    for (int index = 0; index < length; index++)
                    {
                        value[index] = (byte)(totalData++);
                    }
                    break;
                case MAPIPropertyType.DateTime:
                    var dateTime = DateTime.Now.ToFileTimeUtc();
                    buffer = BitConverter.GetBytes(dateTime);
                    Array.Copy(buffer, 0, value, 0, length);
                    break;
                case MAPIPropertyType.Double:
                    double d = (double)random.NextDouble();
                    buffer = BitConverter.GetBytes(d);
                    Array.Copy(buffer, 0, value, 0, length);
                    break;
                case MAPIPropertyType.Int16:
                    Int16 i = (Int16)random.Next(UInt16.MinValue, UInt16.MaxValue);
                    buffer = BitConverter.GetBytes(i);
                    Array.Copy(buffer, 0, value, 0, length);
                    break;
                case MAPIPropertyType.Int32:
                    Int32 i16 = (Int32)random.Next((int)UInt32.MinValue, int.MaxValue);
                    buffer = BitConverter.GetBytes(i16);
                    Array.Copy(buffer, 0, value, 0, length);
                    break;
                case MAPIPropertyType.Int64:
                    Int64 i64 = (Int64)random.Next((int)UInt64.MinValue, int.MaxValue);
                    buffer = BitConverter.GetBytes(i64);
                    Array.Copy(buffer, 0, value, 0, length);
                    break;
                case MAPIPropertyType.String:
                    StringBuilder sb = new StringBuilder();
                    for(int index = 0 ; index < length - 2;index++)
                    {
                        sb.Append(totalData++ % 10);
                    }
                    buffer = Encoding.Unicode.GetBytes(sb.ToString());
                    Array.Copy(buffer, 0, value, 0, buffer.Length);
                    value[length - 2] = 0x00;
                    value[length - 1] = 0x00;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        int tagBegin = 0x7990;
        UInt32 GetPropertyTag(MAPIPropertyType mapiPropertyType)
        {
            UInt16 propertyType = 0;
            switch (mapiPropertyType)
            {
                case MAPIPropertyType.BOOL:
                    propertyType = (UInt16)PropertyType.PT_BOOLEAN;
                    break;
                case MAPIPropertyType.ByteArray:
                    propertyType = (UInt16)PropertyType.PT_BINARY;
                    break;
                case MAPIPropertyType.DateTime:
                    propertyType = (UInt16)PropertyType.PT_SYSTIME;
                    break;
                case MAPIPropertyType.Double:
                    propertyType = (UInt16)PropertyType.PT_DOUBLE;
                    break;
                case MAPIPropertyType.Int16:
                    propertyType = (UInt16)PropertyType.PT_I2;
                    break;
                case MAPIPropertyType.Int32:
                    propertyType = (UInt16)PropertyType.PT_LONG;
                    break;
                case MAPIPropertyType.Int64:
                    propertyType = (UInt16)PropertyType.PT_I8;
                    break;
                case MAPIPropertyType.String:
                    propertyType = (UInt16)PropertyType.PT_UNICODE;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return ((UInt32)tagBegin++ << 16) | propertyType;
        }

        List<IntPtr> allNeedFreePtr = new List<IntPtr>();
        E15PropertyItem GetProperty()
        {
            E15PropertyItem item = new E15PropertyItem();
            item.PropertyType = GetPropertyType();
            item.nLength = GetPropertyLength(item.PropertyType);
            item.nTag = GetPropertyTag(item.PropertyType);
            item.nUsID = (UInt16)(item.nTag >> 16);
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(item.nLength);
            allNeedFreePtr.Add(unmanagedPointer);
            byte[] value = new byte[item.nLength];
            SetValue(value, item.nLength, item.PropertyType);
            Marshal.Copy(value, 0, unmanagedPointer, item.nLength);
            item.pValue = unmanagedPointer;
            if (item.nUsID >= 0x8000)
            {
                item.namedID.ulKind = (uint)( tagBegin % 2 == 0 ? 0 : 1);

                Guid guid = Guid.NewGuid();
                byte[] guidBuffer = guid.ToByteArray();
                var data1 = (UInt32)BitConverter.ToInt32(guidBuffer,0);
                var data2 = (UInt16)BitConverter.ToInt16(guidBuffer,4);
                var data3 = (UInt16)BitConverter.ToInt16(guidBuffer,6);
                var data4 = new byte[8];
                Array.Copy(guidBuffer, 8,data4 ,0,8);
                PropSetGUID propSetGuid = new PropSetGUID();
                propSetGuid.Data1 = data1;
                propSetGuid.Data2 = data2;
                propSetGuid.Data3 = data3;
                propSetGuid.Data4 = data4;
                IntPtr guidPointer = Marshal.AllocHGlobal(Marshal.SizeOf(propSetGuid));
                Marshal.StructureToPtr(propSetGuid, guidPointer, false);
                allNeedFreePtr.Add(guidPointer);
                item.namedID.lpguid = guidPointer;

                if (item.namedID.ulKind == (uint)0)
                {
                    item.namedID.Kind.IID = (int)item.nTag;
                }
                else
                {
                    int length = random.Next() %64;
                    StringBuilder sb = new StringBuilder();
                    for(int index = 0 ; index < length - 2;index++)
                    {
                        sb.Append(totalData++ % 10);
                    }
                    
                    IntPtr stringPointer = Marshal.StringToHGlobalUni(sb.ToString());
                    item.namedID.Kind.lpwstrName = stringPointer;
                    allNeedFreePtr.Add(stringPointer);
                }
            }
            return item;
        }

        [TestMethod]
        public void ParseSuccessResponseInfo()
        {
            string xmlStr = "<?xml version=\"1.0\" encoding=\"utf-8\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Header><h:ServerVersionInfo MajorVersion=\"15\" MinorVersion=\"0\" MajorBuildNumber=\"913\" MinorBuildNumber=\"19\" Version=\"V2_10\" xmlns:h=\"http://schemas.microsoft.com/exchange/services/2006/types\" xmlns=\"http://schemas.microsoft.com/exchange/services/2006/types\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"/></s:Header><s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><m:UploadItemsResponse xmlns:m=\"http://schemas.microsoft.com/exchange/services/2006/messages\" xmlns:t=\"http://schemas.microsoft.com/exchange/services/2006/types\"><m:ResponseMessages><m:UploadItemsResponseMessage ResponseClass=\"Success\"><m:ResponseCode>NoError</m:ResponseCode><m:ItemId Id=\"AAMkAGIxZGI4ODdkLTRjYjEtNDE2Yy1iYjNkLTU3NjcyNWQyNDU0NQBGAAAAAAAJ5WnadDt8Qbug1K0a4qTGBwBigci24c56QqJtpwbjRK+aAAAAAAENAABigci24c56QqJtpwbjRK+aAAA21xzlAAA=\" ChangeKey=\"CQAAABYAAABigci24c56QqJtpwbjRK+aAAA21ufB\"/></m:UploadItemsResponseMessage></m:ResponseMessages></m:UploadItemsResponse></s:Body></s:Envelope>";
            
            string responseCode = GetFirstResponseCode(xmlStr);
            Assert.AreEqual(responseCode, "NoError");
            if(responseCode != "NoError")
            {
                string messageText = GetFirstMessageText(xmlStr);
            }

        }

        [TestMethod]
        public void ParseErrorResponseInfo()
        {
            string xmlStr = "<?xml version=\"1.0\" encoding=\"utf-8\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Header><h:ServerVersionInfo MajorVersion=\"15\" MinorVersion=\"0\" MajorBuildNumber=\"913\" MinorBuildNumber=\"19\" Version=\"V2_10\" xmlns:h=\"http://schemas.microsoft.com/exchange/services/2006/types\" xmlns=\"http://schemas.microsoft.com/exchange/services/2006/types\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"/></s:Header><s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><m:UploadItemsResponse xmlns:m=\"http://schemas.microsoft.com/exchange/services/2006/messages\" xmlns:t=\"http://schemas.microsoft.com/exchange/services/2006/types\"><m:ResponseMessages><m:UploadItemsResponseMessage ResponseClass=\"Error\"><m:MessageText>Data is corrupt.</m:MessageText><m:ResponseCode>ErrorCorruptData</m:ResponseCode><m:DescriptiveLinkKey>0</m:DescriptiveLinkKey></m:UploadItemsResponseMessage></m:ResponseMessages></m:UploadItemsResponse></s:Body></s:Envelope>";
            string responseCode = GetFirstResponseCode(xmlStr);
            Assert.AreNotEqual(responseCode, "NoError");
            if(responseCode != "NoError")
            {
                string messageText = GetFirstMessageText(xmlStr);
                Debug.WriteLine(messageText);
            }
        }

        private string GetFirstResponseCode(string xmlStr)
        {
            Regex regex = new Regex(@"(?<=\<m:ResponseCode\>)\w+(?=\</m:ResponseCode\>)");
            return GetFirstMatchTextWithRegex(xmlStr, regex);
        }

        private string GetFirstMessageText(string xmlString)
        {
            Regex messageRegex = new Regex(@"(?<=\<m:MessageText\>).+(?=\</m:MessageText\>)");
            return GetFirstMatchTextWithRegex(xmlString, messageRegex);
        }

        private string GetFirstMatchTextWithRegex(string xmlStr, Regex regex)
        {
            var matchs = regex.Matches(xmlStr);
            if(matchs.Count == 0)
                return string.Empty;
            string resultInfo = string.Empty;
            foreach(Match match in matchs)
            {
                resultInfo = match.Value;
                return resultInfo;
            }
            return string.Empty;
        }
    }
}
