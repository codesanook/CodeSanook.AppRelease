using CodeSanook.AppRelease.Models;
using Orchard.Data.Migration;
using CodeSanook.Common.Data;
using System;

namespace CodeSanook.AppRelease
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable<AppReleaseRecord>(tableConfig => tableConfig
                .Column<AppReleaseRecord, int>(table => table.Id, column => column.PrimaryKey().Identity())
                .Column<AppReleaseRecord, string>(table => table.VersionNumber)
                .Column<AppReleaseRecord, int>(table => table.VersionCode)
                .Column<AppReleaseRecord, string>(table => table.FileKey)
                .Column<AppReleaseRecord, DateTime?>(table => table.CreatedUtc)
            );

            return 1;
        }
    }
}