using Codesanook.AppRelease.Services;
using System.Web.Mvc;

namespace Codesanook.AppRelease.Controllers {
    public class AppDownloadController : Controller {
        private readonly IAppReleaseService appReleaseService;

        public AppDownloadController(IAppReleaseService appReleaseService) =>
            this.appReleaseService = appReleaseService;

        public ActionResult GetManifest(string bundleId) =>
            Content(appReleaseService.GetManifest(bundleId), "text/xml");
    }
}
