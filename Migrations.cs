using CodeSanook.AppRelease.Models;
using Orchard.Data.Migration;
using CodeSanook.Common.Data;
using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.ContentManagement;
using Orchard.Widgets.Services;
using System.Linq;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Core.Common.Models;
using Orchard.Security;
using CodeSanook.AppRelease.Handlers;
using Orchard.Settings;

namespace CodeSanook.AppRelease
{
    public class Migrations : DataMigrationImpl
    {
        private readonly IContentManager contentManager;
        private readonly IWidgetsService widgetsService;
        private readonly IOrchardServices orchardService;
        private readonly IAppDownloadPartEventHandler appReleaseEventHandler;
        private readonly ISiteService siteService;
        private readonly IMembershipService membershipService;

        public Localizer T { get; set; }

        public Migrations(
            IContentManager contentManager,
            IWidgetsService widgetsService,
            IOrchardServices orchardService,
            IAppDownloadPartEventHandler appReleaseEventHandler,
            ISiteService siteService,
            IMembershipService membershipService)
        {
            this.contentManager = contentManager;
            this.widgetsService = widgetsService;
            this.orchardService = orchardService;
            this.appReleaseEventHandler = appReleaseEventHandler;
            this.siteService = siteService;
            this.membershipService = membershipService;
            this.T = NullLocalizer.Instance;
        }

        public int Create()
        {
            SchemaBuilder.CreateTable<AppInfoRecord>(tableConfig => tableConfig
                .Column<AppInfoRecord, int>(table => table.Id, column => column.PrimaryKey().Identity())
                .Column<AppInfoRecord, string>(table => table.Title)
                .Column<AppInfoRecord, string>(table => table.BundleId)
            );

            SchemaBuilder.CreateTable<AppReleaseRecord>(tableConfig => tableConfig
                .Column<AppReleaseRecord, int>(table => table.Id, column => column.PrimaryKey().Identity())
                .Column<AppReleaseRecord, string>(table => table.VersionNumber)
                .Column<AppReleaseRecord, int>(table => table.VersionCode)
                .Column<AppReleaseRecord, string>(table => table.FileKey)
                .Column<AppReleaseRecord, DateTime?>(table => table.CreatedUtc)
                .Column<AppReleaseRecord, int>(table => table.AppInfo.Id)
            );

            //create record
            SchemaBuilder.CreateTable<AppDownloadPartRecord>(tableConfig => tableConfig
                .ContentPartRecord()//auto assign id from content item 
                .Column<AppDownloadPartRecord, string>(table => table.PlayStoreUrl)
                .Column<AppDownloadPartRecord, string>(table => table.AppStoreUrl)
                .Column<AppDownloadPartRecord, bool>(table => table.IsEnterpriseApp)
                .Column<AppDownloadPartRecord, string>(table => table.BundleId)
            );

            //create part
            ContentDefinitionManager.AlterPartDefinition(nameof(AppDownloadPart),
               cfg => cfg.Attachable());

            //Create a new widget content type with our map. 
            //We make use of the AsWidgetWithIdentity() helper.
            ContentDefinitionManager.AlterTypeDefinition("AppDownloadWidget", cfg => cfg
                .WithPart(nameof(AppDownloadPart))
                .AsWidgetWithIdentity());

            var layer = widgetsService.GetLayers().FirstOrDefault(x => x.Name == "Default");
            if (layer == null)
            {
                orchardService.Notifier.Warning(T("AppDownloadWidget could not be created because no 'Default' layer. Please create it manually."));
                return 1;
            }

            var widgetPart = widgetsService.CreateWidget(layer.Id, "AppDownloadWidget", "App Download", "1.0", "BeforeContent");
            widgetPart.RenderTitle = false;
            var commonPart = widgetPart.As<CommonPart>();
            //var user = this.authenticationServiceFactory().GetAuthenticatedUser();
            var superUser = this.siteService.GetSiteSettings().SuperUser;
            var owner = this.membershipService.GetUser(superUser);
            commonPart.Owner = owner;

            //publish widget
            orchardService.ContentManager.Publish(widgetPart.ContentItem);

            var appDownloadPart = widgetPart.As<AppDownloadPart>();
            this.appReleaseEventHandler.OnInitialized(appDownloadPart);
            return 1;
        }
    }
}