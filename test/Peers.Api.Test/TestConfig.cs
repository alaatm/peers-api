using Microsoft.Extensions.Configuration;

namespace Peers.Api.Test;

public static class TestConfig
{
    private const string TestFirebaseServiceAccount = /*lang=json,strict*/ @"{
  ""type"": ""service_account"",
  ""project_id"": ""test"",
  ""private_key_id"": ""xxx"",
  ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIBVgIBADANBgkqhkiG9w0BAQEFAASCAUAwggE8AgEAAkEAq7BFUpkGp3+LQmlQ\nYx2eqzDV+xeG8kx/sQFV18S5JhzGeIJNA72wSeukEPojtqUyX2J0CciPBh7eqclQ\n2zpAswIDAQABAkAgisq4+zRdrzkwH1ITV1vpytnkO/NiHcnePQiOW0VUybPyHoGM\n/jf75C5xET7ZQpBe5kx5VHsPZj0CBb3b+wSRAiEA2mPWCBytosIU/ODRfq6EiV04\nlt6waE7I2uSPqIC20LcCIQDJQYIHQII+3YaPqyhGgqMexuuuGx+lDKD6/Fu/JwPb\n5QIhAKthiYcYKlL9h8bjDsQhZDUACPasjzdsDEdq8inDyLOFAiEAmCr/tZwA3qeA\nZoBzI10DGPIuoKXBd3nk/eBxPkaxlEECIQCNymjsoI7GldtujVnr1qT+3yedLfHK\nsrDVjIT3LsvTqw==\n-----END PRIVATE KEY-----\n"",
  ""client_email"": ""firebase-adminsdk-xxxxxx@test.iam.gserviceaccount.com"",
  ""client_id"": ""xxx"",
  ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
  ""token_uri"": ""https://oauth2.googleapis.com/token"",
  ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
  ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-xxxxx%40test.iam.gserviceaccount.com""
}";

    public static IConfiguration Configuration { get; } = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            { "Logging:LogLevel:Default", "None" },
            { "Logging:Console:LogLevel:Default", "None" },
            { "ConnectionStrings:Default", "Server=.\\V22;Database=Dummy" },
            { "azure:storageConnectionString", "UseDevelopmentStorage=true" },
            { "jwt:issuer", "https://integration-tests.com/iss" },
            { "jwt:key", Convert.ToBase64String(new byte[32]) },
            { "jwt:durationInMinutes", "10" },
            { "firebase:serviceAccountKey", TestFirebaseServiceAccount },
            { "sms:sender", "Peers" },
            { "sms:key", "123" },
            { "sms:enabled", "false" },
            { "email:host", "smtp.peers.com" },
            { "email:username", "username" },
            { "email:password", "password" },
            { "email:senderName", "Peers" },
            { "email:senderEmail", "email@peers.com" },
            { "email:port", "995" },
            { "email:enableSsl", "true" },
            { "email:enabled", "false" },
            { "rateLimiting:perUserRateLimit:queueLimit", "0" },
            { "rateLimiting:perUserRateLimit:tokenLimit", "300" },
            { "rateLimiting:perUserRateLimit:tokensPerPeriod", "300" },
            { "rateLimiting:perUserRateLimit:autoReplenishment", "true" },
            { "rateLimiting:perUserRateLimit:replenishmentPeriod", "60" },
            { "rateLimiting:anonRateLimit:queueLimit", "0" },
            { "rateLimiting:anonRateLimit:tokenLimit", "200" },
            { "rateLimiting:anonRateLimit:tokensPerPeriod", "200" },
            { "rateLimiting:anonRateLimit:autoReplenishment", "true" },
            { "rateLimiting:anonRateLimit:replenishmentPeriod", "60" },
            { "rateLimiting:anonConcurrencyLimit:queueLimit", "0" },
            { "rateLimiting:anonConcurrencyLimit:permitLimit", "10" },
        })
        .Build();
}
