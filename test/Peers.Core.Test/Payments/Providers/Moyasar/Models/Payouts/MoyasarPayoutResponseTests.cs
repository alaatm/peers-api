using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.Moyasar.Models.Payouts;

namespace Peers.Core.Test.Payments.Providers.Moyasar.Models.Payouts;

public class MoyasarPayoutResponseTests
{
    [Fact]
    public void ToGeneric_converts_moyasar_payout_response_to_generic_payout_response()
    {
        // Arrange
        var response = new MoyasarPayoutResponse
        {
            Entries =
            [
                new MoyasarPayoutResponseEntry
                {
                    Id = "payout_12345",
                    SourceId = "source_12345",
                    SequenceNumber = "1",
                    Channel = "bank_transfer",
                    Status = "paid",
                    Amount = 10000,
                    Currency = "SAR",
                    Purpose = "payment_to_merchant",
                    Comment = "Payment for services",
                    Destination = new MoyasarPayoutDestination
                    {
                        Type = "bankX",
                        Iban = "DE89370400440532013000",
                        BeneficiaryName = "John Doe",
                        PhoneNumber = "+1234567890",
                        Country = "Germany",
                        City = "Berlin"
                    },
                    Message = "Payment completed successfully",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                    UpdatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, string>
                    {
                        { "TransactionId", "12345" },
                        { "Note", "Payment for services" }
                    },
                },
                new MoyasarPayoutResponseEntry
                {
                    Id = "payout_67890",
                    SourceId = "source_67890",
                    SequenceNumber = "2",
                    Channel = "bank_transfer",
                    Status = "failed",
                    Amount = 20000,
                    Currency = "SAR",
                    Purpose = "payment_to_merchant",
                    Comment = "Payment for services",
                    Destination = new MoyasarPayoutDestination
                    {
                        Type = "bankX",
                        Iban = "DE89370400440532013001",
                        BeneficiaryName = "Jane Doe",
                        PhoneNumber = "+1234567899",
                        Country = "Germany",
                        City = "Munich"
                    },
                    Message = "Payment failed",
                    FailureReason = "Insufficient funds",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                    UpdatedAt = DateTime.UtcNow,
                }
            ]
        };

        // Act
        var genericResponse = response.ToGeneric();

        // Assert
        Assert.NotNull(genericResponse);
        Assert.NotNull(genericResponse.Entries);
        Assert.Equal(2, genericResponse.Entries.Length);

        var entry0 = genericResponse.Entries[0];
        Assert.Equal(response.Entries[0].UpdatedAt, entry0.Timestamp);
        Assert.Null(entry0.BatchId);
        Assert.Equal("payout_12345", entry0.EntryId);
        Assert.Equal("DE89370400440532013000", entry0.Iban);
        Assert.Equal("SAR", entry0.Currency);
        Assert.Equal(100, entry0.Amount);
        Assert.Equal(0, entry0.Fee);
        Assert.Equal(100, entry0.Total);
        Assert.Equal("Payment completed successfully", entry0.Message);

        var entry1 = genericResponse.Entries[1];
        Assert.Equal(response.Entries[1].UpdatedAt, entry1.Timestamp);
        Assert.Null(entry1.BatchId);
        Assert.Equal("payout_67890", entry1.EntryId);
        Assert.Equal("DE89370400440532013001", entry1.Iban);
        Assert.Equal("SAR", entry1.Currency);
        Assert.Equal(200, entry1.Amount);
        Assert.Equal(0, entry1.Fee);
        Assert.Equal(200, entry1.Total);
        Assert.Equal("Payment failed: Insufficient funds", entry1.Message);
    }

    [Theory]
    [InlineData("paid", PayoutResponseEntryStatus.Completed)]
    [InlineData("pending", PayoutResponseEntryStatus.Pending)]
    [InlineData("failed", PayoutResponseEntryStatus.Failed)]
    [InlineData("canceled", PayoutResponseEntryStatus.Failed)]
    [InlineData("returned", PayoutResponseEntryStatus.Failed)]
    [InlineData("unknown", PayoutResponseEntryStatus.Pending)]
    public void ToGeneric_correctly_maps_entry_status(string moyasarStatus, PayoutResponseEntryStatus expectedMappedStatus)
    {
        // Arrange
        var entry = new MoyasarPayoutResponseEntry
        {
            Status = moyasarStatus,
            Destination = new(),
        };

        // Act
        var genericEntry = entry.ToGeneric();

        // Assert
        Assert.Equal(expectedMappedStatus, genericEntry.Status);
    }
}
