using System.Text.Json.Serialization;

namespace StrangeSoft.DotNetInstaller.Core.Models.Releases;

public record DownloadInformation
{
    [JsonPropertyName("version")] public required ExtendedVersion Version { get; set; }
    [JsonPropertyName("version-display")] public required string DisplayVersion { get; set; }
    [JsonPropertyName("files")] public required DownloadInformationFile[] Files { get; set; }
}