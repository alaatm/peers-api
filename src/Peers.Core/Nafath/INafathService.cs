using Peers.Core.Nafath.Models;
using System.Security.Claims;

namespace Peers.Core.Nafath;

/// <summary>
/// Represents the Nafath service interface.
/// </summary>
public interface INafathService
{
    /// <summary>
    /// Sends a request to the Nafath service to initiate a verification process.
    /// </summary>
    /// <param name="locale">The locale for the request.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="nationalId">The national ID/Iqama number.</param>
    /// <param name="ctk">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="NafathSendRequestResponse"/></returns>
    Task<NafathSendRequestResponse> SendRequestAsync(string locale, int userId, string nationalId, CancellationToken ctk = default);
    /// <summary>
    /// Asynchronously retrieves the status of a Nafath authentication request for the specified user and transaction.
    /// </summary>
    /// <param name="nationalId">The national ID/Iqama number of the user whose request status is being retrieved.</param>
    /// <param name="transactionId">The unique identifier of the Nafath transaction associated with the authentication request.</param>
    /// <param name="random">A random string used to correlate or validate the request.</param>
    /// <param name="ctk">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="NafathRequestStatus"/></returns>
    Task<NafathRequestStatus> RetrieveRequestStatusAsync(string nationalId, Guid transactionId, string random, CancellationToken ctk = default);
    /// <summary>
    /// Validates the authentication token contained in the specified Nafath callback response asynchronously and
    /// returns the associated claims principal.
    /// </summary>
    /// <param name="callbackResponse">The Nafath callback response containing the authentication token to validate.</param>
    /// <param name="ctk">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ClaimsPrincipal"/>
    /// representing the authenticated user if the token is valid; otherwise, an exception is thrown.</returns>
    Task<ClaimsPrincipal> ValidateTokenAsync(NafathCallbackResponse callbackResponse, CancellationToken ctk = default);
}
