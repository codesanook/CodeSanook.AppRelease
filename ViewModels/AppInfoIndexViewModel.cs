using CodeSanook.AppRelease.Models;
using CodeSanook.Configuration.Models;
using System.Collections.Generic;

namespace CodeSanook.AppRelease.ViewModels
{
    public class AppInfoIndexViewModel
    {
        public IList<AppInfoRecord> AppInfos { get; set; }
        public IList<AppReleaseRecord> AppReleases { get; set; }
        public int? SelectedAppInfoId { get; set; }
        public ModuleSettingPart Setting { get;  set; }

        public AppInfoIndexViewModel()
        {
            this.AppInfos = new List<AppInfoRecord>();
            this.AppReleases = new List<AppReleaseRecord>();
        }
    }
}