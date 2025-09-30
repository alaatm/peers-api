namespace Peers.Modules.Customers.Domain;

/// <summary>
/// Represents a customer's named address information.
/// </summary>
public sealed class CustomerAddress : Entity
{
    /// <summary>
    /// The name of the address (e.g., "Home", "Work").
    /// </summary>
    public string Name { get; set; } = default!;
    /// <summary>
    /// The ID of the associated customer.
    /// </summary>
    public int CustomerId { get; set; }
    /// <summary>
    /// Indicates whether this address is the default address for the customer.
    /// </summary>
    public bool IsDefault { get; set; }
    /// <summary>
    /// The detailed address information.
    /// </summary>
    public Address Address { get; set; } = default!;
    /// <summary>
    /// The associated customer.
    /// </summary>
    public Customer Customer { get; set; } = default!;

    private CustomerAddress() { }

    /// <summary>
    /// Initializes a new instance of the CustomerAddress class with the specified customer, address name, and address
    /// details.
    /// </summary>
    /// <param name="customer">The customer to whom the address belongs.</param>
    /// <param name="name">The name or label for the address (for example, "Home" or "Office").</param>
    /// <param name="address">The address details associated with the customer.</param>
    internal CustomerAddress(Customer customer, string name, Address address)
    {
        Name = name;
        Address = address;
        Customer = customer;
    }
}
