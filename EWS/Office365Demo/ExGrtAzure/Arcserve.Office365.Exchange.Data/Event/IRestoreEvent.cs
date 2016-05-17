﻿using System;

namespace Arcserve.Office365.Exchange.Data.Event
{
    public interface IRestoreEvent
    {
        event EventHandler<RestoreProgressArgs> ProgressChanged;
        event EventHandler<RestoreMailboxArgs> MailboxeProgressChanged;
        event EventHandler<RestoreFolderArgs> FolderProgressChanged;
        event EventHandler<RestoreItemArgs> ItemProgressChanged;
    }
}