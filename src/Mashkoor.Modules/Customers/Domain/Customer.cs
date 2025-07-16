using System.Diagnostics;
using Mashkoor.Modules.Customers.Rules;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Customers.Domain;

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
        };

        customer.User.DisplayName = customer.User.Firstname;

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
}
