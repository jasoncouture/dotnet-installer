#pragma warning disable CA1416 // Code only executes on linux
using System.Formats.Tar;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace StrangeSoft.DotNetInstaller.Core.Platform.Linux;

public class LinuxPlatformPackageInstaller(
    ILogger<LinuxPlatformPackageInstaller> logger,
    CommandLineOptions options
) : IPlatformPackageInstaller
{
    public bool Enabled => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    private const int BufferSize = 16384;
    private const FileShare DefaultFileShare = FileShare.ReadWrite | FileShare.Delete;
    private const string DefaultSystemInstallPath = "/usr/share/dotnet";
    private const string DefaultUserRelativeInstallPath = ".dotnet";

    private const UnixFileMode UserPathFileMode = UnixFileMode.UserRead | UnixFileMode.UserWrite |
                                                  UnixFileMode.UserExecute | UnixFileMode.GroupRead |
                                                  UnixFileMode.GroupExecute;

    private const UnixFileMode SystemPathFileMode =
        UserPathFileMode | UnixFileMode.OtherExecute | UnixFileMode.OtherRead;

    private string GetInstallPath()
    {
        if (options.InstallPath is not null)
        {
            return options.InstallPath.FullName;
        }

        if (options.UserInstall)
        {
            var home = Environment.GetEnvironmentVariable("HOME") ??
                       throw new InvalidOperationException(
                           "Unable to determine user home directory. Please specify an install path");
            var userDirectory =
                Directory.CreateDirectory(Path.Combine(home, DefaultUserRelativeInstallPath), UserPathFileMode);
            userDirectory.UnixFileMode = UserPathFileMode;

            return userDirectory.FullName;
        }

        var systemDirectory = Directory.CreateDirectory(DefaultSystemInstallPath, SystemPathFileMode);
        systemDirectory.UnixFileMode = SystemPathFileMode;
        return systemDirectory.FullName;
    }

    public async Task<int> InstallAsync(string path, bool force, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(path);

        if (extension == ".gz" || extension == ".tgz")
        {
            await using var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, DefaultFileShare);
            path = await DecompressAsync(fileStream, cancellationToken);
        }
        else if (extension != ".tar")
        {
            throw new InvalidOperationException($"Unknown file extension: {extension}");
        }

        await using var tarStream = File.Open(path, FileMode.Open, FileAccess.Read, DefaultFileShare);
        return await UnpackAsync(tarStream, GetInstallPath(), force, cancellationToken);
    }

    private async ValueTask<string> DecompressAsync(Stream inputStream, CancellationToken cancellationToken)
    {
        var tempFile = Path.GetTempFileName();
        await using var tempFileStream = File.Open(tempFile, FileMode.Create, FileAccess.Write,
            FileShare.ReadWrite | FileShare.Delete);
        await DecompressAsync(inputStream, tempFileStream, cancellationToken);
        return tempFile;
    }

    private async ValueTask DecompressAsync(Stream inputStream, Stream outputStream,
        CancellationToken cancellationToken)
    {
        await using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress, false);
        await gzipStream.CopyToAsync(outputStream, BufferSize, cancellationToken);
        await outputStream.FlushAsync(cancellationToken);
    }

    private async Task<int> UnpackAsync(Stream stream, string installPath, bool force,
        CancellationToken cancellationToken)
    {
        await using var tarReader = new TarReader(stream);
        while (true)
        {
            var entry = await tarReader.GetNextEntryAsync(cancellationToken: cancellationToken);
            if (entry is null) break;
            if (entry.EntryType is TarEntryType.GlobalExtendedAttributes)
                continue;
            var target = Path.GetFullPath(Path.Combine(installPath, entry.Name));

            if (ShouldSkipEntry(force, entry, target))
                continue;
            if (entry.EntryType is not TarEntryType.Directory)
            {
                logger.LogInformation("Extracting: {path}, size: {size}, permissions: {permissions:F}", target,
                    entry.Length, entry.Mode);
                await entry.ExtractToFileAsync(target, true, cancellationToken);
            }
            else
            {
                logger.LogInformation("Creating directory {path} with permissions {permissions:F}", target, entry.Mode);
                Directory.CreateDirectory(target, entry.Mode);
            }
        }

        try
        {
            CreateSymlink(installPath, options.UserInstall);
        }
        catch (IOException ex)
        {
            logger.LogWarning(ex, "Failed to create symlink, an IO Exception occurred");
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Failed to create symlink, access is denied");
        }

        return 0;
    }

    private void CreateSymlink(string installPath, bool userInstall)
    {
        var targetPath = Path.Combine(GetTargetPath(userInstall), "dotnet");
        if (File.Exists(targetPath))
        {
            logger.LogInformation("Skipping creating symlink {path}, file exists", targetPath);
            return;
        }
        var installedPath = Path.Combine(installPath, "dotnet");
        logger.LogInformation("Creating symlink {systemPath} -> {installedPath}", targetPath, installedPath);
        File.CreateSymbolicLink(targetPath, installedPath);
    }

    const string SystemInstallPath = "/usr/local/bin";

    private string GetTargetPath(bool userInstall)
    {
        if (!userInstall)
        {
            if (!Directory.Exists(SystemInstallPath))
            {
                Directory.CreateDirectory(
                    SystemInstallPath,
                    UnixFileMode.UserWrite | UnixFileMode.UserRead | UnixFileMode.UserExecute |
                    UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                    UnixFileMode.OtherRead | UnixFileMode.OtherExecute
                );
            }

            return SystemInstallPath;
        }

        var targetPath = Path.GetFullPath("bin", Environment.GetEnvironmentVariable("HOME")!);
        if (Directory.Exists(targetPath)) return targetPath;

        targetPath = Path.GetFullPath(".local/bin", Environment.GetEnvironmentVariable("HOME")!);
        if (Directory.Exists(targetPath)) return targetPath;

        logger.LogWarning("Neither ~/.local/bin nor ~/bin could be found, assuming we should use, and creating: {path}",
            targetPath);
        Directory.CreateDirectory(targetPath,
            UnixFileMode.UserExecute | UnixFileMode.UserRead | UnixFileMode.UserWrite |
            UnixFileMode.GroupRead | UnixFileMode.GroupExecute);

        return targetPath;
    }

    private bool ShouldSkipEntry(bool force, TarEntry entry, string target)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (entry.EntryType)
        {
            case TarEntryType.Directory:
                if (Directory.Exists(target))
                    return true;
                break;
            case TarEntryType.RegularFile:
            case TarEntryType.ContiguousFile:
            case TarEntryType.SparseFile:
            case TarEntryType.V7RegularFile:
                var lastModified = new DateTimeOffset(File.GetLastWriteTimeUtc(target), TimeSpan.Zero);
                if (force) return false;

                var fileInfo = new FileInfo(target);
                if (entry.ModificationTime == lastModified && entry.Length == fileInfo.Length)
                {
                    logger.LogInformation("Skipping {path}, file size and modification times match", target);
                    return true;
                }

                if (entry.ModificationTime < lastModified)
                {
                    logger.LogInformation(
                        "Skipping {path}, system modification time is more recent than the archive file", target);
                    return true;
                }

                break;
        }

        return false;
    }
}