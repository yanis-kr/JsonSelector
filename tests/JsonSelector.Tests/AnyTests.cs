using FluentAssertions;
using Xunit;

namespace JsonSelector.Tests;

public sealed class AnyTests
{
    private readonly IJsonSelector _sut = new JsonSelectorImpl();

    [Theory]
    [InlineData("$.id", true)]
    [InlineData("$.name", true)]
    [InlineData("$.missing", false)]
    [InlineData("$.tags", true)]
    [InlineData("$.name == 'alpha'", true)]
    [InlineData("$.name == 'beta'", false)]
    [InlineData("$.name != 'beta'", true)]
    [InlineData("$.name != 'alpha'", false)]
    [InlineData("$.id == 1001", true)]
    [InlineData("$.id >= 1001", true)]
    [InlineData("$.id >= 1000", true)]
    [InlineData("$.id > 1000", true)]
    [InlineData("$.id < 1002", true)]
    [InlineData("$.id <= 1001", true)]
    [InlineData("$.missing == 'x'", false)]
    public void Any_WithSimplePayload_ReturnsExpected(string selector, bool expected) =>
        _sut.Any(TestPayloads.SimplePayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.ref", true)]
    [InlineData("$.label", true)]
    [InlineData("$.missing", false)]
    public void Any_WithAlternatePayload_ReturnsExpected(string selector, bool expected) =>
        _sut.Any(TestPayloads.AlternatePayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.items", true)]
    [InlineData("$.items[?(@.code >= 10)]", true)]
    [InlineData("$.items[?(@.code > 25)]", true)]
    [InlineData("$.items[?(@.code < 15)]", true)]
    [InlineData("$.items[?(@.code >= 100)]", false)]
    [InlineData("$.items[?(@.kind=='x')]", true)]
    [InlineData("$.items[?(@.kind=='y')]", true)]
    [InlineData("$.items[?(@.kind=='z')]", false)]
    [InlineData("$.items[?(@.kind=='x' && @.code=='10')]", true)]
    [InlineData("$.items[?(@.kind=='x' && @.code=='99')]", false)]
    [InlineData("$.items[?(@.kind=='x' && (@.code=='10' || @.code=='20'))]", true)]
    [InlineData("$.items[?(@.kind=='x' && (@.code=='99' || @.code=='88'))]", false)]
    [InlineData("$.items[?(@.kind=='x' && isOneOf(@.code, '10','30'))]", true)]
    [InlineData("$.items[?(@.kind=='x' && isOneOf(@.code, '99','88'))]", false)]
    public void Any_WithArrayPayload_ReturnsExpected(string selector, bool expected) =>
        _sut.Any(TestPayloads.ArrayPayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.data", true)]
    [InlineData("$.data.id", true)]
    [InlineData("$.data.items", true)]
    [InlineData("$.data.missing", false)]
    [InlineData("$.data.id == 1001", true)]
    [InlineData("$.data.id >= 1000", true)]
    [InlineData("$.data.id == 999", false)]
    public void Any_WithNestedPayload_ReturnsExpected(string selector, bool expected) =>
        _sut.Any(TestPayloads.NestedPayload, selector).Should().Be(expected);

    [Fact]
    public void Any_WithNullJson_ReturnsFalse() =>
        _sut.Any("", "$.id").Should().BeFalse();

    [Fact]
    public void Any_WithInvalidJson_ReturnsFalse() =>
        _sut.Any("{ invalid }", "$.id").Should().BeFalse();

    [Fact]
    public void Any_WithMalformedSelector_ReturnsFalse() =>
        _sut.Any(TestPayloads.SimplePayload, "$.id[?invalid").Should().BeFalse();
}
