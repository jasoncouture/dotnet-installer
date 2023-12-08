using System.Runtime.InteropServices;
using StrangeSoft.DotNetInstaller.Core.Models.Releases;

namespace StrangeSoft.DotNetInstaller.Core;

public interface IDotNetInstaller
{
    public Task<int> DownloadAndInstallAsync(DownloadInformationFile downloadInformation,
        CancellationToken cancellationToken);
}