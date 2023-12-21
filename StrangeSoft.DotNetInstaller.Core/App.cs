using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using StrangeSoft.DotNetInstaller.Core.Platform;
using StrangeSoft.DotNetInstaller.Core.Scanner;
using StrangeSoft.DotNetInstaller.Core.Tools;

namespace StrangeSoft.DotNetInstaller.Core;

public class App(
    ISdkVersionLoader versionLoader,
    CommandLineOptions options,
    IVersionCollector versionCollector,
    ILogger<App> logger,
    IDotNetInstaller installer,
    IRuntimeIdentifierSelector runtimeIdentifierSelector
    ) : IApp
{
    public async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching .NET Release channels");
        var index = await versionLoader.GetDotNetChannels(cancellationToken);
        logger.LogInformation("Scanning \"{path}\" for .NET versions to install", options.BasePath.FullName);
        var versions = await versionCollector.GetInstallersAsync(index, options.BasePath, cancellationToken);
        versions = versions.DistinctBy(i => i.DisplayVersion);
        foreach (var version in versions)
        {
            logger.LogInformation("Downloading and installing .NET SDK {version}", version.DisplayVersion);
            var selectedFile = version.Files.FirstOrDefault(
                i => runtimeIdentifierSelector.IsRuntimeIdentifierSelected(i.RuntimeIdentifier));
            if (selectedFile is null)
            {
                logger.LogWarning("Unable to download and install {version}, no matching runtime identifier for this system was found.", version.DisplayVersion);
                continue;
            }

            var exitCode = await installer.DownloadAndInstallAsync(selectedFile, cancellationToken);
            if (exitCode == 0)
            {
                logger.LogInformation(".NET SDK {version} appears to have installed successfully", version.DisplayVersion);
            }
            else
            {
                logger.LogWarning(".NET SDK {version} appears to have failed to install with exit code {exitCode}", version.DisplayVersion, exitCode);
            }
        }

        var foregroundColor = Console.ForegroundColor;
        Console.Write('[');
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("REMINDER");
        Console.ForegroundColor = foregroundColor;
        Console.Write("] If you downloaded this tool manually, don't forget to install it as a .NET tool, IE: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("dotnet tool install --global StrangeSoft.DotNetInstaller");
        Console.ForegroundColor = foregroundColor;
        Console.WriteLine();
        Console.WriteLine("You may need to restart your shell for the installation to be available");
        return 0;
    }
}