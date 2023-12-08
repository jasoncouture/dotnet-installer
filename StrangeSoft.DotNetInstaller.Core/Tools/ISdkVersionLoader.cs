using StrangeSoft.DotNetInstaller.Core.Models.Releases;

namespace StrangeSoft.DotNetInstaller.Core.Tools;

public interface ISdkVersionLoader
{
    Task<DotNetChannelIndex> GetDotNetChannels(CancellationToken cancellationToken);
    ValueTask<DotNetChannel?> GetChannelAsync(DotNetChannelIndex index, Version version,
        CancellationToken cancellationToken);
}