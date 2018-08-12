using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeSanook.AppRelease.Models
{
    public class AppDownloadPart : ContentPart<AppDownloadPartRecord>
    {
        public string BundleId
        {
            get { return this.Record.BundleId; }
            set { this.Record.BundleId = value; }
        }
    }
}