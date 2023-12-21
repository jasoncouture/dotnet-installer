using System.Runtime.InteropServices;

namespace StrangeSoft.DotNetInstaller.Core.Platform;

public sealed class RuntimeIdentifierSelector() : IRuntimeIdentifierSelector
{
    private static string DetermineCurrentRuntimeIdentifier() =>
        $"{GetOperatingSystemIdentifierPart()}-{GetCpuRuntimeIdentifierPart()}";

    private static string GetOperatingSystemIdentifierPart()
    {
        if (OperatingSystem.IsMacOS())
            return "osx";
        if (OperatingSystem.IsWindows())
            return "win";
        if (OperatingSystem.IsLinux())
            return "linux";

        throw new NotSupportedException("The current platform is not supported");
    }

    private static string GetCpuRuntimeIdentifierPart()
    {
        return RuntimeInformation.OSArchitecture switch
        {
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            _ => throw new NotSupportedException(
                $"Processor architecture {RuntimeInformation.OSArchitecture} is not supported"
            )
        };
    }

    public bool IsRuntimeIdentifierSelected(string? runtimeIdentifier)
    {
        if (runtimeIdentifier is null)
            return false;
        return string.Equals(DetermineCurrentRuntimeIdentifier(), runtimeIdentifier, StringComparison.OrdinalIgnoreCase);
    }
}