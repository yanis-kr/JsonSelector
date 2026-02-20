using FluentAssertions;
using Xunit;

namespace JsonSelector.Tests;

public sealed class IndexTests
{
    private readonly IJsonSelector _sut = new JsonSelectorImpl();

    [Theory]
    [InlineData("$.items[0]", true)]
    [InlineData("$.items[1]", true)]
    [InlineData("$.items[2]", true)]
    [InlineData("$.items[3]", false)]
    [InlineData("$.items[-1]", true)]
    [InlineData("$.items[-2]", true)]
    [InlineData("$.items[-3]", true)]
    [InlineData("$.items[-4]", false)]
    public void Any_WithArrayIndex_ReturnsExpected(string selector, bool expected) =>
        _sut.Any(TestPayloads.ArrayPayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.items[0].id", "A1")]
    [InlineData("$.items[1].id", "B2")]
    [InlineData("$.items[2].id", "C3")]
    [InlineData("$.items[0].kind", "x")]
    [InlineData("$.items[1].kind", "y")]
    [InlineData("$.items[0].code", "10")]
    [InlineData("$.items[2].code", "30")]
    [InlineData("$.items[3].id", null)]
    [InlineData("$.items[-1].id", "C3")]
    [InlineData("$.items[-2].id", "B2")]
    [InlineData("$.items[-3].id", "A1")]
    [InlineData("$.items[-4].id", null)]
    public void FirstString_WithArrayIndex_ReturnsExpected(string selector, string? expected) =>
        _sut.FirstString(TestPayloads.ArrayPayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.items[0].code", 10)]
    [InlineData("$.items[1].code", 20)]
    [InlineData("$.items[2].code", 30)]
    [InlineData("$.items[-1].code", 30)]
    [InlineData("$.items[3].code", null)]
    public void FirstInt_WithArrayIndex_ReturnsExpected(string selector, int? expected) =>
        _sut.FirstInt(TestPayloads.ArrayPayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.data.items[0].id", "A1")]
    [InlineData("$.data.items[0].code", "10")]
    [InlineData("$.data.items[0].kind", "x")]
    [InlineData("$.data.items[1]", null)]
    public void FirstString_WithNestedArrayIndex_ReturnsExpected(string selector, string? expected) =>
        _sut.FirstString(TestPayloads.NestedPayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.data.myArray[0].myItem", "first")]
    [InlineData("$.data.myArray[1].myItem", "second")]
    [InlineData("$.data.myArray[2].myItem", "third")]
    [InlineData("$.data.myArray[0]", null)]
    [InlineData("$.data.myArray[3].myItem", null)]
    [InlineData("$.data.myArray[-1].myItem", "third")]
    [InlineData("$.data.myArray[-2].myItem", "second")]
    public void FirstString_WithIndexPayload_ReturnsExpected(string selector, string? expected) =>
        _sut.FirstString(TestPayloads.IndexPayload, selector).Should().Be(expected);

    [Theory]
    [InlineData("$.data.myArray[0]", true)]
    [InlineData("$.data.myArray[1]", true)]
    [InlineData("$.data.myArray[2]", true)]
    [InlineData("$.data.myArray[3]", false)]
    [InlineData("$.data.myArray[-1]", true)]
    [InlineData("$.data.myArray[-3]", true)]
    [InlineData("$.data.myArray[-4]", false)]
    public void Any_WithIndexPayload_ReturnsExpected(string selector, bool expected) =>
        _sut.Any(TestPayloads.IndexPayload, selector).Should().Be(expected);

    [Fact]
    public void FirstString_IndexOnNonArray_ReturnsNull() =>
        _sut.FirstString(TestPayloads.SimplePayload, "$.id[0]").Should().BeNull();

    [Fact]
    public void Any_IndexOnNonArray_ReturnsFalse() =>
        _sut.Any(TestPayloads.SimplePayload, "$.id[0]").Should().BeFalse();

    [Fact]
    public void FirstString_InvalidIndexSyntax_ReturnsNull() =>
        _sut.FirstString(TestPayloads.ArrayPayload, "$.items[abc]").Should().BeNull();

    [Fact]
    public void Any_InvalidIndexSyntax_ReturnsFalse() =>
        _sut.Any(TestPayloads.ArrayPayload, "$.items[abc]").Should().BeFalse();

    [Fact]
    public void FirstString_EmptyBrackets_ReturnsNull() =>
        _sut.FirstString(TestPayloads.ArrayPayload, "$.items[]").Should().BeNull();

    [Theory]
    [InlineData("$.items[0]", "A1")]
    [InlineData("$.items[1]", "B2")]
    [InlineData("$.items[2]", "C3")]
    public void FirstString_IndexWithDotNotation_ReturnsId(string selector, string expected)
    {
        var result = _sut.FirstString(TestPayloads.ArrayPayload, selector + ".id");
        result.Should().Be(expected);
    }
}
