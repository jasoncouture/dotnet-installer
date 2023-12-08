namespace StrangeSoft.DotNetInstaller.Core;

public interface IApp
{
    ValueTask<int> RunAsync(CancellationToken cancellationToken);
}