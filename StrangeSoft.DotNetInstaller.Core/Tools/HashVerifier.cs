using System.Security.Cryptography;

namespace StrangeSoft.DotNetInstaller.Core.Tools;

public class HashVerifier : IHashVerifier
{
    public async Task<bool> IsHashValidAsync(Stream stream, string expectedHash, CancellationToken cancellationToken)
    {
        using var sha512 = SHA512.Create();
        var hashBytes = await sha512.ComputeHashAsync(stream, cancellationToken);
        var hashString = BitConverter.ToString(hashBytes).Replace("-", "");
        return string.Equals(expectedHash, hashString, StringComparison.OrdinalIgnoreCase);
    }
}