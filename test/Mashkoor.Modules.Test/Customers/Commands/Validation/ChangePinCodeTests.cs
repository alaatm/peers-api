using Mashkoor.Modules.Customers.Commands;

namespace Mashkoor.Modules.Test.Customers.Commands.Validation;

public class ChangePinCodeTests : CommandValidatorTestBase<ChangePinCode.Command, ChangePinCode.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckLen(p => p.CurrentPinCode, 6);
        CheckLen(p => p.PinCode, 6);
        Check(GetValidCommand() with { CurrentPinCode = "000000" }, p => p.PinCode, "000000", true);
        Check(GetValidCommand() with { PinCode = "000000" }, p => p.PinCodeConfirmation, "111111", true);
        Check(GetValidCommand() with { PinCode = "000000" }, p => p.PinCodeConfirmation, "000000", false);

        var cmd = GetValidCommand() with { CurrentPinCode = "000000", PinCode = "000000", PinCodeConfirmation = "000000" };
        Check(cmd, p => p.PinCode, "000000", true);

        CheckOk();
    }

    protected override ChangePinCode.Command GetValidCommand() => new("123456", "222222", "222222");
}
