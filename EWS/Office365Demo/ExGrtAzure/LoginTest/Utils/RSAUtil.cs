using OpenSSL.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LoginTest.Utils
{
    public class RSAUtil
    {
        private static string privateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIICXAIBAAKBgQCz+YkHpta6raSlMoqhYTKOgA/99unGkcgWxqYrcbR6LLqokSfe
woGaGl9SLAerZcgTOgCvQLtom7q2T2KH5WFn4ljyvCBIXrhnB4MIEaywg9FLTTAT
p1QPMlleKd0ZKjiJj4wMNbsqOeMETi5WwPLIRjY/HBUIrWnDuGbD3U3RJwIDAQAB
AoGBAJhoqhVzwQYHhJVtDp8JW7H3WxObpG9HByXahd/mg4qvFEcp3ZS0LOKekLha
lwgbwmodxXSkIdC9iElZQryIkInlJsaNPUKYGG4a2xBjdwEtSJkJg8tRE+edwqcp
KfVynfRe8su2w74SSjbuw7L8aDKWbXQO0mdXFlRoF+smf0UJAkEA9Kv4AlilxUji
c54XFNM+Sa5TrtEj2iiCyjLT77iPKiazIq8Prdqbt0ph2HnrwyH8xqb/9jY7SxIc
yLrROUHrDQJBALxOusQYPPiGttYZe8NH+P/f6mcaMJxD6gS30TG845N/sZDN+UMs
b7AeG0aOM/Fs0JQeP0om0vx+3x8bbSNkUAMCQGtnhkJ5aaO1//pi/pzcsm4jwYZv
Nn7Q48EhGNoVuXr2bBKgqJBX8509YMBnnPAW3mDR9HC/k727oLkZETlCT40CQBF7
6eFtMrQMpgtJAHHIS/lODBCYoOzRbXgUrSrGFAdM8uq0BTHUfWZH1VZ+u5nt9Yvb
Jxs2cZ6aFRNpU3/Wv4sCQCbIbsLtUCWbDbJjlgUyrdfFF2RTUC9lFpX6Pp6oTgxj
1Q00bofQPhdEmpm3wCYuCi2mUzHO0WmE/lvaK0ie5oE=
-----END RSA PRIVATE KEY-----";

        public static string AsymmetricDecrypt(string base64Str)
        {
            var b64 = Convert.FromBase64String(base64Str);

            var str = RSAUtil.AsymmetricDecrypt(b64);
            return Encoding.UTF8.GetString(str);
        }

        private static byte[] AsymmetricDecrypt(byte[] payload)
        {
            return AsymmetricDecrypt(privateKey, payload);
        }

        private static byte[] AsymmetricEncrypt(string publicKeyAsPem, byte[] payload)
        {
            CryptoKey d = CryptoKey.FromPublicKey(publicKeyAsPem, null);
            RSA rsa = d.GetRSA();
            byte[] result = rsa.PublicEncrypt(payload, RSA.Padding.PKCS1);
            rsa.Dispose();
            return result;
        }

        private static byte[] AsymmetricDecrypt(string privateKeyAsPem, byte[] payload)
        {
            CryptoKey d = CryptoKey.FromPrivateKey(privateKeyAsPem, null);
            RSA rsa = d.GetRSA();
            byte[] result = rsa.PrivateDecrypt(payload, RSA.Padding.PKCS1);
            rsa.Dispose();
            return result;
        }
    }
}