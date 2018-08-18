using CodeSanook.AppRelease.Models;
using CodeSanook.AppRelease.ViewModels;
using CodeSanook.Configuration.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Settings;
using Orchard.UI.Admin;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;

namespace CodeSanook.AppRelease.Controllers
{
    [Admin]
    public class AppInfoController : Controller
    {
        private readonly IOrchardServices orchardService;
        private readonly IRepository<AppInfoRecord> repository;
        private readonly ISiteService siteService;

        public AppInfoController(
            IOrchardServices orchardService,
            IRepository<AppInfoRecord> appInfoRepository,
            ISiteService siteService)
        {
            this.orchardService = orchardService;
            this.repository = appInfoRepository;
            this.siteService = siteService;
        }

        public ActionResult Index(int? appInfoId)
        {
            var appInfoes = repository.Table
                .OrderBy(a => a.Title)
                .ToArray();

            var selectedAppInfo = appInfoId.HasValue
                ? appInfoes.SingleOrDefault(a => a.Id == appInfoId)
                : appInfoes.FirstOrDefault();

            var viewModel = new AppInfoIndexViewModel()
            {
                AppInfos = appInfoes,
                AppReleases = GetAppReleasesForAppInfo(selectedAppInfo),
                SelectedAppInfoId = selectedAppInfo?.Id,
                Setting = this.siteService.GetSiteSettings().As<ModuleSettingPart>()
            };

            return View(viewModel);
        }

        private AppReleaseRecord[] GetAppReleasesForAppInfo(AppInfoRecord appInfo)
        {
            if (appInfo == null)
            {
                return Array.Empty<AppReleaseRecord>();
            }

            var releases = appInfo
                .AppReleases
                .OrderByDescending(r => r.VersionCode)
                .ToArray();
            return releases;
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(AppInfoRecord appInfo)
        {
            this.repository.Create(appInfo);
            return RedirectToAction(nameof(Index), new { bundleId = appInfo.BundleId });
        }

        public ActionResult Edit(int id)
        {
            return View();
        }

    }
}