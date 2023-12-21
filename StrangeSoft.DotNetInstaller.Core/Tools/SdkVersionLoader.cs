using System.Text.Json;
using Microsoft.Extensions.Logging;
using StrangeSoft.DotNetInstaller.Core.Models.Releases;
using StrangeSoft.DotNetInstaller.Core.Serialization;

namespace StrangeSoft.DotNetInstaller.Core.Tools;

public class SdkVersionLoader(
    HttpClient httpClient,
    IJsonSerializer jsonSerializer,
    ILogger<SdkVersionLoader> logger,
    CommandLineOptions options)
    : ISdkVersionLoader
{
    public async Task<DotNetChannelIndex> GetDotNetChannels(CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching .NET Release channels from {uri}", options.DownloadUri);
        await using var stream = await httpClient.GetStreamAsync(options.DownloadUri, cancellationToken);
        
        var result = await jsonSerializer.DeserializeAsync<DotNetChannelIndex>(stream, cancellationToken) ?? throw new InvalidOperationException();
        
        logger.LogInformation("Found {count} channels. Available .NET Release channels: {channelList}", result.Releases.Length, string.Join(", ", result.Releases.Select(i => i.ChannelVersion.ToString(2))));

        return result;
    }
    
    public async ValueTask<DotNetChannel?> GetChannelAsync(DotNetChannelIndex index, Version version, CancellationToken cancellationToken)
    {
        var (result, channel) = await TryGetCachedChannelAsync(version, cancellationToken);
        if (result) 
            return channel;
        var indexEntry = index.Releases.FirstOrDefault(i =>
            i.ChannelVersion.Major == version.Major && i.ChannelVersion.Minor == version.Minor);
        if (indexEntry is null) 
            return null;
        var url = indexEntry.ReleasesJson;
        var downloadTarget = Path.GetTempFileName();
        DotNetChannel? dotNetChannel;
        await using var httpStream = await httpClient.GetStreamAsync(url, cancellationToken);
        await using (var fileStream = File.Open(downloadTarget, FileMode.Create, FileAccess.ReadWrite,
                         FileShare.ReadWrite | FileShare.Delete))
        {
            await httpStream.CopyToAsync(fileStream, cancellationToken);
            await fileStream.FlushAsync(cancellationToken);
            fileStream.Seek(0, SeekOrigin.Begin);
            dotNetChannel = await jsonSerializer.DeserializeAsync<DotNetChannel>(fileStream, cancellationToken);
        }
        // We do this here, because if the deserialization fails, we do not want to write the cache file. Hence
        // the temporary file, followed by a move. We could move the file while it's still open, due to the file share
        // settings. But I've seen this misbehave before on windows.
        File.Move(downloadTarget, GetChannelCacheFileName(version), true);
        return dotNetChannel;
    }

    private static string GetChannelCacheFileName(Version version)
    {
        var tempPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "dotnet-sdk-installer", Environment.UserName)).FullName;
        var fileName = $"dotnet-sdk-channel-{version.ToString(2)}.json";
        var fullPath = Path.Combine(tempPath, fileName);
        return fullPath;
    }
    private async ValueTask<(bool result, DotNetChannel? channel)> TryGetCachedChannelAsync(Version version,
        CancellationToken cancellationToken)
    {
        var fullPath = GetChannelCacheFileName(version);
        if (!File.Exists(fullPath)) return (false, null);
        try
        {
            await using var fileStream = File.OpenRead(fullPath);
            var deserializedChannel =
                await jsonSerializer.DeserializeAsync<DotNetChannel>(fileStream, cancellationToken);
            var result = deserializedChannel is not null;
            return (result, deserializedChannel);
        }
        catch (JsonException)
        {
            // Failed to deserialize
        }
        catch(FileNotFoundException)
        {
            // Our file somehow went missing between us checking for it, and then trying to read it.
        }
        
        return (false, null);
    }
}