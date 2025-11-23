using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.Moyasar.Models.Payouts;

namespace Peers.Core.Test.Payments.Providers.Moyasar.Models.Payouts;

public class MoyasarPayoutRequestTests
{
    [Fact]
    public void Create_creates_instance_from_given_generic_payoutRequest()
    {
        // Arrange
        var sourceId = Guid.NewGuid().ToString();
        var payoutRequest = new PayoutRequest
        {
            Entries =
            [
                new PayoutRequestEntry
                {
                    Amount = 100.00m,
                    Bank = new BankInfo { Name = "Bank A", Iban = "DE89370400440532013000" },
                    Beneficiary = new BeneficiaryInfo
                    {
                        Name = "John Doe",
                        PhoneNumber = "+1234567890",
                        EmailAddress = "john@example.com",
                        Address = "123 Main",
                        City = "Berlin",
                        Country = "Germany",
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "TransactionId", "12345" },
                        { "Note", "Payment for services" }
                    },
                },
                new PayoutRequestEntry
                {
                    Amount = 200.00m,
                    Bank = new BankInfo { Name = "Bank B", Iban = "DE89370400440532013001" },
                    Beneficiary = new BeneficiaryInfo
                    {
                        Name = "Jane Smith",
                        PhoneNumber = "+0987654321",
                        EmailAddress = "jane@example.com",
                        Address = "456 Elm",
                        City = "Munich",
                        Country = "Germany",
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "TransactionId", "67890" },
                        { "Note", "Payment for goods" }
                    },
                    }
            ]
        };

        // Act
        var request = MoyasarPayoutRequest.Create(sourceId, payoutRequest);

        // Assert
        Assert.NotNull(request);
        Assert.Equal(sourceId, request.SourceId);
        Assert.NotNull(request.Entries);
        Assert.Equal(2, request.Entries.Length);

        var entry0 = request.Entries[0];
        Assert.Equal(10000, entry0.Amount); // Amount should be in halala
        Assert.Equal("payment_to_merchant", entry0.Purpose);
        Assert.Equal("bank", entry0.Destination.Type);
        Assert.Equal("DE89370400440532013000", entry0.Destination.Iban);
        Assert.Equal("John Doe", entry0.Destination.BeneficiaryName);
        Assert.Equal("+1234567890", entry0.Destination.PhoneNumber);
        Assert.Equal("Germany", entry0.Destination.Country);
        Assert.Equal("Berlin", entry0.Destination.City);
        Assert.Equal("john@example.com", entry0.Comment);
        Assert.Equal(2, entry0.Metadata!.Count);
        Assert.Equal("12345", entry0.Metadata["TransactionId"]);
        Assert.Equal("Payment for services", entry0.Metadata["Note"]);

        var entry1 = request.Entries[1];
        Assert.Equal(20000, entry1.Amount); // Amount should be in halala
        Assert.Equal("payment_to_merchant", entry1.Purpose);
        Assert.Equal("bank", entry1.Destination.Type);
        Assert.Equal("DE89370400440532013001", entry1.Destination.Iban);
        Assert.Equal("Jane Smith", entry1.Destination.BeneficiaryName);
        Assert.Equal("+0987654321", entry1.Destination.PhoneNumber);
        Assert.Equal("Germany", entry1.Destination.Country);
        Assert.Equal("Munich", entry1.Destination.City);
        Assert.Equal("jane@example.com", entry1.Comment);
        Assert.Equal(2, entry1.Metadata!.Count);
        Assert.Equal("67890", entry1.Metadata["TransactionId"]);
        Assert.Equal("Payment for goods", entry1.Metadata["Note"]);
    }
}
