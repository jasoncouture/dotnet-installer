using StrangeSoft.DotNetInstaller.Core.Models;
using StrangeSoft.DotNetInstaller.Core.Models.Releases;
using StrangeSoft.DotNetInstaller.Core.Models.SdkConfiguration;
using StrangeSoft.DotNetInstaller.Core.Scanner;

namespace StrangeSoft.DotNetInstaller.UnitTests;

public class VersionSelectorTests
{
    private readonly VersionRequestMatcher _versionRequestMatcher = new VersionRequestMatcher();

    private readonly DownloadInformation[] _availableVersions = new DownloadInformation[]
    {
        new()
        {
            Files = Array.Empty<DownloadInformationFile>(),
            DisplayVersion = string.Empty,
            Version = new ExtendedVersion(new Version(8, 0, 100))
        },
        new()
        {
            Files = Array.Empty<DownloadInformationFile>(),
            DisplayVersion = string.Empty,
            Version = new ExtendedVersion(new Version(8, 0, 101))
        },
        new()
        {
            Files = Array.Empty<DownloadInformationFile>(),
            DisplayVersion = string.Empty,
            Version = new ExtendedVersion(new Version(8, 0, 102), "pre")
        },
        new()
        {
            Files = Array.Empty<DownloadInformationFile>(),
            DisplayVersion = string.Empty,
            Version = new ExtendedVersion(new Version(7, 0, 100))
        },
        new()
        {
            Files = Array.Empty<DownloadInformationFile>(),
            DisplayVersion = string.Empty,
            Version = new ExtendedVersion(new Version(7, 0, 101))
        },
        new()
        {
            Files = Array.Empty<DownloadInformationFile>(),
            DisplayVersion = string.Empty,
            Version = new ExtendedVersion(new Version(6, 0, 100))
        },
        new()
        {
            Files = Array.Empty<DownloadInformationFile>(),
            DisplayVersion = string.Empty,
            Version = new ExtendedVersion(new Version(6, 0, 101))
        },
    };

    [Theory]
    [MemberData(nameof(GetVersionSelectionTestCases))]
    public void SelectsExactVersion(SdkVersionRequest request, ExtendedVersion expectedVersion)
    {
        var match = _versionRequestMatcher.GetBestSdkDownloadForRequest(request, _availableVersions);

        Assert.Equivalent(expectedVersion, match.Version);
    }

    [Theory]
    [MemberData(nameof(GetVersionSelectionTestCases))]
    public void ThrowsExceptionWhenReleaseCantBeFound(SdkVersionRequest request, ExtendedVersion _)
    {
        Assert.ThrowsAny<Exception>(() => _versionRequestMatcher.GetBestSdkDownloadForRequest(request,
            new DownloadInformation[]
            {
                new DownloadInformation()
                {
                    Files = Array.Empty<DownloadInformationFile>(),
                    DisplayVersion = "",
                    Version = new ExtendedVersion(new Version(1, 0))
                }
            }));
    }

    public static TheoryData<SdkVersionRequest, ExtendedVersion> GetVersionSelectionTestCases()
    {
        var theoryData = new TheoryData<SdkVersionRequest, ExtendedVersion>();
        theoryData.Add(
            new SdkVersionRequest(new ExtendedVersion(new Version(6, 0, 100)), RollForwardOption.Disabled),
            new ExtendedVersion(new Version(6, 0, 100))
        );

        theoryData.Add(
            new SdkVersionRequest(new ExtendedVersion(new Version(6, 0, 100)), RollForwardOption.LatestPatch),
            new ExtendedVersion(new Version(6, 0, 101))
        );

        theoryData.Add(
            new SdkVersionRequest(new ExtendedVersion(new Version(8, 0, 100)), RollForwardOption.LatestPatch),
            new ExtendedVersion(new Version(8, 0, 101))
        );

        theoryData.Add(
            new SdkVersionRequest(new ExtendedVersion(new Version(8, 0, 100)), RollForwardOption.LatestPatch,
                AllowPreRelease: true),
            new ExtendedVersion(new Version(8, 0, 102), "pre")
        );

        return theoryData;
    }
}