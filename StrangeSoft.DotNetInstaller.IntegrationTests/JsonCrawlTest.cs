using System.Net.Http.Headers;
using System.Text.Json;
using StrangeSoft.DotNetInstaller.Core;
using Xunit.Abstractions;
using DotNetMetadataJsonSerializerContext = StrangeSoft.DotNetInstaller.Core.Serialization.DotNetMetadataJsonSerializerContext;

namespace StrangeSoft.DotNetInstaller.IntegrationTests;

public class JsonCrawlTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public JsonCrawlTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }


    [Fact(
        Skip = "This test should be manually run to not spam MS servers. We have tests for other cases as json files."
    )]
    public async Task AllReleases_CanDeserialize()
    {
        var httpClient = new HttpClient()
        {
            DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
        };  
        const string indexUrl = "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json";
        var indexUri = new Uri(indexUrl);

        var jsonDocument = await httpClient.GetStringAsync(indexUri);
        var index = JsonSerializer.Deserialize(jsonDocument,
            DotNetMetadataJsonSerializerContext.Default.DotNetChannelIndex);
        Assert.NotNull(index);
        Assert.NotEmpty(index.Releases);
        foreach (var channelIndex in index.Releases)
        {
            var channelUri = channelIndex.ReleasesJson;
            _testOutputHelper.WriteLine(
                $"Fetching channel {channelIndex.ChannelVersion.ToString(2)} from {channelIndex.ReleasesJson}, type: {channelIndex.ReleaseType}");
            jsonDocument = await httpClient.GetStringAsync(channelUri);
            try
            {
                var channelData = JsonSerializer.Deserialize(jsonDocument,
                    DotNetMetadataJsonSerializerContext.Default.DotNetChannel);
                Assert.NotNull(channelData);
                Assert.NotEmpty(channelData.Releases);
                foreach (var releaseVersion in channelData.Releases)
                {
                    if (releaseVersion.Runtime is not null)
                        Assert.NotEmpty(releaseVersion.Runtime.Files);
                    if (releaseVersion.Sdk is not null)
                        Assert.NotEmpty(releaseVersion.Sdk.Files);
                }
            }
            catch
            {
                _testOutputHelper.WriteLine($"Parser failed, JSON Document: {jsonDocument}");
                throw;
            }
        }
    }
}