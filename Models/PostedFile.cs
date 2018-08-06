using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeSanook.AppRelease.Models
{
    public class PostedFile
    {
        public HttpPostedFileBase File { get; set; }
        public string VersionNumber { get; set; }
        public int VersionCode { get; set; }
    }
}