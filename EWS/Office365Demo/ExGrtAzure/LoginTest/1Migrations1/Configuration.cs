namespace LoginTest.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<LoginTest.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "LoginTest.Models.ApplicationDbContext";
            
        }

        protected override void Seed(LoginTest.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            
            context.Users.AddOrUpdate(p => p.UserName,
                new Models.ApplicationUser()
                {
                    Email = "haiyang.ling@arcserve.com",
                    PasswordHash = "AJefJioh/4+9x/S31QuPfmjUfeeC1ZQJ+wkWgNgGxz2KsmyIm8Co4h79iK17mkDubQ==",
                    SecurityStamp = "c590ae8e-eb4d-47f0-98bc-81a3dbbfff33",
                    EmailConfirmed = false,
                    Id = "4c939d4c-eaf4-4c9c-b58c-965a3e4a7263",
                    PhoneNumber = null,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    UserName = "haiyang.ling@arcserve.com",
                    Organization = "Arcserve"
                });
        }
    }
}
