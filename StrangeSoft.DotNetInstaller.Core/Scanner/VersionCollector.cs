using System.Collections.Immutable;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using StrangeSoft.DotNetInstaller.Core.Models.Releases;
using StrangeSoft.DotNetInstaller.Core.Tools;

namespace StrangeSoft.DotNetInstaller.Core.Scanner;

public class VersionCollector(
    IEnumerable<IVersionScanner> scanners,
    ISdkVersionLoader sdkVersionLoader,
    IVersionRequestMatcher versionRequestMatcher,
    ILogger<VersionCollector> logger)
    : IVersionCollector
{
    public async Task<IEnumerable<DownloadInformation>> GetInstallersAsync(DotNetChannelIndex index,
        DirectoryInfo basePath,
        CancellationToken cancellationToken)
    {
        var filesystemMatcher = new Matcher();
        var allGlobs = scanners.SelectMany(i => i.GetGlobbingPatterns()).ToImmutableArray();
        logger.LogInformation("Scanning {path} with the following glob patterns: {globs}", basePath,
            string.Join(", ", allGlobs.Select(i => $"'{i}'")));
        filesystemMatcher.AddIncludePatterns(allGlobs);
        filesystemMatcher
            .AddExclude("**/.*") // Ignore all dot files/folders
            .AddExclude("**/bin")
            .AddExclude("**/obj");

        var allFiles = filesystemMatcher.GetResultsInFullPath(basePath.FullName).ToArray();
        logger.LogInformation("Found {count} file(s) to check", allFiles.Length);
        var inMemoryDirectory = new InMemoryDirectoryInfo(basePath.FullName, allFiles);
        var versionRequests = new List<SdkVersionRequest>();
        foreach (var scanner in scanners)
        {
            var scannerMatcher = new Matcher();
            scannerMatcher.AddIncludePatterns(scanner.GetGlobbingPatterns());
            var scannerMatcherResults = scannerMatcher.Execute(inMemoryDirectory);
            versionRequests.AddRange(
                scanner.ScanForVersions(
                    scannerMatcherResults.Files.Select(i => Path.Combine(basePath.FullName, i.Path))));
        }


        var availableSdkVersions = new List<DownloadInformation>();
        var channelVersions = versionRequests.Select(i => new Version(i.Version.Version.Major, i.Version.Version.Minor))
            .Distinct();
        foreach (var channelVersion in channelVersions)
        {
            var dotNetChannel = await sdkVersionLoader.GetChannelAsync(index, channelVersion, cancellationToken);
            if (dotNetChannel is null)
                throw new InvalidOperationException(
                    $"Unable to find .NET Release channel {channelVersion.ToString(2)}");
            availableSdkVersions.AddRange(dotNetChannel.Releases.Select(i => i.Sdk).Where(i => i is not null)!);
        }

        foreach (var request in versionRequests)
        {
            logger.LogDebug("Got version request: {request}", request);
        }

        var selectedDownloads = versionRequests.Select(request =>
            versionRequestMatcher.GetBestSdkDownloadForRequest(request, availableSdkVersions)).ToList();
        var returnValue = selectedDownloads.DistinctBy(i => i.DisplayVersion).ToImmutableArray();

        logger.LogInformation("Will download the following SDKs: {sdkList}", string.Join(", ", returnValue.Select(i => i.DisplayVersion)));

        return returnValue;
    }

    private IEnumerable<DotNetReleaseMetadata> GetDotNetVersionsFromChannel(Version version,
        IEnumerable<DotNetChannel> channels)
    {
        var channel = channels.SingleOrDefault(
            i => i.ChannelVersion.Major == version.Major && i.ChannelVersion.Minor == version.Minor);

        if (channel is null)
            throw new InvalidOperationException($"Unable to find channel {version.ToString(2)}");

        return channel.Releases;
    }
}