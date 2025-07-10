using Mashkoor.Modules.Users.Commands;

namespace Mashkoor.Modules.Test.Users.Commands.Validation;

public class ReportDeviceErrorTests : CommandValidatorTestBase<ReportDeviceError.Command, ReportDeviceError.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckNotEmpty(p => p.Key);
        CheckNotEmpty(p => p.DeviceId);
        CheckOk();
    }

    protected override ReportDeviceError.Command GetValidCommand() => new(Guid.NewGuid(), null, null, default, null, null, null, null, null, null, null, "123");
}
