using CodeSanook.AppRelease.Controllers;
using CodeSanook.AppRelease.Models;
using CodeSanook.Common.Web;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.UI.Notify;
using System.Linq;
using Orchard.Widgets.Models;

namespace CodeSanook.AppRelease.Drivers
{
    public class AppDownloadDriver : ContentPartDriver<AppDownloadPart>
    {
        private readonly IOrchardServices orchardService;
        private readonly IRepository<AppInfoRecord> appInfoRepository;
        private readonly ISiteService siteService;

        public Localizer T { get; set; }
        protected override string Prefix => "AppDownload";

        public AppDownloadDriver(
            IOrchardServices orchardService,
            IRepository<AppInfoRecord> appInfoRepository,
            ISiteService siteService
            )
        {
            this.orchardService = orchardService;
            this.appInfoRepository = appInfoRepository;
            this.siteService = siteService;
            this.T = NullLocalizer.Instance;
        }

        //GET for showing in front-end
        protected override DriverResult Display(
            AppDownloadPart part,
            string displayType,
            dynamic shapeHelper)
        {
            var widgetPart = part.As<WidgetPart>();
            var title = widgetPart.Title;

            var AndroidUrl = part.PlayStoreUrl;
            var iOsUrl = GetIOsUrl(part);

            return ContentShape(
                "Parts_AppDownload",//name to reference in placement.info
                () => shapeHelper.Parts_AppDownload(
                    Title: title,
                    IOSUrl: iOsUrl,
                    AndroidUrl: AndroidUrl
                )
            );
        }

        private string GetIOsUrl(AppDownloadPart part)
        {
            if (part.IsEnterpriseApp)
            {
                var appInfo = this.appInfoRepository.Fetch(a => a.BundleId == part.BundleId).FirstOrDefault();
                if (appInfo == null)
                {
                    this.orchardService.Notifier.Warning(T("Please create app info and release from admin panel."));
                    return null;
                }

                var assemblyName = typeof(AppDownloadDriver).Assembly.GetName();
                return Flurl.Url.Combine(
                    assemblyName.Name,
                    MvcHelper.GetControllerName<AppInfoController>(),
                    nameof(AppInfoController.GetManifest),
                    $"?bundleId={part.BundleId}");
            }
            else
            {
                return part.AppStoreUrl;
            }
        }

        //GET for editing 
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