using Codesanook.AppRelease.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Codesanook.AppRelease.Handlers {
    public class AppDownloadPartHandler : ContentHandler {
        public AppDownloadPartHandler(IRepository<AppDownloadPartRecord> repository) =>
            Filters.Add(StorageFilter.For(repository));
    }
}