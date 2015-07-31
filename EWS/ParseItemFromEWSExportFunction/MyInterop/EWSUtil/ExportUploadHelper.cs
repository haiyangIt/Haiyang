using System;
using System.Net;
using Microsoft.Exchange.WebServices.Data;
using System.Text;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using FTStreamUtil;
using MyInterop.EWSUtil.Resource;

namespace EWSUtil
{
    public class ExportUploadHelper
    {
        public static bool ExportItemPost(string ServerVersion, string sItemId, string sFile)
        {
            bool bSuccess = false;
            string sResponseText = string.Empty;
            System.Net.HttpWebRequest oHttpWebRequest = null;
            EwsProxyFactory.CreateHttpWebRequest(ref oHttpWebRequest);

            // Build request body...
            string EwsRequest = TemplateEwsRequests.ExportItems;
            EwsRequest = EwsRequest.Replace("##RequestServerVersion##", ServerVersion);
            EwsRequest = EwsRequest.Replace("##ItemId##", sItemId);
       
            try
            {
 
                // Use request to do POST to EWS so we get back the data for the item to export.
                byte[] bytes = Encoding.UTF8.GetBytes(EwsRequest);
                oHttpWebRequest.ContentLength = bytes.Length;
                using (Stream requestStream = oHttpWebRequest.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Flush();
                    requestStream.Close();
                }
 
                // Get response
                HttpWebResponse oHttpWebResponse = (HttpWebResponse)oHttpWebRequest.GetResponse();

                StreamReader oStreadReader = new StreamReader(oHttpWebResponse.GetResponseStream());
                sResponseText = oStreadReader.ReadToEnd();

   
                // OK?
                if (oHttpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    int BUFFER_SIZE = 1024;
                    int iReadBytes = 0;

                    XmlDocument oDoc = new XmlDocument();
                    XmlNamespaceManager namespaces = new XmlNamespaceManager(oDoc.NameTable);
                    namespaces.AddNamespace("m", "http://schemas.microsoft.com/exchange/services/2006/messages");
                    oDoc.LoadXml(sResponseText);
                    XmlNode oData = oDoc.SelectSingleNode("//m:Data", namespaces);
 
                     // Write base 64 encoded text Data XML string into a binary base 64 text/XML file
                    BinaryWriter oBinaryWriter = new BinaryWriter(File.Open(sFile, FileMode.Create));
                    StringReader oStringReader = new StringReader(oData.OuterXml);
                    XmlTextReader oXmlTextReader = new XmlTextReader(oStringReader);
                    oXmlTextReader.MoveToContent();
                    byte[] buffer = new byte[BUFFER_SIZE];
                    do
                    {
                        iReadBytes = oXmlTextReader.ReadBase64(buffer, 0, BUFFER_SIZE);
                        oBinaryWriter.Write(buffer, 0, iReadBytes);
                    }
                    while (iReadBytes >= BUFFER_SIZE);

                    oXmlTextReader.Close();

                    oBinaryWriter.Flush();
                    oBinaryWriter.Close();

                    bSuccess = true;
                }
                
 
            }
            finally 
            {
 

            }

            return bSuccess;
 
        }


        public static string UploadItemPost(string ServerVersion, FolderId ParentFolderId, CreateActionType oCreateActionType, string sItemId, string sFile)
        {
            try
            {
                string sResponseText = string.Empty;
                System.Net.HttpWebRequest oHttpWebRequest = null;
                EwsProxyFactory.CreateHttpWebRequest(ref oHttpWebRequest);

                string EwsRequest = string.Empty;

                if (oCreateActionType != CreateActionType.CreateNew)
                {
                    EwsRequest = TemplateEwsRequests.UploadItems_Update;

                    if (oCreateActionType == CreateActionType.Update)
                        EwsRequest = EwsRequest.Replace("##CreateAction##", "Update");
                    else
                        EwsRequest = EwsRequest.Replace("##CreateAction##", "UpdateOrCreate");
                    EwsRequest = EwsRequest.Replace("##ItemId##", sItemId);
                }
                else
                {
                    EwsRequest = TemplateEwsRequests.UploadItems_CreateNew;
                    EwsRequest = EwsRequest.Replace("##CreateAction##", "CreateNew");
                }
                EwsRequest = EwsRequest.Replace("##RequestServerVersion##", ServerVersion);
                EwsRequest = EwsRequest.Replace("##ParentFolderId_Id##", ParentFolderId.UniqueId);

                string sBase64Data = string.Empty;
                sBase64Data = FileHelper.GetBinaryFileAsBase64(sFile);
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
                HttpWebResponse oHttpWebResponse = (HttpWebResponse)oHttpWebRequest.GetResponse();

                StreamReader oStreadReader = new StreamReader(oHttpWebResponse.GetResponseStream());
                sResponseText = oStreadReader.ReadToEnd();

                if (oHttpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    string responseCode = GetFirstResponseCode(sResponseText);
                    
                    if (responseCode != "NoError")
                    {
                        string messageText = GetFirstMessageText(sResponseText);
                        LogWriter.Instance.WriteLine("Import ftstream failed with error stream, the detail of response is:");
                        LogWriter.Instance.WriteLine(sResponseText);
                        return messageText;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    LogWriter.Instance.WriteLine("Import ftstream failed with error response status code, the detail of response is:");
                    LogWriter.Instance.WriteLine(sResponseText);
                    
                    return oHttpWebResponse.StatusCode.ToString();
                }
            }
            catch(Exception e){
                LogWriter.Instance.WriteException(typeof(ExportUploadHelper), e);
                return e.Message;
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
