using FluentAssertions;
using Xunit;

namespace JsonSelector.Tests;

public sealed class FirstIntTests
{
    private readonly IJsonSelector _sut = new JsonSelectorImpl();

    [Theory]
    [InlineData("$.id", 1001)]
    [InlineData("$.missing", null)]
    public void FirstInt_WithSimplePayload_ReturnsExpected(string selector, int? expected) =>
        _sut.FirstInt(TestPayloads.SimplePayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.ref", 2002)]
    [InlineData("$.missing", null)]
    public void FirstInt_WithAlternatePayload_ReturnsExpected(string selector, int? expected) =>
        _sut.FirstInt(TestPayloads.AlternatePayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.items[?(@.kind=='x')].code", 10)]
    [InlineData("$.items[?(@.kind=='y')].code", 20)]
    [InlineData("$.items[?(@.kind=='z')].code", null)]
    [InlineData("$.items[?(@.kind=='x' && @.code=='30')].code", 30)]
    public void FirstInt_WithArrayPayload_ReturnsExpected(string selector, int? expected) =>
        _sut.FirstInt(TestPayloads.ArrayPayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.data.id", 1001)]
    [InlineData("$.data.items[?(@.kind=='x')].code", 10)]
    public void FirstInt_WithNestedPayload_ReturnsExpected(string selector, int? expected) =>
        _sut.FirstInt(TestPayloads.NestedPayload, selector).Should().Be(expected);

    [Fact]
    public void FirstInt_WithStringValue_ParsesAsInt() =>
        _sut.FirstInt(TestPayloads.SimplePayload, "$.name").Should().BeNull();

    [Fact]
    public void FirstInt_WithNullJson_ReturnsNull() =>
        _sut.FirstInt("", "$.id").Should().BeNull();

    [Fact]
    public void FirstInt_WithInvalidJson_ReturnsNull() =>
        _sut.FirstInt("{ invalid }", "$.id").Should().BeNull();
}
