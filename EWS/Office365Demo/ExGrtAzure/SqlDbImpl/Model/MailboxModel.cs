﻿using EwsDataInterface;
using EwsFrame.EF;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SqlDbImpl.Model
{
    [Table("MailboxInformation")]
    public class MailboxModel : IMailboxData, ICatalogInfo
    {
        public int ChildFolderCount
        {
            get; set;
        }

        [MaxLength(255)]
        public string DisplayName { get; set; }

        [NotMapped]
        public string Id
        {
            get
            {
                return RootFolderId;
            }
            set { }
        }

        [NotMapped]
        public ItemKind ItemKind
        {
            get
            {
                return ItemKind.Mailbox;
            }
            set { }
        }

        [NotMapped]
        public string Location { get; set; }
        [MaxLength(255)]
        [EmailAddress]
        public string MailAddress { get; set; }
        [Key]
        [Column(Order = 1)]
        [MaxLength(512)]
        [CaseSensitive("RootFolderId, StartTime")]
        public string RootFolderId { get; set; }
        [Key]
        [Column(Order = 2)]
        public DateTime StartTime { get; set; }

        public IMailboxData Clone()
        {
            return new MailboxModel()
            {
                DisplayName = DisplayName,
                Location = Location,
                MailAddress = MailAddress,
                RootFolderId = RootFolderId,
                StartTime = StartTime
            };
        }
    }
}