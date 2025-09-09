using Peers.Modules.Users.Commands;

namespace Peers.Modules.Test.Users.Commands.Validation;

public class ChangePasswordTests : CommandValidatorTestBase<ChangePassword.Command, ChangePassword.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckLen(p => p.CurrentPassword, 6);
        CheckLen(p => p.NewPassword, 6);
        Check(GetValidCommand() with { CurrentPassword = "000000" }, p => p.NewPassword, "000000", true);
        Check(GetValidCommand() with { NewPassword = "000000" }, p => p.NewPasswordConfirm, "111111", true);
        Check(GetValidCommand() with { NewPassword = "000000" }, p => p.NewPasswordConfirm, "000000", false);

        var cmd = GetValidCommand() with { CurrentPassword = "000000", NewPassword = "000000", NewPasswordConfirm = "000000" };
        Check(cmd, p => p.NewPassword, "000000", true);

        CheckOk();
    }

    protected override ChangePassword.Command GetValidCommand()
        => TestChangePassword.Generate();
}
