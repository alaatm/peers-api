using System.Diagnostics;
using Peers.Modules.Customers.Rules;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Customers.Domain;

/// <summary>
/// Represents a system user of type <see cref="Customer"/>.
/// </summary>
public sealed class Customer : Entity, ISystemUser, IAggregateRoot
{
    /// <summary>
    /// The username.
    /// </summary>
    public string Username { get; set; } = default!;
    /// <summary>
    /// The signature secret for the customer account, used for hashing tip request signature.
    /// </summary>
    public string Secret { get; set; } = default!;
    /// <summary>
    /// The PIN code hash for the customer account.
    /// </summary>
    public string? PinCodeHash { get; set; }
    /// <summary>
    /// The linked database user.
    /// </summary>
    public AppUser User { get; set; } = default!;
    public List<CustomerAddress> AddressList { get; set; } = default!;

    /// <summary>
    /// Creates a new instance of <see cref="Customer"/>.
    /// </summary>
    /// <param name="user">The database user.</param>
    /// <param name="secret">The secret used for hashing tip request signature.</param>
    /// <returns></returns>
    public static Customer Create([NotNull] AppUser user, string secret)
    {
        Debug.Assert(user.UserName is not null);

        var customer = new Customer
        {
            User = user,
            Username = user.UserName,
            Secret = secret,
            AddressList = [],
        };

        return customer;
    }

    /// <summary>
    /// Deletes the customer account.
    /// </summary>
    /// <param name="date">The date when the account was deleted.</param>
    public void DeleteAccount(DateTime date)
    {
        CheckRule(new CanDeleteCustomerAccountRule(this));

        User.DeleteAccount(date);
        // Set the username to that of the user which has the string "deleted" appended to it.
        Username = User.UserName!;
    }

    /// <summary>
    /// Retrieves the default address associated with the customer, if one is set.
    /// </summary>
    public Address? GetDefaultAddress()
        => AddressList.SingleOrDefault(a => a.IsDefault)?.Address;

    /// <summary>
    /// Adds a new address with the specified name to the address list and optionally sets it as the default address.
    /// </summary>
    /// <param name="name">The name to associate with the new address.</param>
    /// <param name="address">The address information to add.</param>
    /// <param name="makeDefault">A value indicating whether the new address should be set as the default address.</param>
    public void AddAddress([NotNull] string name, Address address, bool makeDefault)
    {
        name = name.Trim();

        if (GetAddress(name) is not null)
        {
            throw new InvalidOperationException($"An address with the name '{name}' already exists.");
        }

        var customerAddress = new CustomerAddress(this, name, address);
        AddressList.Add(customerAddress);

        SetDefaultAddress(customerAddress, makeDefault || AddressList.Count == 0);
    }

    /// <summary>
    /// Removes the address with the specified name from the address list.
    /// </summary>
    /// <param name="name">The name of the address to remove.</param>
    public void RemoveAddress([NotNull] string name)
    {
        name = name.Trim();

        if (GetAddress(name) is not { } address)
        {
            throw new InvalidOperationException($"No address found with the name '{name}'.");
        }

        if (address.IsDefault && AddressList.Count > 1)
        {
            throw new InvalidOperationException("Cannot remove the default address when multiple addresses exist. Please set another address as default first.");
        }

        AddressList.Remove(address);
    }

    /// <summary>
    /// Updates the address information for the specified entry and optionally sets it as the default address.
    /// </summary>
    /// <param name="name">The current name of the address entry to update.</param>
    /// <param name="newName">The new name to assign to the address entry.</param>
    /// <param name="newAddress">The new address details to associate with the entry.</param>
    /// <param name="makeDefault">A value indicating whether the updated address should be set as the default address.</param>
    public void UpdateAddress([NotNull] string name, [NotNull] string newName, Address newAddress, bool makeDefault)
    {
        name = name.Trim();
        newName = newName.Trim();

        if (GetAddress(name) is not { } address)
        {
            throw new InvalidOperationException($"No address found with the name '{name}'.");
        }

        if (!name.Equals(newName, StringComparison.OrdinalIgnoreCase) &&
            GetAddress(newName) is not null)
        {
            throw new InvalidOperationException($"An address with the name '{newName}' already exists.");
        }

        address.Name = newName;
        address.Address = newAddress;
        SetDefaultAddress(address, makeDefault);
    }

    /// <summary>
    /// Sets the address with the specified name as the default address.
    /// </summary>
    /// <param name="name">The name of the address to set as default.</param>
    public void SetDefaultAddress([NotNull] string name)
    {
        name = name.Trim();
        if (GetAddress(name) is not { } address)
        {
            throw new InvalidOperationException($"No address found with the name '{name}'.");
        }

        SetDefaultAddress(address, true);
    }

    private CustomerAddress? GetAddress(string name)
        => AddressList.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    private void SetDefaultAddress(CustomerAddress address, bool makeDefault)
    {
        if (!makeDefault)
        {
            return;
        }

        foreach (var addr in AddressList)
        {
            addr.IsDefault = false;
        }

        address.IsDefault = true;
    }
}
