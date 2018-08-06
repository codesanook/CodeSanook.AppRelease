using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeSanook.AppRelease.Models
{
    public class AppReleaseRecord
    {
        public virtual int Id { get; set; }
        public virtual string VersionNumber { get; set; }
        public virtual int VersionCode { get; set; }
        public virtual string FileKey { get; set; }
        public virtual DateTime? CreatedUtc { get; set; }
    }
}