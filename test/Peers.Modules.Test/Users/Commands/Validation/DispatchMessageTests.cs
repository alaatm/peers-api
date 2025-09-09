using FluentValidation.TestHelper;
using Peers.Modules.Users.Commands;

namespace Peers.Modules.Test.Users.Commands.Validation;

public class DispatchMessageTests : CommandValidatorTestBase<DispatchMessage.Command, DispatchMessage.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckNotNull(p => p.Title);
        CheckNotNull(p => p.Body);

        var cmd1 = GetValidCommand() with { Title = [] };
        var result = Validator.TestValidate(cmd1);
        Assert.Single(result.Errors);
        result.Assert(p => p.Title, true);

        var cmd2 = GetValidCommand() with { Body = [] };
        result = Validator.TestValidate(cmd2);
        Assert.Single(result.Errors);
        result.Assert(p => p.Body, true);

        CheckOk();
    }

    protected override DispatchMessage.Command GetValidCommand() => new(null, new() { { "en", "title" } }, new() { { "en", "body" } });
}
