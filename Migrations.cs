using CodeSanook.AppRelease.Models;
using Orchard.Data.Migration;
using CodeSanook.Common.Data;
using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;

namespace CodeSanook.AppRelease
{
    public class Migrations : DataMigrationImpl
    {
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
            return 1;
        }

    }
}