using System.Diagnostics;
using FirebaseAdmin.Messaging;
using Mashkoor.Core.Communication.Push;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users;

public static class FirebaseMessagingServiceExtensions
{
    public const string CustomersTopic = "customers";

    /// <summary>
    /// Subscribes the user to the appropriate topic.
    /// </summary>
    /// <param name="firebase">The firebase messaging service.</param>
    /// <param name="user">The user.</param>
    /// <param name="pnsHandle">The fcm token.</param>
    /// <returns></returns>
    /// <exception cref="UnreachableException"></exception>
    public static async Task<TopicManagementResponse?> SubscribeUserTopicAsync(
        [NotNull] this IFirebaseMessagingService firebase,
        [NotNull] AppUser user,
        string? pnsHandle)
    {
        if (pnsHandle is null)
        {
            return null;
        }

        return await firebase.SubscribeToTopicAsync(pnsHandle, $"{CustomersTopic}-{user.PreferredLanguage}");
    }

    /// <summary>
    /// Subscribes the user to the appropriate topic.
    /// </summary>
    /// <param name="firebase">The firebase messaging service.</param>
    /// <param name="user">The user.</param>
    /// <param name="pnsHandle">The fcm token.</param>
    /// <returns></returns>
    /// <exception cref="UnreachableException"></exception>
    public static async Task<TopicManagementResponse?> UnsubscribeUserTopicAsync(
        [NotNull] this IFirebaseMessagingService firebase,
        [NotNull] AppUser user,
        string? pnsHandle)
    {
        if (pnsHandle is null)
        {
            return null;
        }

        return await firebase.UnsubscribeFromTopicAsync(pnsHandle, $"{CustomersTopic}-{user.PreferredLanguage}");
    }
}
