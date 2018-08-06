using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;
using CodeSanook.Common.Modules;

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
                    Name = "AppRelease",
                    Priority = 100,
                    Route = new Route(
                        "AppRelease/{action}", //URL, cannot be start with / or ~
                        new RouteValueDictionary {//default
                            {"area", moduleName},
                            {"controller", "Home"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),//constraint
                        new RouteValueDictionary {//data token
                            {"area", moduleName}
                        },
                        new MvcRouteHandler())//handler
                },
            };
        }
    }
}