using Peers.Core.Domain.Rules;
using Peers.Modules.Customers.Domain;

namespace Peers.Modules.Customers.Rules;

public sealed class CanDeleteCustomerAccountRule : BusinessRule
{
    private readonly IStringLocalizer _l;
    private readonly Customer _customer;

    public override string ErrorTitle => _l["Error deleting user account"];

    public CanDeleteCustomerAccountRule(Customer customer)
    {
        _l = StringLocalizerFactory.Create(typeof(res));
        _customer = customer;
    }

    public override bool IsBroken()
    {
        _ = _customer.User;
        return Errors.Any();
    }
}
