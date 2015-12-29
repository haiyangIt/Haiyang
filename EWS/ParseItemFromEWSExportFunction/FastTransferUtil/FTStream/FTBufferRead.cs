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
    //public delegate void DoWithNewParseFunc();
    //public class FTStreamParseContext : IDisposable
    //{
    //    private IFTStreamReader _parser;
    //    public IFTStreamReader Parser { get { return _parser; } }
       
    //    private FTStreamParseContext()
    //    {
    //    }

    //    public static void NewContext(FTBufferRead bufferReader)
    //    {
    //        if (Instance == null)
    //        {
    //            Instance = new FTStreamParseContext();
    //            Instance._bufferReader = bufferReader;
    //            Instance._parser = new FTStreamReaderForPage(bufferReader);
    //        }
    //    }

    //    public void SetNewParse(IFTStreamReader reader, DoWithNewParseFunc delegateFunc)
    //    {
    //        var oldParser = Parser;
    //        _parser = reader;
    //        delegateFunc();
    //        _parser = oldParser;
    //    }

    //    public FTBufferRead _bufferReader;

    //    public void Dispose()
    //    {
    //        if (_parser != null)
    //            _parser.Dispose();
    //        Instance = null;
    //    }
    //}

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
