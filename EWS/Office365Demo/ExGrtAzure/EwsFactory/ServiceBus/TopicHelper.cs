using EwsFrame.Manager.IF;
using EwsFrame.Manager.Impl;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsFrame.ServiceBus
{
    public class TopicHelper
    {
        public TopicHelper(BrokeredMessage message)
        {
            _message = message;
        }

        public TopicHelper(IProgressInfo progress)
        {
            _progressInfo = progress;
        }

        private BrokeredMessage _message;
        public BrokeredMessage Message
        {
            get
            {
                if(_message == null)
                {
                    if (_progressInfo == null)
                        throw new InvalidProgramException();
                    else
                    {
                        _message = new BrokeredMessage();
                        _message.Properties["JobId"] = _progressInfo.Job.JobId.ToString();
                        _message.Properties["ProgressInfo"] = JobFactoryServer.Convert(_progressInfo);
                    }
                }
                return _message;
            }
        }

        private IProgressInfo _progressInfo;
        public IProgressInfo ProgressInfo
        {
            get
            {
                if (_progressInfo == null)
                {
                    if (_message == null)
                        throw new InvalidProgramException();
                    else
                    {
                        _progressInfo = JobFactoryServer.Convert(_message.Properties["ProgressInfo"].ToString());
                    }
                }
                return _progressInfo;
            }
        }

        public static string GetFilterByJobId(string jobId)
        {
            return string.Format("JobId == {0}", jobId);
        }
    }
}
