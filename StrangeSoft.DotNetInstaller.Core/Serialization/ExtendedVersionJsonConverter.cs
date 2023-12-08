using System.Text.Json;
using System.Text.Json.Serialization;
using StrangeSoft.DotNetInstaller.Core.Models;

namespace StrangeSoft.DotNetInstaller.Core.Serialization;

public class ExtendedVersionJsonConverter : JsonConverter<ExtendedVersion>
{
    public override ExtendedVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => ExtendedVersion.FromVersionString(reader.GetString());

    public override void Write(Utf8JsonWriter writer, ExtendedVersion value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}