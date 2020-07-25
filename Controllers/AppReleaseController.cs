using Amazon.S3;
using Amazon.S3.Model;
using Codesanook.AmazonS3.Models;
using Codesanook.AmazonS3.Services;
using Codesanook.AppRelease.Models;
using Codesanook.AppRelease.ViewModels;
using Codesanook.Common.Models;
using Codesanook.Common.Web;
using Orchard;
using Orchard.Caching.Services;
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

namespace Codesanook.AppRelease.Controllers {
    [Admin]
    public class AppReleaseController : Controller {
        const string rootFolder = "app-releases";
        private static readonly Regex versionNumberPattern = new Regex(@"^(?<major>\d+)\.(?<minor>\d{1,2})\.(?<patch>\d{1,2})$", RegexOptions.Compiled);
        private static readonly Regex replaceTitleNamePattern = new Regex(@"[\s\.]+", RegexOptions.Compiled);
        private readonly ICacheService cacheService;
        private readonly IAmazonS3 amazonS3Client;

        // https://semver.org/
        // MAJOR version when you make incompatible API changes,
        // MINOR version when you add functionality in a backwards-compatible manner, and
        // PATCH version when you make backwards-compatible bug fixes.
        private readonly IRepository<AppInfoRecord> appInfoRepository;
        private readonly IRepository<AppReleaseRecord> appReleaseRepository;
        private readonly IOrchardServices orchardService;
        private readonly ISiteService siteService;
        private readonly CommonSettingPart commonSettingPart;
        private readonly AwsS3SettingPart awsS3SettingPart;

        //property injection
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public AppReleaseController(
            IRepository<AppInfoRecord> appInfoRepository,
            IRepository<AppReleaseRecord> appReleaseRepository,
            IOrchardServices orchardService,
            ISiteService siteService,
            ICacheService cacheService,
            IAmazonS3 amazonS3Client
        ) {
            this.appInfoRepository = appInfoRepository;
            this.appReleaseRepository = appReleaseRepository;
            this.orchardService = orchardService;
            this.siteService = siteService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            this.cacheService = cacheService;
            this.amazonS3Client = amazonS3Client;
            commonSettingPart = this.siteService.GetSiteSettings().As<CommonSettingPart>();
            awsS3SettingPart = this.siteService.GetSiteSettings().As<AwsS3SettingPart>();
        }

        public ActionResult Create(int? appInfoId) {
            if (!appInfoId.HasValue) {
                this.orchardService.Notifier.Error(T("No selected App for creating a new release."));
                return RedirectToAction(nameof(AppInfoController.Index), MvcHelper.GetControllerName<AppInfoController>());
            }

            var viewModel = new AppReleaseCreateViewModel() {
                AppInfoId = appInfoId.Value
            };

            return View(viewModel);
        }

        [ValidateAntiForgeryTokenOrchard(true)]
        [HttpPost]
        public async Task<ActionResult> Create(AppReleaseCreateViewModel viewModel) {
            //TODO move logic to ApReleaseService
            if (!ModelState.IsValid) {
                return View(viewModel);
            }

            var match = versionNumberPattern.Match(viewModel.VersionNumber);
            if (!match.Success) {
                orchardService.Notifier.Error(
                    T($"Version number {viewModel.VersionNumber} is invalid, it must be major.minor.patch pattern."));
                return View(viewModel);
            }

            var major = int.Parse(match.Groups["major"].Value);
            var minor = int.Parse(match.Groups["minor"].Value);
            var patch = int.Parse(match.Groups["patch"].Value);
            int versionCode = (int)(major * Math.Pow(10, 4) + minor * Math.Pow(10, 2) + patch);

            //TODO prevent existing version
            var appInfo = this.appInfoRepository.Get(viewModel.AppInfoId);
            var fileKey = CreateFileKey(viewModel, appInfo);
            using (amazonS3Client)
            using (var inputStream = viewModel.File.InputStream) {
                //var fileTransferUtility = new TransferUtility(client);
                //await fileTransferUtility.UploadAsync(uploadRequest);
                var putObjectRequest = CreateFileUploadRequest(inputStream, fileKey);
                await amazonS3Client.PutObjectAsync(putObjectRequest);
            }

            var appRelease = new AppReleaseRecord() {
                VersionNumber = viewModel.VersionNumber,
                VersionCode = versionCode,
                CreatedUtc = DateTime.UtcNow,
                FileKey = fileKey,
                AppInfo = new AppInfoRecord() { Id = viewModel.AppInfoId }
            };

            appReleaseRepository.Create(appRelease);

            //Remove existing cache after a new release
            cacheService.Remove(LatestAppReleaseInfo.CacheKey);

            orchardService.Notifier.Add(NotifyType.Success, T("New release created successfully."));
            return RedirectToAction(nameof(AppInfoController.Index), MvcHelper.GetControllerName<AppInfoController>(), new { appInfoId = appRelease.AppInfo.Id });
        }

        private string CreateFileKey(AppReleaseCreateViewModel viewModel, AppInfoRecord appInfo) {
            var now = DateTime.UtcNow;
            var folder = $"{rootFolder}/{now:yyyy}/{now:MM}/{now:dd}/{now:HH}/{now:mm}/{now:ss}";

            var title = replaceTitleNamePattern.Replace(appInfo.Title, "-").ToLower();
            var fileName = $"{title}-{viewModel.VersionNumber}.ipa";
            return string.Format("{0}/{1}", folder, fileName).ToLower();
        }

        private PutObjectRequest CreateFileUploadRequest(Stream inputStream, string key) {
            var request = new PutObjectRequest() {
                BucketName = awsS3SettingPart.AwsS3BucketName,
                Key = key,
                InputStream = inputStream,
                CannedACL = S3CannedACL.PublicRead,
                ContentType = "application/octet-stream"
            };
            return request;
        }
    }
}

