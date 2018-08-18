using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using CodeSanook.AppRelease.Models;
using CodeSanook.AppRelease.ViewModels;
using CodeSanook.Common.Web;
using CodeSanook.Configuration.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.AntiForgery;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;

namespace CodeSanook.AppRelease.Controllers
{
    [Admin]
    public class AppReleaseController : Controller
    {
        const string rootFolder = "app-releases";
        private static Regex versionNumberPattern = new Regex(@"^(?<major>\d+)\.(?<minor>\d{1,2})\.(?<patch>\d{1,2})$", RegexOptions.Compiled);
        private static Regex replaceTitleNamePattern = new Regex(@"[\s\.]+", RegexOptions.Compiled);

        //https://semver.org/
        //MAJOR version when you make incompatible API changes,
        //MINOR version when you add functionality in a backwards-compatible manner, and
        //PATCH version when you make backwards-compatible bug fixes.
        private readonly IRepository<AppInfoRecord> appInfoRepository;
        private readonly IRepository<AppReleaseRecord> appReleaseRepository;
        private readonly IOrchardServices orchardService;
        private readonly ISiteService siteService;

        //property injection
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public AppReleaseController(
            IRepository<AppInfoRecord> appInfoRepository,
            IRepository<AppReleaseRecord> appReleaseRepository,
            IOrchardServices orchardService,
            ISiteService siteService)
        {
            this.appInfoRepository = appInfoRepository;
            this.appReleaseRepository = appReleaseRepository;
            this.orchardService = orchardService;
            this.siteService = siteService;
            this.T = NullLocalizer.Instance;
            this.Logger = NullLogger.Instance;
        }

        public ActionResult Index()
        {
            ViewBag.Setting = this.siteService.GetSiteSettings().As<ModuleSettingPart>();
            var items = this.appReleaseRepository.Table.OrderByDescending(i => i.VersionCode).ToArray();
            return View(items);
        }

        public ActionResult Create(int? appInfoId)
        {
            if (!appInfoId.HasValue)
            {
                this.orchardService.Notifier.Error(T("No selected app for creating a new release."));
                return RedirectToAction(nameof(AppInfoController.Index), MvcHelper.GetControllerName<AppInfoController>());
            }

            var viewModel = new AppReleaseCreateViewModel()
            {
                AppInfoId = appInfoId.Value
            };

            return View(viewModel);
        }

        [ValidateAntiForgeryTokenOrchard(true)]
        [HttpPost]
        public async Task<ActionResult> Create(AppReleaseCreateViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return View(viewModel);
            }

            var match = versionNumberPattern.Match(viewModel.VersionNumber);
            if (!match.Success)
            {
                this.orchardService.Notifier.Error(
                    T($"Version number {viewModel.VersionNumber} is invalid, it must be major.minor.patch pattern."));
                return View(viewModel);
            }

            var major = int.Parse(match.Groups["major"].Value);
            var minor = int.Parse(match.Groups["minor"].Value);
            var patch = int.Parse(match.Groups["patch"].Value);
            int versionCode = (int)(major * Math.Pow(10, 4) + minor * Math.Pow(10, 2) + patch);

            //todo prevent existing version

            var appInfo = this.appInfoRepository.Get(viewModel.AppInfoId);

            var fileKey = CreateFileKey(viewModel, appInfo);
            using (var client = GetS3Client())
            using (var inputStream = viewModel.File.InputStream)
            {
                var fileTransferUtility = new TransferUtility(client);
                var uploadRequest = CreateFileUpload(inputStream, fileKey);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            var appRelease = new AppReleaseRecord()
            {
                VersionNumber = viewModel.VersionNumber,
                VersionCode = versionCode,
                CreatedUtc = DateTime.UtcNow,
                FileKey = fileKey,
                AppInfo = new AppInfoRecord() { Id = viewModel.AppInfoId }
            };

            this.appReleaseRepository.Create(appRelease);
            this.orchardService.Notifier.Add(NotifyType.Success, T("New release created successfully."));
            return RedirectToAction(nameof(Index), MvcHelper.GetControllerName<AppInfoController>(), new { appInfoId = appRelease.AppInfo.Id });
        }

        private AmazonS3Client GetS3Client()
        {
            var setting = orchardService.WorkContext.CurrentSite.As<ModuleSettingPart>();
            if (setting.UseLocalS3rver)
            {
                var credentials = new BasicAWSCredentials("", "");
                var config = new AmazonS3Config
                {
                    ServiceURL = setting.LocalS3rverServiceUrl,
                    UseHttp = true,
                    ForcePathStyle = true,
                };
                return new AmazonS3Client(credentials, config);
            }
            else
            {
                var credentials = new BasicAWSCredentials(setting.AwsAccessKey, setting.AwsSecretKey);
                var config = new AmazonS3Config
                {
                    ServiceURL = setting.AwsS3ServiceUrl,
                    UseHttp = false,
                };
                return new AmazonS3Client(credentials, config);
            }
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

        public ActionResult Manifest()
        {
            return Content("okay");
        }


        private string CreateFileKey(AppReleaseCreateViewModel viewModel, AppInfoRecord appInfo)
        {
            var now = DateTime.UtcNow;
            var folder = $"{rootFolder}/{now:yyyy}/{now:MM}/{now:dd}";

            var title = replaceTitleNamePattern.Replace(appInfo.Title, "-").ToLower();
            var fileName = $"{title}-{viewModel.VersionNumber}.ipa";
            return string.Format("{0}/{1}", folder, fileName).ToLower();
        }

        private TransferUtilityUploadRequest CreateFileUpload(Stream inputStream, string key)
        {
            var setting = orchardService.WorkContext.CurrentSite.As<ModuleSettingPart>();
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = inputStream,
                BucketName = setting.AwsS3BucketName,
                CannedACL = S3CannedACL.Private,
                Key = key
            };
            return uploadRequest;
        }
    }

}