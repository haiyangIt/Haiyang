using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public interface IPropInfo
    {
        void ParsePropInfo(byte[] buffer, ref int pos);

        UInt16 PropId { get; }
    }

    public class NamedPropId_NamedPropInfo : IPropInfo
    {
        public INamedPropId NamedPropId;
        public NamedPropInfo NamePropInfo;

        private NamedPropId_NamedPropInfo(INamedPropId namedPropId)
        {
            NamedPropId = namedPropId;
        }

        public static IPropInfo CreateNamedPropId_NamedPropInfo(INamedPropId propId, byte[] buffer, ref int pos)
        {
            NamedPropId_NamedPropInfo namedPropIdInfo = new NamedPropId_NamedPropInfo(propId);
            namedPropIdInfo.ParsePropInfo(buffer, ref pos);
            return namedPropIdInfo;
        }

        public void ParsePropInfo(byte[] buffer, ref int pos)
        {
            if (NamePropInfo == null)
            {
                NamePropInfo = new NamedPropInfo();
                NamePropInfo.ParseNamedPropInfo(buffer, ref pos);
            }
        }

        private string _toString;
        public override string ToString()
        {
            if (string.IsNullOrEmpty(_toString))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("NamePropId:[").Append(NamedPropId).Append("] NamePropInfo:[").Append(NamePropInfo.ToString()).Append("]");
                _toString = sb.ToString();
            }
            return _toString;
        }


        public UInt16 PropId
        {
            get { return ((PtypInteger16)NamedPropId).Value; }
        }
    }
}
