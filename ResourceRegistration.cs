using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Codesanook.AppRelease
{
    public class ResourceRegistration : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();
            //manifest.DefineScript("AngularJs")
            //    .SetUrl("angular.min.js", "angular.js")
            //    .SetVersion(angularVersion);
        }
    }
}