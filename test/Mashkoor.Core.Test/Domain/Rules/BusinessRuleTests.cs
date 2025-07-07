using Mashkoor.Core.Domain.Rules;

namespace Mashkoor.Core.Test.Domain.Rules;

public class BusinessRuleTests
{
    [Fact]
    public void Append_appends_error()
    {
        var rule = new Rule();
        rule.CallAppend("message");
        Assert.Equal("message", Assert.Single(rule.Errors));
    }

    [Fact]
    public void Check_appends_error_message_when_condition_is_true()
    {
        // Arrange
        var rule = new Rule();

        // Act
        rule.CallCheck(true, "message");

        // Assert
        Assert.Equal(Assert.Single(rule.Errors), rule.Errors.Single());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Check_returns_condition_value(bool condition)
    {
        // Arrange
        var rule = new Rule();

        // Act and assert
        Assert.Equal(condition, rule.CallCheck(condition, "message"));
    }

    private class Rule : BusinessRule
    {
        public override string ErrorTitle => "error title";
        public Rule() { }
        public override bool IsBroken() => throw new NotImplementedException();
        public bool CallAppend(string message) => Append(message);
        public bool CallCheck(bool condition, string message) => Check(condition, message);
    }
}
