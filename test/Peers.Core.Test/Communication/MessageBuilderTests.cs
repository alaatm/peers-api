using FirebaseAdmin.Messaging;
using Peers.Core.Communication;
using Peers.Resources;
using static Peers.Core.Test.MockBuilder;

namespace Peers.Core.Test.Communication;

public class MessageBuilderTests
{
    [Fact]
    public void Add_single_user_single_handle_adds_as_message()
    {
        // Arrange
        var builder = MessageBuilder.Create(new SLCultureMoq<res>());

        // Act
        var (messages, multicastMessages) = builder
            .Add(
                new SimpleUser
                {
                    PreferredLanguage = "en",
                    UserHandles = ["111"],
                },
                new Dictionary<string, string>() { { "k", "v" } },
                "Title, {0}",
                "Hello, {0}",
                ["elo"],
                ["John"])
            .Generate();

        // Assert
        var message = Assert.Single(messages);
        Assert.Equal("en:Title, elo", message.Notification.Title);
        Assert.Equal("en:Hello, John", message.Notification.Body);
        Assert.Null(message.Topic);
        var data = Assert.Single(message.Data);
        Assert.Equal("k", data.Key);
        Assert.Equal("v", data.Value);
        Assert.Equal("111", message.Token);
        Assert.Empty(multicastMessages);
        Assert.Equal(Priority.High, message.Android.Priority);
    }

    [Fact]
    public void Add_single_user_multi_handle_adds_as_multicastMessage()
    {
        // Arrange
        var builder = MessageBuilder.Create(new SLCultureMoq<res>());

        // Act
        var (messages, multicastMessages) = builder
            .Add(
                new SimpleUser
                {
                    PreferredLanguage = "en",
                    UserHandles = ["111", "222"],
                },
                new Dictionary<string, string>() { { "k", "v" } },
                "Title, {0}",
                "Hello, {0}",
                ["elo"],
                ["John"])
            .Generate();

        // Assert
        Assert.Empty(messages);
        var multicastMessage = Assert.Single(multicastMessages);
        Assert.Equal("en:Title, elo", multicastMessage.Notification.Title);
        Assert.Equal("en:Hello, John", multicastMessage.Notification.Body);
        var data = Assert.Single(multicastMessage.Data);
        Assert.Equal("k", data.Key);
        Assert.Equal("v", data.Value);
        Assert.Equal(["111", "222"], multicastMessage.Tokens);
        Assert.Equal(Priority.High, multicastMessage.Android.Priority);
    }

    [Fact]
    public void Add_multi_user_single_or_multi_handle_adds_as_multicastMessage_per_language()
    {
        // Arrange
        var builder = MessageBuilder.Create(new SLCultureMoq<res>());

        // Act
        var (messages, multicastMessages) = builder
            .Add(
                [
                    new SimpleUser
                    {
                        PreferredLanguage = "en",
                        UserHandles = ["111"],
                    },
                    new SimpleUser
                    {
                        PreferredLanguage = "en",
                        UserHandles = ["222", "333"],
                    },
                    new SimpleUser
                    {
                        PreferredLanguage = "ar",
                        UserHandles = ["444"],
                    },
                    new SimpleUser
                    {
                        PreferredLanguage = "ar",
                        UserHandles = ["555", "666"],
                    },
                ],
                new Dictionary<string, string>() { { "k", "v" } },
                "Title, {0}",
                "Hello, {0}",
                ["elo"],
                ["John"])
            .Generate();

        // Assert
        Assert.Empty(messages);
        Assert.Equal(2, multicastMessages.Count);

        var multicastMessage1 = multicastMessages[0];
        Assert.Equal("en:Title, elo", multicastMessage1.Notification.Title);
        Assert.Equal("en:Hello, John", multicastMessage1.Notification.Body);
        var data = Assert.Single(multicastMessage1.Data);
        Assert.Equal("k", data.Key);
        Assert.Equal("v", data.Value);
        Assert.Equal(["111", "222", "333"], multicastMessage1.Tokens);
        Assert.Equal(Priority.High, multicastMessage1.Android.Priority);

        var multicastMessage2 = multicastMessages[1];
        Assert.Equal("ar:Title, elo", multicastMessage2.Notification.Title);
        Assert.Equal("ar:Hello, John", multicastMessage2.Notification.Body);
        data = Assert.Single(multicastMessage2.Data);
        Assert.Equal("k", data.Key);
        Assert.Equal("v", data.Value);
        Assert.Equal(["444", "555", "666"], multicastMessage2.Tokens);
        Assert.Equal(Priority.High, multicastMessage1.Android.Priority);
    }

    [Fact]
    public void Add_multi_user_single_or_multi_handle_with_callback_args_resolver_adds_as_multicastMessage_per_language()
    {
        // Arrange
        var builder = MessageBuilder.Create(new SLCultureMoq<res>());

        // Act
        var (messages, multicastMessages) = builder
            .Add(
                [
                    new SimpleUser
                    {
                        PreferredLanguage = "en",
                        UserHandles = ["111"],
                    },
                    new SimpleUser
                    {
                        PreferredLanguage = "en",
                        UserHandles = ["222", "333"],
                    },
                    new SimpleUser
                    {
                        PreferredLanguage = "ar",
                        UserHandles = ["444"],
                    },
                    new SimpleUser
                    {
                        PreferredLanguage = "ar",
                        UserHandles = ["555", "666"],
                    },
                ],
                new Dictionary<string, string>() { { "k", "v" } },
                "Title, {0} | {1}",
                "Hello, {0}",
                [
                    (lang) => lang == "en" ? "1-Title-EN" : (lang == "ar" ? "1-Title-AR" : ""),
                    (lang) => lang == "en" ? "2-Title-EN" : (lang == "ar" ? "2-Title-AR" : ""),
                ],
                [(lang) => lang == "en" ? "Body-EN" : (lang == "ar" ? "Body-AR" : "")])
            .Generate();

        // Assert
        Assert.Empty(messages);
        Assert.Equal(2, multicastMessages.Count);

        var multicastMessage1 = multicastMessages[0];
        Assert.Equal("en:Title, 1-Title-EN | 2-Title-EN", multicastMessage1.Notification.Title);
        Assert.Equal("en:Hello, Body-EN", multicastMessage1.Notification.Body);
        var data = Assert.Single(multicastMessage1.Data);
        Assert.Equal("k", data.Key);
        Assert.Equal("v", data.Value);
        Assert.Equal(["111", "222", "333"], multicastMessage1.Tokens);
        Assert.Equal(Priority.High, multicastMessage1.Android.Priority);

        var multicastMessage2 = multicastMessages[1];
        Assert.Equal("ar:Title, 1-Title-AR | 2-Title-AR", multicastMessage2.Notification.Title);
        Assert.Equal("ar:Hello, Body-AR", multicastMessage2.Notification.Body);
        data = Assert.Single(multicastMessage2.Data);
        Assert.Equal("k", data.Key);
        Assert.Equal("v", data.Value);
        Assert.Equal(["444", "555", "666"], multicastMessage2.Tokens);
        Assert.Equal(Priority.High, multicastMessage1.Android.Priority);
    }

    [Fact]
    public void Add_multi_user_single_or_multi_handle_with_callback_args_resolver_adds_as_multicastMessage_per_language_handles_null_funcArgsCallback()
    {
        // Arrange
        var builder = MessageBuilder.Create(new SLCultureMoq<res>());

        // Act
        var (messages, multicastMessages) = builder
            .Add(
                [
                    new SimpleUser
                    {
                        PreferredLanguage = "en",
                        UserHandles = ["111"],
                    },
                    new SimpleUser
                    {
                        PreferredLanguage = "ar",
                        UserHandles = ["222"],
                    },
                ],
                new Dictionary<string, string>() { { "k", "v" } },
                "Title",
                "Hello",
                (Func<string, string>[])null,
                (Func<string, string>[])null)
            .Generate();

        // Assert
        Assert.Equal(2, messages.Count);
        Assert.Empty(multicastMessages);

        var message1 = messages[0];
        Assert.Equal("en:Title", message1.Notification.Title);
        Assert.Equal("en:Hello", message1.Notification.Body);

        var message2 = messages[1];
        Assert.Equal("ar:Title", message2.Notification.Title);
        Assert.Equal("ar:Hello", message2.Notification.Body);
    }

    [Fact]
    public void Add_single_handle_with_data_adds_as_message()
    {
        // Arrange
        var builder = MessageBuilder.Create(new SLCultureMoq<res>());
        var data = new Dictionary<string, string>
        {
            { "k", "v" },
        };

        // Act
        var (messages, multicastMessages) = builder
            .Add(
                "111",
                data)
            .Generate();

        // Assert
        var message = Assert.Single(messages);
        Assert.Null(message.Notification);
        Assert.Equal("111", message.Token);
        Assert.Null(message.Topic);
        Assert.Same(data, message.Data);
        Assert.Empty(multicastMessages);
        Assert.Equal(Priority.High, message.Android.Priority);
        Assert.True(message.Apns.Aps.ContentAvailable);
        Assert.Equal("background", message.Apns.Headers["apns-push-type"]);
        Assert.Equal("5", message.Apns.Headers["apns-priority"]);
    }

    [Fact]
    public void Add_single_handle_adds_as_message()
    {
        // Arrange
        var builder = MessageBuilder.Create(new SLCultureMoq<res>());

        // Act
        var (messages, multicastMessages) = builder
            .Add(
                "111",
                "Title",
                "Body")
            .Generate();

        // Assert
        var message = Assert.Single(messages);
        Assert.Equal("Title", message.Notification.Title);
        Assert.Equal("Body", message.Notification.Body);
        Assert.Equal("111", message.Token);
        Assert.Null(message.Topic);
        Assert.Null(message.Data);
        Assert.Empty(multicastMessages);
        Assert.Equal(Priority.High, message.Android.Priority);
    }

    [Fact]
    public void Topic_adds_as_message()
    {
        // Arrange
        var builder = MessageBuilder.Create(new SLCultureMoq<res>());

        // Act
        var (messages, multicastMessages) = builder
            .Topic(
                "topic",
                "Title",
                "Body")
            .Generate();

        // Assert
        var message = Assert.Single(messages);
        Assert.Equal("Title", message.Notification.Title);
        Assert.Equal("Body", message.Notification.Body);
        Assert.Equal("topic", message.Topic);
        Assert.Null(message.Token);
        Assert.Null(message.Data);
        Assert.Empty(multicastMessages);
        Assert.Equal(Priority.High, message.Android.Priority);
    }

    [Theory]
    [InlineData("title", null, true)]
    [InlineData(null, "body", true)]
    [InlineData(null, null, false)]
    public void Add_sets_notification_when_title_or_body_is_set(string title, string body, bool notificationNotNull)
    {
        // Arrange
        var builder = MessageBuilder.Create(new SLCultureMoq<res>());

        // Act
        var (messages, multicastMessages) = builder
            .Add(
                new SimpleUser { PreferredLanguage = "en", UserHandles = ["111"] },
                new Dictionary<string, string> { { "k", "v" }, },
                title,
                body,
                null,
                null)
            .Generate();

        // Assert
        var message = Assert.Single(messages);
        if (notificationNotNull)
        {
            Assert.NotNull(message.Notification);
        }
        else
        {
            Assert.Null(message.Notification);
        }
    }

    [Fact]
    public void Add_throws_when_null_data_and_null_title_and_body()
    {
        // Arrange
        var builder = MessageBuilder.Create(new SLCultureMoq<res>());
        var user = new SimpleUser { UserHandles = ["111"], PreferredLanguage = "en" };

        var expectedError = "Either data or title/body or both must have values.";

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() => builder.Add(user, data: null));
        Assert.Equal(expectedError, ex.Message);

        ex = Assert.Throws<InvalidOperationException>(() => builder.Add(user, title: null, body: null, null, null));
        Assert.Equal(expectedError, ex.Message);

        ex = Assert.Throws<InvalidOperationException>(() => builder.Add([user], data: null));
        Assert.Equal(expectedError, ex.Message);

        ex = Assert.Throws<InvalidOperationException>(() => builder.Add([user], title: null, body: null, null, null));
        Assert.Equal(expectedError, ex.Message);
    }

    [Fact]
    public void Add_throws_when_empty_data_and_null_title_and_body()
    {
        // Arrange
        var builder = MessageBuilder.Create(new SLCultureMoq<res>());
        var user = new SimpleUser { UserHandles = ["111"], PreferredLanguage = "en" };
        var data = new Dictionary<string, string>();

        var expectedError = "Either data or title/body or both must have values.";

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() => builder.Add(user, data));
        Assert.Equal(expectedError, ex.Message);

        ex = Assert.Throws<InvalidOperationException>(() => builder.Add([user], data));
        Assert.Equal(expectedError, ex.Message);
    }
}
