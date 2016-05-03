namespace LoginTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifySettingPasswordLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SettingModels", "AdminPassword", c => c.String(nullable: false, maxLength: 1024));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SettingModels", "AdminPassword", c => c.String(nullable: false, maxLength: 64));
        }
    }
}
