using System.Text.Json.Serialization;

namespace StrangeSoft.DotNetInstaller.Core.Models.SdkConfiguration;

public record GlobalJsonSdk
{
    [JsonPropertyName("version")]
    public ExtendedVersion? Version { get; init; }
    [JsonPropertyName("allowPrerelease")]
    public bool? AllowPreRelease { get; init; }
    [JsonPropertyName("rollForward"), JsonConverter(typeof(JsonStringEnumConverter<RollForwardOption>))]
    public RollForwardOption? RollForward { get; init; }
}