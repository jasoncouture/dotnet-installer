using System.Text.Json.Serialization;
using StrangeSoft.DotNetInstaller.Core.Serialization;

namespace StrangeSoft.DotNetInstaller.Core.Models;

[JsonConverter(typeof(ExtendedVersionJsonConverter))]
public readonly record struct ExtendedVersion(Version Version, string? Extra = null) : IComparable<ExtendedVersion>
{
    public static implicit operator ExtendedVersion(Version version)
    {
        return new ExtendedVersion(version);
    }
    public static ExtendedVersion FromVersionString(string? versionString)
    {
        if (versionString is null) throw new InvalidOperationException("Invalid format string");
        var parts = versionString.Split('-', 2);
        var version = Version.Parse(parts[0]);
        // Make sure .NET doesn't pull it's bullshit when there is less than 2 parts. We always want 3.
        version = new Version(version.Major, version.Minor, version.Build == -1 ? 0 : version.Build);
        var extra = parts.Length < 2 ? null : parts[1];
        return new ExtendedVersion(version, extra);
    }

    public override string ToString()
    {
        string versionString;
        if (Version.Build == -1)
            versionString = $"{Version.ToString(2)}.0";
        else
            versionString = Version.ToString(3);
        return $"{versionString}{(Extra == null ? string.Empty : "-")}{Extra}";
    }

    public int CompareTo(ExtendedVersion other)
    {
        var versionComparison = Version.CompareTo(other.Version);
        if (versionComparison != 0) return versionComparison;
        if (Extra is not null && other.Extra is null) return -1;
        if (Extra is null && other.Extra is not null) return 1;
        return string.Compare(Extra, other.Extra, StringComparison.Ordinal);
    }
}