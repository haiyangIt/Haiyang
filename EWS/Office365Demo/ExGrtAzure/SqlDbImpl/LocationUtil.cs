using EwsDataInterface;
using EwsFrame;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SqlDbImpl
{
    public class LocationUtil
    {
        public static string LocationsToString(MailLocation locations, int compressType = 0)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            string temp = js.Serialize(locations);
            switch (compressType)
            {
                case 1:
                    return Compress(temp);
                default:
                    return temp;
            }
        }

        public static MailLocation StringToLocations(string locationString, int compressType = 0)
        {

            string temp;
            switch (compressType)
            {
                case 1:
                    temp = Decompress(locationString);
                    break;
                default:
                    temp = locationString;
                    break;
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Deserialize<MailLocation>(temp);
        }

        public static string Compress(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zip.Write(buffer, 0, buffer.Length);
                }

                ms.Position = 0;

                byte[] compressed = new byte[ms.Length];
                ms.Read(compressed, 0, compressed.Length);

                byte[] gzBuffer = new byte[compressed.Length + 4];
                System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
                System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
                return Convert.ToBase64String(gzBuffer);
            }
        }

        public static string Decompress(string compressedText)
        {
            byte[] gzBuffer = Convert.FromBase64String(compressedText);
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

        public static void TestCompress()
        {
            


        }
    }

   
    public class ExportItemSizeInfo
    {
        public ExportType Type { get; set; }
        public int Size { get; set; }
        public ExportItemSizeInfo() { }
    }

    public class MailLocation
    {
        public string Path { get; set; }
        public List<ExportItemSizeInfo> SizeInfos { get; set; }

        public MailLocation() {
            
        }

        public void AddLocation(ExportItemSizeInfo location)
        {
            if (SizeInfos == null)
                SizeInfos = new List<ExportItemSizeInfo>();
            SizeInfos.Add(location);
        }

        public int GetLength()
        {
            int size = 0;
            if(SizeInfos.Count > 0)
            {
                foreach(var sizeInfo in SizeInfos)
                {
                    size += sizeInfo.Size;
                }
            }
            return size;
        }

        internal static string GetBlobNamePrefix(string itemId)
        {
            return MD5Utility.ConvertToMd5(itemId);
        }

        internal static string GetBlobName(ExportType type, string blobNamePrefix)
        {
            switch (type)
            {
                case ExportType.TransferBin:
                    return string.Format("{0}.bin", blobNamePrefix);
                case ExportType.Eml:
                    return string.Format("{0}.eml", blobNamePrefix);
                default:
                    throw new NotSupportedException();
            }
        }


    }

    


}
