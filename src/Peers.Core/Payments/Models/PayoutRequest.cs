namespace Peers.Core.Payments.Models;

public sealed class PayoutRequest
{
    public IReadOnlyList<PayoutRequestEntry> Entries { get; set; } = default!;
}

public sealed class PayoutRequestEntry
{
    public decimal Amount { get; set; }
    public BankInfo Bank { get; set; } = default!;
    public BeneficiaryInfo Beneficiary { get; set; } = default!;
    public Dictionary<string, string>? Metadata { get; set; }
}

public sealed class BankInfo
{
    public string Name { get; set; } = default!;
    public string Iban { get; set; } = default!;
}

public sealed class BeneficiaryInfo
{
    public string Name { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
}
