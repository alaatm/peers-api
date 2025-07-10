using Mashkoor.Modules.Users.Commands;

namespace Mashkoor.Modules.Test.Users.Commands.Validation;

public class EnrollTests : CommandValidatorTestBase<Enroll.Command, Enroll.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckPhone(p => p.Username);
        CheckOk();
    }

    protected override Enroll.Command GetValidCommand() => TestEnroll();
}
