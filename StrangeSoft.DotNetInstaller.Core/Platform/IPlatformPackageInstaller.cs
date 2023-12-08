namespace StrangeSoft.DotNetInstaller.Core.Platform;

public interface IPlatformPackageInstaller
{
    bool Enabled { get; }
    Task<int> InstallAsync(string path, bool force, CancellationToken cancellationToken);
}