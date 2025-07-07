using Mashkoor.Core.Domain;
using Mashkoor.Core.Domain.Rules;

namespace Mashkoor.Core.Test.Domain;

public class BusinessRuleValidationExceptionTests
{
    [Fact]
    public void Ctor_sets_broken_rule()
    {
        // Arrange
        var error = new Rule();

        // Act
        var ex = new BusinessRuleValidationException(error);

        // Assert
        Assert.Same(error, ex.BrokenRule);
        Assert.Equal($"{error.ErrorTitle}:{Environment.NewLine}", ex.Message);
    }

    [Fact]
    public void Throws_on_unsupported_ctor_overloads()
    {
        Assert.Throws<NotImplementedException>(() => new BusinessRuleValidationException());
        Assert.Throws<NotImplementedException>(() => new BusinessRuleValidationException("message"));
        Assert.Throws<NotImplementedException>(() => new BusinessRuleValidationException("message", new InvalidOperationException()));
    }

    private class Rule : BusinessRule
    {
        public override string ErrorTitle => "error title";
        public Rule() { }
        public override bool IsBroken() => throw new NotImplementedException();
    }
}
