using Microsoft.Extensions.Logging;
using StrangeSoft.DotNetInstaller.Core.Models.Releases;
using StrangeSoft.DotNetInstaller.Core.Platform;
using StrangeSoft.DotNetInstaller.Core.Tools;

namespace StrangeSoft.DotNetInstaller.Core;

public class DotNetInstaller : IDotNetInstaller
{
    private readonly HttpClient _httpClient;
    private readonly IHashVerifier _hashVerifier;
    private readonly ILogger<DotNetInstaller> _logger;
    private readonly IPlatformPackageInstaller _installer;
    private readonly CommandLineOptions _options;

    public DotNetInstaller(
        HttpClient httpClient,
        IHashVerifier hashVerifier,
        IEnumerable<IPlatformPackageInstaller> installers,
        CommandLineOptions options,
        ILogger<DotNetInstaller> logger
    )
    {
        _httpClient = httpClient;
        _hashVerifier = hashVerifier;
        _logger = logger;
        var installer = installers.FirstOrDefault(i => i.Enabled);
        _installer = installer ?? throw new InvalidOperationException(
            "The current operating system is not supported. No installer module could be found."
        );
        _options = options;
    }

    private async Task<(string, bool)> TryGetCachedFile(
        DownloadInformationFile downloadInformation,
        CancellationToken cancellationToken
    )
    {
        var fileTempPath = GetDownloadFilePath(downloadInformation);
        if (_options.NoCache) return (fileTempPath, false);
        if (!File.Exists(fileTempPath)) return (fileTempPath, false);
        await using var existingFileStream = File.Open(fileTempPath, FileMode.Open, FileAccess.Read,
            FileShare.Read | FileShare.Write | FileShare.Delete);
        var valid = await _hashVerifier.IsHashValidAsync(existingFileStream, downloadInformation.Hash,
            cancellationToken);

        return (fileTempPath, valid);
    }

    private static string GetDownloadFilePath(DownloadInformationFile downloadInformation)
    {
        return Path.Combine(Path.GetTempPath(), Path.GetFileName(downloadInformation.Url.LocalPath));
    }

    public async Task<int> DownloadAndInstallAsync(DownloadInformationFile downloadInformation,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking for cached .NET installer with hash: {hash}", downloadInformation.Hash);
        var (fileTempPath, isCached) = await TryGetCachedFile(downloadInformation, cancellationToken);
        if (!isCached)
        {
            _logger.LogInformation("Downloading .NET SDK: {name} from {url}", Path.GetFileName(fileTempPath),
                downloadInformation.Url);
            await DownloadAndWriteFileAsync(downloadInformation.Url, fileTempPath, cancellationToken);
            (_, isCached) = await TryGetCachedFile(downloadInformation, cancellationToken);
            if (!isCached)
            {
                throw new InvalidOperationException($"Failed to verify hash for {fileTempPath}");
            }
        }
        else
        {
            _logger.LogInformation("Using cached .NET SDK Installer: {name}", Path.GetFileName(fileTempPath));
        }

        var returnCode = await InstallAsync(fileTempPath, cancellationToken);
        if (!_options.NoCache) return returnCode;

        _logger.LogInformation("Deleting {file} because no caching was requested", fileTempPath);
        File.Delete(fileTempPath);

        return returnCode;
    }

    private async Task<int> InstallAsync(string fileTempPath, CancellationToken cancellationToken)
    {
        return await _installer.InstallAsync(fileTempPath, false, cancellationToken);
    }

    private async Task DownloadAndWriteFileAsync(Uri url, string fileName, CancellationToken cancellationToken)
    {
        await using var httpStream = await _httpClient.GetStreamAsync(url, cancellationToken);
        await using var fileStream = File.Open(fileName, FileMode.Create);
        await httpStream.CopyToAsync(fileStream, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);
    }
}