using Mashkoor.Modules.Customers.Commands;

namespace Mashkoor.Modules.Test.Customers.Commands.Validation;

public class CreatePinCodeTests : CommandValidatorTestBase<CreatePinCode.Command, CreatePinCode.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckLen(p => p.PinCode, 6);
        Check(GetValidCommand() with { PinCode = "000000" }, p => p.PinCodeConfirmation, "111111", true);
        Check(GetValidCommand() with { PinCode = "000000" }, p => p.PinCodeConfirmation, "000000", false);
        CheckOk();
    }

    protected override CreatePinCode.Command GetValidCommand() => new("123456", "123456");
}
