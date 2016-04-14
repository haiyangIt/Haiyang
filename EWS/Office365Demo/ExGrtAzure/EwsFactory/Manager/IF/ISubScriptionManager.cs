using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Manager.IF
{
    public interface ISubScriptionManager : IManager
    {
        void AddCallback(string subscriptionFilter, Action<IProgressInfo> callback);
        void RemoveCallback(string subscriptionFilter);
    }
}
