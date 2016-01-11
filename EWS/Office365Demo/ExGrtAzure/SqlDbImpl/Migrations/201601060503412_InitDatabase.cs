namespace SqlDbImpl.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class InitDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CatalogInformation",
                c => new
                    {
                        StartTime = c.DateTime(nullable: false),
                        CatalogJobName = c.String(nullable: false, maxLength: 255),
                        Organization = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.StartTime);
            
            CreateTable(
                "dbo.FolderInformation",
                c => new
                    {
                        FolderId = c.String(nullable: false, maxLength: 512,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "CaseSensitive",
                                    new AnnotationValues(oldValue: null, newValue: "FolderId, StartTime")
                                },
                            }),
                        StartTime = c.DateTime(nullable: false),
                        ChildItemCount = c.Int(nullable: false),
                        DisplayName = c.String(nullable: false, maxLength: 255),
                        FolderType = c.String(maxLength: 64),
                        MailboxAddress = c.String(maxLength: 255),
                        ParentFolderId = c.String(nullable: false, maxLength: 512),
                        ChildFolderCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.FolderId, t.StartTime });
            
            CreateTable(
                "dbo.ItemLocation",
                c => new
                    {
                        ItemId = c.String(nullable: false, maxLength: 512,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "CaseSensitive",
                                    new AnnotationValues(oldValue: null, newValue: "ItemId")
                                },
                            }),
                        ParentFolderId = c.String(nullable: false, maxLength: 512,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "CaseSensitive",
                                    new AnnotationValues(oldValue: null, newValue: "")
                                },
                            }),
                        Location = c.String(nullable: false, maxLength: 256),
                        ActualSize = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ItemId);
            
            CreateTable(
                "dbo.ItemInformation",
                c => new
                    {
                        ItemId = c.String(nullable: false, maxLength: 512,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "CaseSensitive",
                                    new AnnotationValues(oldValue: null, newValue: "ItemId, StartTime")
                                },
                            }),
                        StartTime = c.DateTime(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        DisplayName = c.String(),
                        ItemClass = c.String(nullable: false, maxLength: 64),
                        ParentFolderId = c.String(nullable: false, maxLength: 512,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "CaseSensitive",
                                    new AnnotationValues(oldValue: null, newValue: "")
                                },
                            }),
                        Size = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItemId, t.StartTime });
            
            CreateTable(
                "dbo.MailboxInformation",
                c => new
                    {
                        RootFolderId = c.String(nullable: false, maxLength: 512,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "CaseSensitive",
                                    new AnnotationValues(oldValue: null, newValue: "RootFolderId, StartTime")
                                },
                            }),
                        StartTime = c.DateTime(nullable: false),
                        ChildFolderCount = c.Int(nullable: false),
                        DisplayName = c.String(maxLength: 255),
                        MailAddress = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => new { t.RootFolderId, t.StartTime });
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MailboxInformation",
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "RootFolderId",
                        new Dictionary<string, object>
                        {
                            { "CaseSensitive", "RootFolderId, StartTime" },
                        }
                    },
                });
            DropTable("dbo.ItemInformation",
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "ItemId",
                        new Dictionary<string, object>
                        {
                            { "CaseSensitive", "ItemId, StartTime" },
                        }
                    },
                    {
                        "ParentFolderId",
                        new Dictionary<string, object>
                        {
                            { "CaseSensitive", "" },
                        }
                    },
                });
            DropTable("dbo.ItemLocation",
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "ItemId",
                        new Dictionary<string, object>
                        {
                            { "CaseSensitive", "ItemId" },
                        }
                    },
                    {
                        "ParentFolderId",
                        new Dictionary<string, object>
                        {
                            { "CaseSensitive", "" },
                        }
                    },
                });
            DropTable("dbo.FolderInformation",
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "FolderId",
                        new Dictionary<string, object>
                        {
                            { "CaseSensitive", "FolderId, StartTime" },
                        }
                    },
                });
            DropTable("dbo.CatalogInformation");
        }
    }
}
