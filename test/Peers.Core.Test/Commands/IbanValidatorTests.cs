using FluentValidation;
using Peers.Core.Commands;
using Peers.Resources;
using static Peers.Core.Test.MockBuilder;

namespace Peers.Core.Test.Commands;

public class IbanValidatorTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("invalid iban", false)]
    [InlineData("SS6315000000000123456789", false)]
    [InlineData("SA0015000000000123456789", false)]
    [InlineData("SA0115000000000123456789", false)]
    [InlineData("SA9915000000000123456789", false)]
    [InlineData("SA6315000000500123456789", false)]
    [InlineData("SA6015000000000123456789", false)]
    [InlineData("SA631500000000012345678", false)]
    [InlineData("SA6315000000000123456789", true)]
    [InlineData("SA3705000000000123456789", true)]
    [InlineData("SA8360000000000123456789", true)]
    [InlineData("SA3880000000000123456789", true)]
    [InlineData("SA5750000000000123456789", true)]
    [InlineData("SA0530000000000123456789", true)]
    [InlineData("SA4776000000000123456789", true)]
    [InlineData("SA5185000000000123456789", true)]
    [InlineData("SA6081000000000123456789", true)]
    [InlineData("SA7795000000000123456789", true)]
    [InlineData("SA6490000000000123456789", true)]
    [InlineData("SA7386000000000123456789", true)]
    [InlineData("SA3471000000000123456789", true)]
    [InlineData("SA2575000000000123456789", true)]
    [InlineData("SA8282000000000123456789", true)]
    [InlineData("SA5010000000000123456789", true)]
    [InlineData("SA4445000000000123456789", true)]
    [InlineData("SA3140000000000123456789", true)]
    [InlineData("SA7055000000000123456789", true)]
    [InlineData("SA9665000000000123456789", true)]
    [InlineData("SA0783000000000123456789", true)]
    [InlineData("SA2984000000000123456789", true)]
    [InlineData("sa2984000000000123456789", true)]
    [InlineData("SA29 8400 0000 0001 2345 6789", true)]
    [InlineData("SA29-8400-0000-0001-2345-6789", true)]
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
