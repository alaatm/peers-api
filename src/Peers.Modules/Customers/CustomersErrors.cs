using Peers.Core.Domain.Errors;
using static Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Customers;

public static class CustomersErrors
{
    /// <summary>
    /// Payment card not found.
    /// </summary>
    public static DomainError PaymentCardNotFound => new(Titles.NotFound, "customers.payment-card-not-found");
    /// <summary>
    /// Cannot delete a payment card that has pending payments associated with it.
    /// </summary>
    public static DomainError CannotDeletePaymentCardWithPendingPayments => new(Titles.CannotApplyOperation, "customers.cannot-delete-payment-card-with-pending-payments");
    /// <summary>
    /// You have pending orders. Cannot delete customer account.
    /// </summary>
    public static DomainError CannotDeleteCustomerAccountWithPendingOrders => new(Titles.CannotApplyOperation, "customers.cannot-delete-customer-account-with-pending-orders");
}
