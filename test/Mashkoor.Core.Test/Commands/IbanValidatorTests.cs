using FluentValidation;
using Mashkoor.Core.Commands;
using Mashkoor.Resources;
using static Mashkoor.Core.Test.MockBuilder;

namespace Mashkoor.Core.Test.Commands;

public class IbanValidatorTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("invalid iban", false)]
    [InlineData("SS6315000000000123456789", false)]
    [InlineData("AE0015000000000123456789", false)]
    [InlineData("AE0115000000000123456789", false)]
    [InlineData("AE9915000000000123456789", false)]
    [InlineData("AE6315000000500123456789", false)]
    [InlineData("AE6015000000000123456789", false)]
    [InlineData("AE631500000000012345678", false)]
    [InlineData("AE 07 033 1234567890123456", true)]
    [InlineData("AE07 033 1234567890123456", true)]
    [InlineData("AE07-033-1234567890123456", true)]
    public void ValidatorTests(string iban, bool expectedResult)
    {
        // Arrange
        var validator = new TestValidator();

        // Act
        var result = validator.Validate(new TestCommand(iban));

        // Assert
        Assert.Equal(expectedResult, result.IsValid);
        if (!result.IsValid)
        {
            Assert.Contains(result.Errors.Select(p => p.ErrorMessage), p => p == "'Iban' must be a valid IBAN.");
        }
    }

    private record TestCommand(string Iban);

    private class TestValidator : AbstractValidator<TestCommand>
    {
        public TestValidator() => RuleFor(p => p.Iban).IsIban(new SLMoq<res>());
    }
}
