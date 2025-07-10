using Mashkoor.Modules.Users.Commands;

namespace Mashkoor.Modules.Test.Users.Commands.Validation;

public class ChangeStatusTests : CommandValidatorTestBase<ChangeStatus.Command, ChangeStatus.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckId(p => p.Id);
        CheckEnum(p => p.NewStatus);
        CheckNotEmpty(p => p.ChangeReason);
        CheckOk();
    }

    protected override ChangeStatus.Command GetValidCommand() => new(1, default, "test");
}
