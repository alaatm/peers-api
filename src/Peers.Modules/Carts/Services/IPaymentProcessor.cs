namespace Peers.Modules.Carts.Services;

public interface IPaymentProcessor
{
    Task HandleAsync(string paymentId, string? sessionId);
}
