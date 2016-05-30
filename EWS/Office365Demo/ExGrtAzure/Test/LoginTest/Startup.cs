using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LoginTest.Startup))]
namespace LoginTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
