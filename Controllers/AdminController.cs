using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using CodeSanook.AppRelease.Models;
using CodeSanook.Configuration.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.AntiForgery;
using Orchard.UI.Notify;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
namespace CodeSanook.AppRelease.Controllers
{
    public class AdminController : Controller
    {
        const string rootFolder = "app-releases";
        private static Regex versionNumberPattern = new Regex(@"^(?<major>\d+)\.(?<minor>\d{1,2})\.(?<patch>\d{1,2})$", RegexOptions.Compiled);

        //https://semver.org/
        //MAJOR version when you make incompatible API changes,
        //MINOR version when you add functionality in a backwards-compatible manner, and
        //PATCH version when you make backwards-compatible bug fixes.
        private readonly IRepository<AppReleaseRecord> repository;
        private readonly IOrchardServices orchardService;
        private readonly ModuleSettingPart setting;

        //property injection
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public AdminController(
            IRepository<AppReleaseRecord> repository,
            IOrchardServices orchardService)
        {
            this.repository = repository;
            this.orchardService = orchardService;
            this.T = NullLocalizer.Instance;
            this.Logger = NullLogger.Instance;
            setting = orchardService.WorkContext.CurrentSite.As<ModuleSettingPart>();
        }

        public ActionResult Index()
        {
            ViewBag.Setting = this.setting;
            var items = this.repository.Table.OrderByDescending(i=>i.VersionCode).ToArray();
            return View(items);
        }

        public ActionResult Add()
        {
            return View();
        }

        [ValidateAntiForgeryTokenOrchard(true)]
        [HttpPost]
        public async Task<ActionResult> Add(PostedFile postedFile)
        {
            var match = versionNumberPattern.Match(postedFile.VersionNumber);
            if (!match.Success)
            {
                this.orchardService.Notifier.Add(NotifyType.Success, T("New release created successfully."));
                return RedirectToAction(nameof(Index));
            }

            var major = int.Parse(match.Groups["major"].Value);
            var minor = int.Parse(match.Groups["minor"].Value);
            var patch = int.Parse(match.Groups["patch"].Value);
            int versionCode = (int)(major * Math.Pow(10, 4) + minor * Math.Pow(10, 2) + patch);

            var fileKey = CreateFileKey(postedFile.VersionNumber);
            using (var client = GetS3Client())
            using (var inputStream = postedFile.File.InputStream)
            {
                var fileTransferUtility = new TransferUtility(client);
                var uploadRequest = CreateFileUpload(inputStream, fileKey);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            var appRelease = new AppReleaseRecord()
            {
                VersionNumber = postedFile.VersionNumber,
                VersionCode = versionCode,
                CreatedUtc = DateTime.UtcNow,
                FileKey = fileKey,
            };

            this.repository.Create(appRelease);
            this.orchardService.Notifier.Add(NotifyType.Success, T("New release created successfully."));
            return RedirectToAction(nameof(Index));
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

        private string CreateFileKey(string versionNumber)
        {
            var now = DateTime.UtcNow;
            var folder = $"{rootFolder}/{now:yyyy}/{now:MM}/{now:dd}";
            var fileName = $"thailand-fls-{versionNumber}.ipa";
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