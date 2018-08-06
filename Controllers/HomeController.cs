using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace CodeSanook.AppRelease.Controllers
{
    [Themed]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Manifest()
        {
            var assembly = typeof(HomeController).Assembly;
            const string resourceName = "CodeSanook.AppRelease.Data.manifest.plist";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd();
                return Content(result, "text/xml");
            }
        }
    }
}