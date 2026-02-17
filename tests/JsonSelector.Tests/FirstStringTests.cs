using FluentAssertions;
using Xunit;

namespace JsonSelector.Tests;

public sealed class FirstStringTests
{
    private readonly IJsonSelector _sut = new JsonSelectorImpl();

    [Theory]
    [InlineData("$.id", "1001")]
    [InlineData("$.name", "alpha")]
    [InlineData("$.missing", null)]
    public void FirstString_WithSimplePayload_ReturnsExpected(string selector, string? expected) =>
        _sut.FirstString(TestPayloads.SimplePayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.ref", "2002")]
    [InlineData("$.label", "beta")]
    [InlineData("$.missing", null)]
    public void FirstString_WithAlternatePayload_ReturnsExpected(string selector, string? expected) =>
        _sut.FirstString(TestPayloads.AlternatePayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.items[?(@.kind=='x')].id", "A1")]
    [InlineData("$.items[?(@.kind=='y')].id", "B2")]
    [InlineData("$.items[?(@.kind=='z')].id", null)]
    [InlineData("$.items[?(@.kind=='x' && @.code=='10')].id", "A1")]
    [InlineData("$.items[?(@.kind=='x' && @.code=='99')].id", null)]
    [InlineData("$.items[?(@.kind=='x' && (@.code=='10' || @.code=='30'))].id", "A1")]
    [InlineData("$.items[?(@.kind=='x' && isOneOf(@.code, '10','30'))].id", "A1")]
    [InlineData("$.items[?(@.kind=='x')].code", "10")]
    public void FirstString_WithArrayPayload_ReturnsExpected(string selector, string? expected) =>
        _sut.FirstString(TestPayloads.ArrayPayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.data.id", "1001")]
    [InlineData("$.data.items[?(@.kind=='x')].id", "A1")]
    [InlineData("$.data.items[?(@.kind=='x')].code", "10")]
    [InlineData("$.data.missing", null)]
    public void FirstString_WithNestedPayload_ReturnsExpected(string selector, string? expected) =>
        _sut.FirstString(TestPayloads.NestedPayload, selector).Should().Be(expected);

    [Fact]
    public void FirstString_WithNullJson_ReturnsNull() =>
        _sut.FirstString("", "$.id").Should().BeNull();

    [Fact]
    public void FirstString_WithInvalidJson_ReturnsNull() =>
        _sut.FirstString("{ invalid }", "$.id").Should().BeNull();

    [Fact]
    public void FirstString_WithMalformedSelector_ReturnsNull() =>
        _sut.FirstString(TestPayloads.SimplePayload, "$.id[?broken").Should().BeNull();
}
