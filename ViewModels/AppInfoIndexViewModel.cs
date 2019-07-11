using Codesanook.AppRelease.Models;
using Codesanook.Configuration.Models;
using System.Collections.Generic;

namespace Codesanook.AppRelease.ViewModels {
    public class AppInfoIndexViewModel {
        public IList<AppInfoRecord> AppInfos { get; set; }
        public IList<AppReleaseRecord> AppReleases { get; set; }
        public int? SelectedAppInfoId { get; set; }
        public ModuleSettingPart Setting { get; set; }

        public AppInfoIndexViewModel() {
            AppInfos = new List<AppInfoRecord>();
            AppReleases = new List<AppReleaseRecord>();
        }
    }
}