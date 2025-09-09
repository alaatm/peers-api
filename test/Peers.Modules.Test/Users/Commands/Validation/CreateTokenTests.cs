using Peers.Modules.Users.Commands;

namespace Peers.Modules.Test.Users.Commands.Validation;

public class CreateTokenTests : CommandValidatorTestBase<CreateToken.Command, CreateToken.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckEnum(p => p.GrantType);
        CheckNotEmpty(p => p.Username);
        CheckNotEmpty(p => p.Password);
        CheckOk();
    }

    protected override CreateToken.Command GetValidCommand() => TestCreateToken(default);
}
