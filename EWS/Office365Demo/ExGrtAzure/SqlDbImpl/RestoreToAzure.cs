using DataProtectInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EwsDataInterface;
using System.IO.Compression;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using EwsFrame;
using SqlDbImpl.Storage;
using System.Net.Mail;
using EwsFrame.Util;
using SqlDbImpl.Model;
using System.Web.Script.Serialization;
using DataProtectInterface.Util;
using FastTransferUtil.CompoundFile;

namespace SqlDbImpl
{
    public class RestoreToAzure : IRestoreDestinationEx
    {
        private SaveToBlob _instance = new SaveToBlob();
        private string _notifyMailAddress;

        private static char[] _invalidFolderChars;
        public static char[] InvalidFolderChars
        {
            get
            {
                if (_invalidFolderChars == null)
                {
                    HashSet<char> charHash = new HashSet<char>(Path.GetInvalidPathChars());
                    if (!charHash.Contains('\\'))
                        charHash.Add('\\');
                    _invalidFolderChars = charHash.ToArray();
                }
                return _invalidFolderChars;
            }
        }

        private static char[] _invalidFileChars;
        public static char[] InvalidFileChars
        {
            get
            {
                if (_invalidFileChars == null)
                {
                    HashSet<char> charHash = new HashSet<char>(Path.GetInvalidFileNameChars());
                    if (!charHash.Contains('\\'))
                        charHash.Add('\\');
                    _invalidFileChars = charHash.ToArray();
                }
                return _invalidFileChars;
            }
        }

        public RestoreToAzure()
        {

        }



        public ExportType ExportType
        {
            get; set;
        }

        public void DealFolder(string displayName, Stack<IItemBase> dealItemStack)
        {

        }

        private string GetPath(string displayName, Stack<IItemBase> dealItemStack)
        {
            StringBuilder sb = new StringBuilder(255);
            var length = dealItemStack.Count - 1;
            var index = -1;

            foreach (var item in dealItemStack)
            {
                if (++index == 0)
                    continue;
                //if (index != length)
                //{
                sb.Insert(0, "\\");

                sb.Insert(0, string.Join("_", item.DisplayName.Split(InvalidFolderChars)));
                //}
            }
            sb.Append(string.Join("_", displayName.Split(InvalidFileChars)));

            var dealItem = dealItemStack.Pop();
            dealItemStack.Push(dealItem);

            sb.Append(ItemClassUtil.GetItemSuffix(dealItem, ExportType));
            return sb.ToString();
        }

        public void DealItem(string id, string displayName, byte[] itemData, Stack<IItemBase> dealItemStack)
        {
            if(ExportType == ExportType.Msg)
            {
                itemData = CompoundFileUtil.Instance.ConvertBinToMsg(itemData);
            }

            string path = GetPath(displayName, dealItemStack);
            _instance.WriteItem(path, itemData);
        }

        public void DealMailbox(string displayName, Stack<IItemBase> dealItemStack)
        {

        }

        public void DealOrganization(string organization, Stack<IItemBase> dealItemStack)
        {

        }

        public void SetOtherInformation(params object[] args)
        {
            if (args.Length >= 1)
            {
                _notifyMailAddress = args[0] as string;
            }
            if (args.Length >= 2)
                _instance.ContainerName = args[1] as string;
            if (string.IsNullOrEmpty(_instance.ContainerName))
            {
                _instance.ContainerName = Guid.NewGuid().ToString();
            }
        }

        public void Dispose()
        {
            _instance.Dispose();
        }

        public void RestoreComplete(bool success, IRestoreServiceEx restoreService, Exception ex)
        {
            _instance.Dispose();

            if (success)
            {
                List<string> urls = ((QueryCatalogDataAccess)(restoreService.ServiceContext.DataAccessObj)).BlobDataAccessObj.GetBlobShareUris(_instance.ContainerName, _instance.BlobNames);
                string subject = string.Format("Restore {0} Finished", restoreService.RestoreJobName);
                SendMailHelper sendMailHelper = new SendMailHelper();
                sendMailHelper.AddDownloadUrls(urls);
                string body = sendMailHelper.GetHtmlBody();
                var client = Config.MailConfigInstance.Client();
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();

                string[] addresses = _notifyMailAddress.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                foreach (var address in addresses)
                    msg.To.Add(address);

                msg.From = new MailAddress(Config.MailConfigInstance.Sender);
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = true;

                try
                {
                    client.Send(msg);
                }
                catch (Exception e)
                {

                }
            }
        }
    }

    class SaveToBlob : IDisposable
    {
        public string ContainerName;
        public string _blobName;
        private int _blobIndex;
        private MemoryStream _memoryStream;
        private ZipArchive _zipArchive;
        private CloudBlockBlob _blob;

        public List<string> BlobNames = new List<string>();

        private void GetZipArchive()
        {
            if (_zipArchive == null)
            {
                _memoryStream = new MemoryStream();
                _zipArchive = new ZipArchive(_memoryStream, ZipArchiveMode.Create, true);
            }

            if (_memoryStream.Length > Config.RestoreCfgInstance.ZipFileMaxSize)
            {
                WriteToBlob();

                _memoryStream = new MemoryStream();
                _zipArchive = new ZipArchive(_memoryStream, ZipArchiveMode.Create, true);
            }
        }

        private void WriteToBlob()
        {
            if (_memoryStream == null || _memoryStream.Length <= 0)
                return;

            NewBlob();
            try
            {
                _zipArchive.Dispose();
                _zipArchive = null;
                _memoryStream.Seek(0, SeekOrigin.Begin);
                _blob.UploadFromStream(_memoryStream);

                //using (StreamWriter writer = new StreamWriter(@"c:\23.zip"))
                //{
                //    _memoryStream.Seek(0, SeekOrigin.Begin);
                //    _memoryStream.CopyTo(writer.BaseStream);
                //}
            }
            finally
            {
                _memoryStream.Close();
                _memoryStream.Dispose();
                _memoryStream = null;
            }
        }

        private void NewBlob()
        {
            _blobIndex++;
            _blobName = string.Format("{0}.zip", _blobIndex);
            BlobNames.Add(_blobName);
            _blob = ((QueryCatalogDataAccess)(ServiceContext.ContextInstance.DataAccessObj)).BlobDataAccessObj.GetBlockBlobObj(ContainerName, _blobName);
        }

        public void Dispose()
        {
            WriteToBlob();

            if (_zipArchive != null)
            {
                _zipArchive.Dispose();
                _zipArchive = null;
            }

            if (_memoryStream != null)
            {
                _memoryStream.Close();
                _memoryStream.Dispose();
                _memoryStream = null;
            }
        }

        internal void WriteItem(string path, byte[] itemData)
        {
            GetZipArchive();
            var entry = _zipArchive.CreateEntry(path);
            using (var stream = entry.Open())
            {
                stream.Write(itemData, 0, itemData.Length);
            }
        }
    }
}
