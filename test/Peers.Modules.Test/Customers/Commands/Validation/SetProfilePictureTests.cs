using Peers.Modules.Customers.Commands;

namespace Peers.Modules.Test.Customers.Commands.Validation;

public class SetProfilePictureTests : CommandValidatorTestBase<SetProfilePicture.Command, SetProfilePicture.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckNotNull(p => p.File);
        CheckOk();
    }

    protected override SetProfilePicture.Command GetValidCommand() => new(new());
}
