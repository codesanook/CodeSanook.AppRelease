using Autofac;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Codesanook.AppRelease
{
    public class WebApiConfig : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var config = GlobalConfiguration.Configuration;
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
        }
    }
}
