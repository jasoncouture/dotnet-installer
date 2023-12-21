using System.Text.Json;
using StrangeSoft.DotNetInstaller.Core.Models;
using StrangeSoft.DotNetInstaller.Core.Models.SdkConfiguration;
using DotNetMetadataJsonSerializerContext = StrangeSoft.DotNetInstaller.Core.Serialization.DotNetMetadataJsonSerializerContext;

namespace StrangeSoft.DotNetInstaller.UnitTests;

public class JsonParserTests
{
    [Fact]
    public void DotNetChannelIndex_CanDeserialize()
    {
        using var stream = File.OpenRead("releases-index.json");

        var result = JsonSerializer.Deserialize(stream, DotNetMetadataJsonSerializerContext.Default.DotNetChannelIndex);
        Assert.NotNull(result);
        Assert.Equal(11, result.Releases.Length);
    }

    [Theory]
    [InlineData("6.0.123-test", "6.0.123", "test")]
    [InlineData("6.0.123", "6.0.123", null)]
    public void ExtendedVersionParsesCorrectly(string input, string expectedVersion, string? expectedExtra)
    {
        var result = ExtendedVersion.FromVersionString(input);
        Assert.Equal(Version.Parse(expectedVersion), result.Version);
        Assert.Equal(expectedExtra, result.Extra);
    }

    [Fact]
    public void DotNetChannel_CanDeserialize()
    {
        using var stream = File.OpenRead("releases.json");

        var result = JsonSerializer.Deserialize(stream, DotNetMetadataJsonSerializerContext.Default.DotNetChannel);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Releases);
    }

    [Theory]
    [MemberData(nameof(GenerateGlobalJsonTestCases))]
    public void GlobalJson_CanDeserialize(string globalJson)
    {
        JsonSerializer.Deserialize(globalJson, DotNetMetadataJsonSerializerContext.Default.GlobalJson);
    }

    public static IEnumerable<object[]> GenerateGlobalJsonTestCases()
    {
        return GenerateGlobalJsonStringTestCases().Select(i => new object[] { i });
    }

    private static IEnumerable<string> GenerateGlobalJsonStringTestCases()
    {
        yield return """
                      {
                        "sdk": {
                          "version": "6.0.300",
                          "rollForward": "latestFeature"
                        }
                      }
                     """;
        yield return """
                      {
                        "sdk": {
                          "version": "6.0.300",
                          "rollForward": "disabled"
                        }
                      }
                     """;
        yield return """
                      {
                        "sdk": {
                          "version": "6.0.300"
                        }
                      }
                     """;
        yield return """
                      {
                        "sdk": {
                          "rollForward": "latestMajor"
                        }
                      }
                     """;
    }
}