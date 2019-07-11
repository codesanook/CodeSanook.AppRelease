using Orchard.ContentManagement;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Codesanook.AppRelease.Models {
    public class AppDownloadPart : ContentPart<AppDownloadPartRecord>, IValidatableObject {
        [Required]
        public virtual string PlayStoreUrl {
            get => Record.PlayStoreUrl;
            set => Record.PlayStoreUrl = value;
        }

        //For enterprise APP only
        public virtual bool IsEnterpriseApp {
            get => Record.IsEnterpriseApp;
            set => Record.IsEnterpriseApp = value;
        }

        public string BundleId {
            get => this.Record.BundleId;
            set => Record.BundleId = value;
        }

        public virtual string AppStoreUrl {
            get => Record.AppStoreUrl;
            set => Record.AppStoreUrl = value;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            if (IsEnterpriseApp && string.IsNullOrWhiteSpace(BundleId)) {
                yield return new ValidationResult("BundleIs cannot be null when IsEnterpriseApp set to true");
            }

            if (!IsEnterpriseApp && string.IsNullOrWhiteSpace(this.AppStoreUrl)) {
                yield return new ValidationResult("AppStoreUrl is required");
            }
        }
    }
}