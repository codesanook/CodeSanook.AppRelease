using Codesanook.AmazonS3.Models;
using Codesanook.AppRelease.Models;
using Codesanook.AppRelease.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Settings;
using Orchard.UI.Admin;
using System.Linq;
using System.Web.Mvc;

namespace Codesanook.AppRelease.Controllers {
    [Admin]
    public class AppInfoController : Controller {
        private readonly IOrchardServices orchardService;
        private readonly IRepository<AppInfoRecord> repository;
        private readonly ISiteService siteService;

        public AppInfoController(
            IOrchardServices orchardService,
            IRepository<AppInfoRecord> appInfoRepository,
            ISiteService siteService) {
            this.orchardService = orchardService;
            this.repository = appInfoRepository;
            this.siteService = siteService;
        }

        public ActionResult Index(int? appInfoId) {
            var appInfoes = repository.Table
                .OrderBy(a => a.Title)
                .ToArray();

            var selectedAppInfo = appInfoId.HasValue
                ? appInfoes.SingleOrDefault(a => a.Id == appInfoId)
                : appInfoes.FirstOrDefault();

            var viewModel = new AppInfoIndexViewModel() {
                AppInfos = appInfoes,
                AppReleases = GetAppReleasesForAppInfo(selectedAppInfo),
                SelectedAppInfoId = selectedAppInfo?.Id,
                Setting = this.siteService.GetSiteSettings().As<AwsS3SettingPart>()
            };

            return View(viewModel);
        }

        private AppReleaseRecord[] GetAppReleasesForAppInfo(AppInfoRecord appInfo) {
            if (appInfo == null) {
                return new AppReleaseRecord[0];
            }

            var releases = appInfo
                .AppReleases
                .OrderByDescending(r => r.VersionCode)
                .ToArray();
            return releases;
        }

        public ActionResult Create() => View();

        [HttpPost]
        public ActionResult Create(AppInfoRecord appInfo) {
            appInfo.Title = appInfo.Title.Trim();
            appInfo.BundleId = appInfo.BundleId.Trim();
            this.repository.Create(appInfo);
            return RedirectToAction(nameof(Index), new { bundleId = appInfo.BundleId });
        }

        public ActionResult Edit(int id) => View();
    }
}
