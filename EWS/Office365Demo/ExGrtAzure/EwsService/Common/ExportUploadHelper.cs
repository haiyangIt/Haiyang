using System;
using System.Net;
using Microsoft.Exchange.WebServices.Data;
using System.Text;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using EwsServiceInterface;
using EwsFrame;
using LogInterface;
using EwsService.Resource;
using System.Collections.Generic;
using System.Threading;
using System.Configuration;
using EwsFrame.Util;
using System.Xml.Linq;
using System.Linq;

namespace EwsService.Common
{
    public class ExportUploadHelper
    {
        //public static bool ExportItemPost(string ServerVersion, string sItemId, byte[] buffer)
        //{
        //    bool bSuccess = false;
        //    string sResponseText = string.Empty;
        //    System.Net.HttpWebRequest oHttpWebRequest = null;
        //    EwsProxyFactory.CreateHttpWebRequest(ref oHttpWebRequest, argument);

        //    // Build request body...
        //    string EwsRequest = TemplateEwsRequests.ExportItems;
        //    EwsRequest = EwsRequest.Replace("##RequestServerVersion##", ServerVersion);
        //    EwsRequest = EwsRequest.Replace("##ItemId##", sItemId);

        //    try
        //    {

        //        // Use request to do POST to EWS so we get back the data for the item to export.
        //        byte[] bytes = Encoding.UTF8.GetBytes(EwsRequest);
        //        oHttpWebRequest.ContentLength = bytes.Length;
        //        using (Stream requestStream = oHttpWebRequest.GetRequestStream())
        //        {
        //            requestStream.Write(bytes, 0, bytes.Length);
        //            requestStream.Flush();
        //            requestStream.Close();
        //        }

        //        // Get response
        //        HttpWebResponse oHttpWebResponse = (HttpWebResponse)oHttpWebRequest.GetResponse();

        //        StreamReader oStreadReader = new StreamReader(oHttpWebResponse.GetResponseStream());
        //        sResponseText = oStreadReader.ReadToEnd();



        //        // OK?
        //        if (oHttpWebResponse.StatusCode == HttpStatusCode.OK)
        //        {
        //            int BUFFER_SIZE = 1024;
        //            int iReadBytes = 0;

        //            XmlDocument oDoc = new XmlDocument();
        //            XmlNamespaceManager namespaces = new XmlNamespaceManager(oDoc.NameTable);
        //            namespaces.AddNamespace("m", "http://schemas.microsoft.com/exchange/services/2006/messages");
        //            oDoc.LoadXml(sResponseText);
        //            XmlNode oData = oDoc.SelectSingleNode("//m:Data", namespaces);

        //            // Write base 64 encoded text Data XML string into a binary base 64 text/XML file
        //            //BinaryWriter oBinaryWriter = new BinaryWriter(File.Open(sFile, FileMode.Create));
        //            StringReader oStringReader = new StringReader(oData.OuterXml);
        //            XmlTextReader oXmlTextReader = new XmlTextReader(oStringReader);
        //            oXmlTextReader.MoveToContent();
        //            byte[] buffer = new byte[BUFFER_SIZE];
        //            do
        //            {
        //                iReadBytes = oXmlTextReader.ReadBase64(buffer, 0, BUFFER_SIZE);
        //                //oBinaryWriter.Write(buffer, 0, iReadBytes);
        //                writer.Write(buffer, 0, iReadBytes);
        //            }
        //            while (iReadBytes >= BUFFER_SIZE);

        //            oXmlTextReader.Close();

        //            // oBinaryWriter.Flush();
        //            //oBinaryWriter.Close();

        //            bSuccess = true;
        //        }


        //    }
        //    finally
        //    {


        //    }

        //    return bSuccess;
        //}

        public static bool ExportItemPost(string ServerVersion, string sItemId, Stream writer, EwsServiceArgument argument)
        {
            int retryCount = 0;
            Exception lastException = null;
            while (retryCount < 3)
            {
                if (retryCount > 0)
                {
                    const int sleepCount = 10 * 1000;
                    LogFactory.LogInstance.WriteLog(LogLevel.WARN, "retry export", "after sleeping  {0} seconde ,will try the [{1}]th export.", sleepCount, retryCount);
                    Thread.Sleep(sleepCount);
                }
                try
                {
                    var result = DoExportItemPost(ServerVersion, sItemId, writer, argument);
                    return result;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                    LogFactory.LogInstance.WriteException(LogLevel.ERR, "Export error", e, e.Message);
                    lastException = e;
                    retryCount++;
                }
            }
            if (lastException != null)
                throw new ApplicationException("Export failure", lastException);
            return false;
        }

        private static object _lockObj = new object();
        private static int _MaxSupportItemSize = 0;
        public static int MaxSupportItemSize
        {
            get
            {
                if (_MaxSupportItemSize == 0)
                {
                    using (_lockObj.LockWhile(() =>
                    {
                        if (_MaxSupportItemSize == 0)
                        {
                            var result = 0;
                            if (!int.TryParse(ConfigurationManager.AppSettings["SupportMaxSizeItem"], out result))
                            {
                                result = 15;
                            }
                            LogFactory.LogInstance.WriteLog(LogInterface.LogLevel.INFO, string.Format("max item size is {0}M", result));
                            _MaxSupportItemSize = result * 1024 * 1024;
                        }
                    }))
                    { };
                }
                return _MaxSupportItemSize;
            }
        }

        private static int _timeOut = 0;
        public static int TimeOut
        {
            get
            {
                if (_timeOut == 0)
                {
                    using (_lockObj.LockWhile(() =>
                    {
                        if (_timeOut == 0)
                        {
                            int result = 0;
                            if (!int.TryParse(ConfigurationManager.AppSettings["ExportItemTimeOut"], out result))
                            {
                                result = 120;
                            }
                            _timeOut = result * 1000;
                        }
                    }))
                    { }
                }
                return _timeOut;
            }
        }

        public static bool DoExportItemPost(string ServerVersion, string sItemId, Stream writer, EwsServiceArgument argument)
        {
            bool bSuccess = false;
            string sResponseText = string.Empty;
            System.Net.HttpWebRequest oHttpWebRequest = null;
            StreamReader oStreadReader = null;
            HttpWebResponse oHttpWebResponse = null;

            // Build request body...
            try
            {
                EwsProxyFactory.CreateHttpWebRequest(ref oHttpWebRequest, argument);
                string EwsRequest = GetExportSOAPXml(argument, ref oHttpWebRequest);
                EwsRequest = EwsRequest.Replace("##RequestServerVersion##", ServerVersion);
                EwsRequest = EwsRequest.Replace("##ItemId##", sItemId);
                // Use request to do POST to EWS so we get back the data for the item to export.
                byte[] bytes = Encoding.UTF8.GetBytes(EwsRequest);
                oHttpWebRequest.ContentLength = bytes.Length;
                oHttpWebRequest.Timeout = TimeOut;
                using (Stream requestStream = oHttpWebRequest.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Flush();
                    requestStream.Close();
                }

                // Get response
                oHttpWebResponse = (HttpWebResponse)oHttpWebRequest.GetResponse();

                //oStreadReader = new StreamReader(oHttpWebResponse.GetResponseStream());
                //sResponseText = oStreadReader.ReadToEnd();
                //if (sResponseText.Length > MaxSupportItemSize)
                //{
                //    LogFactory.LogInstance.WriteLog(LogLevel.WARN, string.Format("{0} is too much. not support now.", sItemId));
                //    return false;
                //}

                // OK?
                if (oHttpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    int BUFFER_SIZE = 1024;
                    int iReadBytes = 0;
                    //XmlDocument oDoc = new XmlDocument();
                    //XmlNamespaceManager namespaces = new XmlNamespaceManager(oDoc.NameTable);
                    //namespaces.AddNamespace("m", "http://schemas.microsoft.com/exchange/services/2006/messages");
                    //oDoc.LoadXml(sResponseText);
                    //XmlNode oData = oDoc.SelectSingleNode("//m:Data", namespaces);

                    XDocument doc = XDocument.Load(oHttpWebResponse.GetResponseStream());
                    XNamespace mnameSpace = "http://schemas.microsoft.com/exchange/services/2006/messages";

                    //var sb = new StringBuilder(102400);
                    //sb.Append("<Data>");
                    //sb.Append(doc.Descendants(mnameSpace + "Data").FirstOrDefault().Value);
                    //sb.Append("</Data>");
                    var val = doc.Descendants(mnameSpace + "Data").FirstOrDefault().Value;
                    byte[] buffer = Convert.FromBase64String(val);
                    writer.Write(buffer, 0, buffer.Length);
                    // Write base 64 encoded text Data XML string into a binary base 64 text/XML file
                    //BinaryWriter oBinaryWriter = new BinaryWriter(File.Open(sFile, FileMode.Create));
                    //using (StringReader oStringReader = new StringReader(sb.ToString()))
                    //{
                    //    using (XmlTextReader oXmlTextReader = new XmlTextReader(oStringReader))
                    //    {

                    //        oXmlTextReader.MoveToContent();
                    //        byte[] buffer = new byte[BUFFER_SIZE];
                    //        do
                    //        {

                    //            iReadBytes = oXmlTextReader.ReadBase64(buffer, 0, BUFFER_SIZE);
                    //            //oBinaryWriter.Write(buffer, 0, iReadBytes);
                    //            writer.Write(buffer, 0, iReadBytes);
                    //        }
                    //        while (iReadBytes >= BUFFER_SIZE);

                    //        oXmlTextReader.Close();
                    //    }
                    //}

                    bSuccess = true;
                }


            }
            finally
            {
                if (oStreadReader != null)
                {
                    oStreadReader.Dispose();
                    oStreadReader = null;
                }
                if (oHttpWebResponse != null)
                {
                    oHttpWebResponse.Dispose();
                    oHttpWebResponse = null;
                }
            }

            return bSuccess;
        }

        public static bool ExportItemPost(string ServerVersion, string sItemId, string sFile, EwsServiceArgument argument)
        {
            // Write base 64 encoded text Data XML string into a binary base 64 text/XML file
            using (FileStream stream = File.Open(sFile, FileMode.Create))
            {
                var result = ExportItemPost(ServerVersion, sItemId, stream, argument);
                stream.Flush();
                return result;
            }
        }



        private static string GetExportSOAPXml(EwsServiceArgument argument, ref HttpWebRequest webRequest)
        {
            string result;
            if (argument.UserToImpersonate == null)
            {
                result = TemplateEwsRequests.ExportItems;
            }
            else
            {
                switch (argument.UserToImpersonate.IdType)
                {
                    case ConnectingIdType.PrincipalName:
                        result = TemplateEwsRequests.ExportItemsWithImpersonatePrincipleName;
                        break;
                    case ConnectingIdType.SID:
                        result = TemplateEwsRequests.ExportItemsWithImpersonateId;
                        break;
                    case ConnectingIdType.SmtpAddress:
                        result = TemplateEwsRequests.ExportItemsWithImpersonateSMTP;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                result = result.Replace(TemplateEwsRequests.ImpersonateString, argument.UserToImpersonate.Id);
                if (argument.SetXAnchorMailbox)
                    webRequest.Headers.Add("X-AnchorMailbox", argument.XAnchorMailbox);
            }
            return result;
        }

        private static string GetImportSOAPXml(EwsServiceArgument argument, bool isCreateNew, ref HttpWebRequest webRequest)
        {
            string result;
            if (isCreateNew)
            {
                if (argument.UserToImpersonate == null)
                {
                    result = TemplateEwsRequests.UploadItems_CreateNew;
                }
                else
                {
                    switch (argument.UserToImpersonate.IdType)
                    {
                        case ConnectingIdType.PrincipalName:
                            result = TemplateEwsRequests.UploadItems_CreateNewWithImpersonatePrincipleName;
                            break;
                        case ConnectingIdType.SID:
                            result = TemplateEwsRequests.UploadItems_CreateNewWithImpersonateId;
                            break;
                        case ConnectingIdType.SmtpAddress:
                            result = TemplateEwsRequests.UploadItems_CreateNewWithImpersonateSMTPAddress;
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    result = result.Replace(TemplateEwsRequests.ImpersonateString, argument.UserToImpersonate.Id);
                    if (argument.SetXAnchorMailbox)
                        webRequest.Headers.Add("X-AnchorMailbox", argument.XAnchorMailbox);
                }
            }
            else
            {
                if (argument.UserToImpersonate == null)
                {
                    result = TemplateEwsRequests.UploadItems_Update;
                }
                else
                {
                    switch (argument.UserToImpersonate.IdType)
                    {
                        case ConnectingIdType.PrincipalName:
                            result = TemplateEwsRequests.UploadItems_UpdateWithImpersonatePrincipleName;
                            break;
                        case ConnectingIdType.SID:
                            result = TemplateEwsRequests.UploadItems_UpdateWithImpersonateId;
                            break;
                        case ConnectingIdType.SmtpAddress:
                            result = TemplateEwsRequests.UploadItems_UpdateWithImpersonateSMTPAddress;
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    result = result.Replace(TemplateEwsRequests.ImpersonateString, argument.UserToImpersonate.Id);
                    if (argument.SetXAnchorMailbox)
                        webRequest.Headers.Add("X-AnchorMailbox", argument.XAnchorMailbox);
                }
            }

            return result;
        }

        public static string UploadItemPost(string ServerVersion, string parentFolderId, CreateActionType oCreateActionType, string sItemId, Stream stream, EwsServiceArgument argument)
        {
            try
            {
                string sResponseText = string.Empty;
                System.Net.HttpWebRequest oHttpWebRequest = null;
                EwsProxyFactory.CreateHttpWebRequest(ref oHttpWebRequest, argument);

                string EwsRequest = string.Empty;

                if (oCreateActionType != CreateActionType.CreateNew)
                {
                    EwsRequest = GetImportSOAPXml(argument, false, ref oHttpWebRequest);

                    if (oCreateActionType == CreateActionType.Update)
                        EwsRequest = EwsRequest.Replace("##CreateAction##", "Update");
                    else
                        EwsRequest = EwsRequest.Replace("##CreateAction##", "UpdateOrCreate");
                    EwsRequest = EwsRequest.Replace("##ItemId##", sItemId);
                }
                else
                {
                    EwsRequest = GetImportSOAPXml(argument, true, ref oHttpWebRequest);
                    EwsRequest = EwsRequest.Replace("##CreateAction##", "CreateNew");
                }
                EwsRequest = EwsRequest.Replace("##RequestServerVersion##", ServerVersion);
                EwsRequest = EwsRequest.Replace("##ParentFolderId_Id##", parentFolderId);

                string sBase64Data = string.Empty;
                sBase64Data = FileHelper.GetBinaryFileAsBase64(stream);
                System.Diagnostics.Debug.WriteLine("sBase64: " + sBase64Data);

                // Convert byte array to base64
                EwsRequest = EwsRequest.Replace("##Data##", sBase64Data);



                //ShowTextDocument oForm = new ShowTextDocument();
                //oForm.txtEntry.WordWrap = false;
                //oForm.Text = "Info";
                //oForm.txtEntry.Text = EwsRequest;
                //oForm.ShowDialog();

                // Now inject the base64 body into the stream:

                byte[] bytes = Encoding.UTF8.GetBytes(EwsRequest);
                oHttpWebRequest.ContentLength = bytes.Length;

                using (Stream requestStream = oHttpWebRequest.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Flush();
                    requestStream.Close();
                }

                // Get response
                using (HttpWebResponse oHttpWebResponse = (HttpWebResponse)oHttpWebRequest.GetResponse())
                {

                    using (StreamReader oStreadReader = new StreamReader(oHttpWebResponse.GetResponseStream()))
                    {
                        sResponseText = oStreadReader.ReadToEnd();

                        if (oHttpWebResponse.StatusCode == HttpStatusCode.OK)
                        {
                            string responseCode = GetFirstResponseCode(sResponseText);

                            if (responseCode != "NoError")
                            {
                                string messageText = GetFirstMessageText(sResponseText);
                                LogFactory.LogInstance.WriteLog(LogLevel.ERR, "Import Failed", "Import ftstream failed with error stream, the detail of response is:{0}.", sResponseText);
                                return messageText;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        }
                        else
                        {
                            LogFactory.LogInstance.WriteLog(LogLevel.ERR, "Import Failed", "Import ftstream failed with error response status code, the detail of response is:{0}.", sResponseText);

                            return oHttpWebResponse.StatusCode.ToString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.GetExceptionDetail());
                LogFactory.LogInstance.WriteException(LogLevel.ERR, "Import Failed", e, string.Empty);
                return e.Message;
            }

        }

        public static string UploadItemPost(string ServerVersion, FolderId ParentFolderId, CreateActionType oCreateActionType, string sItemId, byte[] data, EwsServiceArgument argument)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                return UploadItemPost(ServerVersion, ParentFolderId.UniqueId, oCreateActionType, sItemId, memoryStream, argument);
            }
        }

        public static string UploadItemPost(string ServerVersion, FolderId ParentFolderId, CreateActionType oCreateActionType, string sItemId, string sFile, EwsServiceArgument argument)
        {
            using (FileStream oFileStream = new FileStream(sFile, FileMode.Open, FileAccess.Read))
            {
                return UploadItemPost(ServerVersion, ParentFolderId.UniqueId, oCreateActionType, sItemId, oFileStream, argument);
            }
        }

        private static string GetFirstResponseCode(string xmlStr)
        {
            Regex regex = new Regex(@"(?<=\<m:ResponseCode\>)\w+(?=\</m:ResponseCode\>)");
            return GetFirstMatchTextWithRegex(xmlStr, regex);
        }

        private static string GetFirstMessageText(string xmlString)
        {
            Regex messageRegex = new Regex(@"(?<=\<m:MessageText\>).+(?=\</m:MessageText\>)");
            return GetFirstMatchTextWithRegex(xmlString, messageRegex);
        }

        private static string GetFirstMatchTextWithRegex(string xmlStr, Regex regex)
        {
            var matchs = regex.Matches(xmlStr);
            if (matchs.Count == 0)
                return string.Empty;
            string resultInfo = string.Empty;
            foreach (Match match in matchs)
            {
                resultInfo = match.Value;
                return resultInfo;
            }
            return string.Empty;
        }
    }

    public enum CreateActionType
    {

        /// <remarks/>
        CreateNew,

        /// <remarks/>
        Update,

        /// <remarks/>
        UpdateOrCreate,
    }
}
