using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Arcserve.Office365.Exchange.DataProtect.Tool.Result
{
    public abstract class ResultBase
    {
        public ResultStatus Status { get; set; }
        public string Message { get; set; }
        protected ResultBase() { }

        protected ResultBase(string errorMsg)
        {
            Status = ResultStatus.Failure;
            Message = errorMsg;
        }

        public static string Serialize(ResultBase obj)
        {
            XmlSerializer s = new XmlSerializer(obj.GetType());
            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb))
            {
                s.Serialize(writer, obj);
            }
            return sb.ToString();
        }
    }
}
