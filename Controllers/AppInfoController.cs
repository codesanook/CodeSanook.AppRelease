using CodeSanook.AppRelease.Models;
using CodeSanook.AppRelease.ViewModels;
using CodeSanook.Configuration.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Settings;
using Orchard.UI.Admin;
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
        private readonly IRepository<AppReleaseRecord> appReleaseRepository;
        private readonly ISiteService siteService;

        public AppInfoController(
            IOrchardServices orchardService,
            IRepository<AppInfoRecord> appInfoRepository,
            IRepository<AppReleaseRecord> appReleaseRepository,
            ISiteService siteService)
        {
            this.orchardService = orchardService;
            this.repository = appInfoRepository;
            this.appReleaseRepository = appReleaseRepository;
            this.siteService = siteService;
        }

        public ActionResult Index(int? appInfoId)
        {
            var appInfoes = repository.Table
                .OrderBy(a => a.Title)
                .ToArray();

            var selectedAppInfoId = appInfoId ?? appInfoes.FirstOrDefault()?.Id;
            var appReleases = GetAppReleases(selectedAppInfoId);

            var setting = this.siteService.GetSiteSettings().As<ModuleSettingPart>();
            var viewModel = new AppInfoIndexViewModel()
            {
                AppInfos = appInfoes,
                AppReleases = appReleases,
                SelectedAppInfoId = selectedAppInfoId,
                Setting = setting
            };

            return View(viewModel);
        }



        private AppReleaseRecord[] GetAppReleases(int? selectedAppInfoId)
        {
            var releaseQuery = appReleaseRepository.Table;
            //selectedBundleId can be null incase that there is no any release
            if (selectedAppInfoId.HasValue)
            {
                releaseQuery = releaseQuery.Where(r => r.AppInfo.Id == selectedAppInfoId);
            }

            var releases = releaseQuery
                .OrderByDescending(r => r.VersionNumber)
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

        public ActionResult GetManifest(string bundleId)
        {
            var assembly = typeof(AppInfoController).Assembly;
            var resourceName = $"{assembly.GetName().Name}.Data.manifest.plist";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                //var result = reader.ReadToEnd();
                var xmlDoc = XDocument.Load(stream);
                var allKeys = xmlDoc.Descendants("key");

                var urlKey = allKeys.Single(e => e.Value == "url");
                var urlValue = urlKey.NextNode as XElement;

                var latestRelease = this.appReleaseRepository
                    //TODO indexing on bundle id
                    .Fetch(r => r.AppInfo.BundleId == bundleId)
                    .OrderByDescending(r => r.VersionNumber)
                    .FirstOrDefault();

                var setting = this.siteService.GetSiteSettings().As<ModuleSettingPart>();
                var url = Flurl.Url.Combine(setting.AwsS3PublicUrl, setting.AwsS3BucketName, latestRelease?.FileKey);
                urlValue.Value = url;

                var bundleIdKey = allKeys.Single(e => e.Value == "bundle-identifier");
                var bundleIdValue = bundleIdKey.NextNode as XElement;
                bundleIdValue.Value = bundleId;


                var bundleVersionKey = allKeys.Single(e => e.Value == "bundle-version");
                var bundleVersionValue = bundleVersionKey.NextNode as XElement;
                bundleVersionValue.Value = latestRelease.VersionNumber;

                var titleKey = allKeys.Where(e => e.Value == "title").First();
                var titleValue = titleKey.NextNode as XElement;
                titleValue.Value = latestRelease?.AppInfo?.Title;

                return Content(xmlDoc.ToString(), "text/xml");
            }
        }
    }
}