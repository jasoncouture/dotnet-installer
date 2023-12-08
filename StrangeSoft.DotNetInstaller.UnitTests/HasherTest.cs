using StrangeSoft.DotNetInstaller.Core;
using StrangeSoft.DotNetInstaller.Core.Tools;

namespace StrangeSoft.DotNetInstaller.UnitTests;

public class HasherTest
{
    [Fact]
    public async Task HashValidatesCorrectly()
    {
        const string expectedHash =
            "129b8af023c541986ac74367b63337363949c2307452246aa61ef05dd33969576db8fd5e515373f14da9bd23428037e60aa7d9000743dce6c75186de065e61e7";
        var hasher = new HashVerifier();
        await using var fileStream = File.OpenRead("dotnet-runtime-3.0.3-linux-arm.bintest");
        await hasher.IsHashValidAsync(fileStream, expectedHash, CancellationToken.None);
    }
}