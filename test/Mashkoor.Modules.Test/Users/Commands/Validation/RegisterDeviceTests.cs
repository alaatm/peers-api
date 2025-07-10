using Mashkoor.Modules.Users.Commands;

namespace Mashkoor.Modules.Test.Users.Commands.Validation;

public class RegisterDeviceTests : CommandValidatorTestBase<RegisterDevice.Command, RegisterDevice.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckNotEmpty(p => p.Id);
        CheckNotEmpty(p => p.Manufacturer);
        CheckNotEmpty(p => p.Model);
        CheckNotEmpty(p => p.Platform);
        CheckNotEmpty(p => p.OSVersion);
        CheckNotEmpty(p => p.Idiom);
        CheckNotEmpty(p => p.Type);
        CheckNotEmpty(p => p.PnsHandle);
        CheckNotEmpty(p => p.App);
        CheckNotEmpty(p => p.AppVersion);
        CheckOk();
    }

    protected override RegisterDevice.Command GetValidCommand() => TestRegisterDevice.Generate();
}
