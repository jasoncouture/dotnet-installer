# .NET SDK Automatic Installer

This tool will scan the specified folder for project files, and global.json files. It will then check the .NET releases, and find the appropriate SDK versions to install.

For help using the command line tool, use --help

Native binaries can be found at https://github.com/jasoncouture/dotnet-installer/releases these are useful when you do not have .NET 8.0 installed.

If you do have .NET 8.0 installed, simply run `dotnet tool install --global StrangeSoft.DotNetInstaller`. Afterwards, you can simply run `dotnet install-sdks`.

### NOTE

OS-X and Linux support are experimental. Please submit an issue if you encounter a bug.