using System.Text.Json.Serialization;
using StrangeSoft.DotNetInstaller.Core.Models.Releases;
using StrangeSoft.DotNetInstaller.Core.Models.SdkConfiguration;

namespace StrangeSoft.DotNetInstaller.Core.Serialization;

[JsonSerializable(typeof(DotNetChannelIndex), GenerationMode = GenerationMode)]
[JsonSerializable(typeof(DotNetChannel), GenerationMode = GenerationMode)]
[JsonSerializable(typeof(GlobalJson), GenerationMode = GenerationMode)]
public partial class DotNetMetadataJsonSerializerContext : JsonSerializerContext
{
    private const JsonSourceGenerationMode GenerationMode =
        JsonSourceGenerationMode.Metadata;
}