using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class UtilTest
    {
        [TestMethod]
        public void TestRegex()
        {
            Regex r = new Regex(@".*[(An existing connection was forcibly closed by the remote host) | (The underlying connection was closed) | (The mailbox database is temporarily unavailable) ].*");
            var str = "throw exception in timeout operator	The mailbox database temporarily unavailable., Cannot open mailbox /o=ExchangeLabs/ou=Exchange Administrative Group (FYDIBOHF23SPDLT)/cn=";
            bool result = r.IsMatch(str);
        }
    }
}
