using System.Text.RegularExpressions;
using StrangeSoft.DotNetInstaller.Core.Models.SdkConfiguration;

namespace StrangeSoft.DotNetInstaller.Core.Scanner;

public partial class ProjectVersionScanner : IVersionScanner
{
    [GeneratedRegex(@"\<TargetFramework\>\s*net(?<version>\d\.\d)\s*\</TargetFramework\>", RegexOptions.Singleline)]
    private static partial Regex GetTargetFrameworkExpression();

    [GeneratedRegex(@"\<TargetFrameworks\>(?<versions>.+?)\</TargetFrameworks\>", RegexOptions.Singleline)]
    private static partial Regex GetTargetFrameworksExpression();

    [GeneratedRegex(@"^net(?<version>\d\.\d)$")]
    private static partial Regex GetFrameworkVersionExpression();

    public IEnumerable<string> GetGlobbingPatterns()
    {
        return new[] { "**/*.csproj" };
    }

    public IEnumerable<SdkVersionRequest> ScanForVersions(IEnumerable<string> files)
    {
        foreach (var match in files)
        {
            var fileText = File.ReadAllText(match);
            var targetFrameworkVersionRequest = GetTargetFrameworkVersion(fileText);
            if (targetFrameworkVersionRequest is not null)
                yield return targetFrameworkVersionRequest;
            foreach (var request in GetTargetFrameworksVersion(fileText))
            {
                yield return request;
            }
        }
    }

    private IEnumerable<SdkVersionRequest> GetTargetFrameworksVersion(string fileText)
    {
        var targetFrameworksExpression = GetTargetFrameworksExpression();
        var match = targetFrameworksExpression.Match(fileText);
        if (!match.Success)
            yield break;
        var versionExpression = GetFrameworkVersionExpression();
        var parts = match.Groups["versions"].Value
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            match = versionExpression.Match(part);
            if (!match.Success)
                continue;
            yield return new SdkVersionRequest(Version.Parse(match.Groups["version"].Value),
                RollForwardOption.LatestFeature);
        }
    }

    private SdkVersionRequest? GetTargetFrameworkVersion(string fileText)
    {
        var targetFrameworkExpression = GetTargetFrameworkExpression();
        var match = targetFrameworkExpression.Match(fileText);
        if (!match.Success)
            return null;
        var version = Version.Parse(match.Groups["version"].Value);
        return new SdkVersionRequest(version, RollForwardOption.LatestFeature);
    }
}