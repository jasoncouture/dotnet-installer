using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace StrangeSoft.DotNetInstaller.Core.Models.Releases;

public record DotNetChannelIndex
{
    [JsonPropertyName("releases-index")]
    public required ImmutableArray<DotNetChannelReleaseIndexEntry> Releases { get; init; }
}