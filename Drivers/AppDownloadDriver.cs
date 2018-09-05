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
using System.Web.Mvc;
using CodeSanook.Common.Modules;
using Orchard.Mvc.Extensions;

namespace CodeSanook.AppRelease.Drivers
{
    public class AppDownloadDriver : ContentPartDriver<AppDownloadPart>
    {
        private readonly IOrchardServices orchardService;
        private readonly IRepository<AppInfoRecord> appInfoRepository;
        private readonly ISiteService siteService;
        private readonly IWorkContextAccessor workContextAccessor;

        public Localizer T { get; set; }
        protected override string Prefix => "AppDownload";

        public AppDownloadDriver(
            IOrchardServices orchardService,
            IRepository<AppInfoRecord> appInfoRepository,
            ISiteService siteService,
            IWorkContextAccessor workContextAccessor)
        {
            this.orchardService = orchardService;
            this.appInfoRepository = appInfoRepository;
            this.siteService = siteService;
            this.workContextAccessor = workContextAccessor;
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
            if (!part.IsEnterpriseApp)
            {
                return part.AppStoreUrl;
            }

            var appInfo = this.appInfoRepository.Fetch(a => a.BundleId == part.BundleId).FirstOrDefault();
            if (appInfo == null)
            {
                this.orchardService.Notifier.Warning(T("Please create app info and release from admin panel."));
                return null;
            }

            var request = orchardService.WorkContext.HttpContext.Request;
            var siteUrl = orchardService.WorkContext.CurrentSite.BaseUrl;
            siteUrl = !string.IsNullOrWhiteSpace(siteUrl)
                ? siteUrl
                : string.Format("{0}://{1}", request.Url.Scheme, request.Url.Host);

            var routeValues = new
            {
                Area = ModuleHelper.GetModuleName<AppDownloadDriver>(),
                bundleId = part.BundleId
            };

            var url = new UrlHelper(request.RequestContext);
            //var downloadUrl = url.Action(
            //      nameof(AppDownloadController.GetManifest),
            //      MvcHelper.GetControllerName<AppDownloadController>(),
            //      routeValues);


            var downloadUrl= url.RouteUrl("AppDownload", new { bundleId = part.BundleId });
            var absoluteUrl = url.MakeAbsolute(downloadUrl, siteUrl);
            return $"itms-services://?action=download-manifest&url={absoluteUrl}";
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