using System.Text.Json.Serialization;

namespace StrangeSoft.DotNetInstaller.Core.Models.Releases;

public record DownloadInformationFile
{
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("rid")] public string? RuntimeIdentifier { get; set; }
    [JsonPropertyName("url")] public required Uri Url { get; set; }
    [JsonPropertyName("hash")] public required string Hash { get; set; }
}