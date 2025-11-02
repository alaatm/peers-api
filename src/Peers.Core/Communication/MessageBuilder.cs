using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FirebaseAdmin.Messaging;

namespace Peers.Core.Communication;

/// <summary>
/// Utility class for building push notification messages.
/// </summary>
public sealed class MessageBuilder
{
    private static readonly AndroidConfig _defaultAndroidConfig = new()
    {
        DirectBootOk = true,
        Priority = Priority.High,
    };

    private static readonly ApnsConfig _silentNotificationApnsConfig = new()
    {
        Aps = new Aps
        {
            ContentAvailable = true,
        },
        Headers = new Dictionary<string, string>
        {
            ["apns-push-type"] = "background",
            ["apns-priority"] = "5",
        },
    };

    private readonly IStrLoc _l = default!;
    private readonly List<Message> _messages = [];
    private readonly List<MulticastMessage> _multicastMessages = [];

    private MessageBuilder(IStrLoc l)
        => _l = l;

    /// <summary>
    /// Creates a new instance of <see cref="MessageBuilder"/>.
    /// </summary>
    /// <param name="l">The localizer.</param>
    /// <returns></returns>
    public static MessageBuilder Create(IStrLoc l)
        => new(l);

    /// <summary>
    /// Adds a message to be dispatched to the specified recipient with the specified data.
    /// </summary>
    /// <param name="recipient">The notification recipient.</param>
    /// <param name="data">The notification data.</param>
    /// <returns></returns>
    public MessageBuilder Add(
        [NotNull] SimpleUser recipient,
        [NotNull] IReadOnlyDictionary<string, string> data)
        => Add(
            recipient: recipient,
            data: data,
            title: null,
            body: null,
            titleArgs: null,
            bodyArgs: null);

    /// <summary>
    ///
    /// </summary>
    /// <param name="recipient">The notification recipient.</param>
    /// <param name="title"> The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <param name="titleArgs"> The arguments used in localized output for the title.</param>
    /// <param name="bodyArgs"> The arguments used in localized output for the body.</param>
    /// <returns></returns>
    public MessageBuilder Add(
        [NotNull] SimpleUser recipient,
        string? title,
        [NotNull] string body,
        object[]? titleArgs,
        object[]? bodyArgs)
        => Add(
            recipient: recipient,
            data: null,
            title: title,
            body: body,
            titleArgs: titleArgs,
            bodyArgs: bodyArgs);

    /// <summary>
    ///
    /// </summary>
    /// <param name="recipient">The notification recipient.</param>
    /// <param name="data">The notification data.</param>
    /// <param name="title"> The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <param name="titleArgs"> The arguments used in localized output for the title.</param>
    /// <param name="bodyArgs"> The arguments used in localized output for the body.</param>
    /// <returns></returns>
    public MessageBuilder Add(
        [NotNull] SimpleUser recipient,
        IReadOnlyDictionary<string, string>? data,
        string? title,
        string? body,
        object[]? titleArgs,
        object[]? bodyArgs)
        => Add(
            handles: [.. recipient.Handles],
            data: data,
            topic: null,
            title: BuildTitle(recipient.PreferredLanguage, title, titleArgs),
            body: BuildBody(recipient.PreferredLanguage, body, bodyArgs));

    /// <summary>
    ///
    /// </summary>
    /// <param name="recipients">The notification recipients.</param>
    /// <param name="data">The notification data.</param>
    /// <returns></returns>
    public MessageBuilder Add(
        [NotNull] IReadOnlyList<SimpleUser> recipients,
        [NotNull] IReadOnlyDictionary<string, string> data)
        => Add(
            recipients: recipients,
            data: data,
            title: null,
            body: null,
            titleArgs: null,
            bodyArgs: null);

    /// <summary>
    ///
    /// </summary>
    /// <param name="recipients">The notification recipients.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <param name="titleArgs">The arguments used in localized output for the title.</param>
    /// <param name="bodyArgs">The arguments used in localized output for the body.</param>
    /// <returns></returns>
    public MessageBuilder Add(
        [NotNull] IReadOnlyList<SimpleUser> recipients,
        string? title,
        [NotNull] string body,
        object[]? titleArgs,
        object[]? bodyArgs)
        => Add(
            recipients: recipients,
            data: null,
            title: title,
            body: body,
            titleArgs: titleArgs,
            bodyArgs: bodyArgs);

    /// <summary>
    ///
    /// </summary>
    /// <param name="recipients">The notification recipients.</param>
    /// <param name="data">The notification data.</param>
    /// <param name="title"> The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <param name="titleArgs">The arguments used in localized output for the title.</param>
    /// <param name="bodyArgs">The arguments used in localized output for the body.</param>
    /// <returns></returns>
    public MessageBuilder Add(
        [NotNull] IReadOnlyList<SimpleUser> recipients,
        IReadOnlyDictionary<string, string>? data,
        string? title,
        string? body,
        object[]? titleArgs,
        object[]? bodyArgs)
    {
        foreach (var g in recipients.GroupBy(p => p.PreferredLanguage))
        {
            Add(
                handles: [.. g.SelectMany(p => p.Handles)],
                data: data,
                topic: null,
                title: BuildTitle(g.Key, title, titleArgs),
                body: BuildBody(g.Key, body, bodyArgs));
        }

        return this;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="recipients"> The notification recipients.</param>
    /// <param name="data">The notification data.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <param name="titleArgs">The arguments used in localized output for the title.</param>
    /// <param name="bodyArgs">The arguments used in localized output for the body.</param>
    /// <returns></returns>
    public MessageBuilder Add(
        [NotNull] IReadOnlyList<SimpleUser> recipients,
        IReadOnlyDictionary<string, string>? data,
        string? title,
        string? body,
        Func<string, string>[]? titleArgs,
        Func<string, string>[]? bodyArgs)
    {
        foreach (var g in recipients.GroupBy(p => p.PreferredLanguage))
        {
            Add(
                handles: [.. g.SelectMany(p => p.Handles)],
                data: data,
                topic: null,
                title: BuildTitleWithCallback(g.Key, title, titleArgs),
                body: BuildBodyWithCallback(g.Key, body, bodyArgs));
        }

        return this;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="handle">The handle to send to.</param>
    /// <param name="data">The notification data.</param>
    /// <returns></returns>
    public MessageBuilder Add(
        [NotNull] string handle,
        IReadOnlyDictionary<string, string>? data)
        => Add(
            handles: [handle],
            data: data,
            topic: null,
            title: null,
            body: null);

    /// <summary>
    ///
    /// </summary>
    /// <param name="handle">The handle to send to.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <returns></returns>
    public MessageBuilder Add(
        [NotNull] string handle,
        string? title,
        [NotNull] string body)
        => Add(
            handles: [handle],
            data: null,
            topic: null,
            title: title,
            body: body);

    /// <summary>
    ///
    /// </summary>
    /// <param name="topic">The topic subscribers to send to.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <returns></returns>
    public MessageBuilder Topic(
        [NotNull] string topic,
        string? title,
        [NotNull] string body)
        => Add(
            handles: null,
            data: null,
            topic: topic,
            title: title,
            body: body);

    /// <summary>
    /// Returns the built messages.
    /// </summary>
    /// <returns></returns>
    public (IReadOnlyList<Message>, IReadOnlyList<MulticastMessage>) Generate()
        => (_messages.AsReadOnly(), _multicastMessages.AsReadOnly());

    private MessageBuilder Add(
        string[]? handles,
        IReadOnlyDictionary<string, string>? data,
        string? topic,
        string? title,
        string? body)
    {
        var notification = title is not null || body is not null
            ? new Notification
            {
                Title = title,
                Body = body,
            }
            : null;

        var message = new Message
        {
            Notification = notification,
            Data = data,
            Android = _defaultAndroidConfig,
            Apns = notification is null ? _silentNotificationApnsConfig : null,
        };
        var multicastMessage = new MulticastMessage
        {
            Notification = notification,
            Data = data,
            Android = _defaultAndroidConfig,
            Apns = notification is null ? _silentNotificationApnsConfig : null,
        };

        if (handles is not null && handles.Length > 0)
        {
            Debug.Assert(topic is null);

            if ((data is null || data.Count == 0) && title is null && body is null)
            {
                throw new InvalidOperationException("Either data or title/body or both must have values.");
            }

            if (handles.Length == 1)
            {
                message.Token = handles[0];
                _messages.Add(message);
            }
            else
            {
                multicastMessage.Tokens = handles;
                _multicastMessages.Add(multicastMessage);
            }
        }

        if (topic is not null)
        {
            Debug.Assert(handles is null);
            message.Topic = topic;
            _messages.Add(message);
        }

        return this;
    }

    private string? BuildTitle(string lang, string? title, object[]? args)
    {
        if (title is not null)
        {
            var uiCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            var message = _l[title, args ?? []];
            Thread.CurrentThread.CurrentUICulture = uiCulture;
            return message;
        }

        return null;
    }

    private string? BuildBody(string lang, string? body, object[]? args)
    {
        if (body is not null)
        {
            var uiCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            var message = _l[body, args ?? []];
            Thread.CurrentThread.CurrentUICulture = uiCulture;
            return message;
        }

        return null;
    }

    private string? BuildTitleWithCallback(string lang, string? title, Func<string, string>[]? funcArgs)
    {
        if (title is not null)
        {
            var uiCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            var message = _l[title];
            Thread.CurrentThread.CurrentUICulture = uiCulture;

            var args = funcArgs?.Select(p => p(lang)).ToArray() ?? Array.Empty<object>();
            return string.Format(CultureInfo.InvariantCulture, message, args);
        }

        return null;
    }

    private string? BuildBodyWithCallback(string lang, string? body, Func<string, string>[]? funcArgs)
    {
        if (body is not null)
        {
            var uiCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            var message = _l[body];
            Thread.CurrentThread.CurrentUICulture = uiCulture;

            var args = funcArgs?.Select(p => p(lang)).ToArray() ?? Array.Empty<object>();
            return string.Format(CultureInfo.InvariantCulture, message, args);
        }

        return null;
    }
}
