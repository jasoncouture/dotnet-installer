using StrangeSoft.DotNetInstaller.Core.Models.Releases;

namespace StrangeSoft.DotNetInstaller.Core.Scanner;

public interface IVersionRequestMatcher
{
    DownloadInformation GetBestSdkDownloadForRequest(SdkVersionRequest request,
        IEnumerable<DownloadInformation> dotNetReleases);
}