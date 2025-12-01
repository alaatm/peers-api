using Microsoft.AspNetCore.Http.HttpResults;
using Peers.Modules.Users.Commands;
using Peers.Modules.Users.Commands.Responses;
using Peers.Modules.Users.Domain;
using Peers.Modules.Users.Queries;

namespace Peers.Modules.Users.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapAccountEndpoints(this RouteGroupBuilder ep)
    {
        var gAccounts = ep
            .MapGroup("/account")
            .WithTags("Accounts");

        var gUsers = ep
            .MapGroup("/users")
            .WithTags("Users");

        var gDevices = ep
            .MapGroup("/devices")
            .WithTags("Devices");

        gAccounts.MapPost("/me/delete", (IMediator mediator)
            => mediator.Send(new DeleteAccount.Command()))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);

        gUsers.MapPost("/me/devices", (IMediator mediator, RegisterDevice.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<IdObj>(StatusCodes.Status201Created)
            .Produces<RegisterDevice.Response>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status204NoContent);

        gUsers.MapPut("/me/preferred-language", (IMediator mediator, SetPreferredLanguage.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status204NoContent);

        gUsers.MapPut("/me/devices/{deviceId:guid}/pns", (Guid deviceId, IMediator mediator, UpdatePnsHandle.Command cmd)
            => mediator.Send(cmd with { DeviceId = deviceId }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);

        gUsers.MapPut("/{id:int}/status", (int id, IMediator mediator, ChangeStatus.Command cmd)
            => mediator.Send(cmd with { Id = id }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);

        gUsers.MapPost("/me/send-email-verification", (IMediator mediator)
            => mediator.Send(new SendEmailVerification.Command()))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);

        gUsers.MapPut("/me/profile", (IMediator mediator, SetProfile.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status409Conflict)
            .Produces<GetProfile.Response>(StatusCodes.Status200OK);

        gUsers.MapPost("/me/initialize", (IMediator mediator, Initialize.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<Initialize.Response>(StatusCodes.Status200OK);

        gDevices.MapPost("/message-dispatch", (IMediator mediator, DispatchMessage.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status204NoContent);

        /* *********** Queries ************* */

        gDevices.MapGet("/crash-reports", (int? page, int? pageSize, string? sortField, string? sortOrder, string? filters, IMediator mediator)
            => mediator.Send(new ListCrashReports.Query(page, pageSize, sortField, sortOrder, filters)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<PagedQueryResponse<DeviceError>>(StatusCodes.Status200OK);

        gDevices.MapGet("/push-notification-problems", (int? page, int? pageSize, string? sortField, string? sortOrder, string? filters, IMediator mediator)
            => mediator.Send(new ListPushNotificationProblems.Query(page, pageSize, sortField, sortOrder, filters)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<PagedQueryResponse<PushNotificationProblem>>(StatusCodes.Status200OK);

        gUsers.MapGet("/{id:int}/devices", (int id, int? page, int? pageSize, string? sortField, string? sortOrder, string? filters, IMediator mediator)
            => mediator.Send(new ListDevices.Query(id, page, pageSize, sortField, sortOrder, filters)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<PagedQueryResponse<ListDevices.Response>>(StatusCodes.Status200OK);

        gUsers.MapGet("/me/profile", (IMediator mediator)
            => mediator.Send(new GetProfile.Query(null)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<GetProfile.Response>(StatusCodes.Status200OK);

        gUsers.MapGet("/{id:int}/profile", (int id, IMediator mediator)
            => mediator.Send(new GetProfile.Query(id)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<GetProfile.Response>(StatusCodes.Status200OK);

        gUsers.MapGet("/{id:int}/app-usage", (int id, int? page, int? pageSize, string? sortField, string? sortOrder, string? filters, IMediator mediator)
            => mediator.Send(new ListAppUsageHistory.Query(id, page, pageSize, sortField, sortOrder, filters)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<PagedQueryResponse<AppUsageHistory>>(StatusCodes.Status200OK);

        gDevices.MapPost("/{deviceId:guid}/client-task", (Guid deviceId, IMediator mediator, ClientTaskRequest.Command cmd)
            => mediator.Send(cmd with { DeviceId = deviceId }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<ClientTaskRequest.RequestResponse>(StatusCodes.Status200OK)
            .Produces<ClientTaskRequest.AcknowledgeResponse>(StatusCodes.Status202Accepted);

        return ep;
    }

    public static RouteGroupBuilder MapPublicAccountEndpoints(this RouteGroupBuilder ep)
    {
        var gAccounts = ep
            .MapGroup("/account")
            .WithTags("Accounts");

        var gOAuth = ep
            .MapGroup("/oauth")
            .WithTags("OAuth");

        var gDevices = ep
            .MapGroup("/devices")
            .WithTags("Devices");

        gAccounts.MapPost("/enroll", EnrollCommandSwitcher)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<JwtResponse>(StatusCodes.Status200OK);

        gOAuth.MapPost("/token", TokenCommandSwitcher)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<JwtResponse>(StatusCodes.Status200OK);

        gAccounts.MapPut("/password", PasswordCommandSwitcher)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status202Accepted);

        gDevices.MapPost("/{deviceId:guid}/crash-reports", (string key, Guid deviceId, IMediator mediator, ReportDeviceError.Command cmd)
            => mediator.Send(cmd with { Key = key, DeviceId = deviceId }))
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<IdObj>(StatusCodes.Status201Created);

        gDevices.MapPut("/{deviceId:guid}/client-task/ack", (Guid deviceId, IMediator mediator, ClientTaskAcknowledge.Command cmd)
            => mediator.Send(cmd with { DeviceId = deviceId }))
            .Produces<NoContent>();

        return ep;
    }

    internal static async Task<IResult> EnrollCommandSwitcher(
        IMediator mediator,
        Register cmd) => cmd.Enroll switch
        {
            null => cmd.EnrollConfirm switch
            {
                null => Result.BadRequest("Malformed input."),
                _ => await mediator.Send(cmd.EnrollConfirm),
            },
            _ => await mediator.Send(cmd.Enroll),
        };

    internal static async Task<IResult> TokenCommandSwitcher(
        IMediator mediator,
        Login cmd) => cmd.SignIn switch
        {
            null => cmd.CreateToken switch
            {
                null => Result.BadRequest("Malformed input."),
                _ => await mediator.Send(cmd.CreateToken),
            },
            _ => await mediator.Send(cmd.SignIn),
        };

    internal static async Task<IResult> PasswordCommandSwitcher(
        IMediator mediator,
        UpdatePassword cmd) => cmd.ChangePassword switch
        {
            null => cmd.ResetPassword switch
            {
                null => cmd.ResetPasswordConfirm switch
                {
                    null => Result.BadRequest("Malformed input."),
                    _ => await mediator.Send(cmd.ResetPasswordConfirm),
                },
                _ => await mediator.Send(cmd.ResetPassword),
            },
            _ => await mediator.Send(cmd.ChangePassword),
        };

    /// <summary>
    /// Authenticates a user and generates a JWT token for subsequent requests.
    ///
    /// ### Authentication flow
    ///
    /// #### When a valid refresh token is available
    ///    1. Use the `createToken` command with the refresh token to obtain a new JWT token.
    ///       - The `grantType` should be set to `RefreshToken` (value `2`).
    ///       - The `password` should be set to the refresh token string.
    ///
    /// #### When no valid refresh token is available
    ///    1. Use the `signIn` command with the user's credentials to obtain a new JWT token.
    ///    2. The backend sends an enrollment OTP to the user's phone number.
    ///    3. Send another request and use the `createToken` command with the OTP to complete enrollment.
    ///       - The `grantType` should be set to `Mfa` (value `0`).
    ///       - The `password` should be set to the OTP string.
    ///       
    /// **Note:** Upon successful authentication, a new refresh token is issued and returned alongside the JWT token.
    /// You should securely store this refresh token for future JWT issuance without requiring re-authentication.
    /// </summary>
    public sealed class Login
    {
        public SignIn.Command? SignIn { get; set; }
        public CreateToken.Command? CreateToken { get; set; }
    }

    /// <summary>
    /// Enrolls a new user in the system.
    /// 
    /// ### Enrollment flow
    ///
    /// 1. Use the `enroll` command to start the enrollment process.
    /// 2. The backend sends an enrollment OTP to the user's phone number.
    /// 3. Use the `enrollConfirm` command with the OTP to complete enrollment.
    ///       
    /// **Note:** Upon successful enrollment, a refresh token is issued and returned alongside the JWT token.
    /// You should securely store this refresh token for future JWT issuance without requiring re-authentication.
    /// </summary>
    public sealed class Register
    {
        public Enroll.Command? Enroll { get; set; }
        public EnrollConfirm.Command? EnrollConfirm { get; set; }
    }

    public sealed class UpdatePassword
    {
        public ChangePassword.Command? ChangePassword { get; set; }
        public ResetPassword.Command? ResetPassword { get; set; }
        public ResetPasswordConfirm.Command? ResetPasswordConfirm { get; set; }
    }
}
