using Peers.Modules.Users.Commands;

namespace Peers.Modules.Test.Users.Commands.Validation;

public class ResetPasswordTests : CommandValidatorTestBase<ResetPassword.Command, ResetPassword.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckEmail(p => p.Username);
        CheckOk();
    }

    protected override ResetPassword.Command GetValidCommand() => new("email@contoso.com");
}
