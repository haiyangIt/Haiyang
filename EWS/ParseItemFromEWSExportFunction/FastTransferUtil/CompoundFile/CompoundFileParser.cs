using FastTransferUtil.CompoundFile.MsgStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastTransferUtil.CompoundFile
{
    public class CompoundFileParser
    {
        private IStorage _rootStorage;
        public void SetRootStorage(IStorage rootStorage)
        {
            _rootStorage = rootStorage;
        }

        private TopLevelStruct _topStruct;

        public void Parser()
        {
            _topStruct = new TopLevelStruct(_rootStorage);
            _topStruct.Parser();
        }
    }
}
