using Codesanook.AppRelease.Models;
using System.Collections.Generic;
using Codesanook.AmazonS3.Models;

namespace Codesanook.AppRelease.ViewModels {
    public class AppInfoIndexViewModel {
        public IList<AppInfoRecord> AppInfos { get; set; }
        public IList<AppReleaseRecord> AppReleases { get; set; }
        public int? SelectedAppInfoId { get; set; }
        public AwsS3SettingPart Setting { get; set; }

        public AppInfoIndexViewModel() {
            AppInfos = new List<AppInfoRecord>();
            AppReleases = new List<AppReleaseRecord>();
        }
    }
}