using Peers.Core.Communication.Sms.Configuration;

namespace Peers.Core.Communication.Sms;

/// <summary>
/// SMS service.
/// </summary>
public sealed class SmsService : ISmsService
{
    private readonly ISmsServiceProvider _provider;
    private readonly SmsConfig _config;
    private readonly ILogger<SmsService> _log;

    public SmsService(
        ISmsServiceProvider provider,
        SmsConfig config,
        ILogger<SmsService> log)
    {
        _provider = provider;
        _config = config;
        _log = log;
    }

    /// <summary>
    /// Asynchronously sends an SMS to a recipient.
    /// </summary>
    /// <param name="recipient">The recipient.</param>
    /// <param name="message">The message.</param>
    public async Task SendAsync(string recipient, string message)
    {
        if (!_config.Enabled)
        {
            _log.SmsServiceDisabled();
            return;
        }

        await _provider.SendAsync(recipient, message);
    }
}
