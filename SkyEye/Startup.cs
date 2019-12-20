using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SkyEye.Startup))]
namespace SkyEye
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
