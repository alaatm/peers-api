using Microsoft.Extensions.Logging;
using Peers.Core.Communication.Sms;
using Peers.Core.Communication.Sms.Configuration;

namespace Peers.Core.Test.Communication.Sms;

public class SmsServiceTests
{
    [Fact]
    public async Task SendAsync_does_not_send_sms_when_service_is_disabled()
    {
        // Arrange
        var sms = new Mock<ISmsServiceProvider>();
        var service = new SmsService(sms.Object, new SmsConfig { Enabled = false }, Mock.Of<ILogger<SmsService>>());

        // Act
        await service.SendAsync("1234567890", "Hello, world!");

        // Assert
        sms.VerifyAll();
    }

    [Fact]
    public async Task SendAsync_forwards_request_to_provider()
    {
        // Arrange
        var sms = new Mock<ISmsServiceProvider>();
        var service = new SmsService(sms.Object, new SmsConfig { Enabled = true }, Mock.Of<ILogger<SmsService>>());

        sms.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new TaqnyatResponse());

        // Act
        await service.SendAsync("1234567890", "Hello, world!");

        // Assert
        sms.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
