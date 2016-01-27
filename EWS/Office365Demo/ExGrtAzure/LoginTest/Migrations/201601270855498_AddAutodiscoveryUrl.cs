namespace LoginTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAutodiscoveryUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SettingModels", "AutodiscoverUrl", c => c.String(nullable: false, maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SettingModels", "AutodiscoverUrl");
        }
    }
}
