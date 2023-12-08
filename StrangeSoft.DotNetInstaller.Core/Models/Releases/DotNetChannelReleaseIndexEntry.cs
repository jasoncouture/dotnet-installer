using System.Text.Json.Serialization;

namespace StrangeSoft.DotNetInstaller.Core.Models.Releases;

public record DotNetChannelReleaseIndexEntry
{
    [JsonPropertyName("channel-version")] public required Version ChannelVersion { get; init; }
    [JsonPropertyName("latest-release")] public required Version LatestRelease { get; init; }

    [JsonPropertyName("latest-release-date")]
    public required string LatestReleaseDate { get; init; }

    [JsonPropertyName("product")] public required string Product { get; init; }
    [JsonPropertyName("release-type")] public required string ReleaseType { get; init; }
    [JsonPropertyName("support-phase")] public required string SupportPhase { get; init; }
    [JsonPropertyName("releases.json")] public required Uri ReleasesJson { get; init; }
}