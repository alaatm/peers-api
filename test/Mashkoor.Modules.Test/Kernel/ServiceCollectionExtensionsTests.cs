using System.Reflection;
using System.Text.Json;
using FluentValidation;
using Mashkoor.Core.AzureServices.Storage;
using Mashkoor.Core.Background;
using Mashkoor.Core.Commands;
using Mashkoor.Core.Communication.Email;
using Mashkoor.Core.Communication.Push;
using Mashkoor.Core.Communication.Sms;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Core.Identity;
using Mashkoor.Core.Security.Jwt;
using Mashkoor.Core.Security.Totp;
using Mashkoor.Modules.Kernel.Pipelines;
using Mashkoor.Modules.Kernel.Startup;
using MediatR;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Mashkoor.Modules.Test.Kernel;

public class ServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void AddMashkoor_adds_all_required_services(string envName)
    {
        // Arrange
#pragma warning disable CS0618 // Used by AppInsights
        var hostEnvMoq = new Mock<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
#pragma warning restore CS0618 // Type or member is obsolete
        hostEnvMoq.SetupGet(p => p.EnvironmentName).Returns(envName).Verifiable();

        var webHostEnvMoq = new Mock<IWebHostEnvironment>(MockBehavior.Strict);
        webHostEnvMoq.SetupGet(p => p.EnvironmentName).Returns(envName).Verifiable();

        var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            { "Logging:LogLevel:Default", "None" },
            { "Logging:Console:LogLevel:Default", "None" },
            { "ConnectionStrings:Default", "Server=.;Database=DummyDb;" },
            { "azure:storageConnectionString", "UseDevelopmentStorage=true" },
            { "jwt:issuer", "https://www.jwt-test.com/iss" },
            { "jwt:key", Convert.ToBase64String(new byte[32]) },
            { "jwt:durationInMinutes", "10" },
            { "sms:sender", "Mashkoor" },
            { "sms:key", "123" },
            { "sms:enabled", "false" },
            { "email:host", "smtp.mashkoor.com" },
            { "email:username", "username" },
            { "email:password", "password" },
            { "email:senderName", "Mashkoor" },
            { "email:senderEmail", "email@mashkoor.com" },
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

        var serviceCollection = new ServiceCollection();

        // Act
        var serviceProvider = serviceCollection
            .AddLogging()
            .AddSingleton(Mock.Of<IWebHostEnvironment>())
            .AddSingleton(hostEnvMoq.Object)
            .AddMashkoor(config, webHostEnvMoq.Object)
            .BuildServiceProvider();

        // Assert
        var hostedServices = serviceProvider.GetServices<IHostedService>().ToArray();
        Assert.Contains(hostedServices, p => p.GetType() == typeof(StartupBackgroundService));
        Assert.Contains(hostedServices, p => p.GetType() == typeof(MessageBroker));

        var context = serviceProvider.GetRequiredService<MashkoorContext>();
        Assert.NotNull(context.Producer);

        serviceProvider.GetRequiredService<IActionDescriptorProvider>();
        var telemetryService = serviceProvider.GetService<ITelemetryInitializer>();

        var jsonHttpOptions = serviceProvider.GetRequiredService<IOptions<JsonOptions>>();
        var serializerOptions = jsonHttpOptions.Value.JsonSerializerOptions;
        Assert.True(serializerOptions.PropertyNameCaseInsensitive);
        Assert.False(serializerOptions.WriteIndented);
        Assert.Same(JsonNamingPolicy.CamelCase, serializerOptions.PropertyNamingPolicy);

        if (envName == Environments.Development)
        {
            Assert.Null(telemetryService);
        }
        else if (envName == Environments.Production)
        {
            Assert.NotNull(telemetryService);
        }
        else
        {
            Assert.Fail("Invalid environment name.");
        }

        webHostEnvMoq.VerifyAll();
    }

    [Fact]
    public void AddMashkoor_with_explicit_db_config_adds_all_required_services()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Logging:LogLevel:Default", "None" },
                { "Logging:Console:LogLevel:Default", "None" },
                { "azure:storageConnectionString", "UseDevelopmentStorage=true" },
                { "firebase:serviceAccountKey", IntegrationTestBase.TestFirebaseServiceAccount },
                { "jwt:issuer", "https://www.jwt-test.com/iss" },
                { "jwt:key", Convert.ToBase64String(new byte[32]) },
                { "jwt:durationInMinutes", "10" },
                { "totp:useDefaultOtp", "false" },
                { "totp:duration" , "00:03:00" },
                { "sms:sender", "Mashkoor" },
                { "sms:key", "123" },
                { "sms:enabled", "false" },
                { "email:host", "smtp.mashkoor.com" },
                { "email:username", "username" },
                { "email:password", "password" },
                { "email:senderName", "Mashkoor" },
                { "email:senderEmail", "email@mashkoor.com" },
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

        // Act
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddMashkoor(
                config,
                cfg => cfg.UseInMemoryDatabase(nameof(AddMashkoor_adds_all_required_services)),
                Assembly.GetExecutingAssembly())
            .BuildServiceProvider();

        // Assert
        var jsonOptions = serviceProvider.GetRequiredService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        Assert.False(jsonOptions.Value.SerializerOptions.WriteIndented);
        Assert.True(jsonOptions.Value.SerializerOptions.PropertyNameCaseInsensitive);
        Assert.Equal(JsonNamingPolicy.CamelCase, jsonOptions.Value.SerializerOptions.PropertyNamingPolicy);

        var pipeLines = serviceProvider.GetServices<IPipelineBehavior<TestCommand, IResult>>().ToArray();
        Assert.Equal(4, pipeLines.Length);
        Assert.IsType<LoggingBehavior<TestCommand, IResult>>(pipeLines[0]);
        Assert.IsType<AuthorizationBehavior<TestCommand, IResult>>(pipeLines[1]);
        Assert.IsType<CommandValidationBehavior<TestCommand, IResult>>(pipeLines[2]);
        Assert.IsType<IdentityCheckBehavior<TestCommand, IResult>>(pipeLines[3]);

        var context = serviceProvider.GetRequiredService<MashkoorContext>();
        Assert.NotNull(context.Producer);

        Assert.Same(TimeProvider.System, serviceProvider.GetRequiredService<TimeProvider>());
        serviceProvider.GetRequiredService<IStringLocalizer<ServiceCollectionExtensionsTests>>();
        serviceProvider.GetRequiredService<IFirebaseMessagingWrapper>();
        serviceProvider.GetRequiredService<IFirebaseMessagingService>();
        serviceProvider.GetRequiredService<IPushNotificationService>();
        serviceProvider.GetRequiredService<ISmsService>();
        Assert.IsType<TaqnyatSmsServiceProvider>(serviceProvider.GetRequiredService<ISmsServiceProvider>());
        serviceProvider.GetRequiredService<IEmailService>();
        serviceProvider.GetRequiredService<IIdentityInfo>();
        serviceProvider.GetRequiredService<JwtConfig>();
        serviceProvider.GetRequiredService<IValidateOptions<JwtConfig>>();
        serviceProvider.GetRequiredService<IOptions<RateLimiterOptions>>();
        serviceProvider.GetRequiredService<ITotpTokenProvider>();
        serviceProvider.GetRequiredService<IMediator>();
        serviceProvider.GetRequiredService<IStorageManager>();
        serviceProvider.GetRequiredService<IPushNotificationProblemReporter>();
        serviceProvider.GetRequiredService<IMemoryCache>();

        serviceProvider.GetRequiredService<IProducer>();
        Assert.Equal(1024, serviceProvider.GetRequiredService<IEnumerable<IConsumer>>().Count());
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.Contains(hostedServices, p => p.GetType() == typeof(MessageBroker));
    }

    public record TestCommand : ICommand, IValidatable;
    public class TestCommandValidator : AbstractValidator<TestCommand> { }
}
