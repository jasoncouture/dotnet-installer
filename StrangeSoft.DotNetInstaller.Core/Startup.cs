using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StrangeSoft.DotNetInstaller.Core.Logging;
using StrangeSoft.DotNetInstaller.Core.Platform;
using StrangeSoft.DotNetInstaller.Core.Platform.Windows;
using StrangeSoft.DotNetInstaller.Core.Releases;
using StrangeSoft.DotNetInstaller.Core.Scanner;
using StrangeSoft.DotNetInstaller.Core.Serialization;

namespace StrangeSoft.DotNetInstaller.Core;

public static class Startup
{
    public static async ValueTask<int> RunAsync(CommandLineOptions options,
        CancellationToken cancellationToken = default)
    {
        var serviceCollection = BuildApplication(options);
        
        
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions()
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
        await using var scope = serviceProvider.CreateAsyncScope();
        
        
        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var eventHandler =
            // ReSharper disable once AccessToDisposedClosure
            new ConsoleCancelEventHandler((_, _) => cancellationTokenSource.Cancel());
        Console.CancelKeyPress += eventHandler;
        try
        {
            return await scope.ServiceProvider.GetRequiredService<IApp>().RunAsync(cancellationTokenSource.Token);
        }
        finally
        {
            Console.CancelKeyPress -= eventHandler;
        }
    }

    public static IServiceCollection BuildApplication(CommandLineOptions options)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(options);
        
        serviceCollection.AddLogging(ConfigureLogging);
        serviceCollection.AddHttpClient<ISdkVersionLoader, SdkVersionLoader>();
        serviceCollection.AddHttpClient<IDotNetInstaller, DotNetInstaller>();
        
        serviceCollection.AddScoped<IPlatformPackageInstaller, WindowsPlatformPackageInstaller>();
        serviceCollection.AddScoped<IVersionRequestMatcher, VersionRequestMatcher>();
        serviceCollection.AddScoped<IVersionScanner, ProjectVersionScanner>();
        serviceCollection.AddScoped<IVersionScanner, GlobalJsonVersionScanner>();
        serviceCollection.AddScoped<IVersionCollector, VersionCollector>();
        serviceCollection.AddScoped<IRuntimeIdentifierSelector, RuntimeIdentifierSelector>();
        serviceCollection.AddScoped<IHashVerifier, HashVerifier>();
        serviceCollection.AddScoped<IApp, App>();
        serviceCollection.AddScoped<IJsonSerializer, SourceGeneratedJsonSerializer>();

        return serviceCollection;
    }

    private static void ConfigureLogging(ILoggingBuilder builder)
    {
        builder.ClearProviders();
        builder.AddProvider(new NakedConsoleOutputLoggingProvider());
    }
}