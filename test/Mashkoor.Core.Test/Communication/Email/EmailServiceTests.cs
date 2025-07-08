using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using Mashkoor.Core.Communication.Email;
using Mashkoor.Core.Communication.Email.Configuration;

namespace Mashkoor.Core.Test.Communication.Email;

public class EmailServiceTests
{
    private static readonly EmailConfig _config = new()
    {
        Host = "smtp.Mashkoor.com",
        Username = "un",
        Password = "pw",
        SenderName = "Mashkoor",
        SenderEmail = "email@Mashkoor.com",
        Port = 995,
        EnableSsl = true,
        Enabled = true,
    };

    [Fact]
    public async Task SendAsync_throws_when_invalid_email()
    {
        // Arrange
        var service = new EmailService(Mock.Of<ISmtpClient>(), new EmailConfig { Enabled = true }, Mock.Of<ILogger<EmailService>>());

        // Act & assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await service.SendAsync("subject", "body", "invalid"));
        Assert.Equal("Recipient email must be a valid email address. (Parameter 'recipient')", ex.Message);
    }

    [Fact]
    public async Task SendAsync_does_not_send_email_when_service_is_disabled()
    {
        // Arrange
        var smtpMoq = new Mock<ISmtpClient>(MockBehavior.Strict);
        var service = new EmailService(smtpMoq.Object, new EmailConfig { Enabled = false }, Mock.Of<ILogger<EmailService>>());

        // Act
        await service.SendAsync("subject", "body", "john.doe@xyz.com");

        // Assert
        smtpMoq.VerifyAll();
    }

    [Fact]
    public async Task SendAsync_sends_email()
    {
        // Arrange
        var subject = "subject";
        var body = "body";
        var to = "john.doe@xyz.com";

        var smtpMoq = new Mock<ISmtpClient>(MockBehavior.Strict);
        smtpMoq.Setup(p => p.ConnectAsync(_config.Host, _config.Port, _config.EnableSsl, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        smtpMoq.SetupGet(p => p.AuthenticationMechanisms).Returns([]);
        smtpMoq.Setup(p => p.AuthenticateAsync(_config.Username, _config.Password, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        // TODO: need to match the message instead of It.IsAny<MimeMessage>()
        smtpMoq.Setup(p => p.SendAsync(It.IsAny<MimeMessage>(), default, default)).Returns(Task.FromResult(""));
        smtpMoq.Setup(p => p.DisconnectAsync(true, default)).Returns(Task.CompletedTask);

        var service = new EmailService(smtpMoq.Object, _config, Mock.Of<ILogger<EmailService>>());

        // Act
        await service.SendAsync(subject, body, to);

        // Assert
        smtpMoq.VerifyAll();
    }

    [Theory]
    [MemberData(nameof(SendAsync_catches_defined_errors_Data))]
    public async Task SendAsync_catches_defined_errors_and_disconnects(Exception exception)
    {
        var smtpMoq = new Mock<ISmtpClient>(MockBehavior.Strict);
        smtpMoq.Setup(p => p.ConnectAsync(_config.Host, _config.Port, _config.EnableSsl, It.IsAny<CancellationToken>())).ThrowsAsync(exception);
        smtpMoq.Setup(p => p.DisconnectAsync(true, default)).Returns(Task.CompletedTask);

        var service = new EmailService(smtpMoq.Object, _config, Mock.Of<ILogger<EmailService>>());

        // Act
        await service.SendAsync("subject", "body", "email@example.com");

        // Assert
        smtpMoq.VerifyAll();
    }

#pragma warning disable IDE0028 // Simplify collection initialization - Invalid suggestion
    public static TheoryData<Exception> SendAsync_catches_defined_errors_Data() => new()
    {
        new AuthenticationException(),
        new DummyProtocolException(),
        new ServiceNotConnectedException(),
        new ServiceNotAuthenticatedException(),
        new SmtpCommandException(default, default, "test"),
    };
#pragma warning restore IDE0028 // Simplify collection initialization

    private class DummyProtocolException : ProtocolException { }
}
