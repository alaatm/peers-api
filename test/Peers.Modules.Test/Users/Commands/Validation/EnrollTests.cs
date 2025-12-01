using Peers.Modules.Users.Commands;

namespace Peers.Modules.Test.Users.Commands.Validation;

public class EnrollTests : CommandValidatorTestBase<Enroll.Command, Enroll.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckNotEmpty(p => p.Username);
        CheckPhone(p => p.PhoneNumber);
        CheckOk();
    }

    protected override Enroll.Command GetValidCommand() => TestEnroll();
}
