using CodeSanook.AppRelease.Models;
using CodeSanook.AppRelease.Services;
using System.Web.Http;

namespace CodeSanook.AppRelease.Controllers
{
    public class LatestAppReleaseInfoController : ApiController
    {
        private readonly IAppReleaseService appReleaseService;

        public LatestAppReleaseInfoController(IAppReleaseService appReleaseService)
        {
            this.appReleaseService = appReleaseService;
        }

        public LatestAppReleaseInfo Get(string id)
        {
            return appReleaseService.GetLatestAppReleaseInfo(id);
        }
    }
}
