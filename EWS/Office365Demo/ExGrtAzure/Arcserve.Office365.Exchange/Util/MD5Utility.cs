using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Arcserve.Office365.Exchange.Util
{
    public class MD5Utility
    {
        private static object _lock = new object();
        private static int _count = 0;
        private static MD5 _md5;
        private static MD5 Md5
        {
            get
            {
                using (_lock.LockWhile(() =>
                {
                    System.Threading.Interlocked.Increment(ref _count);
                    if (_count > 1000)
                    {
                        Dispose();
                    }

                    if (_md5 == null)
                    {
                        _md5 = MD5.Create();
                    }
                }))
                { };

                return _md5;
            }
        }

        private static void Dispose()
        {
            _count = 0;
            if (_md5 != null)
            {
                _md5.Dispose();
                _md5 = null;
            }
        }

        public static string ConvertToMd5(string data)
        {

            try
            {
                return ConvertToMd5(Md5, data);
            }
            catch (Exception e)
            {
                using (_lock.LockWhile(() =>
                {
                    Dispose();
                }))
                { }
                return ConvertToMd5(Md5, data);
            }
        }

        private static string ConvertToMd5(MD5 md5, string data)
        {
            byte[] md5Hash = md5.ComputeHash(Encoding.UTF8.GetBytes(data));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder(32);

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < md5Hash.Length; i++)
            {
                sBuilder.Append(md5Hash[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}
