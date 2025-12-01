using FluentValidation;
using Peers.Core.Commands;
using Peers.Resources;
using static Peers.Core.Test.MockBuilder;

namespace Peers.Core.Test.Commands;

public class UsernameValidatorTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("invalid username", false)]
    [InlineData("5ggggg", false)]
    [InlineData("ab", false)]
    [InlineData("lkjd@fed", false)]
    [InlineData("john_doe", true)]
    public void ValidatorTests(string phoneNumber, bool expectedResult)
    {
        // Arrange
        var validator = new TestValidator();

        // Act
        var result = validator.Validate(new TestCommand(phoneNumber));

        // Assert
        Assert.Equal(expectedResult, result.IsValid);
        if (!result.IsValid)
        {
            Assert.Contains(result.Errors.Select(p => p.ErrorMessage), p => p == "'Username' must start with a letter and can contain only letters, numbers, and underscores, with a minimum length of 4 characters.");
        }
    }

    private record TestCommand(string Username);

    private class TestValidator : AbstractValidator<TestCommand>
    {
        public TestValidator() => RuleFor(p => p.Username).Username(new SLMoq<res>());
    }
}
