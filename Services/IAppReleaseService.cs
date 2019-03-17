using CodeSanook.AppRelease.Models;
using Orchard;

namespace CodeSanook.AppRelease.Services
{
    public interface IAppReleaseService : IDependency
    {
        AppReleaseRecord GetLatestAppRelease(string bundleId);
        LatestAppReleaseInfo GetLatestAppReleaseInfo(string bundleId);
        string GetManifest(string bundleId);
    }
}
