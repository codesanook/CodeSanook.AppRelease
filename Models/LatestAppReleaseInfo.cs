namespace Codesanook.AppRelease.Models {
    public class LatestAppReleaseInfo {
        public const string CacheKey = nameof(LatestAppReleaseInfo);
        public int? IOsVersionCode { get; set; }
        public int? AndroidVersionCode { get; set; }
    }
}
