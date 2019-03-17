using CodeSanook.AppRelease.Models;
using CodeSanook.AppRelease.Services;
using CodeSanook.Configuration.Models;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Orchard.Caching.Services;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
