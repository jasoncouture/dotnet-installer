using System.Diagnostics;

namespace StrangeSoft.DotNetInstaller.Core.Platform.Windows;

public class WindowsPlatformPackageInstaller : IPlatformPackageInstaller
{
    public bool Enabled => OperatingSystem.IsWindows();

    public async Task<int> InstallAsync(string path, bool force, CancellationToken cancellationToken)
    {
        var processStartInformation = new ProcessStartInfo(path)
        {
            ArgumentList =
            {
                "/install",
                "/quiet",
                "/norestart",
                "/log",
                Path.ChangeExtension(path, ".log")
            },
            CreateNoWindow = true
        };

        var process = new Process();
        process.StartInfo = processStartInformation;
        process.Start();
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }
}