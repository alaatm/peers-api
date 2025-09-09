using FluentValidation;
using Peers.Core.Commands;
using Peers.Resources;
using static Peers.Core.Test.MockBuilder;

namespace Peers.Core.Test.Commands;

public class PhoneNumberValidatorTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("invalid phone", false)]
    [InlineData("5511111111", false)]
    [InlineData("0511111111", false)]
    [InlineData("+971511111111", true)]
    [InlineData("+971111111111", false)]
    [InlineData("+971051111111", false)]
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
            Assert.Contains(result.Errors.Select(p => p.ErrorMessage), p => p == "'Phone Number' must be a valid phone number.");
        }
    }

    private record TestCommand(string PhoneNumber);

    private class TestValidator : AbstractValidator<TestCommand>
    {
        public TestValidator() => RuleFor(p => p.PhoneNumber).PhoneNumber(new SLMoq<res>());
    }
}
