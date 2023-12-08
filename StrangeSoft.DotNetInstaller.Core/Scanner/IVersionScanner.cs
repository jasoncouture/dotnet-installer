namespace StrangeSoft.DotNetInstaller.Core.Scanner;

public interface IVersionScanner
{
    IEnumerable<string> GetGlobbingPatterns();
    IEnumerable<SdkVersionRequest> ScanForVersions(IEnumerable<string> files);
}