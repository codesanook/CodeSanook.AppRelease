using CodeSanook.AppRelease.Models;
using CodeSanook.Configuration.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;

namespace CodeSanook.AppRelease.Controllers
{
    public class AppDownloadController : Controller
    {
        private readonly IRepository<AppReleaseRecord> appReleaseRepository;
        private readonly ISiteService siteService;
        private readonly ModuleSettingPart setting;

        //property injection
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public AppDownloadController(
            IRepository<AppReleaseRecord> appReleaseRepository,
            ISiteService siteService)
        {
            this.appReleaseRepository = appReleaseRepository;
            this.siteService = siteService;
            this.T = NullLocalizer.Instance;
            this.Logger = NullLogger.Instance;
            setting = this.siteService.GetSiteSettings().As<ModuleSettingPart>();
        }

        public ActionResult GetManifest(string bundleId)
        {
            var assembly = typeof(AppInfoController).Assembly;
            var resourceName = $"{assembly.GetName().Name}.Data.manifest.plist";
            XDocument xmlDoc;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                //var result = reader.ReadToEnd();
                xmlDoc = XDocument.Load(stream);
            }

            //https://stackoverflow.com/a/12358341/1872200
            xmlDoc.DocumentType.InternalSubset = null;
            var allKeys = xmlDoc.Descendants("key");
            var urlKey = allKeys.Single(e => e.Value == "url");
            var urlValue = urlKey.NextNode as XElement;

            var latestRelease = this.appReleaseRepository
                //TODO indexing on bundle id
                .Fetch(r => r.AppInfo.BundleId == bundleId)
                .OrderByDescending(r => r.VersionCode)
                .FirstOrDefault();

            var url = Flurl.Url.Combine(setting.AwsS3PublicUrl, latestRelease?.FileKey);
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

            var builder = new StringBuilder();
            using (var writer = new Utf8StringWriter(builder))
            {
                xmlDoc.Save(writer);
            }

            return Content(builder.ToString(), "text/xml");
        }

        public ActionResult Manifest()
        {
            return Content("OKay");
        }

        public ActionResult Index()
        {
            return Content("index");
        }


    }
    public class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter(StringBuilder stringBuilder) : base(stringBuilder)
        {

        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}