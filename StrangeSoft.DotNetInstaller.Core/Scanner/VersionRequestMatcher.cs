using StrangeSoft.DotNetInstaller.Core.Models;
using StrangeSoft.DotNetInstaller.Core.Models.Releases;
using StrangeSoft.DotNetInstaller.Core.Models.SdkConfiguration;

namespace StrangeSoft.DotNetInstaller.Core.Scanner;

public class VersionRequestMatcher : IVersionRequestMatcher
{
    // SDK Versions are formatted as: x.y.znn
    // X - Major
    // Y - Minor
    // Z - Feature
    // nn - Patch
    public DownloadInformation GetBestSdkDownloadForRequest(SdkVersionRequest request,
        IEnumerable<DownloadInformation> dotNetReleases)
    {
        var orderedVersions =
            dotNetReleases
                .Where(i => request.AllowPreRelease || i.Version.Extra == null)
                .OrderByDescending(i => i.Version.Version)
                .ThenBy(i => i.Version.Extra ?? string.Empty);

        return request.RollForwardOption switch
        {
            RollForwardOption.Disabled => GetExactMatchOrThrow(request.Version, orderedVersions),
            RollForwardOption.Patch => GetNearestPatch(request.Version, orderedVersions),
            RollForwardOption.Feature => GetNearestFeature(request.Version, orderedVersions),
            RollForwardOption.Minor => GetNearestMinor(request.Version, orderedVersions),
            RollForwardOption.Major => GetNearestMajor(request.Version, orderedVersions),
            RollForwardOption.LatestPatch => GetLatestPatch(request.Version, orderedVersions),
            RollForwardOption.LatestFeature => GetLatestFeature(request.Version, orderedVersions),
            RollForwardOption.LatestMajor => GetLatestMajor(request.Version, orderedVersions),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private DownloadInformation GetLatestMajor(ExtendedVersion requestVersion,
        IOrderedEnumerable<DownloadInformation> orderedVersions)
    {
        return orderedVersions
                   .Where(i => i.Version.Version >= requestVersion.Version)
                   .MaxBy(i => i.Version)
               ?? throw new InvalidOperationException($"Unable to locate max version greater than {requestVersion}");
    }

    private DownloadInformation GetLatestFeature(ExtendedVersion requestVersion,
        IOrderedEnumerable<DownloadInformation> orderedVersions)
    {
        return orderedVersions
                   .Where(i => i.Version.Version.Major == requestVersion.Version.Major)
                   .Where(i => i.Version.Version.Minor == requestVersion.Version.Minor)
                   .Where(i => i.Version.Version >= requestVersion.Version)
                   .MaxBy(i => i.Version) ??
               throw new InvalidOperationException(
                   $"Could not satisfy version constraint latest feature for {requestVersion}");
    }

    private DownloadInformation GetLatestPatch(ExtendedVersion requestVersion,
        IOrderedEnumerable<DownloadInformation> orderedVersions)
    {
        return orderedVersions
                   .Where(i => i.Version.Version.Major == requestVersion.Version.Major &&
                               i.Version.Version.Minor == requestVersion.Version.Minor)
                   .Where(i => i.Version.Version >= requestVersion.Version)
                   .Where(i => i.Version.Version.Minor / 100 == requestVersion.Version.Minor / 100)
                   .MaxBy(i => i.Version)
               ?? throw new InvalidOperationException(
                   $"Could not satisfy version constraint latest patch for {requestVersion}");
    }

    private DownloadInformation GetNearestMajor(ExtendedVersion requestVersion,
        IOrderedEnumerable<DownloadInformation> orderedVersions)
    {
        return orderedVersions
                   .Where(i => i.Version.Version >= requestVersion.Version)
                   .MinBy(i => i.Version) ??
               throw new InvalidOperationException(
                   $"Could not satisfy version constraint feature for {requestVersion}");
    }

    private DownloadInformation GetNearestFeature(ExtendedVersion requestVersion,
        IOrderedEnumerable<DownloadInformation> orderedVersions)
    {
        return orderedVersions
                   .Where(i => i.Version.Version.Major == requestVersion.Version.Major)
                   .Where(i => i.Version.Version.Minor == requestVersion.Version.Minor)
                   .Where(i => i.Version.Version >= requestVersion.Version)
                   .MinBy(i => i.Version) ??
               throw new InvalidOperationException(
                   $"Could not satisfy version constraint feature for {requestVersion}");
    }

    private DownloadInformation GetNearestMinor(ExtendedVersion requestVersion,
        IOrderedEnumerable<DownloadInformation> orderedVersions)
    {
        return orderedVersions
                   .Where(i => i.Version.Version.Major == requestVersion.Version.Major)
                   .Where(i => i.Version.Version >= requestVersion.Version)
                   .MinBy(i => i.Version) ??
               throw new InvalidOperationException($"Could not satisfy version constraint minor for {requestVersion}");
    }

    private DownloadInformation GetNearestPatch(ExtendedVersion requestVersion,
        IOrderedEnumerable<DownloadInformation> orderedVersions)
    {
        return orderedVersions
                   .Where(i => i.Version.Version.Major == requestVersion.Version.Major &&
                               i.Version.Version.Minor == requestVersion.Version.Minor)
                   .Where(i => i.Version.Version >= requestVersion.Version)
                   .Where(i => i.Version.Version.Minor / 100 == requestVersion.Version.Minor / 100)
                   .MinBy(i => i.Version)
               ?? throw new InvalidOperationException(
                   $"Could not satisfy version constraint patch for {requestVersion}");
    }

    private DownloadInformation GetExactMatchOrThrow(ExtendedVersion requestVersion,
        IOrderedEnumerable<DownloadInformation> orderedVersions)
    {
        return orderedVersions.FirstOrDefault(i => i.Version == requestVersion) ??
               throw new InvalidOperationException($"Could not find .NET Sdk version {requestVersion}");
    }
}