namespace LoginTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSettingModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SettingModels",
                c => new
                    {
                        UserMail = c.String(nullable: false, maxLength: 255),
                        AdminUserName = c.String(nullable: false, maxLength: 255),
                        AdminPassword = c.String(nullable: false, maxLength: 64),
                        EwsConnectUrl = c.String(nullable: false, maxLength: 512),
                    })
                .PrimaryKey(t => t.UserMail);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SettingModels");
        }
    }
}
