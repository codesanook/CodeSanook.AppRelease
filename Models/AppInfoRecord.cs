using Orchard.UI.Admin;
using System.Collections.Generic;

namespace Codesanook.AppRelease.Models {

    [Admin] // Admin theme
    public class AppInfoRecord {
        public virtual int Id { get; set; }
        public virtual string BundleId { get; set; }
        public virtual string Title { get; set; }
        public virtual IList<AppReleaseRecord> AppReleases { get; set; }

        public AppInfoRecord() => AppReleases = new List<AppReleaseRecord>();
    }
}