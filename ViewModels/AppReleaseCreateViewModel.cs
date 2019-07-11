using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Codesanook.AppRelease.ViewModels {
    public class AppReleaseCreateViewModel {
        [Required]
        public HttpPostedFileBase File { get; set; }

        [Required]
        public string VersionNumber { get; set; }

        [Required]
        public int AppInfoId { get; set; }
    }
}