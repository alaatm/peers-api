using FluentValidation.TestHelper;
using Mashkoor.Modules.Users.Commands;

namespace Mashkoor.Modules.Test.Users.Commands.Validation;

public class SetProfileTests : CommandValidatorTestBase<SetProfile.Command, SetProfile.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        var cmd = GetValidCommand();

        Validator
            .TestValidate(cmd with { Email = "invalid" })
            .Assert(p => p.Email, true);

        Validator
            .TestValidate(cmd with { Email = "email@example.com" })
            .Assert(p => p.Email, false);

        CheckOk();
    }

    protected override SetProfile.Command GetValidCommand() => new(default, default, default, default);
}
