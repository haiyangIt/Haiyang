using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using EwsFrame.Util;

namespace EwsFrame
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
                        if (_md5 != null)
                            _md5.Dispose();
                        _md5 = null;
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

        public static string ConvertToMd5(string data)
        {
            byte[] md5Hash = Md5.ComputeHash(Encoding.UTF8.GetBytes(data));

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
