using System.Text.Json.Serialization;

namespace StrangeSoft.DotNetInstaller.Core.Models.Releases;

public record DotNetReleaseMetadata
{
    [JsonPropertyName("release-date")] public required string ReleaseDate { get; set; }
    [JsonPropertyName("release-version")] public required ExtendedVersion ReleaseVersion { get; set; }
    [JsonPropertyName("security")] public required bool Security { get; set; }
    [JsonPropertyName("release-notes")] public Uri? ReleaseNotes { get; set; }
    [JsonPropertyName("runtime")] public DownloadInformation? Runtime { get; set; }
    [JsonPropertyName("sdk")] public DownloadInformation? Sdk { get; set; }
}