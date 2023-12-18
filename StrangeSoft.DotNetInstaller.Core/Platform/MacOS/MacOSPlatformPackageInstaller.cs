using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StrangeSoft.DotNetInstaller.Core.Platform.MacOS;

public class MacOSPlatformPackageInstaller : IPlatformPackageInstaller
{
    public bool Enabled => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    public async Task<int> InstallAsync(string path, bool force, CancellationToken cancellationToken)
    {
        var processStartInformation = new ProcessStartInfo("installer")
        {
            ArgumentList =
            {
                "-store",
                "-pkg",
                path,
                "-target",
                "/"
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
