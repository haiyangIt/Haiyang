using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public class FTStreamParseContext
    {
        public static FTStreamParseContext Instance = new FTStreamParseContext();

        private FTStreamParseContext()
        {

        }
        
        private readonly byte[] _buffer;
        private IParseBinary _parser;
        public FTStreamParseContext(byte[] buffer)
        {
            _buffer = buffer;
            _parser = new PropertyTypeReader(buffer);
        }

        #region Output Log
        private int _indentForLog = 0;
        private Dictionary<int, string> _indentString = new Dictionary<int, string>();
        public void IncrementIndent()
        {
            _indentForLog += 2;
        }

        public void ResetIndent()
        {
            _indentForLog -= 2;
            Debug.Write(_outputStreamBuilder.ToString());
            _outputStreamBuilder.Length = 0;
        }

        private StringBuilder _outputStreamBuilder;
        public void SetStringBuilder(StringBuilder sb)
        {
            _outputStreamBuilder = sb;
        }

        public string GetIndent()
        {
            string indentStr;
            if (!_indentString.TryGetValue(_indentForLog, out indentStr))
            {
                StringBuilder sb = new StringBuilder();
                for(int i = 0 ; i < _indentForLog; i++)
                {
                    sb.Append(" ");
                }
               
                indentStr = sb.ToString();
                _indentString.Add(_indentForLog,indentStr);
            }

            return indentStr;
        }
        #endregion
    }
}
