using Mashkoor.Modules.Users.Commands;

namespace Mashkoor.Modules.Test.Users.Commands.Validation;

public class SignInTests : CommandValidatorTestBase<SignIn.Command, SignIn.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckPhone(p => p.PhoneNumber);
        CheckOk();
    }

    protected override SignIn.Command GetValidCommand() => new(TestPhoneNumber(), null);
}
