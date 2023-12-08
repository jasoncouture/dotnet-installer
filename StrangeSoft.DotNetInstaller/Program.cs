using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using StrangeSoft.DotNetInstaller.Core;

namespace StrangeSoft.DotNetInstaller;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        
        var basePathOption = new Argument<DirectoryInfo>("base-path");
        var downloadUrlOption = new Option<Uri>("--download-url", parseArgument: ParseUri!);
        var forceOption = new Option<bool>("--force");
        
        forceOption.AddAlias("-f");
        
        basePathOption.SetDefaultValue(new DirectoryInfo("."));
        downloadUrlOption.SetDefaultValue(
            new Uri("https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json"));
        forceOption.SetDefaultValue(false);
        
        var rootCommand = new RootCommand(".NET SDK Install tool");
        rootCommand.AddOption(downloadUrlOption);
        rootCommand.AddOption(forceOption);
        rootCommand.AddArgument(basePathOption);
        rootCommand.SetHandler(StartApp, downloadUrlOption, basePathOption, forceOption);
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


    public static async Task<int> StartApp(
        Uri downloadUrl,
        DirectoryInfo basePath,
        bool force
    )
    {
        // ReSharper disable once InvertIf
        if (!force && !IsAdministrator())
        {
            Console.WriteLine("Administrative privileges are required, but you don't appear to have them");
            Console.WriteLine("If you'd like to try anyway, please use the force option (--force/-f)");
            return 255;
        }

        return await Startup.RunAsync(new CommandLineOptions(downloadUrl, basePath));
    }

    private static bool IsAdministrator()
    {
        if (!OperatingSystem.IsWindows())
            return string.Equals(Environment.UserName, "root", StringComparison.OrdinalIgnoreCase);

        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}