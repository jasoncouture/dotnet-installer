using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace StrangeSoft.DotNetInstaller.Core.Models.Releases;

public record DotNetChannel
{
    [JsonPropertyName("channel-version")] public required Version ChannelVersion { get; set; }
    [JsonPropertyName("latest-release")] public required ExtendedVersion LatestRelease { get; set; }

    [JsonPropertyName("latest-release-date")]
    public required DateOnly LatestReleaseDate { get; set; }

    [JsonPropertyName("latest-runtime")] public required ExtendedVersion LatestRuntime { get; set; }
    [JsonPropertyName("latest-sdk")] public required ExtendedVersion LatestSdk { get; set; }
    [JsonPropertyName("release-type")] public required string ReleaseType { get; set; }
    [JsonPropertyName("support-phase")] public required string SupportPhase { get; set; }
    [JsonPropertyName("eol-date")] public required DateOnly EolDate { get; set; }
    [JsonPropertyName("lifecycle-policy")] public required string LifecyclePolicy { get; set; }
    [JsonPropertyName("releases")] public required ImmutableArray<DotNetReleaseMetadata> Releases { get; set; }
}