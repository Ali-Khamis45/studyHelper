using AiStudyOS.Infrastructure.AI.Providers;
using FluentAssertions;

namespace AiStudyOS.Infrastructure.UnitTests.AI.Providers;

public class ProviderResponseParserTests
{
    // Deliberately has no [JsonPropertyName] attributes. Every real provider DTO in this project
    // does declare them (defense in depth), so a regression test against those DTOs alone would
    // keep passing even if the shared options were removed from ProviderResponseParser. This type
    // isolates the parser's OWN configuration: it only binds "value"/"count" (lowercase, as any
    // real provider sends) to Value/Count because ProviderResponseParser routes through
    // AiJsonOptions.Default (case-insensitive). Remove that, and these tests fail.
    private record UnannotatedDto(string Value, int Count);

    [Fact]
    public void DeserializeResponse_BindsLowercaseJson_WithoutExplicitPropertyNames_RegressionTest()
    {
        var result = ProviderResponseParser.DeserializeResponse<UnannotatedDto>("test", """{"value":"hello","count":3}""");

        result.Value.Should().Be("hello");
        result.Count.Should().Be(3);
    }

    [Fact]
    public void DeserializeStreamingChunk_BindsLowercaseJson_WithoutExplicitPropertyNames_RegressionTest()
    {
        var result = ProviderResponseParser.DeserializeStreamingChunk<UnannotatedDto>("test", """{"value":"hello","count":3}""");

        result.Should().NotBeNull();
        result!.Value.Should().Be("hello");
        result.Count.Should().Be(3);
    }

    [Fact]
    public void DeserializeResponse_MalformedJson_ThrowsProviderProtocolException()
    {
        var act = () => ProviderResponseParser.DeserializeResponse<UnannotatedDto>("test", "not json");

        act.Should().Throw<ProviderProtocolException>();
    }

    [Fact]
    public void DeserializeResponse_NullLiteral_ThrowsProviderProtocolException()
    {
        var act = () => ProviderResponseParser.DeserializeResponse<UnannotatedDto>("test", "null");

        act.Should().Throw<ProviderProtocolException>();
    }

    [Fact]
    public void DeserializeStreamingChunk_WhitespaceLine_ReturnsNull()
    {
        var result = ProviderResponseParser.DeserializeStreamingChunk<UnannotatedDto>("test", "   ");

        result.Should().BeNull();
    }

    [Fact]
    public void DeserializeStreamingChunk_MalformedLine_ThrowsProviderProtocolException()
    {
        var act = () => ProviderResponseParser.DeserializeStreamingChunk<UnannotatedDto>("test", "{not valid json");

        act.Should().Throw<ProviderProtocolException>();
    }
}
