using Microsoft.Extensions.Logging;
using StrangeSoft.DotNetInstaller.Core.Models;
using StrangeSoft.DotNetInstaller.Core.Models.SdkConfiguration;
using StrangeSoft.DotNetInstaller.Core.Serialization;

namespace StrangeSoft.DotNetInstaller.Core.Scanner;

public class GlobalJsonVersionScanner(IJsonSerializer serializer, ILogger<GlobalJsonVersionScanner> logger)
    : IVersionScanner
{
    public IEnumerable<string> GetGlobbingPatterns()
    {
        return new[] { "**/global.json", "**/sdk.json" };
    }

    // The logic implemented here, follows the documentation found at https://learn.microsoft.com/en-us/dotnet/core/tools/global-json#matching-rules
    public IEnumerable<SdkVersionRequest> ScanForVersions(IEnumerable<string> files)
    {
        foreach (var match in files)
        {
            GlobalJsonSdk? sdk;
            using var fileStream =
                File.Open(match, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            try
            {
                var globalJson = serializer.DeserializeAsync<GlobalJson>(fileStream, CancellationToken.None)
                    .GetAwaiter().GetResult();
                sdk = globalJson?.Sdk;
                if (sdk is null)
                    continue;
                if (sdk.Version is null && sdk.RollForward != RollForwardOption.LatestMajor)
                {
                    logger.LogWarning(
                        "{path} is not a valid global.json, it does not specify a version, and the rollForward option is not set to LatestMajor",
                        match);
                    continue;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process {file}, an exception occurred", match);
                continue;
            }

            yield return new SdkVersionRequest(
                sdk.Version ?? new ExtendedVersion(new Version(1, 0, 0)),
                sdk.RollForward ?? (
                    sdk.AllowPreRelease == true ? RollForwardOption.LatestMajor : RollForwardOption.LatestPatch
                ),
                sdk.AllowPreRelease ?? false
            );
        }
    }
}