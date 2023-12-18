namespace StrangeSoft.DotNetInstaller.Core;

public record CommandLineOptions(Uri DownloadUri, DirectoryInfo BasePath, bool NoCache, DirectoryInfo? InstallPath = null, bool UserInstall = false);