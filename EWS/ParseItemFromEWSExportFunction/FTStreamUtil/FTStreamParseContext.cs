using FTStreamUtil.FTStream;
using FTStreamUtil.Item;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace FTStreamUtil
{
    public delegate void DoWithNewParseFunc();
    public class FTStreamParseContext : IDisposable
    {
        [ThreadStatic]
        public static FTStreamParseContext Instance;
        
        private IFTStreamReader _parser;
        public IFTStreamReader Parser { get { return _parser; } }
       
        private FTStreamParseContext()
        {
            var binFolder = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo directoryInfo = new DirectoryInfo(binFolder);
            var parentFolder = directoryInfo.Parent.FullName;

            var logFolder = Path.Combine(parentFolder, "Logs");
            var logFileName = string.Format("{0}.txt", Guid.NewGuid());
            logFileName = Path.Combine(logFolder, logFileName);
            _logwriter = new StreamWriter(logFileName);
        }

        public static void NewContext(FTBufferRead bufferReader)
        {
            if (Instance == null)
            {
                Instance = new FTStreamParseContext();
                Instance._bufferReader = bufferReader;
                Instance._parser = new FTStreamReaderForPage(bufferReader);
            }
        }

        public void SetNewParse(IFTStreamReader reader, DoWithNewParseFunc delegateFunc)
        {
            var oldParser = Parser;
            _parser = reader;
            delegateFunc();
            _parser = oldParser;
        }

        public FTBufferRead _bufferReader;

        #region readSpecificType

        private bool JudgeIsOutofMemory()
        {
            if (_parser.IsEnd)
                return false;
            return true;
        }

        public PropertyTag ReadPropertyTag()
        {
            if (JudgeIsOutofMemory())
            {
                var result = FTFactory.Instance.CreatePropertyTag(_parser.ReadUInt32(false));
                return result;
            }
            return PropertyTag.Empty;
        }
        #endregion

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
        }

        public StringBuilder OutputStringBuilder;

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
        private StreamWriter _logwriter;
        public void Write(string msg)
        {
            _logwriter.Write(msg);
            //Debug.Write(msg);
            //Console.Write(msg);
        }

        public void WriteLine(string msg)
        {
            _logwriter.WriteLine(msg);
            //Debug.WriteLine(msg);
            //Console.WriteLine(msg);
        }

        public void WriteException(object obj, Exception ex)
        {
            _logwriter.WriteLine("-----------------Exception:-----------------------");
            _logwriter.Write("type:");
            _logwriter.WriteLine(obj.GetType().FullName);
            _logwriter.WriteLine(ex.Message);
            _logwriter.WriteLine(ex.StackTrace);
        }
        #endregion

        public void Dispose()
        {
            if (_logwriter != null)
                _logwriter.Close();
            if (_parser != null)
                _parser.Dispose();
        }
    }

    public class FTBufferRead : IFTPage
    {
        public FTBufferRead(IList<byte[]> buffers)
        {
            _allBufferes = buffers;
            _readed = 0;
        }
        private int _readed;
        private IList<Byte[]> _allBufferes;
        public byte[] GetNextBuffers()
        {
            if (_readed < _allBufferes.Count)
                return _allBufferes[_readed++];
            return null;
        }

        public int CurrentPageIndex
        {
            get { return _readed; }
        }

        public byte[] GetNextPageBuffer()
        {
            return GetNextBuffers();
        }

        public bool IsLastPage
        {
            get { return _readed >= _allBufferes.Count; }
        }
    }
}
