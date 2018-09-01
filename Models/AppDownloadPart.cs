using Orchard.ContentManagement;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CodeSanook.AppRelease.Models
{
    public class AppDownloadPart : ContentPart<AppDownloadPartRecord>, IValidatableObject
    {
        [Required]
        public virtual string PlayStoreUrl
        {
            get { return this.Record.PlayStoreUrl; }
            set { this.Record.PlayStoreUrl = value; }
        }

        //For enterprise app only
        public virtual bool IsEnterpriseApp
        {
            get { return this.Record.IsEnterpriseApp; }
            set { this.Record.IsEnterpriseApp = value; }
        }

        public string BundleId
        {
            get { return this.Record.BundleId; }
            set { this.Record.BundleId = value; }
        }

        public virtual string AppStoreUrl
        {
            get { return this.Record.AppStoreUrl; }
            set { this.Record.AppStoreUrl = value; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsEnterpriseApp && string.IsNullOrWhiteSpace(BundleId))
            {
                yield return new ValidationResult("BundleIs cannot be null when IsEnterpriseApp set to true");
            }

            if (!IsEnterpriseApp && string.IsNullOrWhiteSpace(this.AppStoreUrl))
            {
                yield return new ValidationResult("AppStoreUrl is required");
            }
        }
    }
}