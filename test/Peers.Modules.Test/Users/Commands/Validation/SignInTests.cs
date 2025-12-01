using FluentValidation.TestHelper;
using Peers.Modules.Users.Commands;

namespace Peers.Modules.Test.Users.Commands.Validation;

public class SignInTests : CommandValidatorTestBase<SignIn.Command, SignIn.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        var cmd = GetValidCommand() with { Username = null, PhoneNumber = null };
        var result = Validator.TestValidate(cmd);
        Assert.Equal("Either 'username' or 'phoneNumber' must be provided.", Assert.Single(result.Errors).ErrorMessage);

        cmd = GetValidCommand() with { Username = "", PhoneNumber = "" };
        result = Validator.TestValidate(cmd);
        Assert.Equal("Either 'username' or 'phoneNumber' must be provided.", Assert.Single(result.Errors).ErrorMessage);

        cmd = GetValidCommand() with { Username = " ", PhoneNumber = " " };
        result = Validator.TestValidate(cmd);
        Assert.Equal("Either 'username' or 'phoneNumber' must be provided.", Assert.Single(result.Errors).ErrorMessage);

        cmd = GetValidCommand() with { Username = "z", PhoneNumber = null };
        result = Validator.TestValidate(cmd);
        Assert.Single(result.Errors);
        result.Assert(p => p.Username, true);

        cmd = GetValidCommand() with { Username = null, PhoneNumber = "z" };
        result = Validator.TestValidate(cmd);
        Assert.Single(result.Errors);
        result.Assert(p => p.PhoneNumber, true);

        cmd = GetValidCommand() with { Username = TestUsername(), PhoneNumber = null };
        result = Validator.TestValidate(cmd);
        Assert.Empty(result.Errors);

        cmd = GetValidCommand() with { Username = null, PhoneNumber = TestPhoneNumber() };
        result = Validator.TestValidate(cmd);
        Assert.Empty(result.Errors);

        CheckOk();
    }

    protected override SignIn.Command GetValidCommand() => new(TestUsername(), TestPhoneNumber(), null);
}
