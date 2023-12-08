using StrangeSoft.DotNetInstaller.Core.Models.Releases;

namespace StrangeSoft.DotNetInstaller.Core.Scanner;

public interface IVersionCollector
{
    Task<IEnumerable<DownloadInformation>> GetInstallersAsync(DotNetChannelIndex index, DirectoryInfo basePath,
        CancellationToken cancellationToken);
}