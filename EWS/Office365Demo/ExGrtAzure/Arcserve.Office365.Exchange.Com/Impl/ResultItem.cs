using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Arcserve.Office365.Exchange.Com.Impl
{
    [Guid("CB5F28B2-E094-4BA4-8333-1567B3B90F50")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class ResultItem : IResult
    {
        public string DisplayName
        {
            get; set;
        }

        public int HId
        {
            get; set;
        }

        public int HParentId
        {
            get; set;
        }

        public int LId
        {
            get; set;
        }

        public int LParentId
        {
            get; set;
        }

        public int ObjType
        {
            get; set;
        }

        public IFolderResult GetFolderResult()
        {
            throw new NotImplementedException();
        }

        public IMailboxResult GetMailboxResult()
        {
            throw new NotImplementedException();
        }

        public IMailItemResult GetMailItemResult()
        {
            throw new NotImplementedException();
        }
    }
}
