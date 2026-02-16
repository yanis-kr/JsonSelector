using FluentAssertions;
using Xunit;

namespace JsonSelector.Tests;

public sealed class JsonSelectorTests
{
    private readonly IJsonSelector _sut = new JsonSelectorImpl();

    private const string NsfCheckingPayload = """
        {
          "account": 1234567,
          "availableBalance": 12.34,
          "transactions": []
        }
        """;

    private const string NsfSavingPayload = """
        {
          "accountId": 2345678,
          "availableBalance": 12.34,
          "transactions": []
        }
        """;

    private const string OdpJournalUpdatedPayload = """
        {
          "availableBalance": 0,
          "journalEntries": [
            {
              "accountId": 1234567,
              "entryType": "credit",
              "tranCode": "10101",
              "amount": 21.22
            },
            {
              "accountId": 3456789,
              "entryType": "debit",
              "tranCode": "10101",
              "amount": 21.22
            }
          ]
        }
        """;

    [Fact]
    public void Any_WithNsfChecking_AccountPath_ReturnsTrue() =>
        _sut.Any(NsfCheckingPayload, "$.account").Should().BeTrue();

    [Fact]
    public void Any_WithNsfSaving_AccountIdPath_ReturnsTrue() =>
        _sut.Any(NsfSavingPayload, "$.accountId").Should().BeTrue();

    [Fact]
    public void Any_WithNonExistentPath_ReturnsFalse() =>
        _sut.Any(NsfCheckingPayload, "$.nonexistent").Should().BeFalse();

    [Fact]
    public void Any_WithOdp_CreditEntryFilter_ReturnsTrue() =>
        _sut.Any(OdpJournalUpdatedPayload, "$.journalEntries[?(@.entryType=='credit' && @.tranCode=='10101')]").Should().BeTrue();

    [Fact]
    public void Any_WithOdp_StrictTranCodeFilter_NoMatch_ReturnsFalse() =>
        _sut.Any(OdpJournalUpdatedPayload, "$.journalEntries[?(@.entryType=='credit' && @.tranCode=='99999')]").Should().BeFalse();

    [Fact]
    public void FirstString_WithNsfChecking_AccountPath_ReturnsAccountAsString() =>
        _sut.FirstString(NsfCheckingPayload, "$.account").Should().Be("1234567");

    [Fact]
    public void FirstString_WithNsfSaving_AccountIdPath_ReturnsAccountId() =>
        _sut.FirstString(NsfSavingPayload, "$.accountId").Should().Be("2345678");

    [Fact]
    public void FirstString_WithOdp_CreditEntryAccountId_ReturnsFirstCreditAccountId() =>
        _sut.FirstString(OdpJournalUpdatedPayload, "$.journalEntries[?(@.entryType=='credit')].accountId").Should().Be("1234567");

    [Fact]
    public void FirstString_WithNonExistentPath_ReturnsNull() =>
        _sut.FirstString(NsfCheckingPayload, "$.missing").Should().BeNull();

    [Fact]
    public void FirstInt_WithOdp_CreditEntryTranCode_ReturnsTranCodeAsInt() =>
        _sut.FirstInt(OdpJournalUpdatedPayload, "$.journalEntries[?(@.entryType=='credit')].tranCode").Should().Be(10101);

    [Fact]
    public void FirstInt_WithNsfChecking_Account_ReturnsAccountAsInt() =>
        _sut.FirstInt(NsfCheckingPayload, "$.account").Should().Be(1234567);

    [Fact]
    public void Any_WithDataWrapper_MatchesNestedPath()
    {
        string payload = """{"data":{"account":1234567,"availableBalance":12.34}}""";
        _sut.Any(payload, "$.data.account").Should().BeTrue();
    }

    [Fact]
    public void FirstString_WithDataWrapper_ExtractsAccountId()
    {
        string payload = """{"data":{"journalEntries":[{"accountId":1234567,"entryType":"credit","tranCode":"10101"}]}}""";
        _sut.FirstString(payload, "$.data.journalEntries[?(@.entryType=='credit')].accountId").Should().Be("1234567");
    }
}
