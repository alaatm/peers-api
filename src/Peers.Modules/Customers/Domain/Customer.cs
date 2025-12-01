using System.Diagnostics;
using Peers.Core.Domain.Errors;
using Peers.Core.Payments;
using Peers.Modules.Ordering.Domain;
using Peers.Modules.Users.Domain;
using E = Peers.Modules.Customers.CustomersErrors;

namespace Peers.Modules.Customers.Domain;

/// <summary>
/// Represents a system user of type <see cref="Customer"/>.
/// </summary>
public class Customer : Entity, ISystemUser, IAggregateRoot
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
    /// <summary>
    /// A list of addresses associated with the customer.
    /// </summary>
    public List<CustomerAddress> AddressList { get; set; } = default!;
    /// <summary>
    /// The list of payment methods added by this customer.
    /// </summary>
    public List<PaymentMethod> PaymentMethods { get; set; } = default!;
    /// <summary>
    /// The list of orders placed by this customer.
    /// </summary>
    public List<Order> Orders { get; set; } = default!;

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
            PaymentMethods = [],
            Orders = [],
        };

        return customer;
    }

    /// <summary>
    /// Deletes the customer account.
    /// </summary>
    /// <param name="date">The date when the account was deleted.</param>
    public void DeleteAccount(DateTime date)
    {
        if (Orders.Any(p => p.State is not OrderState.Closed and not OrderState.Cancelled))
        {
            throw new DomainException(E.CannotDeleteCustomerAccountWithPendingOrders);
        }

        User.DeleteAccount(date);
        // Set the username to that of the user which has the string "deleted" appended to it.
        Username = User.UserName!;

        foreach (var card in PaymentMethods.Where(p => p.Type is PaymentType.Card).Cast<PaymentCard>())
        {
            DeletePaymentCard(card, date);
        }
    }

    /// <summary>
    /// Retrieves the default address associated with the customer, if one is set.
    /// </summary>
    public Address? GetDefaultAddress()
        => AddressList.Find(a => a.IsDefault)?.Address;

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

    /// <summary>
    /// Adds a new payment card.
    /// </summary>
    /// <param name="paymentId">The payment ID that was used to tokenize this card.</param>
    /// <param name="brand">The card brand.</param>
    /// <param name="funding">The card funding type.</param>
    /// <param name="maskedNumber">The masked card number.</param>
    /// <param name="expiry">The card expiry date.</param>
    /// <param name="token">The card token received from a payment gateway.</param>
    /// <param name="date">The date when the card was added.</param>
    public PaymentCard AddPaymentCard(
        string paymentId,
        PaymentCardBrand? brand,
        PaymentCardFunding? funding,
        string maskedNumber,
        DateOnly? expiry,
        string token,
        DateTime date)
    {
        var card = PaymentCard.Create(paymentId, brand, funding, maskedNumber, expiry, token, date);
        PaymentMethods.Add(card);

        return card;
    }

    /// <summary>
    /// Activates the specified payment card and marks it as the default payment method.
    /// </summary>
    /// <param name="cardToActivate">The payment card to activate.</param>
    /// <param name="brand">Sets the card brand if not already set.</param>
    /// <param name="funding">Sets the card funding type if not already set.</param>
    /// <param name="expiry">Sets the card expiry date if not already set.</param>
    /// <param name="date">The date when the card was activated.</param>
    public void ActivatePaymentCard(
        [NotNull] PaymentCard cardToActivate,
        PaymentCardBrand? brand,
        PaymentCardFunding? funding,
        DateOnly? expiry,
        DateOnly date)
    {
        Debug.Assert(!cardToActivate.IsVerified);
        Debug.Assert(PaymentMethods.Contains(cardToActivate));

        Debug.Assert((cardToActivate.Brand is null && brand is not null) || cardToActivate.Brand is not null);
        Debug.Assert((cardToActivate.Funding is null && funding is not null) || cardToActivate.Funding is not null);
        Debug.Assert((cardToActivate.Expiry is null && expiry is not null) || cardToActivate.Expiry is not null);

        cardToActivate.IsVerified = true;
        cardToActivate.Brand ??= brand;
        cardToActivate.Funding ??= funding;
        cardToActivate.Expiry ??= expiry;
        SetDefaultPaymentMethod(cardToActivate, date);
    }

    /// <summary>
    /// Deletes an existing payment method.
    /// </summary>
    /// <param name="cardToDelete">The payment method to delete.</param>
    /// <param name="date">The date when the payment method was deleted.</param>
    public void DeletePaymentCard(PaymentCard cardToDelete, DateTime date)
    {
        if (cardToDelete is null ||
            !PaymentMethods.Where(p => !p.IsDeleted).Contains(cardToDelete))
        {
            throw new DomainException(E.PaymentCardNotFound);
        }

        if (Orders.Any(p =>
            p.PaymentMethod == cardToDelete &&
            p.State is not OrderState.Closed and not OrderState.Cancelled))
        {
            throw new DomainException(E.CannotDeletePaymentCardWithPendingPayments);
        }

        if (cardToDelete.IsDefault)
        {
            var d = DateOnly.FromDateTime(date);
            var newDefaultPaymentMethod = PaymentMethods
                .Where(p => !p.IsDeleted && p != cardToDelete && p is PaymentCard card && card.IsVerified)
                .Cast<PaymentCard>()
                .Where(p => !p.IsExpired(d))
                .OrderByDescending(p => p.AddedOn)
                .FirstOrDefault() ?? PaymentMethods.Single(p => p.Type is PaymentType.Cash);

            SetDefaultPaymentMethod(newDefaultPaymentMethod, d);
        }

        if (cardToDelete.IsVerified)
        {
            cardToDelete.Delete(date);
        }
        else
        {
            PaymentMethods.Remove(cardToDelete);
        }
    }

    private CustomerAddress? GetAddress(string name)
        => AddressList.Find(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

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

    private void SetDefaultPaymentMethod(PaymentMethod paymentMethod, DateOnly date)
    {
        Debug.Assert(PaymentMethods.Contains(paymentMethod));
        Debug.Assert(!paymentMethod.IsDeleted);
        Debug.Assert((paymentMethod is PaymentCard card && card.IsVerified && !card.IsExpired(date)) || paymentMethod is not PaymentCard);
        Debug.Assert((paymentMethod is ApplePay applePay && applePay.IsActive) || paymentMethod is not ApplePay);

        foreach (var pm in PaymentMethods.Where(p => p != paymentMethod))
        {
            pm.IsDefault = false;
        }

        paymentMethod.IsDefault = true;
    }
}
