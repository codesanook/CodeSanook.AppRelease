using Orchard.UI.Admin;
using System.Collections.Generic;

namespace CodeSanook.AppRelease.Models
{

    [Admin]//admin theme
    public class AppInfoRecord
    {
        public virtual int Id { get; set; }
        public virtual string BundleId { get; set; }
        public virtual string Title { get; set; }
        public virtual IList<AppReleaseRecord> AppReleases { get; set; }

        public AppInfoRecord()
        {
            this.AppReleases = new List<AppReleaseRecord>();
        }
    }
}