using Mashkoor.Modules.Users.Commands;

namespace Mashkoor.Modules.Test.Users.Commands.Validation;

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
