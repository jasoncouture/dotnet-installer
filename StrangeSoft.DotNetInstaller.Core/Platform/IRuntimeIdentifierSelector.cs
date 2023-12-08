namespace StrangeSoft.DotNetInstaller.Core.Platform;

public interface IRuntimeIdentifierSelector
{
    bool IsRuntimeIdentifierSelected(string? runtimeIdentifier);
}