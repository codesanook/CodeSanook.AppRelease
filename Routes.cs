using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;
using CodeSanook.Common.Modules;
using CodeSanook.Common.Web;
using CodeSanook.AppRelease.Controllers;
using System.Web.Http;

namespace CodeSanook.AppRelease
{
    public class Routes : IRouteProvider
    {
        private static string moduleName = ModuleHelper.GetModuleName<Routes>();

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[]
            {
                new RouteDescriptor
                {
                    Name = "AppDownload",
                    Priority = 100,
                    Route = new Route(
                        "appdownload/{bundleId}/manifest.plist", //URL, cannot be start with / or ~
                        new RouteValueDictionary {//default
                            {"area", moduleName},
                            {"controller", MvcHelper.GetControllerName<AppDownloadController>() },
                            {"action", nameof(AppDownloadController.GetManifest)}
                        },
                        new RouteValueDictionary(),//constraint
                        new RouteValueDictionary {//data token
                            {"area", moduleName}
                        },
                        new MvcRouteHandler())//handler
                },

                new HttpRouteDescriptor
                {
                    Name = "LatestAppReleaseInfo",
                    Priority = 0,
                    RouteTemplate ="LatestAppReleaseInfo/{id}" ,
                    Defaults = new
                    {
                        area = this.GetType().Namespace, //module name
                        controller = "LatestAppReleaseInfo", //controller name without controller subfix
                        id = RouteParameter.Optional
                    },
                }

        };
    }
}
}
