using Mashkoor.Modules.Users.Commands;

namespace Mashkoor.Modules.Test.Users.Commands.Validation;

public class UpdatePnsHandleTests : CommandValidatorTestBase<UpdatePnsHandle.Command, UpdatePnsHandle.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckNotEmpty(p => p.DeviceId);
        Check(GetValidCommand(), p => p.PnsHandle, null, false);
        Check(GetValidCommand(), p => p.PnsHandle, "", true);
        Check(GetValidCommand(), p => p.PnsHandle, "1", false);
        CheckOk();
    }

    protected override UpdatePnsHandle.Command GetValidCommand() => new(Guid.NewGuid(), Guid.NewGuid().ToString());
}
