using Codesanook.AppRelease.Models;
using Orchard;

namespace Codesanook.AppRelease.Services {
    public interface IAppReleaseService : IDependency {
        AppReleaseRecord GetLatestAppRelease(string bundleId);
        LatestAppReleaseInfo GetLatestAppReleaseInfo(string bundleId);
        string GetManifest(string bundleId);
    }
}
