namespace StrangeSoft.DotNetInstaller.Core.Serialization;

public interface IJsonSerializer
{
    Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken) where T : class;
}