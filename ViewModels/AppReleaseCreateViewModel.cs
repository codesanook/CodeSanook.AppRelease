using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CodeSanook.AppRelease.ViewModels
{
    public class AppReleaseCreateViewModel
    {
        [Required]
        public HttpPostedFileBase File { get; set; }

        [Required]
        public string VersionNumber { get; set; }

        [Required]
        public int AppInfoId { get; set; }
    }
}