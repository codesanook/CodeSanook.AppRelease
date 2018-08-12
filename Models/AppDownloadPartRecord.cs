using Orchard.ContentManagement.Records;

namespace CodeSanook.AppRelease.Models
{
    public class AppDownloadPartRecord : ContentPartRecord
    {
        public virtual string BundleId { get; set; }
    }
}