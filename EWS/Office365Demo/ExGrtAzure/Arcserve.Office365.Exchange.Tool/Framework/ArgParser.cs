using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Tool.Framework
{
    public class ArgParser
    {
        public ArgParser(string[] args)
        {
            ArgInfos = new List<ArgInfo>(args.Length);
            foreach (var arg in args)
            {
                var a = arg.Trim();
                var firstIndex = a.IndexOf(":");
                var array = a.Split(split, StringSplitOptions.RemoveEmptyEntries);
                if (array.Length < 2)
                {
                    throw new ArgumentException(string.Format("argument {0} is not right, the format must be: -key:value .", arg));
                }
                else if (array.Length == 2)
                    ArgInfos.Add(new ArgInfo(array));
                else
                {
                    var key = array[0];
                    array = new string[2];
                    array[0] = key;
                    array[1] = a.Substring(firstIndex + 1);
                    ArgInfos.Add(new ArgInfo(array));
                }
            }
        }

        private static char[] split = ":".ToCharArray();
        public List<ArgInfo> ArgInfos { get; private set; }

        public static CommandArgs Parser(string[] args)
        {
            var result = (new ArgParser(args)).ArgInfos;
            CommandArgs dic = new CommandArgs(result.Count);
            foreach (var item in result)
            {
                dic.Add(item.Arg, item);
            }
            return dic;
        }
    }

    public class ArgInfo
    {
        public string Arg;
        public string Value;
        public ArgInfo()
        {

        }

        public ArgInfo(string[] array)
        {
            if (array.Length != 2)
            {
                throw new ArgumentException(string.Format("argument is not right, the format must be: -key:value ."));
            }

            Arg = array[0].Replace("-", string.Empty);
            Value = array[1].Trim('\"');
        }
    }
}
