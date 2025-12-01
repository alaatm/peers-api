using Peers.Modules.Users.Commands;

namespace Peers.Modules.Test.Users.Commands.Validation;

public class EnrollConfirmTests : CommandValidatorTestBase<EnrollConfirm.Command, EnrollConfirm.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckLen(p => p.Otp, 4);
        CheckNotEmpty(p => p.PreferredLanguage);
        CheckLen(p => p.PreferredLanguage, 2, 5);
        CheckNotEmpty(p => p.Username);
        CheckPhone(p => p.PhoneNumber);
        CheckOk();
    }

    protected override EnrollConfirm.Command GetValidCommand() => TestEnrollConfirm();
}
