namespace SqlDbImpl.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class IgnoreMigration : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.PlanAzureSchedulerInfo",
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "Name",
                        new Dictionary<string, object>
                        {
                            { "CaseSensitive", "Name" },
                        }
                    },
                });
            DropTable("dbo.PlanMailInfo",
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "Name",
                        new Dictionary<string, object>
                        {
                            { "CaseSensitive", "Name" },
                        }
                    },
                });
            DropTable("dbo.PlanBaseInfo",
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "Name",
                        new Dictionary<string, object>
                        {
                            { "CaseSensitive", "Name" },
                        }
                    },
                });
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PlanBaseInfo",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 256,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "CaseSensitive",
                                    new AnnotationValues(oldValue: null, newValue: "Name")
                                },
                            }),
                        Organization = c.String(maxLength: 128),
                        CredentialInfo = c.String(maxLength: 512),
                        PlanMailInfos = c.String(),
                        FirstStartTime = c.DateTime(nullable: false),
                        NextFullBackupTime = c.DateTime(nullable: false),
                        LastSyncStaus = c.String(),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.PlanMailInfo",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 256,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "CaseSensitive",
                                    new AnnotationValues(oldValue: null, newValue: "Name")
                                },
                            }),
                        Mailbox = c.String(maxLength: 256),
                        FolderInfos = c.String(),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.PlanAzureSchedulerInfo",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 256,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "CaseSensitive",
                                    new AnnotationValues(oldValue: null, newValue: "Name")
                                },
                            }),
                        CloudService = c.String(maxLength: 256),
                        JobCollectionName = c.String(maxLength: 256),
                        JobInfo = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.Name);
            
        }
    }
}
