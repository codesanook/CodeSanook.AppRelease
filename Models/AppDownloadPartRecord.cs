using Orchard.ContentManagement.Records;

namespace Codesanook.AppRelease.Models
{
    public class AppDownloadPartRecord : ContentPartRecord
    {
        public virtual string PlayStoreUrl { get; set; }
        public virtual string AppStoreUrl { get; set; }

        //For enterprise APP only
        public virtual bool IsEnterpriseApp { get; set; }
        public virtual string BundleId { get; set; }
    }
}