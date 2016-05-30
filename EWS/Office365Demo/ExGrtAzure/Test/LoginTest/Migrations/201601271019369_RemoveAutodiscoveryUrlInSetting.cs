namespace LoginTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveAutodiscoveryUrlInSetting : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SettingModels", "AutodiscoverUrl");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SettingModels", "AutodiscoverUrl", c => c.String(nullable: false, maxLength: 255));
        }
    }
}
