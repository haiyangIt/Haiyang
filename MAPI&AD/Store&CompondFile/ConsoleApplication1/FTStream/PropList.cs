using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public class PropList : ILog
    {
        private PropList()
        {
            PropValues = new List<IPropValue>();
        }

        public List<IPropValue> PropValues;

        public static void AddProperty(byte[] buffer, ref int pos, ref PropList PropertyList)
        {
            IPropValue propValue = StreamUtil.ParsePropValue(buffer, ref pos);
            PropertyList.PropValues.Add(propValue);
        }

        public static PropList CreatePropertyList()
        {
            return new PropList();
        }


        public void LogInfo(StringBuilder logBuilder)
        {
            logBuilder.Append(FTStreamParseContext.Instance.GetIndent()).Append("PropList:").AppendLine();
            FTStreamParseContext.Instance.IncrementIndent();

            foreach(IPropValue value in PropValues)
            {
                value.LogInfo(logBuilder);
            }
            FTStreamParseContext.Instance.ResetIndent();
        }
    }
}
