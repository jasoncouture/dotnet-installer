namespace StrangeSoft.DotNetInstaller.Core.Models.SdkConfiguration;

public enum RollForwardOption
{
    Disabled,
    Patch,
    Feature,
    Minor,
    Major,
    LatestPatch,
    LatestFeature,
    LatestMajor
}