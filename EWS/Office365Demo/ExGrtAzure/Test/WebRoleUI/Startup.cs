using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebRoleUI.Startup))]
namespace WebRoleUI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
