using Peers.Modules.Users.Commands;

namespace Peers.Modules.Test.Users.Commands.Validation;

public class ResetPasswordConfirmTests : CommandValidatorTestBase<ResetPasswordConfirm.Command, ResetPasswordConfirm.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckLen(p => p.Otp, 6, 6);
        CheckEmail(p => p.Username);
        CheckLen(p => p.NewPassword, 6);
        CheckOk();
    }

    protected override ResetPasswordConfirm.Command GetValidCommand() => new("123456", "email@contoso.com", "P@ssword");
}
