using System.Text.Json;

namespace StrangeSoft.DotNetInstaller.Core.Serialization;

public class SourceGeneratedJsonSerializer : IJsonSerializer
{
    public async Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken) where T : class
    {
        return await JsonSerializer.DeserializeAsync(
            stream,
            typeof(T),
            DotNetMetadataJsonSerializerContext.Default,
            cancellationToken) as T;
    }
}