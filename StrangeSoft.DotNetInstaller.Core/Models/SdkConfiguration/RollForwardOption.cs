namespace StrangeSoft.DotNetInstaller.Core.Models.SdkConfiguration;

public enum RollForwardOption
{
    Disable,
    Patch,
    Feature,
    Minor,
    Major,
    LatestPatch,
    LatestFeature,
    LatestMajor
}