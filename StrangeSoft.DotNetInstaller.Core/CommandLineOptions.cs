using System.Collections.Immutable;
using Microsoft.Extensions.Http;

namespace StrangeSoft.DotNetInstaller.Core;

public record CommandLineOptions(Uri DownloadUri, DirectoryInfo BasePath);