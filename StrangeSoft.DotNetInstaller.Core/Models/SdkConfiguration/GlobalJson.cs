using System.Text.Json.Serialization;

namespace StrangeSoft.DotNetInstaller.Core.Models.SdkConfiguration;

public record GlobalJson
{
    [JsonPropertyName("sdk")]
    public GlobalJsonSdk? Sdk { get; init; }
}