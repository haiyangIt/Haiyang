using EwsFrame.Manager.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.Manager.IF
{
    public interface IProgressManager : IManager
    {
        /// <summary>
        /// Add a progress.
        /// </summary>
        /// <param name="progressInfo"></param>
        void AddProgress(IProgressInfo progressInfo);
        string GetLatestProgress();

        /// <summary>
        /// The event is triggered in a internal thread in manager.
        /// Don't trigger it in the AddProgress method.
        /// </summary>
        event EventHandler<ProgressArgs> NewProgressEvent;
    }
}
