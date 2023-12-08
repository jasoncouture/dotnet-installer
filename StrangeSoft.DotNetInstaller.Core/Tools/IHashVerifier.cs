namespace StrangeSoft.DotNetInstaller.Core.Tools;

public interface IHashVerifier
{
    Task<bool> IsHashValidAsync(Stream stream, string expectedHash, CancellationToken cancellationToken);
}