using CodeSanook.AppRelease.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeSanook.AppRelease.Handlers
{
    public class AppDownloadHandler : ContentHandler
    {
        public AppDownloadHandler(IRepository<AppDownloadPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}