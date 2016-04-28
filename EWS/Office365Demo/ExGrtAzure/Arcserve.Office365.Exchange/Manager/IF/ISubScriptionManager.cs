using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Manager.IF
{
    public interface ISubScriptionManager : IManager
    {
        void AddListener(string subscriptionFilter, Action<IProgressInfo> callback);
        void RemoveListener(string subscriptionFilter);

        void DisposeSubScript(string subscriptionFilter);
    }
}
