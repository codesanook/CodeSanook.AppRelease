using CodeSanook.AppRelease.Services;
using System.Web.Mvc;

namespace CodeSanook.AppRelease.Controllers
{
    public class AppDownloadController : Controller
    {
        private readonly IAppReleaseService appReleaseService;

        public AppDownloadController(IAppReleaseService appReleaseService)
        {
            this.appReleaseService = appReleaseService;
        }

        public ActionResult GetManifest(string bundleId)
        {
            var manifestContent = this.appReleaseService.GetManifest(bundleId);
            return Content(manifestContent, "text/xml");
        }
    }
}
