using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(newcar.Startup))]
namespace newcar
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
