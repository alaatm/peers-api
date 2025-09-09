using Peers.Core.Http;
using Peers.Modules.Users.Commands;
using Peers.Modules.Users.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Peers.Modules.Test.Users.Endpoints;

public class EndpointRouteBuilderExtensionsTests
{
    #region EnrollCommandSwitcher
    [Fact]
    public async Task EnrollCommandSwitcher_returns_BadRequest_when_Enroll_and_EnrollConfirm_are_both_null()
    {
        // Arrange
        var cmd = new EndpointRouteBuilderExtensions.Register { Enroll = null, EnrollConfirm = null };
        var mediatorMoq = new Mock<IMediator>(MockBehavior.Strict);

        // Act
        var result = await EndpointRouteBuilderExtensions.EnrollCommandSwitcher(mediatorMoq.Object, cmd);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Malformed input.", problem.Detail);

        mediatorMoq.VerifyAll();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EnrollCommandSwitcher_sends_Enroll_command_when_its_not_null_regardless_of_EnrollConfirm_command_value(bool enrollConfirmHasValue)
    {
        // Arrange
        var enrollCmd = TestEnroll().Generate();
        var cmd = new EndpointRouteBuilderExtensions.Register { Enroll = enrollCmd, EnrollConfirm = enrollConfirmHasValue ? TestEnrollConfirm().Generate() : null };

        var mediatorMoq = new Mock<IMediator>(MockBehavior.Strict);
        mediatorMoq
            .Setup(m => m.Send(enrollCmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok("success"))
            .Verifiable();

        // Act
        var result = await EndpointRouteBuilderExtensions.EnrollCommandSwitcher(mediatorMoq.Object, cmd);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal("success", okResult.Value);
        mediatorMoq.VerifyAll();
    }

    [Fact]
    public async Task EnrollCommandSwitcher_sends_EnrollConfirm_command_when_its_not_null_and_Enroll_command_is_null()
    {
        // Arrange
        var enrollConfirmCmd = TestEnrollConfirm().Generate();
        var cmd = new EndpointRouteBuilderExtensions.Register { Enroll = null, EnrollConfirm = enrollConfirmCmd };

        var mediatorMoq = new Mock<IMediator>(MockBehavior.Strict);
        mediatorMoq
            .Setup(m => m.Send(enrollConfirmCmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok("success"))
            .Verifiable();

        // Act
        var result = await EndpointRouteBuilderExtensions.EnrollCommandSwitcher(mediatorMoq.Object, cmd);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal("success", okResult.Value);
        mediatorMoq.VerifyAll();
    }
    #endregion

    #region TokenCommandSwitcher
    [Fact]
    public async Task TokenCommandSwitcher_returns_BadRequest_when_SignIn_and_CreateToken_are_both_null()
    {
        // Arrange
        var cmd = new EndpointRouteBuilderExtensions.Login { SignIn = null, CreateToken = null };
        var mediatorMoq = new Mock<IMediator>(MockBehavior.Strict);

        // Act
        var result = await EndpointRouteBuilderExtensions.TokenCommandSwitcher(mediatorMoq.Object, cmd);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Malformed input.", problem.Detail);

        mediatorMoq.VerifyAll();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TokenCommandSwitcher_sends_SignIn_command_when_its_not_null_regardless_of_CreateToken_command_value(bool createTokenHasValue)
    {
        // Arrange
        var signInCmd = TestSignIn().Generate();
        var cmd = new EndpointRouteBuilderExtensions.Login { SignIn = signInCmd, CreateToken = createTokenHasValue ? TestCreateToken(default).Generate() : null };

        var mediatorMoq = new Mock<IMediator>(MockBehavior.Strict);
        mediatorMoq
            .Setup(m => m.Send(signInCmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok("success"))
            .Verifiable();

        // Act
        var result = await EndpointRouteBuilderExtensions.TokenCommandSwitcher(mediatorMoq.Object, cmd);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal("success", okResult.Value);
        mediatorMoq.VerifyAll();
    }

    [Fact]
    public async Task TokenCommandSwitcher_sends_CreateToken_command_when_its_not_null_and_SignIn_command_is_null()
    {
        // Arrange
        var createTokenCmd = TestCreateToken(default).Generate();
        var cmd = new EndpointRouteBuilderExtensions.Login { SignIn = null, CreateToken = createTokenCmd };

        var mediatorMoq = new Mock<IMediator>(MockBehavior.Strict);
        mediatorMoq
            .Setup(m => m.Send(createTokenCmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok("success"))
            .Verifiable();

        // Act
        var result = await EndpointRouteBuilderExtensions.TokenCommandSwitcher(mediatorMoq.Object, cmd);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal("success", okResult.Value);
        mediatorMoq.VerifyAll();
    }
    #endregion

    #region PasswordCommandSwitcher
    [Fact]
    public async Task PasswordCommandSwitcher_returns_BadRequest_when_ChangePassword_and_ResetPassword_and_ResetPasswordConfirm_are_all_null()
    {
        // Arrange
        var cmd = new EndpointRouteBuilderExtensions.UpdatePassword { ChangePassword = null, ResetPassword = null, ResetPasswordConfirm = null };
        var mediatorMoq = new Mock<IMediator>(MockBehavior.Strict);

        // Act
        var result = await EndpointRouteBuilderExtensions.PasswordCommandSwitcher(mediatorMoq.Object, cmd);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Malformed input.", problem.Detail);

        mediatorMoq.VerifyAll();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task PasswordCommandSwitcher_sends_ChangePassword_command_when_its_not_null_regardless_of_other_values(bool resetPasswordHasValue, bool resetPasswordConfirmHasValue)
    {
        // Arrange
        var changePasswordCmd = TestChangePassword.Generate();
        var cmd = new EndpointRouteBuilderExtensions.UpdatePassword
        {
            ChangePassword = changePasswordCmd,
            ResetPassword = resetPasswordHasValue ? new ResetPassword.Command(default) : null,
            ResetPasswordConfirm = resetPasswordConfirmHasValue ? new ResetPasswordConfirm.Command(default, default, default) : null,
        };

        var mediatorMoq = new Mock<IMediator>(MockBehavior.Strict);
        mediatorMoq
            .Setup(m => m.Send(changePasswordCmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok("success"))
            .Verifiable();

        // Act
        var result = await EndpointRouteBuilderExtensions.PasswordCommandSwitcher(mediatorMoq.Object, cmd);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal("success", okResult.Value);
        mediatorMoq.VerifyAll();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PasswordCommandSwitcher_sends_ResetPassword_command_when_its_not_null_and_ChangePassword_command_is_null_regardless_of_ResetPasswordConfirm_value(bool resetPasswordConfirmHasValue)
    {
        // Arrange
        var resetPasswordCmd = new ResetPassword.Command(default);
        var cmd = new EndpointRouteBuilderExtensions.UpdatePassword
        {
            ChangePassword = null,
            ResetPassword = resetPasswordCmd,
            ResetPasswordConfirm = resetPasswordConfirmHasValue ? new ResetPasswordConfirm.Command(default, default, default) : null,
        };

        var mediatorMoq = new Mock<IMediator>(MockBehavior.Strict);
        mediatorMoq
            .Setup(m => m.Send(resetPasswordCmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok("success"))
            .Verifiable();

        // Act
        var result = await EndpointRouteBuilderExtensions.PasswordCommandSwitcher(mediatorMoq.Object, cmd);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal("success", okResult.Value);
        mediatorMoq.VerifyAll();
    }

    [Fact]
    public async Task PasswordCommandSwitcher_sends_ResetPasswordConfirm_command_when_its_not_null_and_the_others_are_all_null()
    {
        // Arrange
        var resetPasswordConfirmCmd = new ResetPasswordConfirm.Command(default, default, default);
        var cmd = new EndpointRouteBuilderExtensions.UpdatePassword
        {
            ChangePassword = null,
            ResetPassword = null,
            ResetPasswordConfirm = resetPasswordConfirmCmd,
        };

        var mediatorMoq = new Mock<IMediator>(MockBehavior.Strict);
        mediatorMoq
            .Setup(m => m.Send(resetPasswordConfirmCmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok("success"))
            .Verifiable();

        // Act
        var result = await EndpointRouteBuilderExtensions.PasswordCommandSwitcher(mediatorMoq.Object, cmd);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal("success", okResult.Value);
        mediatorMoq.VerifyAll();
    }
    #endregion
}
