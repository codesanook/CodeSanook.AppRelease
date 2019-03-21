using CodeSanook.AppRelease.Models;
using CodeSanook.Configuration.Models;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Orchard.Caching.Services;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace CodeSanook.AppRelease.Services
{
    public class AppReleaseService : IAppReleaseService
    {
        private readonly IRepository<AppReleaseRecord> appReleaseRepository;
        private readonly ISiteService siteService;
        private readonly ICacheService cacheService;
        private readonly ModuleSettingPart setting;

        private string[] scopes = new string[] { AndroidPublisherService.Scope.Androidpublisher }; // view and manage your Google Analytics data

        //property injection
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public AppReleaseService(
            IRepository<AppReleaseRecord> appReleaseRepository,
            ISiteService siteService,
            ICacheService cacheService
        )
        {
            this.appReleaseRepository = appReleaseRepository;
            this.siteService = siteService;
            this.cacheService = cacheService;
            this.T = NullLocalizer.Instance;
            this.Logger = NullLogger.Instance;
            setting = this.siteService.GetSiteSettings().As<ModuleSettingPart>();
        }

        public LatestAppReleaseInfo GetLatestAppReleaseInfo(string bundleId)
        {
            var latestAppReleaseInfo = this.cacheService.Get<LatestAppReleaseInfo>(LatestAppReleaseInfo.CacheKey);
            if (latestAppReleaseInfo != null)
            {
                return latestAppReleaseInfo;
            }

            //Get p12 key from https://console.developers.google.com
            var data = Convert.FromBase64String(setting.GoogleServiceAccountP12Key);

            //TODO move to configuration before publish this module
            var serviceAccountEmail = "google-play-api@thailand-fls-app.iam.gserviceaccount.com";  // found https://console.developers.google.com

            //loading the Key file
            var certificate = new X509Certificate2(data, "notasecret", X509KeyStorageFlags.Exportable);
            var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
            {
                Scopes = scopes
            }.FromCertificate(certificate));

            var service = new AndroidPublisherService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "CodeSanook.AppRelease"
                });

            // Create a new edit to make changes.
            var edits = service.Edits;
            var appEdit = new Google.Apis.AndroidPublisher.v3.Data.AppEdit();
            var editRequest = edits.Insert(appEdit, bundleId);
            appEdit = editRequest.Execute();

            var response = edits.Apks.List(bundleId, appEdit.Id).Execute();
            var latestApk = response.Apks.OrderByDescending(x => x.VersionCode).First();
            var latestIap = GetLatestAppRelease(bundleId);

            latestAppReleaseInfo = new LatestAppReleaseInfo()
            {
                AndroidVersionCode = latestApk.VersionCode,
                IOsVersionCode = latestIap?.VersionCode
            };

            //Add to cache for two week
            this.cacheService.Put(LatestAppReleaseInfo.CacheKey, latestAppReleaseInfo, TimeSpan.FromDays(14));
            return latestAppReleaseInfo;
        }

        public AppReleaseRecord GetLatestAppRelease(string bundleId)
        {
            var latestRelease = this.appReleaseRepository
                //TODO database indexing on bundle id
                .Fetch(r => r.AppInfo.BundleId == bundleId)
                .OrderByDescending(r => r.VersionCode)
                .FirstOrDefault();
            return latestRelease;
        }

        public string GetManifest(string bundleId)
        {
            var assembly = typeof(AppReleaseService).Assembly;
            var resourceName = $"{assembly.GetName().Name}.Data.manifest.plist";
            XDocument xmlDoc;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                xmlDoc = XDocument.Load(stream);
            }

            //https://stackoverflow.com/a/12358341/1872200
            xmlDoc.DocumentType.InternalSubset = null;
            var allKeys = xmlDoc.Descendants("key");
            var urlKey = allKeys.Single(e => e.Value == "url");
            var urlValue = urlKey.NextNode as XElement;

            var latestRelease = GetLatestAppRelease(bundleId);
            var url = Flurl.Url.Combine(setting.AwsS3PublicUrl, latestRelease?.FileKey);
            urlValue.Value = url;

            var bundleIdKey = allKeys.Single(e => e.Value == "bundle-identifier");
            var bundleIdValue = bundleIdKey.NextNode as XElement;
            bundleIdValue.Value = bundleId;

            var bundleVersionKey = allKeys.Single(e => e.Value == "bundle-version");
            var bundleVersionValue = bundleVersionKey.NextNode as XElement;
            bundleVersionValue.Value = latestRelease?.VersionNumber;

            var titleKey = allKeys.Where(e => e.Value == "title").First();
            var titleValue = titleKey.NextNode as XElement;
            titleValue.Value = latestRelease?.AppInfo?.Title;

            var builder = new StringBuilder();
            using (var writer = new Utf8StringWriter(builder))
            {
                xmlDoc.Save(writer);
            }
            return builder.ToString();
        }
    }
}
