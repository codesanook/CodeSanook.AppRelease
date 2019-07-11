using Codesanook.AppRelease.Models;
using Codesanook.AppRelease.Services;
using System.Web.Http;

namespace Codesanook.AppRelease.Controllers {
    public class LatestAppReleaseInfoController : ApiController {
        private readonly IAppReleaseService appReleaseService;

        public LatestAppReleaseInfoController(IAppReleaseService appReleaseService) =>
            this.appReleaseService = appReleaseService;

        public LatestAppReleaseInfo Get(string id) =>
            appReleaseService.GetLatestAppReleaseInfo(id);
    }
}
