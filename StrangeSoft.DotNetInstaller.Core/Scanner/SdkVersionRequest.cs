using StrangeSoft.DotNetInstaller.Core.Models;
using StrangeSoft.DotNetInstaller.Core.Models.SdkConfiguration;

namespace StrangeSoft.DotNetInstaller.Core.Scanner;

public record SdkVersionRequest(
    ExtendedVersion Version,
    RollForwardOption RollForwardOption = RollForwardOption.Minor,
    bool AllowPreRelease = false);