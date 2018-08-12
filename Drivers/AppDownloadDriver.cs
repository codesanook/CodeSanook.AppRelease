using CodeSanook.AppRelease.Controllers;
using CodeSanook.AppRelease.Models;
using CodeSanook.Common.Web;
using CodeSanook.Configuration.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Settings;
using System.Linq;
using System.Xml.Linq;

namespace CodeSanook.AppRelease.Drivers
{
    public class AppDownloadDriver : ContentPartDriver<AppDownloadPart>
    {
        private readonly IOrchardServices orchardService;
        private readonly IRepository<AppInfoRecord> appInfoRepository;
        private readonly IRepository<AppReleaseRecord> appReleaseRepository;
        private readonly ISiteService siteService;

        protected override string Prefix => "AppDownload";

        public AppDownloadDriver(
            IOrchardServices orchardService,
            IRepository<AppInfoRecord> appInfoRepository,
            IRepository<AppReleaseRecord> appReleaseRepository,
            ISiteService siteService
            )
        {
            this.orchardService = orchardService;
            this.appInfoRepository = appInfoRepository;
            this.appReleaseRepository = appReleaseRepository;
            this.siteService = siteService;
        }

        protected override DriverResult Display(
            AppDownloadPart part,
            string displayType,
            dynamic shapeHelper)
        {
            var setting = this.siteService.GetSiteSettings().As<ModuleSettingPart>();
            var appInfo = this.appInfoRepository.Fetch(a => a.BundleId == part.BundleId).FirstOrDefault();

            string url = null;
            string title = null;
            if (appInfo != null)
            {
                var assemblyName = typeof(AppDownloadDriver).Assembly.GetName();
                url = Flurl.Url.Combine(assemblyName.Name, MvcHelper.GetControllerName<AppInfoController>(), nameof(AppInfoController.GetManifest), $"?bundleId={part.BundleId}");
                title = appInfo.Title;
            }

            return ContentShape(
                "Parts_AppDownload",//name to reference in placement.info
                () => shapeHelper.Parts_AppDownload(
                    BundleId: part.BundleId,
                    Url: url,
                    Title: title
                )
            );
        }

        //GET
        protected override DriverResult Editor(
            AppDownloadPart part,
            dynamic shapeHelper)
        {

            return ContentShape("Parts_AppDownload_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/AppDownload",
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            AppDownloadPart part,
            IUpdateModel updater,
            dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }


    }
}