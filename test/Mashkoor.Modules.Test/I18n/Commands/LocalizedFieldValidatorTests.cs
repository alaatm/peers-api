using FluentValidation.TestHelper;
using Mashkoor.Modules.I18n.Commands;
using Mashkoor.Modules.I18n.Domain;
using Mashkoor.Modules.Test.SharedClasses;

namespace Mashkoor.Modules.Test.I18n.Commands;

public class LocalizedFieldValidatorTests
{
    [Fact]
    public void ValidateTest_passes_for_valid_object()
    {
        // Arrange
        var validator = new LocalizedFieldValidator(new MockBuilder.SLMoq<res>());

        // Act
        var result = validator.TestValidate(new LocalizedField("en", "test"));

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ValidateTest_fails_for_null_or_empty_language(string lang)
    {
        // Arrange
        var validator = new LocalizedFieldValidator(new MockBuilder.SLMoq<res>());

        // Act
        var result = validator.TestValidate(new LocalizedField(lang, "test"));

        // Assert
        result.Assert(p => p.Language, true);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ValidateTest_fails_for_null_or_empty_value(string value)
    {
        // Arrange
        var validator = new LocalizedFieldValidator(new MockBuilder.SLMoq<res>());

        // Act
        var result = validator.TestValidate(new LocalizedField("en", value));

        // Assert
        result.Assert(p => p.Value, true);
    }
}
