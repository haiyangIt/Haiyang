using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Tool.Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ArcserveCommandAttribute : Attribute
    {
        public ArcserveCommandAttribute(string commandName)
        {
            CommandName = commandName;
        }

        public string CommandName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CommandArgumentAttribute : Attribute
    {
        public CommandArgumentAttribute(string argumentName, string errorMsg = "", bool required = false)
        {
            ArgumentName = argumentName;
            ErrorMsg = errorMsg;
            Required = required;
        }

        public bool Required { get; set; }
        public string ArgumentName { get; set; }
        public string ErrorMsg { get; set; }
    }
}
