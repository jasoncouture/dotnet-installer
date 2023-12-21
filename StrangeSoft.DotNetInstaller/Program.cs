using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Principal;
using StrangeSoft.DotNetInstaller.Core;

namespace StrangeSoft.DotNetInstaller;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var basePathOption = new Argument<DirectoryInfo>("base-path", "Path to scan to determine .NET SDK versions to install.");
        var downloadUrlOption = new Option<Uri>("--download-url", parseArgument: ParseUri!, description: "Set the dotnet manifest JSON URL, You shouldn't need to change this.");
        var forceOption = new Option<bool>("--force", "Force installation, even if administrative privileges do not appear to be present.");
        var noCacheOption = new Option<bool>("--no-cache", "Disable caching, and always download the latest installer from Microsoft.");

        forceOption.AddAlias("-f");

        basePathOption.SetDefaultValue(new DirectoryInfo("."));
        downloadUrlOption.SetDefaultValue(
            new Uri("https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json"));
        forceOption.SetDefaultValue(false);
        noCacheOption.SetDefaultValue(false);

        var rootCommand = new RootCommand(".NET SDK Install tool");
        rootCommand.AddOption(downloadUrlOption);
        rootCommand.AddOption(forceOption);
        rootCommand.AddArgument(basePathOption);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var installLocationOption = new Option<DirectoryInfo?>("--install-path", "Path to install .NET SDKs to. Defaults to /usr/share/dotnet for system installation, and ~/.dotnet for user installations.");
            var userInstallOption = new Option<bool>("--user-install", "Install SDK for the current user only. Implies --force.");

            userInstallOption.SetDefaultValue(false);
            installLocationOption.SetDefaultValue(null);

            rootCommand.AddOption(installLocationOption);
            rootCommand.AddOption(userInstallOption);

            rootCommand.SetHandler(StartAppLinux, downloadUrlOption, basePathOption, noCacheOption, installLocationOption,
                userInstallOption, forceOption);
        }
        else
        {
            // Windows and MacOS
            rootCommand.SetHandler(StartApp, downloadUrlOption, basePathOption, noCacheOption, forceOption);
        }

        return await rootCommand.InvokeAsync(args);
    }

    private static Uri? ParseUri(ArgumentResult result)
    {
        var url = result.GetValueOrDefault<string>();
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri;
        }

        result.ErrorMessage = $"'{url}' is not a valid URL";
        return null;
    }


    public static async Task<int> StartAppLinux(Uri downloadUrl, DirectoryInfo basePath, bool noCache, DirectoryInfo? installLocation,
        bool userInstall, bool force)
    {
        return await StartWithOptions(
            new CommandLineOptions(downloadUrl, basePath, noCache, installLocation, userInstall),
            userInstall || force
        );
    }

    private static async ValueTask<int> StartWithOptions(CommandLineOptions options, bool force)
    {
        // ReSharper disable once InvertIf
        if (!force && !IsAdministrator())
        {
            Console.WriteLine("Administrative privileges are required, but you don't appear to have them");
            Console.WriteLine("If you'd like to try anyway, please use the force option (--force/-f)");
            return 255;
        }

        return await Startup.RunAsync(options, CancellationToken.None);
    }

    public static async Task<int> StartApp(
        Uri downloadUrl,
        DirectoryInfo basePath,
        bool noCache,
        bool force
    ) =>
        await StartWithOptions(new CommandLineOptions(downloadUrl, basePath, noCache), force);

    private static bool IsAdministrator()
    {
        if (!OperatingSystem.IsWindows())
            return string.Equals(Environment.UserName, "root", StringComparison.OrdinalIgnoreCase);

        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}